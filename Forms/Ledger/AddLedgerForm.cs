using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SQLite;  // ADD THIS
using BillingSoftware.Models;  // ADD THIS
using BillingSoftware.Modules;
using BillingSoftware.Utilities;

namespace BillingSoftware.Forms.Ledger  // Note: This is Ledger folder, not Ledger class
{
    public partial class AddLedgerForm : Form
    {
        private DatabaseManager dbManager;
        private AuditLogger auditLogger;
        
        private TextBox nameTxt, codeTxt, contactTxt, phoneTxt, emailTxt;
        private TextBox addressTxt, gstinTxt, openingBalanceTxt, creditLimitTxt;
        private ComboBox typeCombo, balanceTypeCombo;
        private Button saveBtn, cancelBtn;
        
        public AddLedgerForm()
        {
            InitializeComponent();
            dbManager = new DatabaseManager();
            auditLogger = new AuditLogger();
            CreateLedgerFormUI();
        }
        
        private void CreateLedgerFormUI()
        {
            this.Text = "Add New Ledger";
            this.Size = new Size(600, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = TallyUIStyles.TallyGray;
            
            // Main Container
            GroupBox mainGroup = TallyUIStyles.CreateTallyGroupBox("Ledger Details", new Point(20, 20), new Size(550, 450));
            
            int yPos = 30;
            
            // Ledger Name
            Label nameLabel = TallyUIStyles.CreateTallyLabel("Ledger Name*:", new Point(20, yPos), new Size(120, 20), true);
            nameTxt = TallyUIStyles.CreateTallyTextBox(new Point(150, yPos), new Size(350, 25));
            mainGroup.Controls.AddRange(new Control[] { nameLabel, nameTxt });
            yPos += 35;
            
            // Ledger Code
            Label codeLabel = TallyUIStyles.CreateTallyLabel("Ledger Code*:", new Point(20, yPos), new Size(120, 20), true);
            codeTxt = TallyUIStyles.CreateTallyTextBox(new Point(150, yPos), new Size(150, 25));
            codeTxt.Text = GenerateLedgerCode();
            mainGroup.Controls.AddRange(new Control[] { codeLabel, codeTxt });
            yPos += 35;
            
            // Ledger Type
            Label typeLabel = TallyUIStyles.CreateTallyLabel("Ledger Type*:", new Point(20, yPos), new Size(120, 20), true);
            typeCombo = TallyUIStyles.CreateTallyComboBox(new Point(150, yPos), new Size(200, 25));
            typeCombo.Items.AddRange(new string[] { "Customer", "Supplier", "Bank", "Cash", "Expense", "Income", "Asset", "Liability" });
            typeCombo.SelectedIndex = 0;
            mainGroup.Controls.AddRange(new Control[] { typeLabel, typeCombo });
            yPos += 35;
            
            // Opening Balance
            Label openingLabel = TallyUIStyles.CreateTallyLabel("Opening Balance:", new Point(20, yPos), new Size(120, 20), true);
            openingBalanceTxt = TallyUIStyles.CreateTallyTextBox(new Point(150, yPos), new Size(150, 25));
            openingBalanceTxt.Text = "0.00";
            mainGroup.Controls.Add(openingLabel);
            mainGroup.Controls.Add(openingBalanceTxt);
            
            // Balance Type
            Label balanceLabel = TallyUIStyles.CreateTallyLabel("Balance Type:", new Point(320, yPos), new Size(100, 20));
            balanceTypeCombo = TallyUIStyles.CreateTallyComboBox(new Point(420, yPos), new Size(80, 25));
            balanceTypeCombo.Items.AddRange(new string[] { "Dr", "Cr" });
            balanceTypeCombo.SelectedIndex = 0;
            mainGroup.Controls.AddRange(new Control[] { balanceLabel, balanceTypeCombo });
            yPos += 35;
            
            // Contact Person
            Label contactLabel = TallyUIStyles.CreateTallyLabel("Contact Person:", new Point(20, yPos), new Size(120, 20));
            contactTxt = TallyUIStyles.CreateTallyTextBox(new Point(150, yPos), new Size(350, 25));
            mainGroup.Controls.AddRange(new Control[] { contactLabel, contactTxt });
            yPos += 35;
            
            // Phone
            Label phoneLabel = TallyUIStyles.CreateTallyLabel("Phone:", new Point(20, yPos), new Size(120, 20));
            phoneTxt = TallyUIStyles.CreateTallyTextBox(new Point(150, yPos), new Size(150, 25));
            mainGroup.Controls.AddRange(new Control[] { phoneLabel, phoneTxt });
            
            // Email
            Label emailLabel = TallyUIStyles.CreateTallyLabel("Email:", new Point(320, yPos), new Size(80, 20));
            emailTxt = TallyUIStyles.CreateTallyTextBox(new Point(420, yPos), new Size(150, 25));
            mainGroup.Controls.AddRange(new Control[] { emailLabel, emailTxt });
            yPos += 35;
            
            // Address
            Label addressLabel = TallyUIStyles.CreateTallyLabel("Address:", new Point(20, yPos), new Size(120, 20));
            addressTxt = new TextBox
            {
                Location = new Point(150, yPos),
                Size = new Size(350, 60),
                Font = TallyUIStyles.NormalFont,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            mainGroup.Controls.AddRange(new Control[] { addressLabel, addressTxt });
            yPos += 70;
            
            // GSTIN
            Label gstinLabel = TallyUIStyles.CreateTallyLabel("GSTIN:", new Point(20, yPos), new Size(120, 20));
            gstinTxt = TallyUIStyles.CreateTallyTextBox(new Point(150, yPos), new Size(200, 25));
            mainGroup.Controls.AddRange(new Control[] { gstinLabel, gstinTxt });
            
            // Credit Limit
            Label creditLabel = TallyUIStyles.CreateTallyLabel("Credit Limit:", new Point(370, yPos), new Size(100, 20));
            creditLimitTxt = TallyUIStyles.CreateTallyTextBox(new Point(470, yPos), new Size(100, 25));
            creditLimitTxt.Text = "0.00";
            mainGroup.Controls.AddRange(new Control[] { creditLabel, creditLimitTxt });
            yPos += 40;
            
            this.Controls.Add(mainGroup);
            
            // Buttons
            saveBtn = TallyUIStyles.CreateTallyButton("💾 Save Ledger", TallyUIStyles.TallyGreen, new Point(150, 490));
            saveBtn.Click += SaveBtn_Click;
            
            cancelBtn = TallyUIStyles.CreateTallyButton("❌ Cancel", TallyUIStyles.TallyGray, new Point(280, 490));
            cancelBtn.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            
            this.Controls.AddRange(new Control[] { saveBtn, cancelBtn });
        }
        
        private string GenerateLedgerCode()
        {
            try
            {
                string sql = "SELECT COUNT(*) FROM ledgers";
                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                {
                    int count = Convert.ToInt32(cmd.ExecuteScalar()) + 1;
                    return count.ToString("000");
                }
            }
            catch { return "001"; }
        }
        
        private void SaveBtn_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;
            
            try
            {
                // Use fully qualified name to avoid conflict
                var ledger = new BillingSoftware.Models.Ledger
                {
                    Name = nameTxt.Text.Trim(),
                    Code = codeTxt.Text.Trim(),
                    Type = typeCombo.SelectedItem.ToString(),
                    ContactPerson = contactTxt.Text.Trim(),
                    Phone = phoneTxt.Text.Trim(),
                    Email = emailTxt.Text.Trim(),
                    Address = addressTxt.Text.Trim(),
                    GSTIN = gstinTxt.Text.Trim(),
                    OpeningBalance = decimal.Parse(openingBalanceTxt.Text),
                    BalanceType = balanceTypeCombo.SelectedItem.ToString(),
                    CreditLimit = decimal.Parse(creditLimitTxt.Text),
                    CurrentBalance = decimal.Parse(openingBalanceTxt.Text),
                    CreatedBy = Program.CurrentUser
                };
                
                if (dbManager.AddLedger(ledger))
                {
                    // Audit log
                    auditLogger.LogAction("CREATE", "LEDGER", ledger.Code, 
                                        $"Created ledger: {ledger.Name}", "Ledgers");
                    
                    MessageBox.Show("Ledger created successfully!", "Success", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving ledger: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(nameTxt.Text))
            {
                MessageBox.Show("Please enter ledger name!", "Validation Error");
                nameTxt.Focus();
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(codeTxt.Text))
            {
                MessageBox.Show("Please enter ledger code!", "Validation Error");
                codeTxt.Focus();
                return false;
            }
            
            if (!decimal.TryParse(openingBalanceTxt.Text, out _))
            {
                MessageBox.Show("Please enter valid opening balance!", "Validation Error");
                openingBalanceTxt.Focus();
                return false;
            }
            
            return true;
        }
        
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(300, 200);
            this.Name = "AddLedgerForm";
            this.ResumeLayout(false);
        }
    }
}