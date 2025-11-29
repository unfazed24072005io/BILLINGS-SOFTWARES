using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using BillingSoftware.Modules;
using BillingSoftware.Models;

namespace BillingSoftware.Forms.Reports
{
    public partial class FinancialReportForm : Form
    {
        private VoucherManager voucherManager;
        private DataGridView financialGridView;
        private DateTimePicker fromDatePicker, toDatePicker;
        private ComboBox reportTypeCombo;
        private Button generateBtn, exportBtn, printBtn;
        private Label summaryLabel;
        private Panel chartPanel;

        public FinancialReportForm()
        {
            InitializeComponent();
            voucherManager = new VoucherManager();
            CreateFinancialReportUI();
            GenerateReport();
        }

        private void CreateFinancialReportUI()
        {
            this.Text = "Financial Reports";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(248, 249, 250);

            // Title
            Label titleLabel = new Label();
            titleLabel.Text = "📊 Financial Reports";
            titleLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(44, 62, 80);
            titleLabel.Location = new Point(20, 20);
            titleLabel.Size = new Size(300, 30);
            this.Controls.Add(titleLabel);

            // Controls panel
            Panel controlPanel = new Panel();
            controlPanel.BackColor = Color.White;
            controlPanel.Location = new Point(20, 70);
            controlPanel.Size = new Size(1050, 60);
            controlPanel.BorderStyle = BorderStyle.FixedSingle;

            // Report type
            Label typeLabel = new Label();
            typeLabel.Text = "Report Type:";
            typeLabel.Font = new Font("Segoe UI", 9);
            typeLabel.Location = new Point(20, 20);
            typeLabel.Size = new Size(80, 20);
            controlPanel.Controls.Add(typeLabel);

            reportTypeCombo = new ComboBox();
            reportTypeCombo.Location = new Point(110, 18);
            reportTypeCombo.Size = new Size(150, 25);
            reportTypeCombo.Items.AddRange(new string[] { "Income Statement", "Cash Flow", "Trial Balance", "Voucher Summary" });
            reportTypeCombo.SelectedIndex = 0;
            controlPanel.Controls.Add(reportTypeCombo);

            // From date
            Label fromLabel = new Label();
            fromLabel.Text = "From:";
            fromLabel.Font = new Font("Segoe UI", 9);
            fromLabel.Location = new Point(280, 20);
            fromLabel.Size = new Size(40, 20);
            controlPanel.Controls.Add(fromLabel);

            fromDatePicker = new DateTimePicker();
            fromDatePicker.Location = new Point(330, 18);
            fromDatePicker.Size = new Size(120, 25);
            fromDatePicker.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            controlPanel.Controls.Add(fromDatePicker);

            // To date
            Label toLabel = new Label();
            toLabel.Text = "To:";
            toLabel.Font = new Font("Segoe UI", 9);
            toLabel.Location = new Point(460, 20);
            toLabel.Size = new Size(30, 20);
            controlPanel.Controls.Add(toLabel);

            toDatePicker = new DateTimePicker();
            toDatePicker.Location = new Point(500, 18);
            toDatePicker.Size = new Size(120, 25);
            toDatePicker.Value = DateTime.Now;
            controlPanel.Controls.Add(toDatePicker);

            // Generate button
            generateBtn = CreateButton("Generate", Color.FromArgb(46, 204, 113), new Point(640, 15));
            generateBtn.Click += (s, e) => GenerateReport();
            controlPanel.Controls.Add(generateBtn);

            // Export button
            exportBtn = CreateButton("Export", Color.FromArgb(52, 152, 219), new Point(770, 15));
            exportBtn.Click += ExportBtn_Click;
            controlPanel.Controls.Add(exportBtn);

            // Print button
            printBtn = CreateButton("Print", Color.FromArgb(155, 89, 182), new Point(900, 15));
            printBtn.Click += PrintBtn_Click;
            controlPanel.Controls.Add(printBtn);

            this.Controls.Add(controlPanel);

            // Summary label
            summaryLabel = new Label();
            summaryLabel.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            summaryLabel.ForeColor = Color.FromArgb(44, 62, 80);
            summaryLabel.Location = new Point(20, 140);
            summaryLabel.Size = new Size(1050, 25);
            this.Controls.Add(summaryLabel);

            // Financial grid
            financialGridView = new DataGridView();
            financialGridView.Location = new Point(20, 170);
            financialGridView.Size = new Size(750, 450);
            financialGridView.BackgroundColor = Color.White;
            financialGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            financialGridView.ReadOnly = true;
            financialGridView.Font = new Font("Segoe UI", 9);
            financialGridView.RowHeadersVisible = false;
            financialGridView.AllowUserToAddRows = false;

            this.Controls.Add(financialGridView);

            // Chart panel (for visualizations)
            chartPanel = new Panel();
            chartPanel.Location = new Point(780, 170);
            chartPanel.Size = new Size(290, 450);
            chartPanel.BackColor = Color.White;
            chartPanel.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(chartPanel);
        }

        private Button CreateButton(string text, Color color, Point location)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.BackColor = color;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 9);
            btn.Size = new Size(80, 30);
            btn.Location = location;
            btn.Cursor = Cursors.Hand;
            return btn;
        }

        private void GenerateReport()
        {
            try
            {
                var fromDate = fromDatePicker.Value.Date;
                var toDate = toDatePicker.Value.Date;
                var reportType = reportTypeCombo.SelectedItem.ToString();

                if (fromDate > toDate)
                {
                    MessageBox.Show("From date cannot be after To date!", "Validation Error", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var vouchers = voucherManager.GetAllVouchers()
                    .Where(v => v.Date >= fromDate && v.Date <= toDate && v.Status == "Active")
                    .ToList();

                // Generate different reports based on type
                switch (reportType)
                {
                    case "Income Statement":
                        GenerateIncomeStatement(vouchers, fromDate, toDate);
                        break;
                    case "Cash Flow":
                        GenerateCashFlow(vouchers, fromDate, toDate);
                        break;
                    case "Trial Balance":
                        GenerateTrialBalance(vouchers, fromDate, toDate);
                        break;
                    case "Voucher Summary":
                        GenerateVoucherSummary(vouchers, fromDate, toDate);
                        break;
                }

                UpdateChartPanel(vouchers, reportType);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GenerateIncomeStatement(System.Collections.Generic.List<Voucher> vouchers, DateTime fromDate, DateTime toDate)
        {
            var income = vouchers.Where(v => v.Type == "Sales").Sum(v => v.Amount);
            var expenses = vouchers.Where(v => v.Type == "Payment").Sum(v => v.Amount);
            var netIncome = income - expenses;

            // Create summary data
            var reportData = new[]
            {
                new { Category = "REVENUE", Amount = income, Type = "" },
                new { Category = "Sales Income", Amount = income, Type = "Revenue" },
                new { Category = "TOTAL REVENUE", Amount = income, Type = "Total" },
                new { Category = "", Amount = 0m, Type = "Spacer" },
                new { Category = "EXPENSES", Amount = expenses, Type = "" },
                new { Category = "Payments Made", Amount = expenses, Type = "Expense" },
                new { Category = "TOTAL EXPENSES", Amount = expenses, Type = "Total" },
                new { Category = "", Amount = 0m, Type = "Spacer" },
                new { Category = "NET INCOME", Amount = netIncome, Type = "Net" }
            };

            financialGridView.DataSource = reportData.ToList();
            summaryLabel.Text = $"💰 Income Statement ({fromDate:dd-MMM-yyyy} to {toDate:dd-MMM-yyyy}) | Net Income: ₹{netIncome:N2}";
            FormatFinancialGrid();
        }

        private void GenerateCashFlow(System.Collections.Generic.List<Voucher> vouchers, DateTime fromDate, DateTime toDate)
        {
            var cashIn = vouchers.Where(v => v.Type == "Receipt").Sum(v => v.Amount);
            var cashOut = vouchers.Where(v => v.Type == "Payment").Sum(v => v.Amount);
            var netCashFlow = cashIn - cashOut;

            var reportData = new[]
            {
                new { Category = "CASH INFLOW", Amount = cashIn, Type = "" },
                new { Category = "Receipts", Amount = cashIn, Type = "Inflow" },
                new { Category = "TOTAL INFLOW", Amount = cashIn, Type = "Total" },
                new { Category = "", Amount = 0m, Type = "Spacer" },
                new { Category = "CASH OUTFLOW", Amount = cashOut, Type = "" },
                new { Category = "Payments", Amount = cashOut, Type = "Outflow" },
                new { Category = "TOTAL OUTFLOW", Amount = cashOut, Type = "Total" },
                new { Category = "", Amount = 0m, Type = "Spacer" },
                new { Category = "NET CASH FLOW", Amount = netCashFlow, Type = "Net" }
            };

            financialGridView.DataSource = reportData.ToList();
            summaryLabel.Text = $"💸 Cash Flow Statement ({fromDate:dd-MMM-yyyy} to {toDate:dd-MMM-yyyy}) | Net Cash Flow: ₹{netCashFlow:N2}";
            FormatFinancialGrid();
        }

        private void GenerateVoucherSummary(System.Collections.Generic.List<Voucher> vouchers, DateTime fromDate, DateTime toDate)
        {
            var summary = vouchers.GroupBy(v => v.Type)
                                 .Select(g => new
                                 {
                                     VoucherType = g.Key,
                                     Count = g.Count(),
                                     TotalAmount = g.Sum(v => v.Amount),
                                     AverageAmount = g.Average(v => v.Amount)
                                 })
                                 .ToList();

            financialGridView.DataSource = summary;
            summaryLabel.Text = $"📋 Voucher Summary ({fromDate:dd-MMM-yyyy} to {toDate:dd-MMM-yyyy}) | Total Transactions: {vouchers.Count}";
            FormatFinancialGrid();
        }

        private void GenerateTrialBalance(System.Collections.Generic.List<Voucher> vouchers, DateTime fromDate, DateTime toDate)
        {
            // Simplified trial balance - in real app, you'd have proper account heads
            var trialBalance = vouchers.GroupBy(v => v.Type)
                                      .Select(g => new
                                      {
                                          Account = g.Key + " Account",
                                          Debit = g.Key == "Payment" || g.Key == "Journal" ? g.Sum(v => v.Amount) : 0,
                                          Credit = g.Key == "Sales" || g.Key == "Receipt" ? g.Sum(v => v.Amount) : 0
                                      })
                                      .ToList();

            financialGridView.DataSource = trialBalance;
            summaryLabel.Text = $"⚖️ Trial Balance ({fromDate:dd-MMM-yyyy} to {toDate:dd-MMM-yyyy})";
            FormatFinancialGrid();
        }

        private void FormatFinancialGrid()
        {
            foreach (DataGridViewColumn column in financialGridView.Columns)
            {
                if (column.Name == "Amount" || column.Name == "TotalAmount" || column.Name == "AverageAmount" || 
                    column.Name == "Debit" || column.Name == "Credit")
                {
                    column.DefaultCellStyle.Format = "N2";
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    
                    // Color coding
                    if (column.Name == "Amount" || column.Name == "TotalAmount")
                    {
                        column.DefaultCellStyle.ForeColor = Color.Green;
                    }
                }
            }

            financialGridView.AutoResizeColumns();
        }

        private void UpdateChartPanel(System.Collections.Generic.List<Voucher> vouchers, string reportType)
        {
            chartPanel.Controls.Clear();

            // Simple text-based chart (in real app, use proper chart control)
            var chartLabel = new Label();
            chartLabel.Text = $"📈 {reportType}\n\n";
            chartLabel.Font = new Font("Segoe UI", 10);
            chartLabel.Location = new Point(10, 10);
            chartLabel.Size = new Size(270, 400);
            chartLabel.TextAlign = ContentAlignment.TopLeft;

            if (reportType == "Income Statement")
            {
                var income = vouchers.Where(v => v.Type == "Sales").Sum(v => v.Amount);
                var expenses = vouchers.Where(v => v.Type == "Payment").Sum(v => v.Amount);
                
                chartLabel.Text += $"Income: ₹{income:N2}\n";
                chartLabel.Text += $"Expenses: ₹{expenses:N2}\n";
                chartLabel.Text += $"Net: ₹{income - expenses:N2}";
            }
            else if (reportType == "Voucher Summary")
            {
                foreach (var type in vouchers.GroupBy(v => v.Type))
                {
                    chartLabel.Text += $"{type.Key}: {type.Count()} transactions\n";
                }
            }

            chartPanel.Controls.Add(chartLabel);
        }

        private void ExportBtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Export functionality would be implemented here!", "Export", 
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void PrintBtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Print functionality would be implemented here!", "Print", 
                          MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(300, 250);
            this.Name = "FinancialReportForm";
            this.ResumeLayout(false);
        }
    }
}