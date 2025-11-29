using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using BillingSoftware.Modules;
using BillingSoftware.Models;

namespace BillingSoftware.Forms.Reports
{
    public partial class StockReportForm : Form
    {
        private ReportManager reportManager;
        private DataGridView stockGridView;
        private ComboBox reportTypeCombo;
        private Button generateBtn, exportBtn;
        private Label summaryLabel;

        public StockReportForm()
        {
            InitializeComponent();
            reportManager = new ReportManager();
            CreateStockReportUI();
            GenerateReport();
        }

        private void CreateStockReportUI()
        {
            this.Text = "Stock Reports";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(248, 249, 250);

            // Title
            Label titleLabel = new Label();
            titleLabel.Text = "📦 Stock Reports";
            titleLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(44, 62, 80);
            titleLabel.Location = new Point(20, 20);
            titleLabel.Size = new Size(300, 30);
            this.Controls.Add(titleLabel);

            // Controls panel
            Panel controlPanel = new Panel();
            controlPanel.BackColor = Color.White;
            controlPanel.Location = new Point(20, 70);
            controlPanel.Size = new Size(850, 60);
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
            reportTypeCombo.Items.AddRange(new string[] { "Current Stock", "Low Stock Items", "Stock Valuation" });
            reportTypeCombo.SelectedIndex = 0;
            controlPanel.Controls.Add(reportTypeCombo);

            // Generate button
            generateBtn = CreateButton("Generate Report", Color.FromArgb(46, 204, 113), new Point(280, 15));
            generateBtn.Click += (s, e) => GenerateReport();
            controlPanel.Controls.Add(generateBtn);

            // Export button
            exportBtn = CreateButton("Export to Excel", Color.FromArgb(52, 152, 219), new Point(410, 15));
            exportBtn.Click += ExportBtn_Click;
            controlPanel.Controls.Add(exportBtn);

            this.Controls.Add(controlPanel);

            // Summary label
            summaryLabel = new Label();
            summaryLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            summaryLabel.ForeColor = Color.FromArgb(44, 62, 80);
            summaryLabel.Location = new Point(20, 140);
            summaryLabel.Size = new Size(850, 25);
            this.Controls.Add(summaryLabel);

            // Stock grid
            stockGridView = new DataGridView();
            stockGridView.Location = new Point(20, 170);
            stockGridView.Size = new Size(850, 350);
            stockGridView.BackgroundColor = Color.White;
            stockGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            stockGridView.ReadOnly = true;
            stockGridView.Font = new Font("Segoe UI", 9);
            stockGridView.RowHeadersVisible = false;

            this.Controls.Add(stockGridView);
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

        private void GenerateReport()
        {
            try
            {
                var reportType = reportTypeCombo.SelectedItem.ToString();
                var report = reportManager.GenerateStockReport(reportType);

                // Configure grid based on report type
                stockGridView.Columns.Clear();

                if (reportType == "Low Stock Items")
                {
                    var lowStockProducts = reportManager.GetLowStockProducts();
                    stockGridView.DataSource = lowStockProducts;

                    summaryLabel.Text = $"🚨 Low Stock Alert: {lowStockProducts.Count} items below minimum stock level";
                    summaryLabel.ForeColor = Color.Red;
                }
                else
                {
                    stockGridView.DataSource = report.Items;

                    if (reportType == "Current Stock")
                    {
                        summaryLabel.Text = $"📊 Current Stock Summary: {report.TotalRecords} items | Total Value: ₹{report.TotalAmount:N2}";
                        summaryLabel.ForeColor = Color.FromArgb(44, 62, 80);
                    }
                    else if (reportType == "Stock Valuation")
                    {
                        summaryLabel.Text = $"💰 Stock Valuation: Total Inventory Value: ₹{report.TotalAmount:N2}";
                        summaryLabel.ForeColor = Color.Green;
                    }
                }

                // Format columns
                FormatStockGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormatStockGrid()
        {
            foreach (DataGridViewColumn column in stockGridView.Columns)
            {
                if (column.Name == "Price" || column.Name == "Amount" || column.Name.EndsWith("Price"))
                {
                    column.DefaultCellStyle.Format = "N2";
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                else if (column.Name == "Stock" || column.Name == "Quantity" || column.Name.EndsWith("Stock"))
                {
                    column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    column.DefaultCellStyle.Format = "N3";
                }
            }

            // Auto-resize columns
            stockGridView.AutoResizeColumns();
        }

        private void ExportBtn_Click(object sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "Excel Files (*.xlsx)|*.xlsx|CSV Files (*.csv)|*.csv";
                    saveDialog.FileName = $"Stock_Report_{DateTime.Now:yyyyMMdd_HHmmss}";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Simple export to CSV
                        ExportToCsv(stockGridView, saveDialog.FileName);
                        MessageBox.Show($"Report exported successfully to:\n{saveDialog.FileName}", 
                                      "Export Complete", 
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
            this.Name = "StockReportForm";
            this.ResumeLayout(false);
        }
    }
}