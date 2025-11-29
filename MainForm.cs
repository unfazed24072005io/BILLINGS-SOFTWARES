using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using BillingSoftware.Modules;
using BillingSoftware.Models;

namespace BillingSoftware
{
    public partial class MainForm : Form
    {
        private DatabaseManager dbManager;
        private VoucherManager voucherManager;
        private ReportManager reportManager;
        private UserManager userManager;

        private TabControl mainTabControl;
        private DataGridView vouchersGridView;
        private Label dashboardSummaryLabel;

        private Color primaryGreen = Color.FromArgb(46, 204, 113);
        private Color primaryBlue = Color.FromArgb(52, 152, 219);
        private Color darkText = Color.FromArgb(44, 62, 80);
        private Color lightBg = Color.FromArgb(248, 249, 250);

        public MainForm()
        {
            InitializeComponent();
            
            dbManager = new DatabaseManager();
            voucherManager = new VoucherManager();
            reportManager = new ReportManager();
            userManager = new UserManager();
            
            InitializeApplication();
        }

        private void InitializeApplication()
        {
            dbManager.CreateDatabase();
            CreateMainUI();
            LoadDashboardData();
            
            this.Text = "Modern Billing Software";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = lightBg;
        }

        private void CreateMainUI()
        {
            mainTabControl = new TabControl();
            mainTabControl.Dock = DockStyle.Fill;
            mainTabControl.Font = new Font("Segoe UI", 10);
            
            CreateDashboardTab();
            CreateVouchersTab();
            CreateReportsTab();
            CreateUsersTab();
            CreateSettingsTab();
            
            this.Controls.Add(mainTabControl);
        }

        private void CreateDashboardTab()
        {
            TabPage dashboardTab = new TabPage("📊 Dashboard");
            dashboardTab.BackColor = lightBg;
            
            Label dashboardTitle = new Label();
            dashboardTitle.Text = "Business Dashboard";
            dashboardTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            dashboardTitle.ForeColor = darkText;
            dashboardTitle.Location = new Point(20, 20);
            dashboardTitle.Size = new Size(300, 30);
            
            dashboardSummaryLabel = new Label();
            dashboardSummaryLabel.Text = "Loading dashboard data...";
            dashboardSummaryLabel.Font = new Font("Segoe UI", 11);
            dashboardSummaryLabel.ForeColor = darkText;
            dashboardSummaryLabel.Location = new Point(20, 70);
            dashboardSummaryLabel.Size = new Size(500, 200);
            dashboardSummaryLabel.AutoSize = false;
            
            Button quickSalesBtn = CreateModernButton("Create Sales Voucher", primaryGreen, new Point(20, 150));
            Button quickReceiptBtn = CreateModernButton("Create Receipt", primaryBlue, new Point(200, 150));
            Button stockReportBtn = CreateModernButton("Stock Report", Color.Orange, new Point(380, 150));
            
            quickSalesBtn.Click += (s, e) => { 
                mainTabControl.SelectedIndex = 1; 
                ShowSalesForm();
            };
            quickReceiptBtn.Click += (s, e) => { 
                mainTabControl.SelectedIndex = 1; 
                ShowReceiptForm();
            };
            stockReportBtn.Click += (s, e) => { mainTabControl.SelectedIndex = 2; };
            
            dashboardTab.Controls.Add(dashboardTitle);
            dashboardTab.Controls.Add(dashboardSummaryLabel);
            dashboardTab.Controls.Add(quickSalesBtn);
            dashboardTab.Controls.Add(quickReceiptBtn);
            dashboardTab.Controls.Add(stockReportBtn);
            
            mainTabControl.TabPages.Add(dashboardTab);
        }

        private void CreateVouchersTab()
        {
            TabPage vouchersTab = new TabPage("🧾 Vouchers");
            vouchersTab.BackColor = lightBg;
            
            ComboBox voucherTypeCombo = new ComboBox();
            voucherTypeCombo.Location = new Point(20, 20);
            voucherTypeCombo.Size = new Size(200, 30);
            voucherTypeCombo.Font = new Font("Segoe UI", 10);
            voucherTypeCombo.Items.AddRange(new string[] { 
                "Sales Voucher", "Receipt Voucher", "Payment Voucher", 
                "Journal Voucher", "Estimate" 
            });
            voucherTypeCombo.SelectedIndex = 0;
            
            Button createVoucherBtn = CreateModernButton("Create Selected Voucher", primaryGreen, new Point(240, 18));
            createVoucherBtn.Click += (s, e) => CreateSelectedVoucher(voucherTypeCombo.SelectedItem.ToString());
            
            vouchersGridView = new DataGridView();
            vouchersGridView.Location = new Point(20, 70);
            vouchersGridView.Size = new Size(900, 400);
            vouchersGridView.BackgroundColor = Color.White;
            vouchersGridView.BorderStyle = BorderStyle.None;
            vouchersGridView.Font = new Font("Segoe UI", 9);
            vouchersGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            
            Button refreshVouchersBtn = CreateModernButton("Refresh List", primaryBlue, new Point(20, 490));
            refreshVouchersBtn.Click += (s, e) => LoadVouchersData();
            
            vouchersTab.Controls.Add(voucherTypeCombo);
            vouchersTab.Controls.Add(createVoucherBtn);
            vouchersTab.Controls.Add(vouchersGridView);
            vouchersTab.Controls.Add(refreshVouchersBtn);
            
            mainTabControl.TabPages.Add(vouchersTab);
            LoadVouchersData();
        }

        private void CreateReportsTab()
        {
            TabPage reportsTab = new TabPage("📈 Reports");
            reportsTab.BackColor = lightBg;
            
            ComboBox reportTypeCombo = new ComboBox();
            reportTypeCombo.Location = new Point(20, 20);
            reportTypeCombo.Size = new Size(200, 30);
            reportTypeCombo.Font = new Font("Segoe UI", 10);
            reportTypeCombo.Items.AddRange(new string[] { 
                "Daily Stock Report", "Monthly Stock Report", "Yearly Stock Report",
                "Sales Report", "Financial Summary" 
            });
            reportTypeCombo.SelectedIndex = 0;
            
            Button generateReportBtn = CreateModernButton("Generate Report", primaryGreen, new Point(240, 18));
            generateReportBtn.Click += (s, e) => GenerateSelectedReport(reportTypeCombo.SelectedItem.ToString());
            
            DataGridView reportsGridView = new DataGridView();
            reportsGridView.Location = new Point(20, 70);
            reportsGridView.Size = new Size(900, 400);
            reportsGridView.BackgroundColor = Color.White;
            
            reportsTab.Controls.Add(reportTypeCombo);
            reportsTab.Controls.Add(generateReportBtn);
            reportsTab.Controls.Add(reportsGridView);
            
            mainTabControl.TabPages.Add(reportsTab);
        }

        private void CreateUsersTab()
        {
            TabPage usersTab = new TabPage("👥 Users");
            usersTab.BackColor = lightBg;
            
            Label usersTitle = new Label();
            usersTitle.Text = "User Management";
            usersTitle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            usersTitle.Location = new Point(20, 20);
            usersTitle.Size = new Size(200, 30);
            usersTitle.ForeColor = darkText;
            
            Button addUserBtn = CreateModernButton("Add New User", primaryGreen, new Point(20, 70));
            Button manageUsersBtn = CreateModernButton("Manage Users", primaryBlue, new Point(180, 70));
            
            addUserBtn.Click += (s, e) => ShowUserManagementForm();
            manageUsersBtn.Click += (s, e) => ShowUserManagementForm();
            
            usersTab.Controls.Add(usersTitle);
            usersTab.Controls.Add(addUserBtn);
            usersTab.Controls.Add(manageUsersBtn);
            
            mainTabControl.TabPages.Add(usersTab);
        }

        private void CreateSettingsTab()
        {
            TabPage settingsTab = new TabPage("⚙️ Settings");
            settingsTab.BackColor = lightBg;
            
            Label settingsTitle = new Label();
            settingsTitle.Text = "Application Settings";
            settingsTitle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            settingsTitle.Location = new Point(20, 20);
            settingsTitle.Size = new Size(200, 30);
            settingsTitle.ForeColor = darkText;
            
            Button companySettingsBtn = CreateModernButton("Company Settings", primaryBlue, new Point(20, 70));
            Button backupBtn = CreateModernButton("Backup Data", primaryGreen, new Point(200, 70));
            
            companySettingsBtn.Click += (s, e) => ShowSettingsForm();
            backupBtn.Click += (s, e) => BackupDatabase();
            
            settingsTab.Controls.Add(settingsTitle);
            settingsTab.Controls.Add(companySettingsBtn);
            settingsTab.Controls.Add(backupBtn);
            
            mainTabControl.TabPages.Add(settingsTab);
        }

        private Button CreateModernButton(string text, Color color, Point location)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.BackColor = color;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 10);
            btn.Size = new Size(180, 35);
            btn.Location = location;
            btn.Cursor = Cursors.Hand;
            
            return btn;
        }

        private void LoadDashboardData()
        {
            int totalVouchers = voucherManager.GetVouchersCount();
            decimal totalSales = voucherManager.GetTotalSales();
            int lowStockItems = reportManager.GetLowStockCount();
            int activeUsers = userManager.GetActiveUsersCount();
            
            dashboardSummaryLabel.Text = $@"📊 BUSINESS SUMMARY

Total Vouchers: {totalVouchers}
Total Sales: ₹{totalSales:N2}
Low Stock Items: {lowStockItems}
Active Users: {activeUsers}

Last Updated: {DateTime.Now:HH:mm:ss}";
        }

        private void LoadVouchersData()
        {
            var vouchers = voucherManager.GetAllVouchers();
            vouchersGridView.DataSource = vouchers;
            vouchersGridView.Refresh();
        }

        private void CreateSelectedVoucher(string voucherType)
        {
            switch (voucherType)
            {
                case "Sales Voucher":
                    ShowSalesForm();
                    break;
                case "Receipt Voucher":
                    ShowReceiptForm();
                    break;
                case "Payment Voucher":
                    ShowPaymentForm();
                    break;
                case "Journal Voucher":
                    ShowJournalForm();
                    break;
                case "Estimate":
                    ShowEstimateForm();
                    break;
            }
        }

        private void GenerateSelectedReport(string reportType)
        {
            MessageBox.Show($"Generating {reportType}...\nThis feature will be fully implemented!", "Report Generation", 
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowSalesForm()
        {
            Forms.Vouchers.SalesForm salesForm = new Forms.Vouchers.SalesForm();
            salesForm.ShowDialog();
            LoadVouchersData();
            LoadDashboardData();
        }

        private void ShowReceiptForm()
        {
            Forms.Vouchers.ReceiptForm receiptForm = new Forms.Vouchers.ReceiptForm();
            receiptForm.ShowDialog();
            LoadVouchersData();
            LoadDashboardData();
        }

        private void ShowPaymentForm()
        {
            Forms.Vouchers.PaymentForm paymentForm = new Forms.Vouchers.PaymentForm();
            paymentForm.ShowDialog();
            LoadVouchersData();
            LoadDashboardData();
        }

        private void ShowJournalForm()
        {
            Forms.Vouchers.JournalForm journalForm = new Forms.Vouchers.JournalForm();
            journalForm.ShowDialog();
            LoadVouchersData();
            LoadDashboardData();
        }

        private void ShowEstimateForm()
        {
            Forms.Vouchers.EstimateForm estimateForm = new Forms.Vouchers.EstimateForm();
            estimateForm.ShowDialog();
            LoadVouchersData();
            LoadDashboardData();
        }

        private void ShowUserManagementForm()
        {
            Forms.UserManagementForm userForm = new Forms.UserManagementForm();
            userForm.ShowDialog();
            LoadDashboardData();
        }

        private void ShowSettingsForm()
        {
            Forms.SettingsForm settingsForm = new Forms.SettingsForm();
            settingsForm.ShowDialog();
        }

        private void BackupDatabase()
        {
            MessageBox.Show("Database backup feature will be implemented!", "Backup", 
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Name = "MainForm";
            this.ResumeLayout(false);
        }
    }
}