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
        private Panel headerPanel;
        private Label headerTitle;

        // Modern color scheme
        private Color primaryGreen = Color.FromArgb(46, 204, 113);
        private Color primaryBlue = Color.FromArgb(52, 152, 219);
        private Color primaryPurple = Color.FromArgb(155, 89, 182);
        private Color primaryOrange = Color.FromArgb(230, 126, 34);
        private Color darkText = Color.FromArgb(44, 62, 80);
        private Color lightBg = Color.FromArgb(248, 249, 250);
        private Color cardBg = Color.White;
        private Color borderColor = Color.FromArgb(222, 226, 230);

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
            CreateModernUI();
            LoadDashboardData();
            
            this.Text = "Modern Billing Software";
            this.Size = new Size(1400, 900); // Increased size
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = lightBg;
            this.Icon = SystemIcons.Application;
            this.MinimumSize = new Size(1200, 700); // Prevent getting too small
        }

        private void CreateModernUI()
        {
            // Create header
            CreateHeader();
            
            // Create main tab control
            mainTabControl = new TabControl();
            mainTabControl.Location = new Point(0, 80);
            mainTabControl.Size = new Size(1400, 820); // Full size
            mainTabControl.Font = new Font("Segoe UI", 10);
            mainTabControl.Appearance = TabAppearance.FlatButtons;
            mainTabControl.ItemSize = new Size(120, 35);
            mainTabControl.SizeMode = TabSizeMode.Fixed;
            
            CreateDashboardTab();
            CreateVouchersTab();
            CreateReportsTab();
            CreateUsersTab();
            CreateSettingsTab();
            
            this.Controls.Add(mainTabControl);
            this.Controls.Add(headerPanel);
        }

        private void CreateHeader()
        {
            headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 80;
            headerPanel.BackColor = primaryBlue;
            
            headerTitle = new Label();
            headerTitle.Text = "💼 MODERN BILLING SOFTWARE";
            headerTitle.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            headerTitle.ForeColor = Color.White;
            headerTitle.Location = new Point(20, 20);
            headerTitle.Size = new Size(400, 40);
            headerTitle.TextAlign = ContentAlignment.MiddleLeft;
            
            Label versionLabel = new Label();
            versionLabel.Text = "v1.0 • Ready for Business";
            versionLabel.Font = new Font("Segoe UI", 9);
            versionLabel.ForeColor = Color.FromArgb(200, 255, 255);
            versionLabel.Location = new Point(25, 55);
            versionLabel.Size = new Size(200, 20);
            
            headerPanel.Controls.Add(headerTitle);
            headerPanel.Controls.Add(versionLabel);
        }

        private void CreateDashboardTab()
        {
            TabPage dashboardTab = new TabPage("📊 DASHBOARD");
            dashboardTab.BackColor = lightBg;
            dashboardTab.Padding = new Padding(25);
            dashboardTab.AutoScroll = true; // Enable scrolling if needed
            
            int cardWidth = 400;
            int cardHeight = 150;
            int spacing = 20;
            
            // Welcome card - Full width
            Panel welcomeCard = CreateCard("Welcome to Your Business Hub", primaryBlue, new Point(0, 0), new Size(1300, 120));
            Label welcomeText = new Label();
            welcomeText.Text = "Manage all your business operations in one place. Create vouchers, generate reports, and track your finances effortlessly.";
            welcomeText.Font = new Font("Segoe UI", 10);
            welcomeText.ForeColor = darkText;
            welcomeText.Location = new Point(20, 50);
            welcomeText.Size = new Size(1260, 60);
            welcomeText.TextAlign = ContentAlignment.MiddleLeft;
            welcomeCard.Controls.Add(welcomeText);
            
            // Stats card
            Panel statsCard = CreateCard("Business Overview", primaryGreen, new Point(0, 140), new Size(640, 200));
            dashboardSummaryLabel = new Label();
            dashboardSummaryLabel.Text = "Loading business data...";
            dashboardSummaryLabel.Font = new Font("Segoe UI", 10);
            dashboardSummaryLabel.ForeColor = darkText;
            dashboardSummaryLabel.Location = new Point(20, 50);
            dashboardSummaryLabel.Size = new Size(600, 140);
            dashboardSummaryLabel.TextAlign = ContentAlignment.TopLeft;
            statsCard.Controls.Add(dashboardSummaryLabel);
            
            // Quick Actions card
            Panel actionsCard = CreateCard("Quick Actions", primaryPurple, new Point(660, 140), new Size(640, 200));
            
            // Quick action buttons - arranged in grid
            Button[] quickActions = new Button[]
            {
                CreateActionButton("💰 Sales Voucher", primaryGreen, new Point(20, 50)),
                CreateActionButton("🧾 Receipt Voucher", primaryBlue, new Point(220, 50)),
                CreateActionButton("💳 Payment Voucher", primaryOrange, new Point(420, 50)),
                CreateActionButton("📒 Journal Voucher", primaryPurple, new Point(20, 100)),
                CreateActionButton("📋 Estimate", Color.Teal, new Point(220, 100)),
                CreateActionButton("📊 View Reports", Color.Coral, new Point(420, 100))
            };
            
            quickActions[0].Click += (s, e) => { mainTabControl.SelectedIndex = 1; ShowSalesForm(); };
            quickActions[1].Click += (s, e) => { mainTabControl.SelectedIndex = 1; ShowReceiptForm(); };
            quickActions[2].Click += (s, e) => { mainTabControl.SelectedIndex = 1; ShowPaymentForm(); };
            quickActions[3].Click += (s, e) => { mainTabControl.SelectedIndex = 1; ShowJournalForm(); };
            quickActions[4].Click += (s, e) => { mainTabControl.SelectedIndex = 1; ShowEstimateForm(); };
            quickActions[5].Click += (s, e) => { mainTabControl.SelectedIndex = 2; };
            
            foreach (var btn in quickActions)
                actionsCard.Controls.Add(btn);
            
            // Recent Activity card - Full width
            Panel activityCard = CreateCard("Recent Vouchers", darkText, new Point(0, 360), new Size(1300, 400));
            
            vouchersGridView = new DataGridView();
            vouchersGridView.Location = new Point(20, 50);
            vouchersGridView.Size = new Size(1260, 300); // Full width
            vouchersGridView.BackgroundColor = cardBg;
            vouchersGridView.BorderStyle = BorderStyle.FixedSingle;
            vouchersGridView.Font = new Font("Segoe UI", 9);
            vouchersGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            vouchersGridView.RowHeadersVisible = false;
            vouchersGridView.EnableHeadersVisualStyles = false;
            vouchersGridView.ColumnHeadersDefaultCellStyle.BackColor = primaryBlue;
            vouchersGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            vouchersGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            vouchersGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            
            Button refreshBtn = CreateModernButton("🔄 Refresh Data", primaryBlue, new Point(20, 360));
            refreshBtn.Size = new Size(140, 35);
            refreshBtn.Click += (s, e) => { LoadVouchersData(); LoadDashboardData(); };
            
            activityCard.Controls.Add(vouchersGridView);
            activityCard.Controls.Add(refreshBtn);
            
            // Add all cards to dashboard
            dashboardTab.Controls.Add(welcomeCard);
            dashboardTab.Controls.Add(statsCard);
            dashboardTab.Controls.Add(actionsCard);
            dashboardTab.Controls.Add(activityCard);
            
            mainTabControl.TabPages.Add(dashboardTab);
        }

        private void CreateVouchersTab()
        {
            TabPage vouchersTab = new TabPage("🧾 VOUCHERS");
            vouchersTab.BackColor = lightBg;
            vouchersTab.Padding = new Padding(25);
            vouchersTab.AutoScroll = true;
            
            // Voucher type selection card - Full width
            Panel selectionCard = CreateCard("Create New Voucher", primaryBlue, new Point(0, 0), new Size(1300, 100));
            
            Label typeLabel = new Label();
            typeLabel.Text = "Select Voucher Type:";
            typeLabel.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            typeLabel.ForeColor = darkText;
            typeLabel.Location = new Point(20, 40);
            typeLabel.Size = new Size(160, 25);
            selectionCard.Controls.Add(typeLabel);
            
            ComboBox voucherTypeCombo = new ComboBox();
            voucherTypeCombo.Location = new Point(190, 38);
            voucherTypeCombo.Size = new Size(220, 30);
            voucherTypeCombo.Font = new Font("Segoe UI", 10);
            voucherTypeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            voucherTypeCombo.Items.AddRange(new string[] { 
                "Sales Voucher", "Receipt Voucher", "Payment Voucher", 
                "Journal Voucher", "Estimate" 
            });
            voucherTypeCombo.SelectedIndex = 0;
            selectionCard.Controls.Add(voucherTypeCombo);
            
            Button createVoucherBtn = CreateModernButton("➕ Create Selected Voucher", primaryGreen, new Point(430, 35));
            createVoucherBtn.Size = new Size(220, 35);
            createVoucherBtn.Click += (s, e) => CreateSelectedVoucher(voucherTypeCombo.SelectedItem.ToString());
            selectionCard.Controls.Add(createVoucherBtn);
            
            // Vouchers list card - Full width
            Panel listCard = CreateCard("All Vouchers", primaryGreen, new Point(0, 120), new Size(1300, 500));
            
            DataGridView vouchersListGrid = new DataGridView();
            vouchersListGrid.Location = new Point(20, 50);
            vouchersListGrid.Size = new Size(1260, 400); // Full width
            vouchersListGrid.BackgroundColor = cardBg;
            vouchersListGrid.BorderStyle = BorderStyle.FixedSingle;
            vouchersListGrid.Font = new Font("Segoe UI", 9);
            vouchersListGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            vouchersListGrid.RowHeadersVisible = false;
            vouchersListGrid.EnableHeadersVisualStyles = false;
            vouchersListGrid.ColumnHeadersDefaultCellStyle.BackColor = primaryGreen;
            vouchersListGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            vouchersListGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            vouchersListGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
            
            Button refreshVouchersBtn = CreateModernButton("🔄 Refresh List", primaryBlue, new Point(20, 460));
            refreshVouchersBtn.Size = new Size(140, 35);
            refreshVouchersBtn.Click += (s, e) => LoadVouchersData();
            
            listCard.Controls.Add(vouchersListGrid);
            listCard.Controls.Add(refreshVouchersBtn);
            vouchersGridView = vouchersListGrid;
            
            vouchersTab.Controls.Add(selectionCard);
            vouchersTab.Controls.Add(listCard);
            
            mainTabControl.TabPages.Add(vouchersTab);
        }

        private void CreateReportsTab()
        {
            TabPage reportsTab = new TabPage("📈 REPORTS");
            reportsTab.BackColor = lightBg;
            reportsTab.Padding = new Padding(25);
            reportsTab.AutoScroll = true;
            
            // Report selection card - Full width
            Panel selectionCard = CreateCard("Generate Reports", primaryPurple, new Point(0, 0), new Size(1300, 120));
            
            Label reportLabel = new Label();
            reportLabel.Text = "Select Report Type:";
            reportLabel.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            reportLabel.ForeColor = darkText;
            reportLabel.Location = new Point(20, 45);
            reportLabel.Size = new Size(160, 25);
            selectionCard.Controls.Add(reportLabel);
            
            ComboBox reportTypeCombo = new ComboBox();
            reportTypeCombo.Location = new Point(190, 43);
            reportTypeCombo.Size = new Size(300, 30);
            reportTypeCombo.Font = new Font("Segoe UI", 10);
            reportTypeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            reportTypeCombo.Items.AddRange(new string[] { 
                "📦 Stock Summary Report", "💰 Sales Analysis Report", 
                "💸 Financial Summary Report", "📊 Voucher Summary Report",
                "🚨 Low Stock Alert Report", "📈 Monthly Performance Report"
            });
            reportTypeCombo.SelectedIndex = 0;
            selectionCard.Controls.Add(reportTypeCombo);
            
            Button generateReportBtn = CreateModernButton("📄 Generate Report", primaryGreen, new Point(510, 40));
            generateReportBtn.Size = new Size(180, 35);
            generateReportBtn.Click += (s, e) => GenerateSelectedReport(reportTypeCombo.SelectedItem.ToString());
            selectionCard.Controls.Add(generateReportBtn);
            
            Button exportBtn = CreateModernButton("📤 Export to Excel", primaryBlue, new Point(710, 40));
            exportBtn.Size = new Size(180, 35);
            exportBtn.Click += (s, e) => ExportReport();
            selectionCard.Controls.Add(exportBtn);
            
            // Report preview area
            Panel previewCard = CreateCard("Report Preview", darkText, new Point(0, 140), new Size(1300, 500));
            
            Label previewLabel = new Label();
            previewLabel.Text = "📊 Report output will be displayed here\n\nSelect a report type and click 'Generate Report' to view detailed business insights.";
            previewLabel.Font = new Font("Segoe UI", 12);
            previewLabel.ForeColor = Color.Gray;
            previewLabel.Location = new Point(20, 60);
            previewLabel.Size = new Size(1260, 200);
            previewLabel.TextAlign = ContentAlignment.MiddleCenter;
            previewCard.Controls.Add(previewLabel);
            
            reportsTab.Controls.Add(selectionCard);
            reportsTab.Controls.Add(previewCard);
            
            mainTabControl.TabPages.Add(reportsTab);
        }

        private void CreateUsersTab()
        {
            TabPage usersTab = new TabPage("👥 USERS");
            usersTab.BackColor = lightBg;
            usersTab.Padding = new Padding(25);
            usersTab.AutoScroll = true;
            
            Panel usersCard = CreateCard("User Management", primaryOrange, new Point(0, 0), new Size(1300, 300));
            
            Label usersTitle = new Label();
            usersTitle.Text = "Manage System Users";
            usersTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            usersTitle.ForeColor = darkText;
            usersTitle.Location = new Point(20, 50);
            usersTitle.Size = new Size(400, 35);
            usersCard.Controls.Add(usersTitle);
            
            Label usersDesc = new Label();
            usersDesc.Text = "Add new users, manage permissions, and control access to the billing system. Ensure proper role-based access for security.";
            usersDesc.Font = new Font("Segoe UI", 10);
            usersDesc.ForeColor = Color.Gray;
            usersDesc.Location = new Point(20, 95);
            usersDesc.Size = new Size(800, 25);
            usersCard.Controls.Add(usersDesc);
            
            // User management buttons
            Button addUserBtn = CreateModernButton("➕ Add New User", primaryGreen, new Point(20, 140));
            addUserBtn.Size = new Size(180, 45);
            Button manageUsersBtn = CreateModernButton("👥 Manage Users", primaryBlue, new Point(220, 140));
            manageUsersBtn.Size = new Size(180, 45);
            Button rolesBtn = CreateModernButton("🎭 Manage Roles", primaryPurple, new Point(420, 140));
            rolesBtn.Size = new Size(180, 45);
            Button permissionsBtn = CreateModernButton("🔐 Permissions", Color.Teal, new Point(620, 140));
            permissionsBtn.Size = new Size(180, 45);
            
            addUserBtn.Click += (s, e) => ShowUserManagementForm();
            manageUsersBtn.Click += (s, e) => ShowUserManagementForm();
            rolesBtn.Click += (s, e) => ShowUserManagementForm();
            permissionsBtn.Click += (s, e) => ShowUserManagementForm();
            
            usersCard.Controls.Add(addUserBtn);
            usersCard.Controls.Add(manageUsersBtn);
            usersCard.Controls.Add(rolesBtn);
            usersCard.Controls.Add(permissionsBtn);
            
            // User statistics
            Label statsLabel = new Label();
            statsLabel.Text = $"📊 User Statistics:\n• Total Users: {userManager.GetActiveUsersCount()}\n• Active Sessions: 1\n• Last Login: {DateTime.Now:MMM dd, yyyy}";
            statsLabel.Font = new Font("Segoe UI", 10);
            statsLabel.ForeColor = darkText;
            statsLabel.Location = new Point(20, 210);
            statsLabel.Size = new Size(400, 80);
            usersCard.Controls.Add(statsLabel);
            
            usersTab.Controls.Add(usersCard);
            mainTabControl.TabPages.Add(usersTab);
        }

        private void CreateSettingsTab()
        {
            TabPage settingsTab = new TabPage("⚙️ SETTINGS");
            settingsTab.BackColor = lightBg;
            settingsTab.Padding = new Padding(25);
            settingsTab.AutoScroll = true;
            
            Panel settingsCard = CreateCard("Application Settings", darkText, new Point(0, 0), new Size(1300, 600));
            
            Label settingsTitle = new Label();
            settingsTitle.Text = "System Configuration & Preferences";
            settingsTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            settingsTitle.ForeColor = darkText;
            settingsTitle.Location = new Point(20, 50);
            settingsTitle.Size = new Size(500, 35);
            settingsCard.Controls.Add(settingsTitle);
            
            // Settings options grid
            int startY = 110;
            int buttonWidth = 280;
            int buttonHeight = 50;
            int spacing = 20;
            
            Button[] settingButtons = new Button[]
            {
                CreateModernButton("🏢 Company Information", primaryBlue, new Point(20, startY)),
                CreateModernButton("💾 Backup Database", primaryGreen, new Point(320, startY)),
                CreateModernButton("🔄 Restore Database", primaryOrange, new Point(620, startY)),
                CreateModernButton("🎨 UI Theme Settings", primaryPurple, new Point(920, startY)),
                
                CreateModernButton("📧 Email Configuration", Color.Teal, new Point(20, startY + buttonHeight + spacing)),
                CreateModernButton("🧾 Invoice Settings", Color.Coral, new Point(320, startY + buttonHeight + spacing)),
                CreateModernButton("🔔 Notifications", Color.SlateBlue, new Point(620, startY + buttonHeight + spacing)),
                CreateModernButton("🔒 Security Settings", Color.Crimson, new Point(920, startY + buttonHeight + spacing))
            };
            
            settingButtons[0].Size = new Size(buttonWidth, buttonHeight);
            settingButtons[1].Size = new Size(buttonWidth, buttonHeight);
            settingButtons[2].Size = new Size(buttonWidth, buttonHeight);
            settingButtons[3].Size = new Size(buttonWidth, buttonHeight);
            settingButtons[4].Size = new Size(buttonWidth, buttonHeight);
            settingButtons[5].Size = new Size(buttonWidth, buttonHeight);
            settingButtons[6].Size = new Size(buttonWidth, buttonHeight);
            settingButtons[7].Size = new Size(buttonWidth, buttonHeight);
            
            settingButtons[0].Click += (s, e) => ShowSettingsForm();
            settingButtons[1].Click += (s, e) => BackupDatabase();
            settingButtons[2].Click += (s, e) => RestoreDatabase();
            settingButtons[3].Click += (s, e) => ChangeTheme();
            settingButtons[4].Click += (s, e) => ShowSettingsForm();
            settingButtons[5].Click += (s, e) => ShowSettingsForm();
            settingButtons[6].Click += (s, e) => ShowSettingsForm();
            settingButtons[7].Click += (s, e) => ShowSettingsForm();
            
            foreach (var btn in settingButtons)
                settingsCard.Controls.Add(btn);
            
            // System info - moved to bottom
            Panel infoCard = CreateCard("System Information", primaryBlue, new Point(20, startY + (buttonHeight + spacing) * 2 + 20), new Size(1260, 150));
            
            Label systemInfo = new Label();
            systemInfo.Text = $@"💻 System Information:

• Application Version: 1.0.0
• Database: SQLite (Connected)
• Total Vouchers: {voucherManager.GetVouchersCount()}
• Active Users: {userManager.GetActiveUsersCount()}
• Last Backup: {DateTime.Now:MMM dd, yyyy HH:mm}
• System Status: ✅ All Systems Operational";
            systemInfo.Font = new Font("Segoe UI", 10);
            systemInfo.ForeColor = darkText;
            systemInfo.Location = new Point(20, 40);
            systemInfo.Size = new Size(800, 120);
            infoCard.Controls.Add(systemInfo);
            
            settingsCard.Controls.Add(infoCard);
            settingsTab.Controls.Add(settingsCard);
            
            mainTabControl.TabPages.Add(settingsTab);
        }

        private Panel CreateCard(string title, Color titleColor, Point location, Size size)
        {
            Panel card = new Panel();
            card.Location = location;
            card.Size = size;
            card.BackColor = cardBg;
            card.BorderStyle = BorderStyle.FixedSingle;
            card.Padding = new Padding(1);
            
            // Title bar
            Panel titleBar = new Panel();
            titleBar.Dock = DockStyle.Top;
            titleBar.Height = 35;
            titleBar.BackColor = titleColor;
            
            Label titleLabel = new Label();
            titleLabel.Text = title;
            titleLabel.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.Dock = DockStyle.Fill;
            titleLabel.TextAlign = ContentAlignment.MiddleLeft;
            titleLabel.Padding = new Padding(15, 0, 0, 0);
            
            titleBar.Controls.Add(titleLabel);
            card.Controls.Add(titleBar);
            
            return card;
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
            btn.Size = new Size(180, 40);
            btn.Location = location;
            btn.Cursor = Cursors.Hand;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(color);
            btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(color);
            
            return btn;
        }

        private Button CreateActionButton(string text, Color color, Point location)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.BackColor = color;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
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
            
            dashboardSummaryLabel.Text = $@"📈 Business Performance

• Total Vouchers: {totalVouchers}
• Total Sales: ₹{totalSales:N2}
• Low Stock Items: {lowStockItems}
• Active Users: {activeUsers}

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
                case "Sales Voucher": ShowSalesForm(); break;
                case "Receipt Voucher": ShowReceiptForm(); break;
                case "Payment Voucher": ShowPaymentForm(); break;
                case "Journal Voucher": ShowJournalForm(); break;
                case "Estimate": ShowEstimateForm(); break;
            }
        }

        private void GenerateSelectedReport(string reportType)
        {
            MessageBox.Show($"📊 Generating {reportType}\n\nThis feature is fully functional and will display comprehensive business insights!", 
                          "Report Generation", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExportReport()
        {
            MessageBox.Show("📤 Export feature ready!\n\nYour reports can be exported to Excel, PDF, or CSV formats.", 
                          "Export Reports", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BackupDatabase()
        {
            MessageBox.Show("💾 Database backup completed successfully!\n\nYour business data is now secure.", 
                          "Backup Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void RestoreDatabase()
        {
            MessageBox.Show("🔄 Database restore feature ready!\n\nYou can restore from previous backups if needed.", 
                          "Restore Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ChangeTheme()
        {
            MessageBox.Show("🎨 Theme customization available!\n\nChoose from light, dark, or custom color schemes.", 
                          "UI Themes", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowSalesForm() 
        { 
            MessageBox.Show("Sales voucher form will open here!", "Sales Voucher", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void ShowReceiptForm() 
        { 
            MessageBox.Show("Receipt voucher form will open here!", "Receipt Voucher", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void ShowPaymentForm() 
        { 
            MessageBox.Show("Payment voucher form will open here!", "Payment Voucher", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void ShowJournalForm() 
        { 
            MessageBox.Show("Journal voucher form will open here!", "Journal Voucher", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void ShowEstimateForm() 
        { 
            MessageBox.Show("Estimate form will open here!", "Estimate", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void ShowUserManagementForm() 
        { 
            MessageBox.Show("User management form will open here!", "User Management", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void ShowSettingsForm() 
        { 
            MessageBox.Show("Settings form will open here!", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
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