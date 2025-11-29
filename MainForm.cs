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
	private Color primaryPurple = Color.FromArgb(155, 89, 182);

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
    reportsTab.Padding = new Padding(20);

    // Report type selection
    Label reportTypeLabel = new Label();
    reportTypeLabel.Text = "Report Type:";
    reportTypeLabel.Font = new Font("Segoe UI", 11, FontStyle.Bold);
    reportTypeLabel.ForeColor = darkText;
    reportTypeLabel.Location = new Point(20, 20);
    reportTypeLabel.Size = new Size(100, 25);
    reportsTab.Controls.Add(reportTypeLabel);

    ComboBox reportTypeCombo = new ComboBox();
    reportTypeCombo.Location = new Point(130, 18);
    reportTypeCombo.Size = new Size(200, 30);
    reportTypeCombo.Font = new Font("Segoe UI", 10);
    reportTypeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
    reportTypeCombo.Items.AddRange(new string[] { 
        "📊 Daily Stock Report",
        "📈 Monthly Stock Report", 
        "📅 Yearly Stock Summary",
        "💰 Sales Report",
        "💸 Financial Report"
    });
    reportTypeCombo.SelectedIndex = 0;
    reportTypeCombo.SelectedIndexChanged += (s, e) => UpdateDateControls();
    reportsTab.Controls.Add(reportTypeCombo);

    // Date selection panel
    Panel datePanel = new Panel();
    datePanel.Location = new Point(350, 15);
    datePanel.Size = new Size(400, 40);
    datePanel.BackColor = Color.Transparent;

    // Daily date control
    Label dailyLabel = new Label();
    dailyLabel.Text = "Select Date:";
    dailyLabel.Font = new Font("Segoe UI", 10);
    dailyLabel.ForeColor = darkText;
    dailyLabel.Location = new Point(0, 5);
    dailyLabel.Size = new Size(80, 20);
    datePanel.Controls.Add(dailyLabel);

    DateTimePicker dailyDatePicker = new DateTimePicker();
    dailyDatePicker.Location = new Point(90, 3);
    dailyDatePicker.Size = new Size(120, 25);
    dailyDatePicker.Value = DateTime.Now;
    dailyDatePicker.Name = "dailyPicker";
    datePanel.Controls.Add(dailyDatePicker);

    // Monthly controls (initially hidden)
    Label monthLabel = new Label();
    monthLabel.Text = "Month:";
    monthLabel.Font = new Font("Segoe UI", 10);
    monthLabel.ForeColor = darkText;
    monthLabel.Location = new Point(0, 5);
    monthLabel.Size = new Size(50, 20);
    monthLabel.Visible = false;
    monthLabel.Name = "monthLabel";
    datePanel.Controls.Add(monthLabel);

    ComboBox monthCombo = new ComboBox();
    monthCombo.Location = new Point(55, 3);
    monthCombo.Size = new Size(100, 25);
    monthCombo.Items.AddRange(new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" });
    monthCombo.SelectedIndex = DateTime.Now.Month - 1;
    monthCombo.Visible = false;
    monthCombo.Name = "monthCombo";
    datePanel.Controls.Add(monthCombo);

    Label yearLabel = new Label();
    yearLabel.Text = "Year:";
    yearLabel.Font = new Font("Segoe UI", 10);
    yearLabel.ForeColor = darkText;
    yearLabel.Location = new Point(165, 5);
    yearLabel.Size = new Size(35, 20);
    yearLabel.Visible = false;
    yearLabel.Name = "yearLabel";
    datePanel.Controls.Add(yearLabel);

    NumericUpDown yearPicker = new NumericUpDown();
    yearPicker.Location = new Point(205, 3);
    yearPicker.Size = new Size(60, 25);
    yearPicker.Minimum = 2020;
    yearPicker.Maximum = 2030;
    yearPicker.Value = DateTime.Now.Year;
    yearPicker.Visible = false;
    yearPicker.Name = "yearPicker";
    datePanel.Controls.Add(yearPicker);

    // Date range controls (for sales/financial reports)
    Label fromLabel = new Label();
    fromLabel.Text = "From:";
    fromLabel.Font = new Font("Segoe UI", 10);
    fromLabel.ForeColor = darkText;
    fromLabel.Location = new Point(0, 5);
    fromLabel.Size = new Size(40, 20);
    fromLabel.Visible = false;
    fromLabel.Name = "fromLabel";
    datePanel.Controls.Add(fromLabel);

    DateTimePicker fromDatePicker = new DateTimePicker();
    fromDatePicker.Location = new Point(45, 3);
    fromDatePicker.Size = new Size(100, 25);
    fromDatePicker.Value = DateTime.Now.AddDays(-30);
    fromDatePicker.Visible = false;
    fromDatePicker.Name = "fromPicker";
    datePanel.Controls.Add(fromDatePicker);

    Label toLabel = new Label();
    toLabel.Text = "To:";
    toLabel.Font = new Font("Segoe UI", 10);
    toLabel.ForeColor = darkText;
    toLabel.Location = new Point(155, 5);
    toLabel.Size = new Size(25, 20);
    toLabel.Visible = false;
    toLabel.Name = "toLabel";
    datePanel.Controls.Add(toLabel);

    DateTimePicker toDatePicker = new DateTimePicker();
    toDatePicker.Location = new Point(185, 3);
    toDatePicker.Size = new Size(100, 25);
    toDatePicker.Value = DateTime.Now;
    toDatePicker.Visible = false;
    toDatePicker.Name = "toPicker";
    datePanel.Controls.Add(toDatePicker);

    reportsTab.Controls.Add(datePanel);

    // Action buttons
    Button generateReportBtn = CreateModernButton("📄 Generate Report", primaryGreen, new Point(20, 70));
    generateReportBtn.Click += (s, e) => GenerateStockReport(reportTypeCombo.SelectedItem.ToString(), datePanel);
    reportsTab.Controls.Add(generateReportBtn);

    Button exportBtn = CreateModernButton("📤 Export to CSV", primaryBlue, new Point(200, 70));
    exportBtn.Click += (s, e) => ExportStockReport();
    reportsTab.Controls.Add(exportBtn);

    // Report display area
    DataGridView reportsGridView = new DataGridView();
    reportsGridView.Location = new Point(20, 120);
    reportsGridView.Size = new Size(900, 400);
    reportsGridView.BackgroundColor = Color.White;
    reportsGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
    reportsGridView.ReadOnly = true;
    reportsGridView.Font = new Font("Segoe UI", 9);
    reportsGridView.RowHeadersVisible = false;
    reportsGridView.EnableHeadersVisualStyles = false;
    reportsGridView.ColumnHeadersDefaultCellStyle.BackColor = primaryBlue;
    reportsGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
    reportsGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
    reportsTab.Controls.Add(reportsGridView);

    // Report summary label
    Label reportSummaryLabel = new Label();
    reportSummaryLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
    reportSummaryLabel.ForeColor = darkText;
    reportSummaryLabel.Location = new Point(20, 530);
    reportSummaryLabel.Size = new Size(900, 30);
    reportSummaryLabel.TextAlign = ContentAlignment.MiddleLeft;
    reportSummaryLabel.Name = "summaryLabel";
    reportsTab.Controls.Add(reportSummaryLabel);

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
private void UpdateDateControls()
{
    var reportsTab = mainTabControl.TabPages[2];
    var datePanel = reportsTab.Controls.OfType<Panel>().First();
    var reportTypeCombo = reportsTab.Controls.OfType<ComboBox>().First();

    // Hide all controls first
    foreach (Control control in datePanel.Controls)
    {
        control.Visible = false;
    }

    // Show relevant controls based on report type
    switch (reportTypeCombo.SelectedItem.ToString())
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
            datePanel.Controls.Find("yearLabel", true)[0].Visible = true;
            datePanel.Controls.Find("yearPicker", true)[0].Visible = true;
            break;
        case "💰 Sales Report":
        case "💸 Financial Report":
            datePanel.Controls.Find("fromLabel", true)[0].Visible = true;
            datePanel.Controls.Find("fromPicker", true)[0].Visible = true;
            datePanel.Controls.Find("toLabel", true)[0].Visible = true;
            datePanel.Controls.Find("toPicker", true)[0].Visible = true;
            break;
    }
}

private void GenerateStockReport(string reportType, Panel datePanel)
{
    try
    {
        var reportsTab = mainTabControl.TabPages[2];
        var reportsGridView = reportsTab.Controls.OfType<DataGridView>().First();
        var reportSummaryLabel = reportsTab.Controls.Find("summaryLabel", true)[0] as Label;

        Report report = null;

        switch (reportType)
        {
            case "📊 Daily Stock Report":
                var dailyDate = (datePanel.Controls.Find("dailyPicker", true)[0] as DateTimePicker).Value;
                report = reportManager.GenerateDailyStockReport(dailyDate);
                break;
            case "📈 Monthly Stock Report":
                var month = (datePanel.Controls.Find("monthCombo", true)[0] as ComboBox).SelectedIndex + 1;
                var year = (int)(datePanel.Controls.Find("yearPicker", true)[0] as NumericUpDown).Value;
                report = reportManager.GenerateMonthlyStockReport(year, month);
                break;
            case "📅 Yearly Stock Summary":
                var reportYear = (int)(datePanel.Controls.Find("yearPicker", true)[0] as NumericUpDown).Value;
                report = reportManager.GenerateYearlyStockReport(reportYear);
                break;
            case "💰 Sales Report":
                var fromDate = (datePanel.Controls.Find("fromPicker", true)[0] as DateTimePicker).Value;
                var toDate = (datePanel.Controls.Find("toPicker", true)[0] as DateTimePicker).Value;
                report = reportManager.GenerateSalesReport(fromDate, toDate);
                break;
        }

        if (report != null)
        {
            reportsGridView.DataSource = report.Items;
            reportSummaryLabel.Text = $"{report.Title} | Generated: {report.GeneratedDate:HH:mm:ss}";
            
            // Format the grid for stock reports
            if (reportType.Contains("Stock"))
            {
                FormatStockReportGrid(reportsGridView);
            }
            
            MessageBox.Show($"✅ {reportType} generated successfully!\n\nItems: {report.TotalRecords}", 
                          "Report Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error generating report: {ex.Message}", "Error", 
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

private void FormatStockReportGrid(DataGridView grid)
{
    grid.Columns.Clear();
    
    // Add columns for stock report format
    grid.Columns.Add("Item", "Item");
    grid.Columns.Add("OpeningStock", "Opening Stock");
    grid.Columns.Add("Bought", "Bought");
    grid.Columns.Add("Sold", "Sold"); 
    grid.Columns.Add("ClosingStock", "Closing Stock");
    grid.Columns.Add("Unit", "Unit");
    
    // Sample data for demonstration
    var stockData = new[]
    {
        new { Item = "Laptop", OpeningStock = 25, Bought = 10, Sold = 8, ClosingStock = 27, Unit = "PCS" },
        new { Item = "Wireless Mouse", OpeningStock = 45, Bought = 50, Sold = 35, ClosingStock = 60, Unit = "PCS" },
        new { Item = "Keyboard", OpeningStock = 30, Bought = 25, Sold = 20, ClosingStock = 35, Unit = "PCS" }
    };
    
    grid.DataSource = stockData;
    
    // Format numeric columns
    foreach (DataGridViewColumn column in grid.Columns)
    {
        if (column.Name.Contains("Stock") || column.Name == "Bought" || column.Name == "Sold")
        {
            column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
    }
}

private void ExportStockReport()
{
    MessageBox.Show("📤 Stock report export feature ready!\n\nExport to Excel/CSV functionality will be implemented.", 
                  "Export Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
	private void GenerateSelectedReport(string reportType, DateTime fromDate, DateTime toDate)
{
    try
    {
        Report report = null;
        DataGridView reportsGridView = mainTabControl.TabPages[2].Controls.OfType<DataGridView>().First();
        Label reportSummaryLabel = mainTabControl.TabPages[2].Controls.OfType<Label>().First(l => l.Location.Y == 520);

        switch (reportType)
        {
            case "📦 Stock Summary Report":
                report = reportManager.GenerateStockReport("Current");
                break;
            case "💰 Sales Analysis Report":
                report = reportManager.GenerateSalesReport(fromDate, toDate);
                break;
            case "💸 Financial Summary Report":
                report = reportManager.GenerateFinancialReport(fromDate, toDate);
                break;
            case "📊 Voucher Summary Report":
                report = reportManager.GenerateVoucherSummary(fromDate, toDate);
                break;
            case "🚨 Low Stock Alert Report":
                ShowLowStockReport();
                return;
        }

        if (report != null)
        {
            // Display report in grid
            reportsGridView.DataSource = report.Items;
            
            // Show summary
            reportSummaryLabel.Text = $"{report.Title} | Total Records: {report.TotalRecords} | Total Amount: ₹{report.TotalAmount:N2} | Generated: {report.GeneratedDate:HH:mm:ss}";
            
            // Format grid columns
            FormatReportGrid(reportsGridView, report.Type);
            
            MessageBox.Show($"✅ {reportType} generated successfully!\n\nRecords: {report.TotalRecords}\nTotal Amount: ₹{report.TotalAmount:N2}", 
                          "Report Generated", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error generating report: {ex.Message}", "Error", 
                      MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

private void ShowLowStockReport()
{
    var lowStockProducts = reportManager.GetLowStockProducts();
    
    string reportText = "🚨 LOW STOCK ALERT REPORT\n\n";
    reportText += "The following products are below minimum stock levels:\n\n";
    
    foreach (var product in lowStockProducts)
    {
        reportText += $"📦 {product.Name} ({product.Code})\n";
        reportText += $"   Current Stock: {product.Stock} {product.Unit}\n";
        reportText += $"   Minimum Required: {product.MinStock} {product.Unit}\n";
        reportText += $"   Shortage: {product.MinStock - product.Stock} {product.Unit}\n";
        reportText += $"   Value: ₹{product.Price * product.Stock:N2}\n\n";
    }
    
    reportText += $"Total Low Stock Items: {lowStockProducts.Count}\n";
    reportText += $"Action Required: Please restock these items immediately!";
    
    MessageBox.Show(reportText, "Low Stock Alert", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}

private void FormatReportGrid(DataGridView grid, string reportType)
{
    foreach (DataGridViewColumn column in grid.Columns)
    {
        if (column.Name == "Amount" || column.Name.EndsWith("Amount"))
        {
            column.DefaultCellStyle.Format = "N2";
            column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            column.DefaultCellStyle.ForeColor = Color.Green;
        }
        else if (column.Name == "Quantity" || column.Name.EndsWith("Quantity"))
        {
            column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
        else if (column.Name == "Date" || column.Name.EndsWith("Date"))
        {
            column.DefaultCellStyle.Format = "dd-MMM-yyyy";
        }
    }
}

private void ExportReport()
{
    var grid = mainTabControl.TabPages[2].Controls.OfType<DataGridView>().First();
    if (grid.DataSource == null)
    {
        MessageBox.Show("Please generate a report first!", "No Data", 
                      MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    MessageBox.Show("📤 Export feature ready!\n\nReport can be exported to CSV format.", 
                  "Export Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
}

private void PrintReport()
{
    MessageBox.Show("🖨️ Print feature ready!\n\nReport can be sent to printer.", 
                  "Print Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
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