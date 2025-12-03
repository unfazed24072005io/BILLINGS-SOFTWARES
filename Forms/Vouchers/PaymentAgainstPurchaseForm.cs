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
    public partial class PaymentAgainstPurchaseForm : Form
    {
        private DatabaseManager dbManager;
        private AuditLogger auditLogger;
        
        private ComboBox purchaseVoucherCombo;
        private TextBox paymentNoTxt, paidToTxt, amountTxt, amountInWordsTxt;
        private TextBox chequeNoTxt, bankNameTxt, narrationTxt;
        private DateTimePicker datePicker, chequeDatePicker;
        private ComboBox paymentModeCombo;
        private Button saveBtn, calculateBtn;
        
        private decimal pendingAmount = 0;
        private string selectedVoucherNo = "";
        private string selectedSupplier = "";
        
        public PaymentAgainstPurchaseForm()
        {
            InitializeComponent();
            dbManager = new DatabaseManager();
            auditLogger = new AuditLogger();
            CreatePaymentAgainstPurchaseFormUI();
            LoadPurchaseVouchers();
        }
        
        private void CreatePaymentAgainstPurchaseFormUI()
        {
            this.Text = "Payment Against Purchase";
            this.Size = new Size(700, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = TallyUIStyles.TallyGray;
            
            // Title
            Label titleLabel = new Label();
            titleLabel.Text = "💳 PAYMENT AGAINST PURCHASE";
            titleLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            titleLabel.ForeColor = TallyUIStyles.TallyBlue;
            titleLabel.Location = new Point(20, 20);
            titleLabel.Size = new Size(400, 30);
            this.Controls.Add(titleLabel);
            
            // Main Container
            GroupBox mainGroup = TallyUIStyles.CreateTallyGroupBox("Payment Details", new Point(20, 60), new Size(650, 420));
            
            int yPos = 30;
            
            // Select Purchase Voucher
            Label purchaseLabel = TallyUIStyles.CreateTallyLabel("Select Purchase Voucher*:", new Point(20, yPos), new Size(150, 20), true);
            purchaseVoucherCombo = TallyUIStyles.CreateTallyComboBox(new Point(180, yPos), new Size(300, 25));
            purchaseVoucherCombo.SelectedIndexChanged += PurchaseVoucherCombo_SelectedIndexChanged;
            mainGroup.Controls.AddRange(new Control[] { purchaseLabel, purchaseVoucherCombo });
            yPos += 35;
            
            // Payment Number
            Label paymentNoLabel = TallyUIStyles.CreateTallyLabel("Payment No*:", new Point(20, yPos), new Size(120, 20), true);
            paymentNoTxt = TallyUIStyles.CreateTallyTextBox(new Point(180, yPos), new Size(150, 25));
            paymentNoTxt.Text = GeneratePaymentNumber();
            paymentNoTxt.ReadOnly = true;
            mainGroup.Controls.AddRange(new Control[] { paymentNoLabel, paymentNoTxt });
            
            // Date
            Label dateLabel = TallyUIStyles.CreateTallyLabel("Date*:", new Point(350, yPos), new Size(50, 20), true);
            datePicker = TallyUIStyles.CreateTallyDateTimePicker(new Point(410, yPos), new Size(120, 25));
            mainGroup.Controls.AddRange(new Control[] { dateLabel, datePicker });
            yPos += 35;
            
            // Paid To
            Label paidToLabel = TallyUIStyles.CreateTallyLabel("Paid To*:", new Point(20, yPos), new Size(120, 20), true);
            paidToTxt = TallyUIStyles.CreateTallyTextBox(new Point(180, yPos), new Size(300, 25));
            paidToTxt.ReadOnly = true;
            mainGroup.Controls.AddRange(new Control[] { paidToLabel, paidToTxt });
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
            saveBtn = TallyUIStyles.CreateTallyButton("💾 Save Payment", TallyUIStyles.TallyGreen, new Point(150, 500));
            saveBtn.Click += SaveBtn_Click;
            
            Button cancelBtn = TallyUIStyles.CreateTallyButton("❌ Cancel", TallyUIStyles.TallyGray, new Point(280, 500));
            cancelBtn.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            
            this.Controls.AddRange(new Control[] { saveBtn, cancelBtn });
        }
        
        private void LoadPurchaseVouchers()
        {
            try
            {
                string sql = @"SELECT v.number, v.date, v.party, v.amount, 
                              COALESCE(SUM(pd.amount), 0) as paid_amount
                              FROM vouchers v
                              LEFT JOIN payment_details pd ON v.number = pd.voucher_reference
                              WHERE v.type = 'Stock Purchase' AND v.status = 'Active'
                              GROUP BY v.id
                              HAVING v.amount > COALESCE(SUM(pd.amount), 0)
                              ORDER BY v.date DESC";
                
                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                using (var reader = cmd.ExecuteReader())
                {
                    purchaseVoucherCombo.Items.Clear();
                    while (reader.Read())
                    {
                        string voucherNo = reader["number"].ToString();
                        string party = reader["party"].ToString();
                        decimal amount = Convert.ToDecimal(reader["amount"]);
                        decimal paid = Convert.ToDecimal(reader["paid_amount"]);
                        decimal pending = amount - paid;
                        
                        purchaseVoucherCombo.Items.Add($"{voucherNo} - {party} (₹{amount:N2}, Pending: ₹{pending:N2})");
                    }
                    
                    if (purchaseVoucherCombo.Items.Count > 0)
                        purchaseVoucherCombo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading purchase vouchers: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void PurchaseVoucherCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (purchaseVoucherCombo.SelectedItem != null)
            {
                string selectedText = purchaseVoucherCombo.SelectedItem.ToString();
                
                // Extract voucher number and party
                string[] parts = selectedText.Split('-');
                if (parts.Length >= 2)
                {
                    selectedVoucherNo = parts[0].Trim();
                    selectedSupplier = parts[1].Split('(')[0].Trim();
                    
                    paidToTxt.Text = selectedSupplier;
                    
                    // Extract pending amount
                    string amountText = selectedText.Split('₹')[2].Split(',')[0];
                    if (decimal.TryParse(amountText, out pendingAmount))
                    {
                        amountTxt.Text = pendingAmount.ToString("N2");
                    }
                }
            }
        }
        
        private string GeneratePaymentNumber()
        {
            try
            {
                string sql = "SELECT COUNT(*) FROM payment_vouchers";
                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                {
                    int count = Convert.ToInt32(cmd.ExecuteScalar()) + 1;
                    return $"PAY-{DateTime.Now:yyyyMMdd}-{count.ToString("000")}";
                }
            }
            catch { return $"PAY-{DateTime.Now:yyyyMMdd}-001"; }
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
            if (!ValidatePayment()) return;
            
            try
            {
                var payment = new PaymentVoucher
                {
                    Number = paymentNoTxt.Text,
                    Date = datePicker.Value,
                    PaidTo = selectedSupplier,
                    AmountInWords = amountInWordsTxt.Text,
                    Amount = decimal.Parse(amountTxt.Text),
                    PaymentMode = paymentModeCombo.SelectedItem.ToString(),
                    ChequeNo = chequeNoTxt.Text,
                    ChequeDate = chequeDatePicker.Value,
                    BankName = bankNameTxt.Text,
                    Narration = narrationTxt.Text.Trim(),
                    CreatedBy = Program.CurrentUser,
                    Details = new List<PaymentDetail>
                    {
                        new PaymentDetail
                        {
                            LedgerName = "Sundry Creditors",
                            Particulars = $"Payment against Purchase {selectedVoucherNo}",
                            Amount = decimal.Parse(amountTxt.Text),
                            VoucherReference = selectedVoucherNo
                        }
                    }
                };
                
                if (dbManager.AddPaymentVoucher(payment))
                {
                    // Audit log
                    auditLogger.LogAction("CREATE", "PAYMENT", payment.Number, 
                                        $"Created payment against purchase {selectedVoucherNo} - ₹{payment.Amount:N2}", "Payments");
                    
                    string message = $"Payment saved successfully!\n\n" +
                                   $"Payment #: {payment.Number}\n" +
                                   $"Against Purchase: {selectedVoucherNo}\n" +
                                   $"Paid To: {payment.PaidTo}\n" +
                                   $"Amount: ₹{payment.Amount:N2}\n" +
                                   $"Payment Mode: {payment.PaymentMode}";
                    
                    MessageBox.Show(message, "Success", 
                                  MessageBoxButtons.OK, 
                                  MessageBoxIcon.Information);
                    
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving payment: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private bool ValidatePayment()
        {
            if (purchaseVoucherCombo.SelectedItem == null)
            {
                MessageBox.Show("Please select a purchase voucher!", "Validation Error");
                purchaseVoucherCombo.Focus();
                return false;
            }
            
            if (!decimal.TryParse(amountTxt.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Please enter valid amount!", "Validation Error");
                amountTxt.Focus();
                return false;
            }
            
            if (amount > pendingAmount)
            {
                MessageBox.Show($"Amount cannot exceed pending amount of ₹{pendingAmount:N2}!", "Validation Error");
                amountTxt.Focus();
                return false;
            }
            
            return true;
        }
        
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(300, 250);
            this.Name = "PaymentAgainstPurchaseForm";
            this.ResumeLayout(false);
        }
    }
}