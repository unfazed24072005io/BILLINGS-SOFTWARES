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
    public partial class ReceiptAgainstSalesForm : Form
    {
        private DatabaseManager dbManager;
        private AuditLogger auditLogger;
        
        private ComboBox salesVoucherCombo;
        private TextBox receiptNoTxt, receivedFromTxt, amountTxt, amountInWordsTxt;
        private TextBox chequeNoTxt, bankNameTxt, narrationTxt;
        private DateTimePicker datePicker, chequeDatePicker;
        private ComboBox paymentModeCombo;
        private Button saveBtn, calculateBtn;
        
        public ReceiptAgainstSalesForm()
        {
            InitializeComponent();
            dbManager = new DatabaseManager();
            auditLogger = new AuditLogger();
            CreateReceiptAgainstSalesFormUI();
            LoadSalesVouchers();
        }
        
        private void CreateReceiptAgainstSalesFormUI()
        {
            this.Text = "Receipt Against Sales";
            this.Size = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = TallyUIStyles.TallyGray;
            
            // Title
            Label titleLabel = new Label();
            titleLabel.Text = "💰 RECEIPT AGAINST SALES";
            titleLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            titleLabel.ForeColor = TallyUIStyles.TallyBlue;
            titleLabel.Location = new Point(20, 20);
            titleLabel.Size = new Size(400, 30);
            this.Controls.Add(titleLabel);
            
            // Main Container
            GroupBox mainGroup = TallyUIStyles.CreateTallyGroupBox("Receipt Details", new Point(20, 60), new Size(650, 380));
            
            int yPos = 30;
            
            // Select Sales Voucher
            Label salesLabel = TallyUIStyles.CreateTallyLabel("Select Sales Voucher*:", new Point(20, yPos), new Size(150, 20), true);
            salesVoucherCombo = TallyUIStyles.CreateTallyComboBox(new Point(180, yPos), new Size(300, 25));
            salesVoucherCombo.SelectedIndexChanged += SalesVoucherCombo_SelectedIndexChanged;
            mainGroup.Controls.AddRange(new Control[] { salesLabel, salesVoucherCombo });
            yPos += 35;
            
            // Receipt Number
            Label receiptNoLabel = TallyUIStyles.CreateTallyLabel("Receipt No*:", new Point(20, yPos), new Size(120, 20), true);
            receiptNoTxt = TallyUIStyles.CreateTallyTextBox(new Point(180, yPos), new Size(150, 25));
            receiptNoTxt.Text = GenerateReceiptNumber();
            receiptNoTxt.ReadOnly = true;
            mainGroup.Controls.AddRange(new Control[] { receiptNoLabel, receiptNoTxt });
            
            // Date
            Label dateLabel = TallyUIStyles.CreateTallyLabel("Date*:", new Point(350, yPos), new Size(50, 20), true);
            datePicker = TallyUIStyles.CreateTallyDateTimePicker(new Point(410, yPos), new Size(120, 25));
            mainGroup.Controls.AddRange(new Control[] { dateLabel, datePicker });
            yPos += 35;
            
            // Received From
            Label receivedLabel = TallyUIStyles.CreateTallyLabel("Received From*:", new Point(20, yPos), new Size(120, 20), true);
            receivedFromTxt = TallyUIStyles.CreateTallyTextBox(new Point(180, yPos), new Size(300, 25));
            receivedFromTxt.ReadOnly = true;
            mainGroup.Controls.AddRange(new Control[] { receivedLabel, receivedFromTxt });
            yPos += 35;
            
            // Payment Mode
            Label modeLabel = TallyUIStyles.CreateTallyLabel("Payment Mode*:", new Point(20, yPos), new Size(120, 20), true);
            paymentModeCombo = TallyUIStyles.CreateTallyComboBox(new Point(180, yPos), new Size(150, 25));
            paymentModeCombo.Items.AddRange(new string[] { "Cash", "Cheque", "Bank Transfer", "UPI", "Card" });
            paymentModeCombo.SelectedIndex = 0;
            paymentModeCombo.SelectedIndexChanged += PaymentModeCombo_SelectedIndexChanged;
            mainGroup.Controls.AddRange(new Control[] { modeLabel, paymentModeCombo });
            
            // Amount
            Label amountLabel = TallyUIStyles.CreateTallyLabel("Amount*:", new Point(350, yPos), new Size(60, 20), true);
            amountTxt = TallyUIStyles.CreateTallyTextBox(new Point(420, yPos), new Size(150, 25));
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
            calculateBtn = TallyUIStyles.CreateTallyButton("🧮 Calculate", TallyUIStyles.TallyOrange, new Point(470, yPos + 15), new Size(120, 30));
            calculateBtn.Click += CalculateBtn_Click;
            mainGroup.Controls.Add(calculateBtn);
            
            this.Controls.Add(mainGroup);
            
            // Form Buttons
            saveBtn = TallyUIStyles.CreateTallyButton("💾 Save Receipt", TallyUIStyles.TallyGreen, new Point(150, 450));
            saveBtn.Click += SaveBtn_Click;
            
            Button cancelBtn = TallyUIStyles.CreateTallyButton("❌ Cancel", TallyUIStyles.TallyGray, new Point(280, 450));
            cancelBtn.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            
            this.Controls.AddRange(new Control[] { saveBtn, cancelBtn });
        }
        
        private void LoadSalesVouchers()
        {
            try
            {
                string sql = @"SELECT v.number, v.date, v.party, v.amount, 
                              COALESCE(SUM(r.amount), 0) as received_amount
                              FROM vouchers v
                              LEFT JOIN receipt_details rd ON v.number = rd.voucher_reference
                              LEFT JOIN receipt_vouchers r ON rd.receipt_number = r.number
                              WHERE v.type = 'Sales' AND v.status = 'Active'
                              GROUP BY v.id
                              HAVING v.amount > COALESCE(SUM(r.amount), 0)
                              ORDER BY v.date DESC";
                
                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                using (var reader = cmd.ExecuteReader())
                {
                    salesVoucherCombo.Items.Clear();
                    while (reader.Read())
                    {
                        string voucherNo = reader["number"].ToString();
                        string party = reader["party"].ToString();
                        decimal amount = Convert.ToDecimal(reader["amount"]);
                        decimal received = Convert.ToDecimal(reader["received_amount"]);
                        decimal pending = amount - received;
                        
                        salesVoucherCombo.Items.Add($"{voucherNo} - {party} (₹{amount:N2}, Pending: ₹{pending:N2})");
                    }
                    
                    if (salesVoucherCombo.Items.Count > 0)
                        salesVoucherCombo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading sales vouchers: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void SalesVoucherCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (salesVoucherCombo.SelectedItem != null)
            {
                string selectedText = salesVoucherCombo.SelectedItem.ToString();
                
                // Extract voucher number and party
                string[] parts = selectedText.Split('-');
                if (parts.Length >= 2)
                {
                    string voucherNo = parts[0].Trim();
                    string party = parts[1].Split('(')[0].Trim();
                    
                    receivedFromTxt.Text = party;
                    
                    // Extract pending amount
                    string amountText = selectedText.Split('₹')[2].Split(',')[0];
                    if (decimal.TryParse(amountText, out decimal pendingAmount))
                    {
                        amountTxt.Text = pendingAmount.ToString("N2");
                    }
                }
            }
        }
        
        private string GenerateReceiptNumber()
        {
            try
            {
                string sql = "SELECT COUNT(*) FROM receipt_vouchers";
                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                {
                    int count = Convert.ToInt32(cmd.ExecuteScalar()) + 1;
                    return $"RCP-{DateTime.Now:yyyyMMdd}-{count.ToString("000")}";
                }
            }
            catch { return $"RCP-{DateTime.Now:yyyyMMdd}-001"; }
        }
        
        private void PaymentModeCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            var chequePanel = this.Controls.Find("chequePanel", true)[0] as Panel;
            chequePanel.Visible = (paymentModeCombo.SelectedItem.ToString() == "Cheque");
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
            // Simple implementation
            int rupees = (int)Math.Floor(number);
            int paise = (int)((number - rupees) * 100);
            
            string words = NumberToWords(rupees) + " Rupees";
            if (paise > 0)
                words += " and " + NumberToWords(paise) + " Paise";
            
            return words + " Only";
        }
        
        private string NumberToWords(int number)
        {
            if (number == 0)
                return "Zero";
            
            string words = "";
            
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
            
            return words;
        }
        
        private void CalculateBtn_Click(object sender, EventArgs e)
        {
            // Update amount in words
            AmountTxt_TextChanged(null, EventArgs.Empty);
        }
        
        private void SaveBtn_Click(object sender, EventArgs e)
        {
            if (!ValidateReceipt()) return;
            
            try
            {
                // Get selected sales voucher
                string selectedText = salesVoucherCombo.SelectedItem.ToString();
                string salesVoucherNo = selectedText.Split('-')[0].Trim();
                string party = receivedFromTxt.Text.Trim();
                
                var receipt = new ReceiptVoucher
                {
                    Number = receiptNoTxt.Text,
                    Date = datePicker.Value,
                    ReceivedFrom = party,
                    AmountInWords = amountInWordsTxt.Text,
                    Amount = decimal.Parse(amountTxt.Text),
                    PaymentMode = paymentModeCombo.SelectedItem.ToString(),
                    ChequeNo = chequeNoTxt.Text,
                    ChequeDate = chequeDatePicker.Value,
                    BankName = bankNameTxt.Text,
                    Narration = narrationTxt.Text.Trim(),
                    CreatedBy = Program.CurrentUser,
                    Details = new List<ReceiptDetail>
                    {
                        new ReceiptDetail
                        {
                            LedgerName = "Sundry Debtors",
                            Particulars = $"Receipt against Sales {salesVoucherNo}",
                            Amount = decimal.Parse(amountTxt.Text),
                            VoucherReference = salesVoucherNo
                        }
                    }
                };
                
                if (dbManager.AddReceiptVoucher(receipt))
                {
                    // Audit log
                    auditLogger.LogAction("CREATE", "RECEIPT", receipt.Number, 
                                        $"Created receipt against sales {salesVoucherNo} - ₹{receipt.Amount:N2}", "Receipts");
                    
                    string message = $"Receipt saved successfully!\n\n" +
                                   $"Receipt #: {receipt.Number}\n" +
                                   $"Against Sales: {salesVoucherNo}\n" +
                                   $"Received From: {receipt.ReceivedFrom}\n" +
                                   $"Amount: ₹{receipt.Amount:N2}\n" +
                                   $"Payment Mode: {receipt.PaymentMode}";
                    
                    MessageBox.Show(message, "Success", 
                                  MessageBoxButtons.OK, 
                                  MessageBoxIcon.Information);
                    
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving receipt: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private bool ValidateReceipt()
        {
            if (salesVoucherCombo.SelectedItem == null)
            {
                MessageBox.Show("Please select a sales voucher!", "Validation Error");
                salesVoucherCombo.Focus();
                return false;
            }
            
            if (!decimal.TryParse(amountTxt.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Please enter valid amount!", "Validation Error");
                amountTxt.Focus();
                return false;
            }
            
            return true;
        }
        
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(300, 250);
            this.Name = "ReceiptAgainstSalesForm";
            this.ResumeLayout(false);
        }
    }
}