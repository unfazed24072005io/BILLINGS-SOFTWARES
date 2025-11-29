using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SQLite;
using BillingSoftware.Modules;
using BillingSoftware.Models;

namespace BillingSoftware.Forms
{
    public partial class ProductManagementForm : Form
    {
        private DatabaseManager dbManager;
        private DataGridView productsGrid;
        private Button refreshBtn, addProductBtn;

        public ProductManagementForm()
        {
            InitializeComponent();
            dbManager = new DatabaseManager();
            CreateProductManagementUI();
            LoadProducts();
        }

        private void CreateProductManagementUI()
        {
            this.Text = "Product Management";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(248, 249, 250);

            // Title
            Label titleLabel = new Label();
            titleLabel.Text = "📦 Product Management";
            titleLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(44, 62, 80);
            titleLabel.Location = new Point(20, 20);
            titleLabel.Size = new Size(300, 30);
            this.Controls.Add(titleLabel);

            // Products Grid
            productsGrid = new DataGridView();
            productsGrid.Location = new Point(20, 70);
            productsGrid.Size = new Size(750, 350);
            productsGrid.BackgroundColor = Color.White;
            productsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            productsGrid.ReadOnly = true;
            productsGrid.RowHeadersVisible = false;
            productsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.Controls.Add(productsGrid);

            // Buttons
            refreshBtn = CreateButton("🔄 Refresh", Color.FromArgb(52, 152, 219), new Point(20, 440));
            refreshBtn.Click += (s, e) => LoadProducts();
            
            addProductBtn = CreateButton("➕ Add Product", Color.FromArgb(46, 204, 113), new Point(150, 440));
            addProductBtn.Click += AddProductBtn_Click;

            this.Controls.Add(refreshBtn);
            this.Controls.Add(addProductBtn);
        }

        private Button CreateButton(string text, Color color, Point location)
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

        private void LoadProducts()
        {
            var products = dbManager.GetAllProducts();
            productsGrid.DataSource = products;
            
            // Format the grid
            if (productsGrid.Columns.Contains("price"))
            {
                productsGrid.Columns["price"].DefaultCellStyle.Format = "N2";
                productsGrid.Columns["price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            if (productsGrid.Columns.Contains("stock"))
            {
                productsGrid.Columns["stock"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }

        private void AddProductBtn_Click(object sender, EventArgs e)
        {
            using (var addForm = new AddProductForm())
            {
                if (addForm.ShowDialog() == DialogResult.OK)
                {
                    LoadProducts(); // Refresh the list
                }
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(300, 250);
            this.Name = "ProductManagementForm";
            this.ResumeLayout(false);
        }
    }

    public class AddProductForm : Form
    {
        private DatabaseManager dbManager;
        private TextBox nameTxt, codeTxt, priceTxt, stockTxt, minStockTxt;
        private ComboBox unitCombo;
        private Button saveBtn, cancelBtn;

        public AddProductForm()
        {
            InitializeComponent();
            dbManager = new DatabaseManager();
            CreateAddProductUI();
        }

        private void CreateAddProductUI()
        {
            this.Text = "Add New Product";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterParent;

            // Product Name
            CreateLabel("Product Name:", 20, 30);
            nameTxt = CreateTextBox(150, 30, 200);

            // Product Code
            CreateLabel("Product Code:", 20, 70);
            codeTxt = CreateTextBox(150, 70, 150);

            // Price
            CreateLabel("Price:", 20, 110);
            priceTxt = CreateTextBox(150, 110, 100);
            priceTxt.KeyPress += NumericTextBox_KeyPress;

            // Stock
            CreateLabel("Stock:", 20, 150);
            stockTxt = CreateTextBox(150, 150, 100);
            stockTxt.KeyPress += NumericTextBox_KeyPress;
            stockTxt.Text = "0";

            // Min Stock
            CreateLabel("Min Stock:", 20, 190);
            minStockTxt = CreateTextBox(150, 190, 100);
            minStockTxt.KeyPress += NumericTextBox_KeyPress;
            minStockTxt.Text = "10";

            // Unit
            CreateLabel("Unit:", 20, 230);
            unitCombo = new ComboBox();
            unitCombo.Location = new Point(150, 230);
            unitCombo.Size = new Size(100, 25);
            unitCombo.Items.AddRange(new string[] { "PCS", "KG", "LTR", "M", "BOX" });
            unitCombo.SelectedIndex = 0;
            this.Controls.Add(unitCombo);

            // Buttons
            saveBtn = CreateButton("Save", Color.FromArgb(46, 204, 113), new Point(100, 270));
            saveBtn.Click += SaveBtn_Click;

            cancelBtn = CreateButton("Cancel", Color.FromArgb(149, 165, 166), new Point(220, 270));
            cancelBtn.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.Add(saveBtn);
            this.Controls.Add(cancelBtn);
        }

        private void CreateLabel(string text, int x, int y)
        {
            var label = new Label { Text = text, Location = new Point(x, y), Size = new Size(120, 20) };
            this.Controls.Add(label);
        }

        private TextBox CreateTextBox(int x, int y, int width)
        {
            var txt = new TextBox { Location = new Point(x, y), Size = new Size(width, 25) };
            this.Controls.Add(txt);
            return txt;
        }

        private Button CreateButton(string text, Color color, Point location)
        {
            return new Button
            {
                Text = text,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(80, 30),
                Location = location,
                Cursor = Cursors.Hand
            };
        }

        private void NumericTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                e.Handled = true;
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameTxt.Text))
            {
                MessageBox.Show("Please enter product name!", "Validation Error");
                return;
            }

            if (!decimal.TryParse(priceTxt.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Please enter valid price!", "Validation Error");
                return;
            }

            if (!decimal.TryParse(stockTxt.Text, out decimal stock) || stock < 0)
            {
                MessageBox.Show("Please enter valid stock quantity!", "Validation Error");
                return;
            }

            if (!decimal.TryParse(minStockTxt.Text, out decimal minStock) || minStock < 0)
            {
                MessageBox.Show("Please enter valid minimum stock!", "Validation Error");
                return;
            }

            try
            {
                string sql = @"INSERT INTO products (name, code, price, stock, unit, min_stock) 
                              VALUES (@name, @code, @price, @stock, @unit, @minStock)";

                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@name", nameTxt.Text.Trim());
                    cmd.Parameters.AddWithValue("@code", string.IsNullOrWhiteSpace(codeTxt.Text) ? 
                        GenerateCode(nameTxt.Text) : codeTxt.Text.Trim());
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@stock", stock);
                    cmd.Parameters.AddWithValue("@unit", unitCombo.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@minStock", minStock);

                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        MessageBox.Show("Product added successfully!", "Success");
                        this.DialogResult = DialogResult.OK;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding product: {ex.Message}", "Error");
            }
        }

        private string GenerateCode(string productName)
        {
            string code = productName.Replace(" ", "").ToUpper();
            if (code.Length > 6) code = code.Substring(0, 6);
            Random random = new Random();
            return code + random.Next(100, 999);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(300, 250);
            this.Name = "AddProductForm";
            this.ResumeLayout(false);
        }
    }
}