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
            mainTabControl = new TabControl();
            mainTabControl.Dock = DockStyle.Fill;
            mainTabControl.Font = new Font("Segoe UI", 10);
            
            CreateDashboardTab();
            CreateVouchersTab();
            CreateProductsTab();
            CreateReportsTab();
            
            this.Controls.Add(mainTabControl);
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
            Button stockReportBtn = CreateModernButton("Stock Report", primaryBlue, new Point(560, 150));
            
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
            stockReportBtn.Click += (s, e) => { mainTabControl.SelectedIndex = 3; };
            
            dashboardTab.Controls.Add(dashboardTitle);
            dashboardTab.Controls.Add(dashboardSummaryLabel);
            dashboardTab.Controls.Add(quickSalesBtn);
            dashboardTab.Controls.Add(quickPurchaseBtn);
            dashboardTab.Controls.Add(quickEstimateBtn);
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
            
            Button refreshVouchersBtn = CreateModernButton("Refresh List", primaryBlue, new Point(20, 490));
            refreshVouchersBtn.Click += (s, e) => LoadVouchersData();
            // Add print button for vouchers
Button printVoucherBtn = CreateModernButton("🖨️ Print Selected", primaryPurple, new Point(400, 18));
printVoucherBtn.Click += (s, e) => PrintSelectedVoucher();
vouchersTab.Controls.Add(printVoucherBtn);
            vouchersTab.Controls.Add(voucherTypeCombo);
            vouchersTab.Controls.Add(createVoucherBtn);
            vouchersTab.Controls.Add(vouchersGridView);
            vouchersTab.Controls.Add(refreshVouchersBtn);
            
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

        private void CreateReportsTab()
{
    TabPage reportsTab = new TabPage("📈 Stock Reports");
    reportsTab.BackColor = lightBg;
    reportsTab.Padding = new Padding(20);

    // Title
    Label reportsTitle = new Label();
    reportsTitle.Text = "📊 Stock Movement Reports";
    reportsTitle.Font = new Font("Segoe UI", 18, FontStyle.Bold);
    reportsTitle.ForeColor = darkText;
    reportsTitle.Location = new Point(20, 20);
    reportsTitle.Size = new Size(400, 35);
    reportsTab.Controls.Add(reportsTitle);

    // Report Card Panel (New Modern Design)
    Panel reportCard = new Panel();
    reportCard.Location = new Point(20, 70);
    reportCard.Size = new Size(940, 120);
    reportCard.BackColor = Color.White;
    reportCard.BorderStyle = BorderStyle.None;
    reportCard.Padding = new Padding(15);

    // Report type selection
    Label reportTypeLabel = new Label();
    reportTypeLabel.Text = "📋 Report Type:";
    reportTypeLabel.Font = new Font("Segoe UI", 11, FontStyle.Bold);
    reportTypeLabel.ForeColor = darkText;
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
    dailyLabel.ForeColor = darkText;
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
    monthLabel.ForeColor = darkText;
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
    yearLabel.ForeColor = darkText;
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
    yearOnlyLabel.ForeColor = darkText;
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
    actionPanel.Size = new Size(140, 90);
    actionPanel.BackColor = Color.Transparent;

    // Generate Button
    Button generateReportBtn = new Button();
    generateReportBtn.Text = "📄 Generate";
    generateReportBtn.BackColor = primaryGreen;
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
    exportBtn.BackColor = primaryBlue;
    exportBtn.ForeColor = Color.White;
    exportBtn.FlatStyle = FlatStyle.Flat;
    exportBtn.FlatAppearance.BorderSize = 0;
    exportBtn.Font = new Font("Segoe UI", 10);
    exportBtn.Size = new Size(130, 35);
    exportBtn.Location = new Point(0, 45);
    exportBtn.Cursor = Cursors.Hand;
    exportBtn.Click += (s, e) => ExportStockReport();
    actionPanel.Controls.Add(exportBtn);
// Add print button next to export button in the actionPanel
Button printReportBtn = new Button();
printReportBtn.Text = "🖨️ Print";
printReportBtn.BackColor = primaryPurple;
printReportBtn.ForeColor = Color.White;
printReportBtn.FlatStyle = FlatStyle.Flat;
printReportBtn.FlatAppearance.BorderSize = 0;
printReportBtn.Font = new Font("Segoe UI", 10);
printReportBtn.Size = new Size(130, 35);
printReportBtn.Location = new Point(0, 90);
printReportBtn.Cursor = Cursors.Hand;
printReportBtn.Click += (s, e) => PrintStockReport();
actionPanel.Controls.Add(printReportBtn);

// Adjust actionPanel size
actionPanel.Size = new Size(140, 130);
    reportCard.Controls.Add(actionPanel);

    // Update controls based on report type
    reportTypeCombo.SelectedIndexChanged += (s, e) => UpdateReportControls(datePanel, reportTypeCombo.SelectedItem.ToString());

    reportsTab.Controls.Add(reportCard);

    // Statistics Panel
    Panel statsPanel = new Panel();
    statsPanel.Location = new Point(20, 210);
    statsPanel.Size = new Size(940, 80);
    statsPanel.BackColor = Color.FromArgb(52, 152, 219);
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

    // Report display area with modern styling
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
    reportsGridView.ColumnHeadersDefaultCellStyle.BackColor = primaryPurple;
    reportsGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
    reportsGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
    reportsGridView.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
    reportsGridView.EnableHeadersVisualStyles = false;
    
    // Style cells
    reportsGridView.DefaultCellStyle.Font = new Font("Segoe UI", 9);
    reportsGridView.DefaultCellStyle.ForeColor = darkText;
    reportsGridView.DefaultCellStyle.SelectionBackColor = Color.FromArgb(230, 230, 230);
    reportsGridView.DefaultCellStyle.SelectionForeColor = darkText;
    
    reportsGridView.Name = "reportsGridView";
    reportsTab.Controls.Add(reportsGridView);

    mainTabControl.TabPages.Add(reportsTab);
    
    // Initialize controls
    UpdateReportControls(datePanel, reportTypeCombo.SelectedItem.ToString());
}

// Update the DisplayStockReport method for better formatting
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

// Update the GenerateStockReport method to show better summary
private void GenerateStockReport(Panel datePanel, string reportType)
{
    try
    {
        var reportsTab = mainTabControl.TabPages[3];
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
private void PrintStockReport()
{
    try
    {
        var reportsTab = mainTabControl.TabPages[3];
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
                // REMOVED amount from SELECT to hide price
                string sql = "SELECT number, type, date, party, description FROM vouchers WHERE status = 'Active' ORDER BY date DESC";
                
                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                using (var adapter = new SQLiteDataAdapter(cmd))
                {
                    adapter.Fill(dataTable);
                }
                
                vouchersGridView.DataSource = dataTable;
                
                // No amount column to format since we removed it
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
                    var reportsTab = mainTabControl.TabPages[3];
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

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Name = "MainForm";
            this.ResumeLayout(false);
        }
    }
}