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
    } // <-- This closes the ProductManagementForm class
} // <-- This closes the namespace