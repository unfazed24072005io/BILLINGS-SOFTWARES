using System;
using System.Drawing;
using System.Windows.Forms;
using BillingSoftware.Modules;
using BillingSoftware.Utilities;

namespace BillingSoftware.Forms.Ledger
{
    public partial class LedgerStatementForm : Form
    {
        private DatabaseManager dbManager;
        private string ledgerName;
        
        private DataGridView statementGrid;
        private DateTimePicker fromDatePicker, toDatePicker;
        
        public LedgerStatementForm(string ledgerName)
        {
            InitializeComponent();
            dbManager = new DatabaseManager();
            this.ledgerName = ledgerName;
            CreateStatementFormUI();
            LoadStatementData();
        }
        
        private void CreateStatementFormUI()
        {
            this.Text = $"Ledger Statement - {ledgerName}";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = TallyUIStyles.TallyGray;
            
            // Title
            Label titleLabel = new Label();
            titleLabel.Text = $"📊 Ledger Statement: {ledgerName}";
            titleLabel.Font = TallyUIStyles.TitleFont;
            titleLabel.ForeColor = TallyUIStyles.TallyBlue;
            titleLabel.Location = new Point(20, 20);
            titleLabel.Size = new Size(400, 30);
            this.Controls.Add(titleLabel);
            
            // Filter Panel
            Panel filterPanel = new Panel();
            filterPanel.Location = new Point(20, 70);
            filterPanel.Size = new Size(850, 40);
            filterPanel.BackColor = Color.Transparent;
            
            Label fromLabel = TallyUIStyles.CreateTallyLabel("From:", new Point(0, 8), new Size(40, 20));
            fromDatePicker = TallyUIStyles.CreateTallyDateTimePicker(new Point(45, 5), new Size(120, 25));
            fromDatePicker.Value = DateTime.Now.AddDays(-30);
            
            Label toLabel = TallyUIStyles.CreateTallyLabel("To:", new Point(175, 8), new Size(30, 20));
            toDatePicker = TallyUIStyles.CreateTallyDateTimePicker(new Point(210, 5), new Size(120, 25));
            
            Button filterBtn = TallyUIStyles.CreateTallyButton("🔍 Filter", TallyUIStyles.TallyBlue, new Point(340, 5), new Size(100, 25));
            Button printBtn = TallyUIStyles.CreateTallyButton("🖨️ Print", TallyUIStyles.TallyPurple, new Point(450, 5), new Size(100, 25));
            Button exportBtn = TallyUIStyles.CreateTallyButton("📤 Export", TallyUIStyles.TallyGreen, new Point(560, 5), new Size(100, 25));
            
            filterPanel.Controls.AddRange(new Control[] { fromLabel, fromDatePicker, toLabel, toDatePicker, filterBtn, printBtn, exportBtn });
            this.Controls.Add(filterPanel);
            
            // Statement Grid
            statementGrid = TallyUIStyles.CreateTallyGrid(new Point(20, 120), new Size(850, 400));
            statementGrid.Columns.Add("Date", "Date");
            statementGrid.Columns.Add("VoucherType", "Voucher Type");
            statementGrid.Columns.Add("VoucherNumber", "Voucher No");
            statementGrid.Columns.Add("Particulars", "Particulars");
            statementGrid.Columns.Add("Debit", "Debit");
            statementGrid.Columns.Add("Credit", "Credit");
            statementGrid.Columns.Add("Balance", "Balance");
            statementGrid.Columns.Add("Reference", "Reference");
            
            statementGrid.Columns["Debit"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            statementGrid.Columns["Credit"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            statementGrid.Columns["Balance"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            statementGrid.Columns["Debit"].DefaultCellStyle.Format = "N2";
            statementGrid.Columns["Credit"].DefaultCellStyle.Format = "N2";
            statementGrid.Columns["Balance"].DefaultCellStyle.Format = "N2";
            
            this.Controls.Add(statementGrid);
            
            // Close Button
            Button closeBtn = TallyUIStyles.CreateTallyButton("Close", TallyUIStyles.TallyGray, new Point(400, 540), new Size(100, 30));
            closeBtn.Click += (s, e) => this.Close();
            this.Controls.Add(closeBtn);
            
            // Event Handlers
            filterBtn.Click += (s, e) => LoadStatementData();
        }
        
        private void LoadStatementData()
        {
            try
            {
                var statement = dbManager.GetLedgerStatement(ledgerName, fromDatePicker.Value, toDatePicker.Value);
                statementGrid.DataSource = statement;
                
                // Calculate totals
                decimal totalDebit = 0;
                decimal totalCredit = 0;
                decimal _ = 0;
                
                foreach (DataGridViewRow row in statementGrid.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        decimal.TryParse(row.Cells["Debit"].Value?.ToString(), out decimal debit);
                        decimal.TryParse(row.Cells["Credit"].Value?.ToString(), out decimal credit);
                        totalDebit += debit;
                        totalCredit += credit;
                    }
                }
                
                // Update title with totals
                this.Text = $"Ledger Statement - {ledgerName} | Debit: ₹{totalDebit:N2} | Credit: ₹{totalCredit:N2}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading statement: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new Size(300, 250);
            this.Name = "LedgerStatementForm";
            this.ResumeLayout(false);
        }
    }
}