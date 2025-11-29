using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using BillingSoftware.Modules;
using BillingSoftware.Models;

namespace BillingSoftware.Forms.Reports
{
    public partial class SalesReportForm : Form
    {
        private VoucherManager voucherManager;
        private ReportManager reportManager;
        private DataGridView salesGridView;
        private DateTimePicker fromDatePicker, toDatePicker;
        private ComboBox periodCombo;
        private Button generateBtn, exportBtn;
        private Label summaryLabel;

        public SalesReportForm()
        {
            InitializeComponent();
            voucherManager = new VoucherManager();
            reportManager = new ReportManager();
            CreateSalesReportUI();
            GenerateReport();
        }

        private void CreateSalesReportUI()
        {
            this.Text = "Sales Reports";
            this.Size = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(248, 249, 250);

            // Title
            Label titleLabel = new Label();
            titleLabel.Text = "💰 Sales Reports";
            titleLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(44, 62, 80);
            titleLabel.Location = new Point(20, 20);
            titleLabel.Size = new Size(300, 30);
            this.Controls.Add(titleLabel);

            // Controls panel
            Panel controlPanel = new Panel();
            controlPanel.BackColor = Color.White;
            controlPanel.Location = new Point(20, 70);
            controlPanel.Size = new Size(950, 80);
            controlPanel.BorderStyle = BorderStyle.FixedSingle;

            // Period selection
            Label periodLabel = new Label();
            periodLabel.Text = "Period:";
            periodLabel.Font = new Font("Segoe UI", 9);
            periodLabel.Location = new Point(20, 20);
            periodLabel.Size = new Size(50, 20);
            controlPanel.Controls.Add(periodLabel);

            periodCombo = new ComboBox();
            periodCombo.Location = new Point(80, 18);
            periodCombo.Size = new Size(120, 25);
            periodCombo.Items.AddRange(new string[] { "Today", "Yesterday", "This Week", "This Month", "Last Month", "This Year", "Custom" });
            periodCombo.SelectedIndex = 3; // This Month
            periodCombo.SelectedIndexChanged += PeriodCombo_SelectedIndexChanged;
            controlPanel.Controls.Add(periodCombo);

            // From date
            Label fromLabel = new Label();
            fromLabel.Text = "From:";
            fromLabel.Font = new Font("Segoe UI", 9);
            fromLabel.Location = new Point(220, 20);
            fromLabel.Size = new Size(40, 20);
            controlPanel.Controls.Add(fromLabel);

            fromDatePicker = new DateTimePicker();
            fromDatePicker.Location = new Point(270, 18);
            fromDatePicker.Size = new Size(120, 25);
            fromDatePicker.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            controlPanel.Controls.Add(fromDatePicker);

            // To date
            Label toLabel = new Label();
            toLabel.Text = "To:";
            toLabel.Font = new Font("Segoe UI", 9);
            toLabel.Location = new Point(400, 20);
            toLabel.Size = new Size(30, 20);
            controlPanel.Controls.Add(toLabel);

            toDatePicker = new DateTimePicker();
            toDatePicker.Location = new Point(440, 18);
            toDatePicker.Size = new Size(120, 25);
            toDatePicker.Value = DateTime.Now;
            controlPanel.Controls.Add(toDatePicker);

            // Generate button
            generateBtn = CreateButton("Generate Report", Color.FromArgb(46, 204, 113), new Point(580, 15));
            generateBtn.Click += (s, e) => GenerateReport();
            controlPanel.Controls.Add(generateBtn);

            // Export button
            exportBtn = CreateButton("Export to Excel", Color.FromArgb(52, 152, 219), new Point(710, 15));
            exportBtn.Click += ExportBtn_Click;
            controlPanel.Controls.Add(exportBtn);

            // Quick stats
            Label statsLabel = new Label();
            statsLabel.Text = "Quick Stats:";
            statsLabel.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            statsLabel.Location = new Point(20, 50);
            statsLabel.Size = new Size(80, 20);
            controlPanel.Controls.Add(statsLabel);

            this.Controls.Add(controlPanel);

            // Summary label
            summaryLabel = new Label();
            summaryLabel.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            summaryLabel.ForeColor = Color.FromArgb(44, 62, 80);
            summaryLabel.Location = new Point(20, 160);
            summaryLabel.Size = new Size(950, 25);
            this.Controls.Add(summaryLabel);

            // Sales grid
            salesGridView = new DataGridView();
            salesGridView.Location = new Point(20, 190);
            salesGridView.Size = new Size(950, 400);
            salesGridView.BackgroundColor = Color.White;
            salesGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            salesGridView.ReadOnly = true;
            salesGridView.Font = new Font("Segoe UI", 9);
            salesGridView.RowHeadersVisible = false;
            salesGridView.AllowUserToAddRows = false;

            this.Controls.Add(salesGridView);
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
            btn.Size = new Size(120, 30);
            btn.Location = location;
            btn.Cursor = Cursors.Hand;
            return btn;
        }

        private void PeriodCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            var period = periodCombo.SelectedItem.ToString();
            DateTime fromDate, toDate;

            switch (period)
            {
                case "Today":
                    fromDate = DateTime.Today;
                    toDate = DateTime.Today;
                    break;
                case "Yesterday":
                    fromDate = DateTime.Today.AddDays(-1);
                    toDate = DateTime.Today.AddDays(-1);
                    break;
                case "This Week":
                    fromDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
                    toDate = DateTime.Today;
                    break;
                case "This Month":
                    fromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    toDate = DateTime.Today;
                    break;
                case "Last Month":
                    fromDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1);
                    toDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddDays(-1);
                    break;
                case "This Year":
                    fromDate = new DateTime(DateTime.Today.Year, 1, 1);
                    toDate = DateTime.Today;
                    break;
                default: // Custom
                    return;
            }

            fromDatePicker.Value = fromDate;
            toDatePicker.Value = toDate;
        }

        private void GenerateReport()
        {
            try
            {
                var fromDate = fromDatePicker.Value.Date;
                var toDate = toDatePicker.Value.Date;

                if (fromDate > toDate)
                {
                    MessageBox.Show("From date cannot be after To date!", "Validation Error", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var report = reportManager.GenerateSalesReport(fromDate, toDate);

                // Display in grid
                salesGridView.DataSource = report.Items;

                // Calculate additional statistics
                var totalSales = report.TotalAmount;
                var averageSale = report.TotalRecords > 0 ? totalSales / report.TotalRecords : 0;
                var maxSale = report.Items.Count > 0 ? report.Items.Max(i => i.Amount) : 0;

                summaryLabel.Text = $@"📈 Sales Report Summary ({fromDate:dd-MMM-yyyy} to {toDate:dd-MMM-yyyy}) | 
                                    Total Sales: ₹{totalSales:N2} | 
                                    Transactions: {report.TotalRecords} | 
                                    Average Sale: ₹{averageSale:N2} | 
                                    Largest Sale: ₹{maxSale:N2}";

                FormatSalesGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormatSalesGrid()
        {
            foreach (DataGridViewColumn column in salesGridView.Columns)
            {
                if (column.Name == "Amount")
                {
                    column.DefaultCellStyle.Format = "N2";
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    column.DefaultCellStyle.ForeColor = Color.Green;
                    column.DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                }
                else if (column.Name == "Date")
                {
                    column.DefaultCellStyle.Format = "dd-MMM-yyyy";
                }
            }

            salesGridView.AutoResizeColumns();
        }

        private void ExportBtn_Click(object sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "Excel Files (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv";
                    saveDialog.FileName = $"Sales_Report_{DateTime.Now:yyyyMMdd_HHmmss}";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        ExportToCsv(salesGridView, saveDialog.FileName);
                        MessageBox.Show($"Sales report exported successfully!", "Export Complete", 
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToCsv(DataGridView grid, string filename)
        {
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(filename))
            {
                // Write headers
                var headers = new System.Text.StringBuilder();
                foreach (DataGridViewColumn column in grid.Columns)
                {
                    headers.Append(column.HeaderText + ",");
                }
                writer.WriteLine(headers.ToString().TrimEnd(','));

                // Write data
                foreach (DataGridViewRow row in grid.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        var rowData = new System.Text.StringBuilder();
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            rowData.Append(cell.Value?.ToString() + ",");
                        }
                        writer.WriteLine(rowData.ToString().TrimEnd(','));
                    }
                }
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(300, 250);
            this.Name = "SalesReportForm";
            this.ResumeLayout(false);
        }
    }
}