using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Data.SQLite;
using BillingSoftware.Models;
using BillingSoftware.Modules;
using BillingSoftware.Utilities;

namespace BillingSoftware.Forms.Vouchers
{
    public partial class ReceiptForm : Form
    {
        private DatabaseManager dbManager;
        private AuditLogger auditLogger;
        private LedgerManager ledgerManager;
        
        private TextBox receiptNoTxt, receivedFromTxt, amountTxt, amountInWordsTxt;
        private TextBox chequeNoTxt, bankNameTxt, narrationTxt;
        private DateTimePicker datePicker, chequeDatePicker;
        private ComboBox paymentModeCombo, ledgerCombo;
        private DataGridView detailsGrid;
        private Button saveBtn, addDetailBtn, removeDetailBtn, calculateBtn;
        private List<ReceiptDetail> receiptDetails;
        
        public ReceiptForm()
        {
            InitializeComponent();
            dbManager = new DatabaseManager();
            auditLogger = new AuditLogger();
            ledgerManager = new LedgerManager();
            receiptDetails = new List<ReceiptDetail>();
            CreateReceiptFormUI();
            LoadLedgerList();
        }
        
        private void CreateReceiptFormUI()
        {
            this.Text = "Receipt Voucher";
            this.Size = new Size(850, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = TallyUIStyles.TallyGray;
            
            // Title
            Label titleLabel = new Label();
            titleLabel.Text = "💰 RECEIPT VOUCHER";
            titleLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            titleLabel.ForeColor = TallyUIStyles.TallyBlue;
            titleLabel.Location = new Point(20, 20);
            titleLabel.Size = new Size(300, 30);
            this.Controls.Add(titleLabel);
            
            // Main Container
            GroupBox mainGroup = TallyUIStyles.CreateTallyGroupBox("Receipt Details", new Point(20, 60), new Size(800, 550));
            
            int yPos = 30;
            
            // Receipt Number
            Label receiptNoLabel = TallyUIStyles.CreateTallyLabel("Receipt No*:", new Point(20, yPos), new Size(100, 20), true);
            receiptNoTxt = TallyUIStyles.CreateTallyTextBox(new Point(130, yPos), new Size(150, 25));
            receiptNoTxt.Text = GenerateReceiptNumber();
            receiptNoTxt.ReadOnly = true;
            mainGroup.Controls.AddRange(new Control[] { receiptNoLabel, receiptNoTxt });
            
            // Date
            Label dateLabel = TallyUIStyles.CreateTallyLabel("Date*:", new Point(300, yPos), new Size(50, 20), true);
            datePicker = TallyUIStyles.CreateTallyDateTimePicker(new Point(360, yPos), new Size(120, 25));
            mainGroup.Controls.AddRange(new Control[] { dateLabel, datePicker });
            yPos += 35;
            
            // Received From
            Label receivedLabel = TallyUIStyles.CreateTallyLabel("Received From*:", new Point(20, yPos), new Size(100, 20), true);
            receivedFromTxt = TallyUIStyles.CreateTallyTextBox(new Point(130, yPos), new Size(350, 25));
            mainGroup.Controls.AddRange(new Control[] { receivedLabel, receivedFromTxt });
            yPos += 35;
            
            // Payment Mode
            Label modeLabel = TallyUIStyles.CreateTallyLabel("Payment Mode*:", new Point(20, yPos), new Size(100, 20), true);
            paymentModeCombo = TallyUIStyles.CreateTallyComboBox(new Point(130, yPos), new Size(150, 25));
            paymentModeCombo.Items.AddRange(new string[] { "Cash", "Cheque", "Bank Transfer", "UPI", "Card" });
            paymentModeCombo.SelectedIndex = 0;
            paymentModeCombo.SelectedIndexChanged += PaymentModeCombo_SelectedIndexChanged;
            mainGroup.Controls.AddRange(new Control[] { modeLabel, paymentModeCombo });
            
            // Amount
            Label amountLabel = TallyUIStyles.CreateTallyLabel("Amount*:", new Point(300, yPos), new Size(60, 20), true);
            amountTxt = TallyUIStyles.CreateTallyTextBox(new Point(370, yPos), new Size(150, 25));
            amountTxt.Text = "0.00";
            amountTxt.TextChanged += AmountTxt_TextChanged;
            mainGroup.Controls.AddRange(new Control[] { amountLabel, amountTxt });
            yPos += 35;
            
            // Cheque Details (Initially hidden)
            Panel chequePanel = new Panel();
            chequePanel.Location = new Point(20, yPos);
            chequePanel.Size = new Size(500, 30);
            chequePanel.Visible = false;
            chequePanel.Name = "chequePanel";
            
            Label chequeNoLabel = TallyUIStyles.CreateTallyLabel("Cheque No:", new Point(0, 5), new Size(70, 20));
            chequeNoTxt = TallyUIStyles.CreateTallyTextBox(new Point(75, 5), new Size(100, 25));
            
            Label chequeDateLabel = TallyUIStyles.CreateTallyLabel("Date:", new Point(185, 5), new Size(40, 20));
            chequeDatePicker = TallyUIStyles.CreateTallyDateTimePicker(new Point(230, 5), new Size(120, 25));
            
            Label bankLabel = TallyUIStyles.CreateTallyLabel("Bank:", new Point(360, 5), new Size(40, 20));
            bankNameTxt = TallyUIStyles.CreateTallyTextBox(new Point(405, 5), new Size(150, 25));
            
            chequePanel.Controls.AddRange(new Control[] { chequeNoLabel, chequeNoTxt, chequeDateLabel, chequeDatePicker, bankLabel, bankNameTxt });
            mainGroup.Controls.Add(chequePanel);
            yPos += 40;
            
            // Amount in Words
            Label wordsLabel = TallyUIStyles.CreateTallyLabel("Amount in Words:", new Point(20, yPos), new Size(120, 20), true);
            amountInWordsTxt = TallyUIStyles.CreateTallyTextBox(new Point(150, yPos), new Size(400, 25));
            amountInWordsTxt.ReadOnly = true;
            mainGroup.Controls.AddRange(new Control[] { wordsLabel, amountInWordsTxt });
            yPos += 35;
            
            // Details Section
            Label detailsLabel = TallyUIStyles.CreateTallyLabel("Receipt Details:", new Point(20, yPos), new Size(120, 20), true);
            mainGroup.Controls.Add(detailsLabel);
            yPos += 25;
            
            // Details Controls Panel
            Panel detailControlPanel = new Panel();
            detailControlPanel.Location = new Point(20, yPos);
            detailControlPanel.Size = new Size(760, 40);
            detailControlPanel.BackColor = Color.Transparent;
            
            Label ledgerLabel = TallyUIStyles.CreateTallyLabel("Ledger:", new Point(0, 8), new Size(50, 20));
            ledgerCombo = TallyUIStyles.CreateTallyComboBox(new Point(55, 5), new Size(200, 25));
            
            Label particularsLabel = TallyUIStyles.CreateTallyLabel("Particulars:", new Point(265, 8), new Size(70, 20));
            TextBox particularsTxt = TallyUIStyles.CreateTallyTextBox(new Point(340, 5), new Size(200, 25));
            particularsTxt.PlaceholderText = "Enter particulars";
            
            Label detailAmountLabel = TallyUIStyles.CreateTallyLabel("Amount:", new Point(550, 8), new Size(50, 20));
            TextBox detailAmountTxt = TallyUIStyles.CreateTallyTextBox(new Point(605, 5), new Size(100, 25));
            detailAmountTxt.Text = "0.00";
            
            addDetailBtn = TallyUIStyles.CreateTallyButton("➕ Add", TallyUIStyles.TallyGreen, new Point(715, 5), new Size(80, 25));
            
            detailControlPanel.Controls.AddRange(new Control[] { ledgerLabel, ledgerCombo, particularsLabel, particularsTxt, 
                                                               detailAmountLabel, detailAmountTxt, addDetailBtn });
            mainGroup.Controls.Add(detailControlPanel);
            yPos += 50;
            
            // Details Grid
            detailsGrid = TallyUIStyles.CreateTallyGrid(new Point(20, yPos), new Size(760, 200));
            detailsGrid.Columns.Add("Ledger", "Ledger Account");
            detailsGrid.Columns.Add("Particulars", "Particulars");
            detailsGrid.Columns.Add("Amount", "Amount");
            detailsGrid.Columns.Add("VoucherReference", "Voucher Reference");
            
            detailsGrid.Columns["Amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            detailsGrid.Columns["Amount"].DefaultCellStyle.Format = "N2";
            
            mainGroup.Controls.Add(detailsGrid);
            yPos += 210;
            
            // Remove Detail Button
            removeDetailBtn = TallyUIStyles.CreateTallyButton("➖ Remove Selected", TallyUIStyles.TallyRed, new Point(20, yPos), new Size(150, 25));
            mainGroup.Controls.Add(removeDetailBtn);
            yPos += 35;
            
            // Narration
            Label narrationLabel = TallyUIStyles.CreateTallyLabel("Narration:", new Point(20, yPos), new Size(80, 20), true);
            narrationTxt = new TextBox
            {
                Location = new Point(105, yPos),
                Size = new Size(350, 60),
                Font = TallyUIStyles.NormalFont,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            mainGroup.Controls.AddRange(new Control[] { narrationLabel, narrationTxt });
            
            // Calculate Button
            calculateBtn = TallyUIStyles.CreateTallyButton("🧮 Calculate Total", TallyUIStyles.TallyOrange, new Point(470, yPos + 15), new Size(150, 30));
            calculateBtn.Click += CalculateBtn_Click;
            mainGroup.Controls.Add(calculateBtn);
            
            this.Controls.Add(mainGroup);
            
            // Form Buttons
            saveBtn = TallyUIStyles.CreateTallyButton("💾 Save Receipt", TallyUIStyles.TallyGreen, new Point(150, 630));
            saveBtn.Click += SaveBtn_Click;
            
            Button cancelBtn = TallyUIStyles.CreateTallyButton("❌ Cancel", TallyUIStyles.TallyGray, new Point(280, 630));
            cancelBtn.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            
            Button printBtn = TallyUIStyles.CreateTallyButton("🖨️ Print Preview", TallyUIStyles.TallyPurple, new Point(410, 630));
            printBtn.Click += PrintBtn_Click;
            
            this.Controls.AddRange(new Control[] { saveBtn, cancelBtn, printBtn });
            
            // Event Handlers
            addDetailBtn.Click += (s, e) => AddDetail(ledgerCombo, particularsTxt, detailAmountTxt);
            removeDetailBtn.Click += RemoveDetailBtn_Click;
        }
        
        private string GenerateReceiptNumber()
        {
            try
            {
                string sql = "SELECT COUNT(*) FROM receipt_vouchers WHERE strftime('%Y-%m-%d', date) = date('now')";
                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                {
                    int count = Convert.ToInt32(cmd.ExecuteScalar()) + 1;
                    return $"RCP-{DateTime.Now:yyyyMMdd}-{count.ToString("000")}";
                }
            }
            catch { return $"RCP-{DateTime.Now:yyyyMMdd}-001"; }
        }
        
        private void LoadLedgerList()
        {
            try
            {
                string sql = @"SELECT name FROM ledgers 
                              WHERE type IN ('Customer', 'Sundry Debtors', 'Income', 'Sales') 
                              AND is_active = 1 
                              ORDER BY name";
                
                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                using (var reader = cmd.ExecuteReader())
                {
                    ledgerCombo.Items.Clear();
                    ledgerCombo.Items.Add("-- Select Ledger --");
                    while (reader.Read())
                    {
                        ledgerCombo.Items.Add(reader["name"].ToString());
                    }
                    if (ledgerCombo.Items.Count > 0)
                        ledgerCombo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading ledgers: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void PaymentModeCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            var chequePanel = this.Controls.Find("chequePanel", true)[0] as Panel;
            bool isCheque = (paymentModeCombo.SelectedItem?.ToString() == "Cheque");
            chequePanel.Visible = isCheque;
            
            // Clear cheque details if not cheque mode
            if (!isCheque)
            {
                chequeNoTxt.Clear();
                bankNameTxt.Clear();
                chequeDatePicker.Value = DateTime.Now;
            }
        }
        
        private void AmountTxt_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(amountTxt.Text, out decimal amount))
            {
                amountInWordsTxt.Text = ConvertNumberToWords(amount);
            }
        }
        
        private string ConvertNumberToWords(decimal number)
        {
            try
            {
                int rupees = (int)Math.Floor(number);
                int paise = (int)((number - rupees) * 100);
                
                string words = NumberToWords(rupees) + " Rupees";
                if (paise > 0)
                    words += " and " + NumberToWords(paise) + " Paise";
                
                return words + " Only";
            }
            catch
            {
                return "Amount in words";
            }
        }
        
        private string NumberToWords(int number)
        {
            if (number == 0) return "Zero";
            
            string words = "";
            
            if ((number / 10000000) > 0)
            {
                words += NumberToWords(number / 10000000) + " Crore ";
                number %= 10000000;
            }
            
            if ((number / 100000) > 0)
            {
                words += NumberToWords(number / 100000) + " Lakh ";
                number %= 100000;
            }
            
            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " Thousand ";
                number %= 1000;
            }
            
            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " Hundred ";
                number %= 100;
            }
            
            if (number > 0)
            {
                if (words != "")
                    words += "and ";
                
                var unitsMap = new[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", 
                                      "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
                var tensMap = new[] { "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };
                
                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }
            
            return words.Trim();
        }
        
        private void AddDetail(ComboBox ledgerCombo, TextBox particularsTxt, TextBox amountTxt)
        {
            if (ledgerCombo.SelectedIndex <= 0)
            {
                MessageBox.Show("Please select a ledger!", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (!decimal.TryParse(amountTxt.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Please enter valid amount greater than 0!", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            var detail = new ReceiptDetail
            {
                LedgerName = ledgerCombo.SelectedItem.ToString(),
                Particulars = particularsTxt.Text.Trim(),
                Amount = amount,
                VoucherReference = ""
            };
            
            receiptDetails.Add(detail);
            
            detailsGrid.Rows.Add(
                detail.LedgerName,
                detail.Particulars,
                detail.Amount,
                detail.VoucherReference
            );
            
            particularsTxt.Clear();
            amountTxt.Text = "0.00";
            
            CalculateTotal();
        }
        
        private void RemoveDetailBtn_Click(object sender, EventArgs e)
        {
            if (detailsGrid.SelectedRows.Count > 0)
            {
                int index = detailsGrid.SelectedRows[0].Index;
                if (index < receiptDetails.Count)
                {
                    receiptDetails.RemoveAt(index);
                    detailsGrid.Rows.RemoveAt(index);
                    CalculateTotal();
                }
            }
        }
        
        private void CalculateTotal()
        {
            decimal total = receiptDetails.Sum(d => d.Amount);
            amountTxt.Text = total.ToString("N2");
            AmountTxt_TextChanged(null, EventArgs.Empty);
        }
        
        private void CalculateBtn_Click(object sender, EventArgs e)
        {
            CalculateTotal();
        }
        
        private void SaveBtn_Click(object sender, EventArgs e)
        {
            if (!ValidateReceipt()) return;
            
            try
            {
                var receipt = new ReceiptVoucher
                {
                    Number = receiptNoTxt.Text,
                    Date = datePicker.Value,
                    ReceivedFrom = receivedFromTxt.Text.Trim(),
                    AmountInWords = amountInWordsTxt.Text,
                    Amount = decimal.Parse(amountTxt.Text),
                    PaymentMode = paymentModeCombo.SelectedItem.ToString(),
                    ChequeNo = chequeNoTxt.Text,
                    ChequeDate = chequeDatePicker.Value,
                    BankName = bankNameTxt.Text,
                    Narration = narrationTxt.Text.Trim(),
                    CreatedBy = Program.CurrentUser,
                    Details = receiptDetails,
                    IsPosted = true
                };
                
                // Save receipt to database
                if (dbManager.AddReceiptVoucher(receipt))
                {
                    // Post to ledger
                    if (ledgerManager.PostReceiptTransaction(receipt))
                    {
                        // Audit log
                        auditLogger.LogAction("CREATE", "RECEIPT", receipt.Number, 
                                            $"Created receipt for {receipt.ReceivedFrom} - Amount: ₹{receipt.Amount:N2}", 
                                            "Receipts", receipt.ReceivedFrom);
                        
                        string message = $"✅ Receipt saved successfully!\n\n" +
                                       $"Receipt #: {receipt.Number}\n" +
                                       $"Received From: {receipt.ReceivedFrom}\n" +
                                       $"Amount: ₹{receipt.Amount:N2}\n" +
                                       $"Payment Mode: {receipt.PaymentMode}\n\n" +
                                       $"Do you want to print this receipt?";
                        
                        var result = MessageBox.Show(message, "Success", 
                                                   MessageBoxButtons.YesNo, 
                                                   MessageBoxIcon.Information);
                        
                        if (result == DialogResult.Yes)
                        {
                            PrintBtn_Click(null, EventArgs.Empty);
                        }
                        
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Error posting receipt to ledger!", "Ledger Error", 
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error saving receipt: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private bool ValidateReceipt()
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(receivedFromTxt.Text))
            {
                MessageBox.Show("Please enter 'Received From'!", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                receivedFromTxt.Focus();
                return false;
            }
            
            if (!decimal.TryParse(amountTxt.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Please enter valid amount greater than 0!", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                amountTxt.Focus();
                return false;
            }
            
            if (receiptDetails.Count == 0)
            {
                MessageBox.Show("Please add at least one receipt detail!", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            
            // Validate cheque details if payment mode is cheque
            if (paymentModeCombo.SelectedItem.ToString() == "Cheque")
            {
                if (string.IsNullOrWhiteSpace(chequeNoTxt.Text))
                {
                    MessageBox.Show("Please enter cheque number!", "Validation Error", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    chequeNoTxt.Focus();
                    return false;
                }
                
                if (string.IsNullOrWhiteSpace(bankNameTxt.Text))
                {
                    MessageBox.Show("Please enter bank name!", "Validation Error", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    bankNameTxt.Focus();
                    return false;
                }
            }
            
            return true;
        }
        
        private void PrintBtn_Click(object sender, EventArgs e)
        {
            if (receiptDetails.Count == 0)
            {
                MessageBox.Show("Please add details before printing!", "No Details", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            try
            {
                var receiptVoucher = new Voucher
                {
                    Type = "Receipt",
                    Number = receiptNoTxt.Text,
                    Date = datePicker.Value,
                    Party = receivedFromTxt.Text.Trim(),
                    Amount = decimal.Parse(amountTxt.Text),
                    Description = $"Receipt from {receivedFromTxt.Text.Trim()} | Mode: {paymentModeCombo.SelectedItem}"
                };
                
                var voucherItems = new List<VoucherItem>();
                foreach (var detail in receiptDetails)
                {
                    voucherItems.Add(new VoucherItem
                    {
                        ProductName = detail.Particulars,
                        Quantity = 1,
                        UnitPrice = detail.Amount
                    });
                }
                
                PrintHelper printHelper = new PrintHelper();
                printHelper.PrintVoucher(receiptVoucher, voucherItems);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing receipt: {ex.Message}", "Print Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(300, 250);
            this.Name = "ReceiptForm";
            this.ResumeLayout(false);
        }
    }
}