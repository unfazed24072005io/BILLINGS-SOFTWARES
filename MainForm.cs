using BillingSoftware.Utilities;
using BillingSoftware.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Linq;
using System.Data.SQLite;
using System.Collections.Generic;
using BillingSoftware.Modules;
using BillingSoftware.Models;

namespace BillingSoftware
{
    public partial class MainForm : Form
    {
        private MenuStrip mainMenu;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel userStatusLabel;
        private ToolStripStatusLabel dateStatusLabel;
        private DatabaseManager dbManager;
        private VoucherManager voucherManager;
        private ReportManager reportManager;

        private TabControl mainTabControl;
        private DataGridView vouchersGridView;
        private Label dashboardSummaryLabel;

        private Color primaryGreen = Color.FromArgb(46, 204, 113);
        private Color primaryBlue = Color.FromArgb(52, 152, 219);
        private Color darkText = Color.FromArgb(44, 62, 80);
        private Color lightBg = Color.FromArgb(248, 249, 250);
        private Color primaryOrange = Color.FromArgb(230, 126, 34);
        private Color primaryPurple = Color.FromArgb(155, 89, 182);

        public MainForm()
        {
            InitializeComponent();
            
            dbManager = new DatabaseManager();
            voucherManager = new VoucherManager();
            reportManager = new ReportManager();
            
            InitializeApplication();
        }

        private void InitializeApplication()
        {
            dbManager.CreateDatabase();
            CreateMainUI();
            LoadDashboardData();
            
            this.Text = "Simple Billing Software";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = lightBg;
        }

        private void CreateMainUI()
        {
            // Create menu strip
            CreateMainMenu();
            
            // Create status strip
            CreateStatusStrip();
            
            mainTabControl = new TabControl();
            mainTabControl.Dock = DockStyle.Fill;
            mainTabControl.Font = new Font("Segoe UI", 10);
            mainTabControl.Padding = new Point(10, 5);
            
            CreateDashboardTab();
            CreateVouchersTab();
            CreateProductsTab();
            CreateLedgersTab();
            CreateReceiptsTab();
            CreatePaymentsTab();
            CreateReportsTab();
            CreateAuditLogTab();
            
            // Main layout
            TableLayoutPanel mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.RowCount = 2;
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            
            mainLayout.Controls.Add(mainMenu, 0, 0);
            mainLayout.Controls.Add(mainTabControl, 0, 1);
            
            this.Controls.Add(mainLayout);
            this.Controls.Add(statusStrip);
        }

        private void CreateMainMenu()
        {
            mainMenu = new MenuStrip();
            mainMenu.BackColor = Color.FromArgb(52, 152, 219);
            mainMenu.ForeColor = Color.White;
            mainMenu.Dock = DockStyle.Top;
            mainMenu.Font = new Font("Segoe UI", 10);

            // File Menu
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("📁 File");
            
            ToolStripMenuItem backupItem = new ToolStripMenuItem("💾 Backup Database");
            backupItem.Click += (s, e) => BackupDatabase();
            
            ToolStripMenuItem restoreItem = new ToolStripMenuItem("📤 Restore Database");
            restoreItem.Click += (s, e) => RestoreDatabase();
            
            ToolStripMenuItem printItem = new ToolStripMenuItem("🖨️ Print Current");
            printItem.Click += (s, e) => PrintCurrentTab();
            
            ToolStripMenuItem exitItem = new ToolStripMenuItem("🚪 Exit");
            exitItem.Click += (s, e) => Application.Exit();
            
            fileMenu.DropDownItems.AddRange(new ToolStripItem[] { backupItem, restoreItem, new ToolStripSeparator(), printItem, new ToolStripSeparator(), exitItem });
            
            // View Menu
            ToolStripMenuItem viewMenu = new ToolStripMenuItem("👁️ View");
            
            ToolStripMenuItem refreshItem = new ToolStripMenuItem("🔄 Refresh All");
            refreshItem.Click += (s, e) => RefreshAllData();
            
            ToolStripMenuItem dashboardItem = new ToolStripMenuItem("🏠 Dashboard");
            dashboardItem.Click += (s, e) => mainTabControl.SelectedIndex = 0;
            
            ToolStripMenuItem vouchersItem = new ToolStripMenuItem("🧾 Vouchers");
            vouchersItem.Click += (s, e) => mainTabControl.SelectedIndex = 1;
            
            ToolStripMenuItem productsItem = new ToolStripMenuItem("📦 Products");
            productsItem.Click += (s, e) => mainTabControl.SelectedIndex = 2;
            
            ToolStripMenuItem ledgersItem = new ToolStripMenuItem("📒 Ledgers");
            ledgersItem.Click += (s, e) => mainTabControl.SelectedIndex = 3;
            
            ToolStripMenuItem receiptsItem = new ToolStripMenuItem("💰 Receipts");
            receiptsItem.Click += (s, e) => mainTabControl.SelectedIndex = 4;
            
            ToolStripMenuItem paymentsItem = new ToolStripMenuItem("💳 Payments");
            paymentsItem.Click += (s, e) => mainTabControl.SelectedIndex = 5;
            
            ToolStripMenuItem reportsItem = new ToolStripMenuItem("📈 Reports");
            reportsItem.Click += (s, e) => mainTabControl.SelectedIndex = 6;
            
            ToolStripMenuItem auditItem = new ToolStripMenuItem("📋 Audit Log");
            auditItem.Click += (s, e) => mainTabControl.SelectedIndex = 7;
            
            viewMenu.DropDownItems.AddRange(new ToolStripItem[] { refreshItem, new ToolStripSeparator(), 
                dashboardItem, vouchersItem, productsItem, ledgersItem, receiptsItem, paymentsItem, reportsItem, auditItem });
            
            // Tools Menu
            ToolStripMenuItem toolsMenu = new ToolStripMenuItem("🔧 Tools");
            
            ToolStripMenuItem calculatorItem = new ToolStripMenuItem("🧮 Calculator");
            calculatorItem.Click += (s, e) => System.Diagnostics.Process.Start("calc.exe");
            
            ToolStripMenuItem settingsItem = new ToolStripMenuItem("⚙️ Settings");
            settingsItem.Click += (s, e) => ShowSettings();
            
            toolsMenu.DropDownItems.AddRange(new ToolStripItem[] { calculatorItem, new ToolStripSeparator(), settingsItem });
            
            // Help Menu
            ToolStripMenuItem helpMenu = new ToolStripMenuItem("❓ Help");
            
            ToolStripMenuItem aboutItem = new ToolStripMenuItem("ℹ️ About");
            aboutItem.Click += (s, e) => ShowAbout();
            
            ToolStripMenuItem userManualItem = new ToolStripMenuItem("📖 User Manual");
            userManualItem.Click += (s, e) => ShowUserManual();
            
            helpMenu.DropDownItems.AddRange(new ToolStripItem[] { aboutItem, userManualItem });
            
            // User Menu (Right-aligned)
            ToolStripMenuItem userMenu = new ToolStripMenuItem($"👤 {Program.CurrentUser}");
            userMenu.Alignment = ToolStripItemAlignment.Right;
            userMenu.ForeColor = Color.FromArgb(255, 255, 150);
            userMenu.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            
            ToolStripMenuItem profileItem = new ToolStripMenuItem("👤 My Profile");
            profileItem.Click += (s, e) => ShowUserProfile();
            
            ToolStripMenuItem changePassItem = new ToolStripMenuItem("🔐 Change Password");
            changePassItem.Click += (s, e) => ChangePassword();
            
            ToolStripMenuItem logoutItem = new ToolStripMenuItem("🚪 Logout");
            logoutItem.Click += (s, e) => Logout();
            
            userMenu.DropDownItems.AddRange(new ToolStripItem[] { profileItem, changePassItem, new ToolStripSeparator(), logoutItem });
            
            mainMenu.Items.AddRange(new ToolStripItem[] { fileMenu, viewMenu, toolsMenu, helpMenu, userMenu });
        }

        private void CreateStatusStrip()
        {
            statusStrip = new StatusStrip();
            statusStrip.BackColor = Color.FromArgb(44, 62, 80);
            statusStrip.ForeColor = Color.White;
            statusStrip.Dock = DockStyle.Bottom;
            
            userStatusLabel = new ToolStripStatusLabel();
            userStatusLabel.Text = $"👤 User: {Program.CurrentUser} | Role: {Program.UserRole}";
            userStatusLabel.ForeColor = Color.White;
            
            dateStatusLabel = new ToolStripStatusLabel();
            dateStatusLabel.Text = $"📅 {DateTime.Now:dddd, dd MMMM yyyy} | 🕒 {DateTime.Now:hh:mm:ss tt}";
            dateStatusLabel.ForeColor = Color.White;
            dateStatusLabel.Alignment = ToolStripItemAlignment.Right;
            
            // Timer to update time
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000;
            timer.Tick += (s, e) => dateStatusLabel.Text = $"📅 {DateTime.Now:dddd, dd MMMM yyyy} | 🕒 {DateTime.Now:hh:mm:ss tt}";
            timer.Start();
            
            statusStrip.Items.AddRange(new ToolStripItem[] { userStatusLabel, dateStatusLabel });
        }

        // Add these methods to MainForm class
        private void BackupDatabase()
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Backup Files (*.backup)|*.backup|All Files (*.*)|*.*";
                saveDialog.FileName = $"billing_backup_{DateTime.Now:yyyyMMdd_HHmmss}.backup";
                
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    dbManager.BackupDatabase(saveDialog.FileName);
                    MessageBox.Show("✅ Database backup created successfully!", "Backup", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error backing up database: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RestoreDatabase()
        {
            try
            {
                OpenFileDialog openDialog = new OpenFileDialog();
                openDialog.Filter = "Backup Files (*.backup)|*.backup|All Files (*.*)|*.*";
                
                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    if (dbManager.RestoreDatabase(openDialog.FileName))
                    {
                        MessageBox.Show("✅ Database restored successfully!", "Restore", 
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                        RefreshAllData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error restoring database: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintCurrentTab()
        {
            int currentTab = mainTabControl.SelectedIndex;
            switch (currentTab)
            {
                case 0: // Dashboard
                    MessageBox.Show("Print dashboard functionality");
                    break;
                case 1: // Vouchers
                    PrintSelectedVoucher();
                    break;
                case 2: // Products
                    MessageBox.Show("Print products functionality");
                    break;
                case 3: // Ledgers
                    MessageBox.Show("Print ledger statement functionality");
                    break;
                case 4: // Receipts
                    MessageBox.Show("Print receipt functionality");
                    break;
                case 5: // Payments
                    MessageBox.Show("Print payment functionality");
                    break;
                case 6: // Reports
                    PrintStockReport();
                    break;
                case 7: // Audit Log
                    MessageBox.Show("Print audit log functionality");
                    break;
            }
        }

        private void RefreshAllData()
        {
            LoadDashboardData();
            
            var vouchersTab = mainTabControl.TabPages[1];
            var productsTab = mainTabControl.TabPages[2];
            
            LoadVouchersData();
            
            var productsGrid = productsTab.Controls.OfType<DataGridView>().First();
            LoadProductsData(productsGrid);
            
            MessageBox.Show("✅ All data refreshed!", "Refresh", 
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowSettings()
        {
            MessageBox.Show("Settings dialog will be implemented here", "Settings", 
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowAbout()
        {
            string aboutText = $"📊 Billing Software v1.0\n\n" +
                              $"Developed for efficient business management\n" +
                              $"© 2024 All Rights Reserved\n\n" +
                              $"Features:\n" +
                              $"• Sales & Purchase Management\n" +
                              $"• Ledger & Accounting System\n" +
                              $"• Receipt & Payment Vouchers\n" +
                              $"• Stock Tracking\n" +
                              $"• Reports & Analytics\n" +
                              $"• Professional Printing\n\n" +
                              $"Logged in as: {Program.CurrentUser} ({Program.UserRole})";
            
            MessageBox.Show(aboutText, "About Billing Software", 
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowUserManual()
        {
            MessageBox.Show("User manual will open here", "User Manual", 
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowUserProfile()
        {
            MessageBox.Show($"User Profile:\n\nUsername: {Program.CurrentUser}\nRole: {Program.UserRole}", 
                          "My Profile", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ChangePassword()
        {
            using (var changePassForm = new ChangePasswordForm())
            {
                if (changePassForm.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show("Password changed successfully!", "Success", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void Logout()
        {
            var result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout", 
                                       MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                Application.Restart();
            }
        }

        private void CreateDashboardTab()
        {
            TabPage dashboardTab = new TabPage("🏠 Dashboard");
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
            
            Button quickSalesBtn = CreateModernButton("Create Sales", primaryGreen, new Point(20, 150));
            Button quickPurchaseBtn = CreateModernButton("Create Purchase", primaryOrange, new Point(200, 150));
            Button quickEstimateBtn = CreateModernButton("Create Estimate", primaryPurple, new Point(380, 150));
            Button quickReceiptBtn = CreateModernButton("Create Receipt", Color.FromArgb(52, 152, 219), new Point(560, 150));
            Button quickPaymentBtn = CreateModernButton("Create Payment", Color.FromArgb(155, 89, 182), new Point(740, 150));
            Button stockReportBtn = CreateModernButton("Stock Report", primaryBlue, new Point(920, 150));
            
            quickSalesBtn.Click += (s, e) => { 
                mainTabControl.SelectedIndex = 1; 
                ShowSalesForm();
            };
            quickPurchaseBtn.Click += (s, e) => { 
                mainTabControl.SelectedIndex = 1; 
                ShowStockPurchaseForm();
            };
            quickEstimateBtn.Click += (s, e) => { 
                mainTabControl.SelectedIndex = 1; 
                ShowEstimateForm();
            };
            quickReceiptBtn.Click += (s, e) => { 
                mainTabControl.SelectedIndex = 4; 
                ShowReceiptForm();
            };
            quickPaymentBtn.Click += (s, e) => { 
                mainTabControl.SelectedIndex = 5; 
                ShowPaymentForm();
            };
            stockReportBtn.Click += (s, e) => { mainTabControl.SelectedIndex = 6; };
            
            dashboardTab.Controls.Add(dashboardTitle);
            dashboardTab.Controls.Add(dashboardSummaryLabel);
            dashboardTab.Controls.Add(quickSalesBtn);
            dashboardTab.Controls.Add(quickPurchaseBtn);
            dashboardTab.Controls.Add(quickEstimateBtn);
            dashboardTab.Controls.Add(quickReceiptBtn);
            dashboardTab.Controls.Add(quickPaymentBtn);
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
        "Sales Voucher", 
        "Purchase Voucher",
        "Estimate Voucher"
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
    
    // Add columns including created_by
    vouchersGridView.Columns.Add("number", "Voucher No");
    vouchersGridView.Columns.Add("type", "Type");
    vouchersGridView.Columns.Add("date", "Date");
    vouchersGridView.Columns.Add("party", "Party");
    vouchersGridView.Columns.Add("description", "Description");
    vouchersGridView.Columns.Add("created_by", "Created By"); // NEW
    
    Button refreshVouchersBtn = CreateModernButton("Refresh List", primaryBlue, new Point(20, 490));
    refreshVouchersBtn.Click += (s, e) => LoadVouchersData();
    
    // Add print button for vouchers
    Button printVoucherBtn = CreateModernButton("🖨️ Print Selected", primaryPurple, new Point(400, 18));
    printVoucherBtn.Click += (s, e) => PrintSelectedVoucher();
    
    vouchersTab.Controls.Add(voucherTypeCombo);
    vouchersTab.Controls.Add(createVoucherBtn);
    vouchersTab.Controls.Add(vouchersGridView);
    vouchersTab.Controls.Add(refreshVouchersBtn);
    vouchersTab.Controls.Add(printVoucherBtn);
    
    mainTabControl.TabPages.Add(vouchersTab);
    LoadVouchersData();
}

        private void PrintSelectedVoucher()
        {
            try
            {
                if (vouchersGridView.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select a voucher to print!", 
                                  "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DataGridViewRow selectedRow = vouchersGridView.SelectedRows[0];
                string voucherNumber = selectedRow.Cells["number"].Value.ToString();
                
                // Get voucher details from database
                var voucher = GetVoucherByNumber(voucherNumber);
                
                if (voucher != null)
                {
                    PrintHelper printHelper = new PrintHelper();
                    printHelper.PrintVoucher(voucher, voucher.Items);
                }
                else
                {
                    MessageBox.Show("Voucher not found!", "Error", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing voucher: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Voucher GetVoucherByNumber(string voucherNumber)
        {
            try
            {
                string sql = @"SELECT v.*, 
                              GROUP_CONCAT(vi.product_name || '|' || vi.quantity || '|' || vi.unit_price, ';') as items
                              FROM vouchers v
                              LEFT JOIN voucher_items vi ON v.number = vi.voucher_number
                              WHERE v.number = @number
                              GROUP BY v.id";
                
                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@number", voucherNumber);
                    
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var voucher = new Voucher
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Type = reader["type"].ToString(),
                                Number = reader["number"].ToString(),
                                Date = DateTime.Parse(reader["date"].ToString()),
                                Party = reader["party"].ToString(),
                                Amount = Convert.ToDecimal(reader["amount"]),
                                Description = reader["description"].ToString(),
                                Status = reader["status"].ToString()
                            };

                            // Parse items
                            var itemsData = reader["items"].ToString();
                            if (!string.IsNullOrEmpty(itemsData))
                            {
                                var items = itemsData.Split(';');
                                foreach (var item in items)
                                {
                                    var parts = item.Split('|');
                                    if (parts.Length >= 3)
                                    {
                                        voucher.Items.Add(new VoucherItem
                                        {
                                            ProductName = parts[0],
                                            Quantity = decimal.Parse(parts[1]),
                                            UnitPrice = decimal.Parse(parts[2])
                                        });
                                    }
                                }
                            }

                            return voucher;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading voucher: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            return null;
        }

        private void CreateProductsTab()
        {
            TabPage productsTab = new TabPage("📦 Products");
            productsTab.BackColor = lightBg;
            
            Label productsTitle = new Label();
            productsTitle.Text = "Product Management";
            productsTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            productsTitle.ForeColor = darkText;
            productsTitle.Location = new Point(20, 20);
            productsTitle.Size = new Size(300, 30);
            productsTab.Controls.Add(productsTitle);
            
            // Products Grid
            DataGridView productsGrid = new DataGridView();
            productsGrid.Location = new Point(20, 70);
            productsGrid.Size = new Size(900, 400);
            productsGrid.BackgroundColor = Color.White;
            productsGrid.BorderStyle = BorderStyle.None;
            productsGrid.Font = new Font("Segoe UI", 9);
            productsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            productsGrid.ReadOnly = true;
            productsTab.Controls.Add(productsGrid);
            
            // Buttons
            Button addProductBtn = CreateModernButton("➕ Add Product", primaryGreen, new Point(20, 490));
            addProductBtn.Click += (s, e) => ShowAddProductForm();
            productsTab.Controls.Add(addProductBtn);
            
            Button refreshProductsBtn = CreateModernButton("🔄 Refresh", primaryBlue, new Point(180, 490));
            refreshProductsBtn.Click += (s, e) => LoadProductsData(productsGrid);
            productsTab.Controls.Add(refreshProductsBtn);
            
            mainTabControl.TabPages.Add(productsTab);
            LoadProductsData(productsGrid);
        }

        private void CreateLedgersTab()
        {
            TabPage ledgersTab = new TabPage("📒 Ledgers");
            ledgersTab.BackColor = TallyUIStyles.TallyGray;
            
            // Create Tally-style panel container
            Panel mainPanel = TallyUIStyles.CreateTallyPanel("Ledger Management", new Point(20, 20), new Size(940, 520));
            
            // Search Section
            Panel searchPanel = new Panel();
            searchPanel.Location = new Point(20, 40);
            searchPanel.Size = new Size(900, 40);
            searchPanel.BackColor = Color.Transparent;
            
            Label searchLabel = TallyUIStyles.CreateTallyLabel("Search:", new Point(0, 8), new Size(60, 20));
            TextBox searchTxt = TallyUIStyles.CreateTallyTextBox(new Point(70, 5), new Size(200, 25), "Enter ledger name or code");
            
            Button searchBtn = TallyUIStyles.CreateTallyButton("🔍 Search", TallyUIStyles.TallyBlue, new Point(280, 5));
            Button clearBtn = TallyUIStyles.CreateTallyButton("Clear", TallyUIStyles.TallyGray, new Point(410, 5), new Size(80, 25));
            
            searchPanel.Controls.AddRange(new Control[] { searchLabel, searchTxt, searchBtn, clearBtn });
            mainPanel.Controls.Add(searchPanel);
            
            // Ledgers Grid
            DataGridView ledgersGrid = TallyUIStyles.CreateTallyGrid(new Point(20, 90), new Size(900, 300));
            ledgersGrid.Name = "ledgersGrid";
            ledgersGrid.Columns.Add("Code", "Code");
            ledgersGrid.Columns.Add("Name", "Name");
            ledgersGrid.Columns.Add("Type", "Type");
            ledgersGrid.Columns.Add("Balance", "Balance");
	    ledgersGrid.Columns.Add("TotalSales", "Total Sales");
            ledgersGrid.Columns.Add("TotalPurchases", "Total Purchases");
            ledgersGrid.Columns.Add("Status", "Status");
            ledgersGrid.Columns.Add("Phone", "Phone");
            
            // Format columns
            ledgersGrid.Columns["Balance"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            ledgersGrid.Columns["Balance"].DefaultCellStyle.Format = "N2";
            
            mainPanel.Controls.Add(ledgersGrid);
            
            // Action Buttons
            Panel actionPanel = new Panel();
            actionPanel.Location = new Point(20, 400);
            actionPanel.Size = new Size(900, 40);
            actionPanel.BackColor = Color.Transparent;
            
            Button addLedgerBtn = TallyUIStyles.CreateTallyButton("➕ Add Ledger", TallyUIStyles.TallyGreen, new Point(0, 0));
            Button editLedgerBtn = TallyUIStyles.CreateTallyButton("✏️ Edit Ledger", TallyUIStyles.TallyBlue, new Point(130, 0));
            Button viewStatementBtn = TallyUIStyles.CreateTallyButton("📊 View Statement", TallyUIStyles.TallyOrange, new Point(260, 0));
            Button refreshBtn = TallyUIStyles.CreateTallyButton("🔄 Refresh", TallyUIStyles.TallyGray, new Point(390, 0));
            
            actionPanel.Controls.AddRange(new Control[] { addLedgerBtn, editLedgerBtn, viewStatementBtn, refreshBtn });
            mainPanel.Controls.Add(actionPanel);
            
            // Event Handlers
            addLedgerBtn.Click += (s, e) => ShowAddLedgerForm();
            refreshBtn.Click += (s, e) => LoadLedgersData(ledgersGrid);
            searchBtn.Click += (s, e) => SearchLedgers(ledgersGrid, searchTxt.Text);
            clearBtn.Click += (s, e) => { searchTxt.Clear(); LoadLedgersData(ledgersGrid); };
            viewStatementBtn.Click += (s, e) => ShowLedgerStatementForm(ledgersGrid);
            
            ledgersTab.Controls.Add(mainPanel);
            mainTabControl.TabPages.Add(ledgersTab);
            
            // Load initial data
            LoadLedgersData(ledgersGrid);
        }

        private void CreateReceiptsTab()
        {
            TabPage receiptsTab = new TabPage("💰 Receipts");
            receiptsTab.BackColor = TallyUIStyles.TallyGray;
            
            Panel mainPanel = TallyUIStyles.CreateTallyPanel("Receipt Vouchers", new Point(20, 20), new Size(940, 520));
            
            // Quick Actions
            Panel quickPanel = new Panel();
            quickPanel.Location = new Point(20, 40);
            quickPanel.Size = new Size(900, 60);
            quickPanel.BackColor = Color.Transparent;
            
            Button createReceiptBtn = TallyUIStyles.CreateTallyButton("➕ Create Receipt", TallyUIStyles.TallyGreen, new Point(0, 10), new Size(150, 35));
            Button receiptAgainstSalesBtn = TallyUIStyles.CreateTallyButton("📝 Against Sales", TallyUIStyles.TallyBlue, new Point(160, 10), new Size(150, 35));
            Button printReceiptBtn = TallyUIStyles.CreateTallyButton("🖨️ Print", TallyUIStyles.TallyPurple, new Point(320, 10), new Size(120, 35));
            Button refreshReceiptsBtn = TallyUIStyles.CreateTallyButton("🔄 Refresh", TallyUIStyles.TallyGray, new Point(450, 10), new Size(120, 35));
            
            quickPanel.Controls.AddRange(new Control[] { createReceiptBtn, receiptAgainstSalesBtn, printReceiptBtn, refreshReceiptsBtn });
            mainPanel.Controls.Add(quickPanel);
            
            // Receipts Grid
            DataGridView receiptsGrid = TallyUIStyles.CreateTallyGrid(new Point(20, 110), new Size(900, 350));
            receiptsGrid.Name = "receiptsGrid";
            receiptsGrid.Columns.Add("Number", "Receipt No");
            receiptsGrid.Columns.Add("Date", "Date");
            receiptsGrid.Columns.Add("ReceivedFrom", "Received From");
            receiptsGrid.Columns.Add("Amount", "Amount");
            receiptsGrid.Columns.Add("PaymentMode", "Mode");
            receiptsGrid.Columns.Add("Status", "Status");
            
            receiptsGrid.Columns["Amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            receiptsGrid.Columns["Amount"].DefaultCellStyle.Format = "N2";
            
            mainPanel.Controls.Add(receiptsGrid);
            
            // Event Handlers
            createReceiptBtn.Click += (s, e) => ShowReceiptForm();
            receiptAgainstSalesBtn.Click += (s, e) => ShowReceiptAgainstSalesForm();
            refreshReceiptsBtn.Click += (s, e) => LoadReceiptsData(receiptsGrid);
            
            receiptsTab.Controls.Add(mainPanel);
            mainTabControl.TabPages.Add(receiptsTab);
            
            LoadReceiptsData(receiptsGrid);
        }

        private void CreatePaymentsTab()
        {
            TabPage paymentsTab = new TabPage("💳 Payments");
            paymentsTab.BackColor = TallyUIStyles.TallyGray;
            
            Panel mainPanel = TallyUIStyles.CreateTallyPanel("Payment Vouchers", new Point(20, 20), new Size(940, 520));
            
            // Quick Actions
            Panel quickPanel = new Panel();
            quickPanel.Location = new Point(20, 40);
            quickPanel.Size = new Size(900, 60);
            quickPanel.BackColor = Color.Transparent;
            
            Button createPaymentBtn = TallyUIStyles.CreateTallyButton("➕ Create Payment", TallyUIStyles.TallyGreen, new Point(0, 10), new Size(150, 35));
            Button paymentAgainstPurchaseBtn = TallyUIStyles.CreateTallyButton("📝 Against Purchase", TallyUIStyles.TallyBlue, new Point(160, 10), new Size(150, 35));
            Button printPaymentBtn = TallyUIStyles.CreateTallyButton("🖨️ Print", TallyUIStyles.TallyPurple, new Point(320, 10), new Size(120, 35));
            Button refreshPaymentsBtn = TallyUIStyles.CreateTallyButton("🔄 Refresh", TallyUIStyles.TallyGray, new Point(450, 10), new Size(120, 35));
            
            quickPanel.Controls.AddRange(new Control[] { createPaymentBtn, paymentAgainstPurchaseBtn, printPaymentBtn, refreshPaymentsBtn });
            mainPanel.Controls.Add(quickPanel);
            
            // Payments Grid
            DataGridView paymentsGrid = TallyUIStyles.CreateTallyGrid(new Point(20, 110), new Size(900, 350));
            paymentsGrid.Name = "paymentsGrid";
            paymentsGrid.Columns.Add("Number", "Payment No");
            paymentsGrid.Columns.Add("Date", "Date");
            paymentsGrid.Columns.Add("PaidTo", "Paid To");
            paymentsGrid.Columns.Add("Amount", "Amount");
            paymentsGrid.Columns.Add("PaymentMode", "Mode");
            paymentsGrid.Columns.Add("Status", "Status");
            
            paymentsGrid.Columns["Amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            paymentsGrid.Columns["Amount"].DefaultCellStyle.Format = "N2";
            
            mainPanel.Controls.Add(paymentsGrid);
            
            // Event Handlers
            createPaymentBtn.Click += (s, e) => ShowPaymentForm();
            paymentAgainstPurchaseBtn.Click += (s, e) => ShowPaymentAgainstPurchaseForm();
            refreshPaymentsBtn.Click += (s, e) => LoadPaymentsData(paymentsGrid);
            
            paymentsTab.Controls.Add(mainPanel);
            mainTabControl.TabPages.Add(paymentsTab);
            
            LoadPaymentsData(paymentsGrid);
        }

        private void CreateReportsTab()
        {
            TabPage reportsTab = new TabPage("📈 Stock Reports");
            reportsTab.BackColor = TallyUIStyles.TallyGray;
            reportsTab.Padding = new Padding(20);

            // Title
            Label reportsTitle = new Label();
            reportsTitle.Text = "📊 Stock Movement Reports";
            reportsTitle.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            reportsTitle.ForeColor = TallyUIStyles.DarkText;
            reportsTitle.Location = new Point(20, 20);
            reportsTitle.Size = new Size(400, 35);
            reportsTab.Controls.Add(reportsTitle);

            // Report Card Panel
            Panel reportCard = new Panel();
            reportCard.Location = new Point(20, 70);
            reportCard.Size = new Size(940, 120);
            reportCard.BackColor = TallyUIStyles.White;
            reportCard.BorderStyle = BorderStyle.None;
            reportCard.Padding = new Padding(15);

            // Report type selection
            Label reportTypeLabel = new Label();
            reportTypeLabel.Text = "📋 Report Type:";
            reportTypeLabel.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            reportTypeLabel.ForeColor = TallyUIStyles.DarkText;
            reportTypeLabel.Location = new Point(20, 25);
            reportTypeLabel.Size = new Size(120, 25);
            reportCard.Controls.Add(reportTypeLabel);

            ComboBox reportTypeCombo = new ComboBox();
            reportTypeCombo.Location = new Point(150, 23);
            reportTypeCombo.Size = new Size(250, 30);
            reportTypeCombo.Font = new Font("Segoe UI", 10);
            reportTypeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            reportTypeCombo.Items.AddRange(new string[] { 
                "📊 Daily Stock Report",
                "📈 Monthly Stock Report", 
                "📅 Yearly Stock Summary"
            });
            reportTypeCombo.SelectedIndex = 0;
            reportCard.Controls.Add(reportTypeCombo);

            // Date controls panel
            Panel datePanel = new Panel();
            datePanel.Location = new Point(420, 20);
            datePanel.Size = new Size(350, 40);
            datePanel.BackColor = Color.Transparent;

            // Daily date control
            Label dailyLabel = new Label();
            dailyLabel.Text = "📅 Date:";
            dailyLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dailyLabel.ForeColor = TallyUIStyles.DarkText;
            dailyLabel.Location = new Point(0, 8);
            dailyLabel.Size = new Size(60, 20);
            dailyLabel.Name = "dailyLabel";
            datePanel.Controls.Add(dailyLabel);

            DateTimePicker dailyDatePicker = new DateTimePicker();
            dailyDatePicker.Location = new Point(65, 5);
            dailyDatePicker.Size = new Size(140, 25);
            dailyDatePicker.Value = DateTime.Now;
            dailyDatePicker.Format = DateTimePickerFormat.Custom;
            dailyDatePicker.CustomFormat = "dd-MMM-yyyy";
            dailyDatePicker.Font = new Font("Segoe UI", 10);
            dailyDatePicker.Name = "dailyPicker";
            datePanel.Controls.Add(dailyDatePicker);

            // Monthly controls
            Label monthLabel = new Label();
            monthLabel.Text = "📅 Month:";
            monthLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            monthLabel.ForeColor = TallyUIStyles.DarkText;
            monthLabel.Location = new Point(0, 8);
            monthLabel.Size = new Size(65, 20);
            monthLabel.Visible = false;
            monthLabel.Name = "monthLabel";
            datePanel.Controls.Add(monthLabel);

            ComboBox monthCombo = new ComboBox();
            monthCombo.Location = new Point(70, 5);
            monthCombo.Size = new Size(110, 25);
            monthCombo.Font = new Font("Segoe UI", 10);
            monthCombo.Items.AddRange(new string[] { 
                "January", "February", "March", "April", "May", "June", 
                "July", "August", "September", "October", "November", "December" 
            });
            monthCombo.SelectedIndex = DateTime.Now.Month - 1;
            monthCombo.Visible = false;
            monthCombo.Name = "monthCombo";
            datePanel.Controls.Add(monthCombo);

            Label yearLabel = new Label();
            yearLabel.Text = "Year:";
            yearLabel.Font = new Font("Segoe UI", 10);
            yearLabel.ForeColor = TallyUIStyles.DarkText;
            yearLabel.Location = new Point(190, 8);
            yearLabel.Size = new Size(40, 20);
            yearLabel.Visible = false;
            yearLabel.Name = "yearLabel";
            datePanel.Controls.Add(yearLabel);

            NumericUpDown yearPicker = new NumericUpDown();
            yearPicker.Location = new Point(235, 5);
            yearPicker.Size = new Size(80, 25);
            yearPicker.Minimum = 2020;
            yearPicker.Maximum = 2030;
            yearPicker.Value = DateTime.Now.Year;
            yearPicker.Font = new Font("Segoe UI", 10);
            yearPicker.Visible = false;
            yearPicker.Name = "yearPicker";
            datePanel.Controls.Add(yearPicker);

            // Yearly controls
            Label yearOnlyLabel = new Label();
            yearOnlyLabel.Text = "📅 Year:";
            yearOnlyLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            yearOnlyLabel.ForeColor = TallyUIStyles.DarkText;
            yearOnlyLabel.Location = new Point(0, 8);
            yearOnlyLabel.Size = new Size(50, 20);
            yearOnlyLabel.Visible = false;
            yearOnlyLabel.Name = "yearOnlyLabel";
            datePanel.Controls.Add(yearOnlyLabel);

            NumericUpDown yearOnlyPicker = new NumericUpDown();
            yearOnlyPicker.Location = new Point(55, 5);
            yearOnlyPicker.Size = new Size(80, 25);
            yearOnlyPicker.Minimum = 2020;
            yearOnlyPicker.Maximum = 2030;
            yearOnlyPicker.Value = DateTime.Now.Year;
            yearOnlyPicker.Font = new Font("Segoe UI", 10);
            yearOnlyPicker.Visible = false;
            yearOnlyPicker.Name = "yearOnlyPicker";
            datePanel.Controls.Add(yearOnlyPicker);

            reportCard.Controls.Add(datePanel);

            // Action Buttons Panel
            Panel actionPanel = new Panel();
            actionPanel.Location = new Point(790, 15);
            actionPanel.Size = new Size(140, 130);
            actionPanel.BackColor = Color.Transparent;

            // Generate Button
            Button generateReportBtn = new Button();
            generateReportBtn.Text = "📄 Generate";
            generateReportBtn.BackColor = TallyUIStyles.TallyGreen;
            generateReportBtn.ForeColor = Color.White;
            generateReportBtn.FlatStyle = FlatStyle.Flat;
            generateReportBtn.FlatAppearance.BorderSize = 0;
            generateReportBtn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            generateReportBtn.Size = new Size(130, 35);
            generateReportBtn.Location = new Point(0, 0);
            generateReportBtn.Cursor = Cursors.Hand;
            generateReportBtn.Click += (s, e) => GenerateStockReport(datePanel, reportTypeCombo.SelectedItem.ToString());
            actionPanel.Controls.Add(generateReportBtn);

            // Export Button
            Button exportBtn = new Button();
            exportBtn.Text = "📤 Export CSV";
            exportBtn.BackColor = TallyUIStyles.TallyBlue;
            exportBtn.ForeColor = Color.White;
            exportBtn.FlatStyle = FlatStyle.Flat;
            exportBtn.FlatAppearance.BorderSize = 0;
            exportBtn.Font = new Font("Segoe UI", 10);
            exportBtn.Size = new Size(130, 35);
            exportBtn.Location = new Point(0, 45);
            exportBtn.Cursor = Cursors.Hand;
            exportBtn.Click += (s, e) => ExportStockReport();
            actionPanel.Controls.Add(exportBtn);

            // Print Button
            Button printReportBtn = new Button();
            printReportBtn.Text = "🖨️ Print";
            printReportBtn.BackColor = TallyUIStyles.TallyPurple;
            printReportBtn.ForeColor = Color.White;
            printReportBtn.FlatStyle = FlatStyle.Flat;
            printReportBtn.FlatAppearance.BorderSize = 0;
            printReportBtn.Font = new Font("Segoe UI", 10);
            printReportBtn.Size = new Size(130, 35);
            printReportBtn.Location = new Point(0, 90);
            printReportBtn.Cursor = Cursors.Hand;
            printReportBtn.Click += (s, e) => PrintStockReport();
            actionPanel.Controls.Add(printReportBtn);

            reportCard.Controls.Add(actionPanel);

            // Update controls based on report type
            reportTypeCombo.SelectedIndexChanged += (s, e) => UpdateReportControls(datePanel, reportTypeCombo.SelectedItem.ToString());

            reportsTab.Controls.Add(reportCard);

            // Statistics Panel
            Panel statsPanel = new Panel();
            statsPanel.Location = new Point(20, 210);
            statsPanel.Size = new Size(940, 80);
            statsPanel.BackColor = TallyUIStyles.TallyBlue;
            statsPanel.BorderStyle = BorderStyle.None;
            statsPanel.Padding = new Padding(20);

            Label statsTitle = new Label();
            statsTitle.Text = "📈 Report Summary";
            statsTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            statsTitle.ForeColor = Color.White;
            statsTitle.Location = new Point(0, 5);
            statsTitle.Size = new Size(200, 25);
            statsPanel.Controls.Add(statsTitle);

            Label reportSummaryLabel = new Label();
            reportSummaryLabel.Font = new Font("Segoe UI", 10);
            reportSummaryLabel.ForeColor = Color.White;
            reportSummaryLabel.Location = new Point(0, 35);
            reportSummaryLabel.Size = new Size(900, 25);
            reportSummaryLabel.TextAlign = ContentAlignment.MiddleLeft;
            reportSummaryLabel.Name = "summaryLabel";
            reportSummaryLabel.Text = "Select report type and generate to see summary";
            statsPanel.Controls.Add(reportSummaryLabel);

            reportsTab.Controls.Add(statsPanel);

            // Report display area
            DataGridView reportsGridView = new DataGridView();
            reportsGridView.Location = new Point(20, 310);
            reportsGridView.Size = new Size(940, 280);
            reportsGridView.BackgroundColor = Color.White;
            reportsGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            reportsGridView.ReadOnly = true;
            reportsGridView.Font = new Font("Segoe UI", 9);
            reportsGridView.RowHeadersVisible = false;
            reportsGridView.AllowUserToAddRows = false;
            reportsGridView.AllowUserToDeleteRows = false;
            reportsGridView.AllowUserToResizeRows = false;
            reportsGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);
            
            // Style the headers
            reportsGridView.ColumnHeadersDefaultCellStyle.BackColor = TallyUIStyles.TallyPurple;
            reportsGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            reportsGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            reportsGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            reportsGridView.EnableHeadersVisualStyles = false;
            
            // Style cells
            reportsGridView.DefaultCellStyle.Font = new Font("Segoe UI", 9);
            reportsGridView.DefaultCellStyle.ForeColor = TallyUIStyles.DarkText;
            reportsGridView.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 230, 230);
            reportsGridView.DefaultCellStyle.SelectionForeColor = TallyUIStyles.DarkText;
            
            reportsGridView.Name = "reportsGridView";
            reportsTab.Controls.Add(reportsGridView);

            mainTabControl.TabPages.Add(reportsTab);
            
            // Initialize controls
            UpdateReportControls(datePanel, reportTypeCombo.SelectedItem.ToString());
        }

        private void CreateAuditLogTab()
        {
            TabPage auditTab = new TabPage("📋 Audit Log");
            auditTab.BackColor = TallyUIStyles.TallyGray;
            
            Panel mainPanel = TallyUIStyles.CreateTallyPanel("User Activity Audit Log", new Point(20, 20), new Size(940, 520));
            
            // Filter Controls
            Panel filterPanel = new Panel();
            filterPanel.Location = new Point(20, 40);
            filterPanel.Size = new Size(900, 40);
            filterPanel.BackColor = Color.Transparent;
            
            Label fromLabel = TallyUIStyles.CreateTallyLabel("From:", new Point(0, 8), new Size(40, 20));
            DateTimePicker fromDate = TallyUIStyles.CreateTallyDateTimePicker(new Point(45, 5), new Size(120, 25));
            fromDate.Value = DateTime.Now.AddDays(-7);
            
            Label toLabel = TallyUIStyles.CreateTallyLabel("To:", new Point(175, 8), new Size(30, 20));
            DateTimePicker toDate = TallyUIStyles.CreateTallyDateTimePicker(new Point(210, 5), new Size(120, 25));
            
            Label userLabel = TallyUIStyles.CreateTallyLabel("User:", new Point(340, 8), new Size(40, 20));
            ComboBox userCombo = TallyUIStyles.CreateTallyComboBox(new Point(385, 5), new Size(120, 25));
            
            Button filterBtn = TallyUIStyles.CreateTallyButton("🔍 Filter", TallyUIStyles.TallyBlue, new Point(515, 5), new Size(100, 25));
            Button exportBtn = TallyUIStyles.CreateTallyButton("📤 Export", TallyUIStyles.TallyGreen, new Point(625, 5), new Size(100, 25));
            
            filterPanel.Controls.AddRange(new Control[] { fromLabel, fromDate, toLabel, toDate, userLabel, userCombo, filterBtn, exportBtn });
            mainPanel.Controls.Add(filterPanel);
            
            // Audit Grid
            DataGridView auditGrid = TallyUIStyles.CreateTallyGrid(new Point(20, 90), new Size(900, 380));
            auditGrid.Columns.Add("Timestamp", "Timestamp");
            auditGrid.Columns.Add("Username", "User");
            auditGrid.Columns.Add("Action", "Action");
            auditGrid.Columns.Add("EntityType", "Entity");
            auditGrid.Columns.Add("EntityId", "Entity ID");
            auditGrid.Columns.Add("Details", "Details");
            auditGrid.Columns.Add("Module", "Module");
            
            mainPanel.Controls.Add(auditGrid);
            
            // Event Handlers
            filterBtn.Click += (s, e) => LoadAuditLogs(auditGrid, fromDate.Value, toDate.Value, userCombo.Text);
            exportBtn.Click += (s, e) => ExportAuditLogs(auditGrid);
            
            auditTab.Controls.Add(mainPanel);
            mainTabControl.TabPages.Add(auditTab);
            
            // Load user list
            LoadUserList(userCombo);
            LoadAuditLogs(auditGrid, fromDate.Value, toDate.Value);
        }

        // ============ Helper Methods ============

        private void LoadLedgersData(DataGridView grid)
{
    try
    {
        // Query to get ledger balances from ledger_transactions
        string sql = @"SELECT 
                      l.name as 'Ledger Name',
                      l.code as 'Code',
                      l.type as 'Type',
                      COALESCE(
                          (SELECT balance FROM ledger_transactions lt 
                           WHERE lt.ledger_name = l.name 
                           ORDER BY lt.id DESC LIMIT 1), 0) as 'Balance',
                      l.balance_type as 'Balance Type',
                      CASE WHEN l.is_active = 1 THEN 'Active' ELSE 'Inactive' END as 'Status'
                      FROM ledgers l
                      ORDER BY l.name";
        
        using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
        using (var adapter = new SQLiteDataAdapter(cmd))
        {
            var dataTable = new DataTable();
            adapter.Fill(dataTable);
            grid.DataSource = dataTable;
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error loading ledgers: {ex.Message}", "Error", 
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

        private void LoadReceiptsData(DataGridView grid)
{
    try
    {
        string sql = @"SELECT 
                      r.number as 'Receipt No',
                      r.date as 'Date',
                      r.received_from as 'Received From',
                      r.amount as 'Amount',
                      r.payment_mode as 'Mode',
                      r.bank_name as 'Bank',
                      r.cheque_no as 'Cheque No',
                      r.created_by as 'Created By',
                      CASE WHEN r.is_posted = 1 THEN 'Posted' ELSE 'Draft' END as 'Status'
                      FROM receipt_vouchers r
                      ORDER BY r.date DESC";
        
        using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
        using (var adapter = new SQLiteDataAdapter(cmd))
        {
            var dataTable = new DataTable();
            adapter.Fill(dataTable);
            grid.DataSource = dataTable;
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error loading receipts: {ex.Message}", "Error", 
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

private void LoadPaymentsData(DataGridView grid)
{
    try
    {
        string sql = @"SELECT 
                      p.number as 'Payment No',
                      p.date as 'Date',
                      p.paid_to as 'Paid To',
                      p.amount as 'Amount',
                      p.payment_mode as 'Mode',
                      p.bank_name as 'Bank',
                      p.cheque_no as 'Cheque No',
                      p.created_by as 'Created By',
                      CASE WHEN p.is_posted = 1 THEN 'Posted' ELSE 'Draft' END as 'Status'
                      FROM payment_vouchers p
                      ORDER BY p.date DESC";
        
        using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
        using (var adapter = new SQLiteDataAdapter(cmd))
        {
            var dataTable = new DataTable();
            adapter.Fill(dataTable);
            grid.DataSource = dataTable;
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error loading payments: {ex.Message}", "Error", 
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

        private void LoadAuditLogs(DataGridView grid, DateTime fromDate, DateTime toDate, string username = "")
        {
            try
            {
                AuditLogger auditLogger = new AuditLogger();
                var logs = auditLogger.GetAuditLogs(fromDate, toDate, username);
                grid.DataSource = logs;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading audit logs: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadUserList(ComboBox comboBox)
        {
            try
            {
                string sql = "SELECT DISTINCT username FROM users ORDER BY username";
                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                using (var reader = cmd.ExecuteReader())
                {
                    comboBox.Items.Clear();
                    comboBox.Items.Add(""); // Empty option
                    while (reader.Read())
                    {
                        comboBox.Items.Add(reader["username"].ToString());
                    }
                }
            }
            catch { }
        }

        private void ShowAddLedgerForm()
        {
            using (var form = new Forms.Ledger.AddLedgerForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    var ledgersTab = mainTabControl.TabPages[3];
                    var ledgersGrid = ledgersTab.Controls.Find("ledgersGrid", true)[0] as DataGridView;
                    LoadLedgersData(ledgersGrid);
                }
            }
        }

        private void ShowReceiptForm()
        {
            using (var form = new Forms.Vouchers.ReceiptForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    var receiptsTab = mainTabControl.TabPages[4];
                    var receiptsGrid = receiptsTab.Controls.Find("receiptsGrid", true)[0] as DataGridView;
                    LoadReceiptsData(receiptsGrid);
                }
            }
        }

        private void ShowPaymentForm()
        {
            using (var form = new Forms.Vouchers.PaymentForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    var paymentsTab = mainTabControl.TabPages[5];
                    var paymentsGrid = paymentsTab.Controls.Find("paymentsGrid", true)[0] as DataGridView;
                    LoadPaymentsData(paymentsGrid);
                }
            }
        }

        private void SearchLedgers(DataGridView grid, string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    LoadLedgersData(grid);
                    return;
                }
                
                string sql = @"SELECT code, name, type, current_balance, balance_type, 
                              phone, email, is_active
                              FROM ledgers 
                              WHERE name LIKE @search OR code LIKE @search
                              ORDER BY name";
                
                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
                    
                    using (var adapter = new SQLiteDataAdapter(cmd))
                    {
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        grid.DataSource = dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching ledgers: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowLedgerStatementForm(DataGridView ledgersGrid)
        {
            if (ledgersGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a ledger to view statement!", "No Selection", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            string ledgerName = ledgersGrid.SelectedRows[0].Cells["Name"].Value.ToString();
            
            using (var form = new Forms.Ledger.LedgerStatementForm(ledgerName))
            {
                form.ShowDialog();
            }
        }

        private void ExportAuditLogs(DataGridView grid)
        {
            try
            {
                if (grid.DataSource == null)
                {
                    MessageBox.Show("No data to export!", "Export", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "CSV Files (*.csv)|*.csv";
                saveDialog.FileName = $"audit_logs_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    
                    // Headers
                    var headers = grid.Columns.Cast<DataGridViewColumn>();
                    sb.AppendLine(string.Join(",", headers.Select(column => $"\"{column.HeaderText}\"")));
                    
                    // Data
                    foreach (DataGridViewRow row in grid.Rows)
                    {
                        if (!row.IsNewRow)
                        {
                            var cells = row.Cells.Cast<DataGridViewCell>();
                            sb.AppendLine(string.Join(",", cells.Select(cell => $"\"{cell.Value}\"")));
                        }
                    }
                    
                    System.IO.File.WriteAllText(saveDialog.FileName, sb.ToString());
                    
                    MessageBox.Show($"Audit logs exported successfully to: {saveDialog.FileName}", 
                                  "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting audit logs: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateSelectedVoucher(string voucherType)
        {
            switch (voucherType)
            {
                case "Sales Voucher":
                    ShowSalesForm();
                    break;
                case "Purchase Voucher":
                    ShowStockPurchaseForm();
                    break;
                case "Estimate Voucher":
                    ShowEstimateForm();
                    break;
            }
        }

        private void ShowSalesForm()
        {
            Forms.Vouchers.SalesForm salesForm = new Forms.Vouchers.SalesForm();
            salesForm.ShowDialog();
            LoadVouchersData();
            LoadDashboardData();
        }

        private void ShowStockPurchaseForm()
        {
            Forms.Vouchers.StockPurchaseForm stockForm = new Forms.Vouchers.StockPurchaseForm();
            stockForm.ShowDialog();
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

        private void ShowAddProductForm()
        {
            using (var addForm = new AddProductForm())
            {
                if (addForm.ShowDialog() == DialogResult.OK)
                {
                    var productsTab = mainTabControl.TabPages[2];
                    var productsGrid = productsTab.Controls.OfType<DataGridView>().First();
                    LoadProductsData(productsGrid);
                }
            }
        }

        private void LoadProductsData(DataGridView grid)
        {
            try
            {
                var products = dbManager.GetAllProducts();
                grid.DataSource = products;
                
                foreach (DataGridViewColumn column in grid.Columns)
                {
                    if (column.Name == "Price" || column.Name == "Stock" || column.Name == "MinStock")
                    {
                        column.DefaultCellStyle.Format = "N2";
                        column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            btn.Size = new Size(120, 35);
            btn.Location = location;
            btn.Cursor = Cursors.Hand;
            return btn;
        }

        private void LoadDashboardData()
        {
            try
            {
                string stats = dbManager.GetDatabaseStats();
                dashboardSummaryLabel.Text = $"📊 Dashboard Statistics:\n\n{stats}";
            }
            catch (Exception ex)
            {
                dashboardSummaryLabel.Text = $"Error loading dashboard data: {ex.Message}";
            }
        }

        private void LoadVouchersData()
{
    try
    {
        var dataTable = new DataTable();
        // Organized query with all important fields
        string sql = @"SELECT 
                      number as 'Voucher No',
                      type as 'Type',
                      date as 'Date',
                      party as 'Party',
                      amount as 'Amount',
                      description as 'Description',
                      created_by as 'Created By',
                      CASE WHEN status = 'Active' THEN '✅ Active' 
                           WHEN status = 'Cancelled' THEN '❌ Cancelled'
                           ELSE status END as 'Status'
                      FROM vouchers 
                      ORDER BY date DESC, number DESC";
        
        using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
        using (var adapter = new SQLiteDataAdapter(cmd))
        {
            adapter.Fill(dataTable);
        }
        
        vouchersGridView.DataSource = dataTable;
        
        // Format columns
        if (vouchersGridView.Columns.Contains("Amount"))
        {
            vouchersGridView.Columns["Amount"].DefaultCellStyle.Format = "N2";
            vouchersGridView.Columns["Amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
        
        if (vouchersGridView.Columns.Contains("Date"))
        {
            vouchersGridView.Columns["Date"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
        
        // Auto-size columns for better readability
        vouchersGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error loading vouchers: {ex.Message}", "Error", 
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

        private void ExportStockReport()
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "CSV Files (*.csv)|*.csv";
                saveFileDialog.Title = "Export Stock Report";
                
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var reportsTab = mainTabControl.TabPages[6];
                    var reportsGridView = reportsTab.Controls.Find("reportsGridView", true)[0] as DataGridView;
                    
                    if (reportsGridView.DataSource != null)
                    {
                        System.Text.StringBuilder sb = new System.Text.StringBuilder();
                        
                        // Headers
                        var headers = reportsGridView.Columns.Cast<DataGridViewColumn>();
                        sb.AppendLine(string.Join(",", headers.Select(column => $"\"{column.HeaderText}\"")));
                        
                        // Data
                        foreach (DataGridViewRow row in reportsGridView.Rows)
                        {
                            if (!row.IsNewRow)
                            {
                                var cells = row.Cells.Cast<DataGridViewCell>();
                                sb.AppendLine(string.Join(",", cells.Select(cell => $"\"{cell.Value}\"")));
                            }
                        }
                        
                        System.IO.File.WriteAllText(saveFileDialog.FileName, sb.ToString());
                        
                        MessageBox.Show($"Report exported successfully to: {saveFileDialog.FileName}", 
                                      "Export Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting report: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateReportControls(Panel datePanel, string reportType)
        {
            // Hide all controls first
            foreach (Control control in datePanel.Controls)
            {
                control.Visible = false;
            }

            // Show relevant controls
            switch (reportType)
            {
                case "📊 Daily Stock Report":
                    datePanel.Controls.Find("dailyPicker", true)[0].Visible = true;
                    datePanel.Controls.Find("dailyLabel", true)[0].Visible = true;
                    break;
                case "📈 Monthly Stock Report":
                    datePanel.Controls.Find("monthLabel", true)[0].Visible = true;
                    datePanel.Controls.Find("monthCombo", true)[0].Visible = true;
                    datePanel.Controls.Find("yearLabel", true)[0].Visible = true;
                    datePanel.Controls.Find("yearPicker", true)[0].Visible = true;
                    break;
                case "📅 Yearly Stock Summary":
                    datePanel.Controls.Find("yearOnlyLabel", true)[0].Visible = true;
                    datePanel.Controls.Find("yearOnlyPicker", true)[0].Visible = true;
                    break;
            }
        }

        private void GenerateStockReport(Panel datePanel, string reportType)
        {
            try
            {
                var reportsTab = mainTabControl.TabPages[6];
                var reportsGridView = reportsTab.Controls.Find("reportsGridView", true)[0] as DataGridView;
                var reportSummaryLabel = reportsTab.Controls.Find("summaryLabel", true)[0] as Label;

                List<SimpleStockReportItem> stockItems = null;
                string periodText = "";

                switch (reportType)
                {
                    case "📊 Daily Stock Report":
                        var dailyDate = (datePanel.Controls.Find("dailyPicker", true)[0] as DateTimePicker).Value;
                        stockItems = reportManager.GenerateDailyStockReport(dailyDate);
                        periodText = $"📅 Date: {dailyDate:dd-MMM-yyyy}";
                        break;
                    case "📈 Monthly Stock Report":
                        var month = (datePanel.Controls.Find("monthCombo", true)[0] as ComboBox).SelectedIndex + 1;
                        var year = (int)(datePanel.Controls.Find("yearPicker", true)[0] as NumericUpDown).Value;
                        stockItems = reportManager.GenerateMonthlyStockReport(year, month);
                        periodText = $"📅 Month: {new DateTime(year, month, 1):MMMM yyyy}";
                        break;
                    case "📅 Yearly Stock Summary":
                        var reportYear = (int)(datePanel.Controls.Find("yearOnlyPicker", true)[0] as NumericUpDown).Value;
                        stockItems = reportManager.GenerateYearlyStockReport(reportYear);
                        periodText = $"📅 Year: {reportYear}";
                        break;
                }

                if (stockItems != null && stockItems.Count > 0)
                {
                    DisplayStockReport(reportsGridView, stockItems);
                    var totalItems = stockItems.Count;
                    
                    // Calculate totals
                    decimal totalOpening = stockItems.Sum(x => x.OpeningBalance);
                    decimal totalPurchased = stockItems.Sum(x => x.Purchased);
                    decimal totalSold = stockItems.Sum(x => x.Sold);
                    decimal totalClosing = stockItems.Sum(x => x.ClosingBalance);
                    
                    reportSummaryLabel.Text = $"{reportType} | {periodText} | 📦 Items: {totalItems} | " +
                                            $"📥 Opening: {totalOpening:N2} | " +
                                            $"📈 Purchased: {totalPurchased:N2} | " +
                                            $"📤 Sold: {totalSold:N2} | " +
                                            $"📦 Closing: {totalClosing:N2}";
                    
                    MessageBox.Show($"✅ Report Generated Successfully!\n\n" +
                                  $"📊 Report: {reportType}\n" +
                                  $"{periodText}\n" +
                                  $"📦 Total Items: {totalItems}\n" +
                                  $"📥 Total Opening: {totalOpening:N2}\n" +
                                  $"📈 Total Purchased: {totalPurchased:N2}\n" +
                                  $"📤 Total Sold: {totalSold:N2}\n" +
                                  $"📦 Total Closing: {totalClosing:N2}", 
                                  "Report Generated", 
                                  MessageBoxButtons.OK, 
                                  MessageBoxIcon.Information);
                }
                else
                {
                    reportsGridView.DataSource = null;
                    reportSummaryLabel.Text = "❌ No data found for the selected period.";
                    MessageBox.Show("No stock data available for the selected period.", 
                                  "No Data", 
                                  MessageBoxButtons.OK, 
                                  MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error generating report: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayStockReport(DataGridView grid, List<SimpleStockReportItem> items)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("Sr.", typeof(int));
            dataTable.Columns.Add("Product", typeof(string));
            dataTable.Columns.Add("Code", typeof(string));
            dataTable.Columns.Add("Opening", typeof(decimal));
            dataTable.Columns.Add("Purchased", typeof(decimal));
            dataTable.Columns.Add("Sold", typeof(decimal));
            dataTable.Columns.Add("Closing", typeof(decimal));
            dataTable.Columns.Add("Unit", typeof(string));
            
            int srNo = 1;
            decimal totalOpening = 0;
            decimal totalPurchased = 0;
            decimal totalSold = 0;
            decimal totalClosing = 0;
            
            foreach (var item in items)
            {
                dataTable.Rows.Add(
                    srNo++,
                    item.Item,
                    item.Code,
                    item.OpeningBalance,
                    item.Purchased,
                    item.Sold,
                    item.ClosingBalance,
                    item.Unit
                );
                
                totalOpening += item.OpeningBalance;
                totalPurchased += item.Purchased;
                totalSold += item.Sold;
                totalClosing += item.ClosingBalance;
            }
            
            // Add total row
            if (items.Count > 0)
            {
                dataTable.Rows.Add(
                    srNo,
                    "TOTAL",
                    "",
                    totalOpening,
                    totalPurchased,
                    totalSold,
                    totalClosing,
                    ""
                );
            }
            
            grid.DataSource = dataTable;
            
            // Format columns
            foreach (DataGridViewColumn column in grid.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                
                if (column.Name == "Sr.")
                {
                    column.Width = 50;
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
                else if (column.Name == "Opening" || column.Name == "Purchased" || 
                        column.Name == "Sold" || column.Name == "Closing")
                {
                    column.DefaultCellStyle.Format = "N2";
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                else if (column.Name == "Unit")
                {
                    column.Width = 80;
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
            }
            
            // Highlight total row
            if (grid.Rows.Count > 0)
            {
                grid.Rows[grid.Rows.Count - 1].DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                grid.Rows[grid.Rows.Count - 1].DefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
            }
        }

        private void PrintStockReport()
        {
            try
            {
                var reportsTab = mainTabControl.TabPages[6];
                var reportsGridView = reportsTab.Controls.Find("reportsGridView", true)[0] as DataGridView;
                
                if (reportsGridView.DataSource == null)
                {
                    MessageBox.Show("Please generate a report first before printing!", 
                                  "No Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var reportSummaryLabel = reportsTab.Controls.Find("summaryLabel", true)[0] as Label;
                string reportTitle = reportSummaryLabel.Text;
                
                var dataTable = (reportsGridView.DataSource as DataTable);
                
                PrintHelper printHelper = new PrintHelper();
                printHelper.PrintDataTable(dataTable, reportTitle);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing report: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Stub methods for future implementation
        private void ShowReceiptAgainstSalesForm()
{
    using (var form = new Forms.Vouchers.ReceiptAgainstSalesForm())
    {
        if (form.ShowDialog() == DialogResult.OK)
        {
            var receiptsTab = mainTabControl.TabPages[4];
            var receiptsGrid = receiptsTab.Controls.Find("receiptsGrid", true)[0] as DataGridView;
            LoadReceiptsData(receiptsGrid);
        }
    }
}

        private void ShowPaymentAgainstPurchaseForm()
{
    using (var form = new Forms.Vouchers.PaymentAgainstPurchaseForm())
    {
        if (form.ShowDialog() == DialogResult.OK)
        {
            var paymentsTab = mainTabControl.TabPages[5];
            var paymentsGrid = paymentsTab.Controls.Find("paymentsGrid", true)[0] as DataGridView;
            LoadPaymentsData(paymentsGrid);
        }
    }
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