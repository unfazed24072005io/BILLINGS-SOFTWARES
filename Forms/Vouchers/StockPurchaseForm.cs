using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Data.SQLite; // ADD THIS MISSING USING DIRECTIVE
using BillingSoftware.Modules;
using BillingSoftware.Models;

namespace BillingSoftware.Forms.Vouchers
{
    public partial class StockPurchaseForm : Form
    {
        private VoucherManager voucherManager;
        private DatabaseManager dbManager;
        
        private TextBox voucherNumberTxt, supplierTxt, totalAmountTxt;
        private DateTimePicker datePicker;
        private DataGridView itemsGrid;
        private Button saveBtn, clearBtn, addItemBtn, removeItemBtn;
        private List<StockPurchaseItem> purchaseItems;

        public StockPurchaseForm()
        {
            InitializeComponent();
            voucherManager = new VoucherManager();
            dbManager = new DatabaseManager();
            purchaseItems = new List<StockPurchaseItem>();
            CreateStockPurchaseFormUI();
        }

        private void CreateStockPurchaseFormUI()
        {
            this.Text = "Stock Purchase Voucher";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(248, 249, 250);

            // Title
            Label titleLabel = new Label();
            titleLabel.Text = "📦 Stock Purchase Voucher";
            titleLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(44, 62, 80);
            titleLabel.Location = new Point(20, 20);
            titleLabel.Size = new Size(300, 30);
            this.Controls.Add(titleLabel);

            // Main container
            GroupBox mainGroup = new GroupBox();
            mainGroup.Text = "Purchase Details";
            mainGroup.Font = new Font("Segoe UI", 10);
            mainGroup.Location = new Point(20, 70);
            mainGroup.Size = new Size(750, 450);
            mainGroup.BackColor = Color.White;

            // Voucher Number
            CreateLabel("Purchase Number:", 20, 40, mainGroup);
            voucherNumberTxt = CreateTextBox(150, 40, 200, mainGroup);
            voucherNumberTxt.Text = voucherManager.GenerateVoucherNumber("Stock Purchase");
            voucherNumberTxt.ReadOnly = true;
            voucherNumberTxt.BackColor = Color.FromArgb(240, 240, 240);

            // Date
            CreateLabel("Purchase Date:", 20, 80, mainGroup);
            datePicker = new DateTimePicker();
            datePicker.Location = new Point(150, 80);
            datePicker.Size = new Size(200, 25);
            datePicker.Value = DateTime.Now;
            mainGroup.Controls.Add(datePicker);

            // Supplier
            CreateLabel("Supplier:", 20, 120, mainGroup);
            supplierTxt = CreateTextBox(150, 120, 250, mainGroup);
            supplierTxt.PlaceholderText = "Enter supplier name";

            // Total Amount
            CreateLabel("Total Amount:", 450, 120, mainGroup);
            totalAmountTxt = CreateTextBox(550, 120, 150, mainGroup);
            totalAmountTxt.ReadOnly = true;
            totalAmountTxt.Text = "0.00";
            totalAmountTxt.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            totalAmountTxt.ForeColor = Color.Green;

            // Items Grid
            itemsGrid = new DataGridView();
            itemsGrid.Location = new Point(20, 160);
            itemsGrid.Size = new Size(710, 200);
            itemsGrid.BackgroundColor = Color.White;
            itemsGrid.AllowUserToAddRows = false;
            itemsGrid.RowHeadersVisible = false;
            
            // Add columns
            itemsGrid.Columns.Add("Product", "Product Name");
            itemsGrid.Columns.Add("Quantity", "Quantity");
            itemsGrid.Columns.Add("Unit", "Unit");
            itemsGrid.Columns.Add("Price", "Unit Price");
            itemsGrid.Columns.Add("Amount", "Total Amount");
            
            // Format columns
            itemsGrid.Columns["Quantity"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            itemsGrid.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            itemsGrid.Columns["Amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            itemsGrid.Columns["Price"].DefaultCellStyle.Format = "N2";
            itemsGrid.Columns["Amount"].DefaultCellStyle.Format = "N2";
            
            itemsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            itemsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            mainGroup.Controls.Add(itemsGrid);

            // Buttons for items
            addItemBtn = CreateButton("➕ Add Item", Color.FromArgb(52, 152, 219), new Point(20, 370));
            addItemBtn.Click += AddItemBtn_Click;
            mainGroup.Controls.Add(addItemBtn);

            removeItemBtn = CreateButton("➖ Remove Item", Color.FromArgb(231, 76, 60), new Point(150, 370));
            removeItemBtn.Click += RemoveItemBtn_Click;
            mainGroup.Controls.Add(removeItemBtn);

            this.Controls.Add(mainGroup);

            // Form Buttons
            saveBtn = CreateButton("💾 Save Purchase", Color.FromArgb(46, 204, 113), new Point(20, 540));
            saveBtn.Click += SaveBtn_Click;

            clearBtn = CreateButton("🗑️ Clear", Color.FromArgb(149, 165, 166), new Point(150, 540));
            clearBtn.Click += ClearBtn_Click;

            this.Controls.Add(saveBtn);
            this.Controls.Add(clearBtn);
        }

        private void CreateLabel(string text, int x, int y, Control parent)
        {
            Label label = new Label();
            label.Text = text;
            label.Font = new Font("Segoe UI", 9);
            label.Location = new Point(x, y);
            label.Size = new Size(120, 20);
            parent.Controls.Add(label);
        }

        private TextBox CreateTextBox(int x, int y, int width, Control parent)
        {
            TextBox txt = new TextBox();
            txt.Font = new Font("Segoe UI", 9);
            txt.Location = new Point(x, y);
            txt.Size = new Size(width, 25);
            parent.Controls.Add(txt);
            return txt;
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

        private void AddItemBtn_Click(object sender, EventArgs e)
        {
            // Show dialog to add purchase item
            using (var itemForm = new StockPurchaseItemForm())
            {
                if (itemForm.ShowDialog() == DialogResult.OK)
                {
                    var item = itemForm.PurchaseItem;
                    purchaseItems.Add(item);
                    
                    // Add to grid
                    itemsGrid.Rows.Add(item.ProductName, item.Quantity, item.Unit, item.UnitPrice, item.TotalAmount);
                    
                    // Update total amount
                    UpdateTotalAmount();
                }
            }
        }

        private void RemoveItemBtn_Click(object sender, EventArgs e)
        {
            if (itemsGrid.SelectedRows.Count > 0)
            {
                var selectedIndex = itemsGrid.SelectedRows[0].Index;
                if (selectedIndex < purchaseItems.Count)
                {
                    purchaseItems.RemoveAt(selectedIndex);
                    itemsGrid.Rows.RemoveAt(selectedIndex);
                    UpdateTotalAmount();
                }
            }
            else
            {
                MessageBox.Show("Please select an item to remove.", "Selection Required", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdateTotalAmount()
        {
            decimal total = 0;
            foreach (var item in purchaseItems)
            {
                total += item.TotalAmount;
            }
            totalAmountTxt.Text = total.ToString("N2");
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(supplierTxt.Text) || purchaseItems.Count == 0)
            {
                MessageBox.Show("Please enter supplier and add at least one item!", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Save purchase voucher
                var purchaseVoucher = new Voucher
                {
                    Type = "Stock Purchase",
                    Number = voucherNumberTxt.Text,
                    Date = datePicker.Value,
                    Party = supplierTxt.Text.Trim(),
                    Amount = decimal.Parse(totalAmountTxt.Text),
                    Description = $"Stock purchase from {supplierTxt.Text.Trim()} - {purchaseItems.Count} items",
                    Status = "Active"
                };

                if (voucherManager.AddVoucher(purchaseVoucher))
                {
                    // Update product stock and add stock transactions
                    UpdateProductStock();
                    
                    MessageBox.Show("Stock purchase saved successfully! Stock levels updated.", "Success", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    voucherNumberTxt.Text = voucherManager.GenerateVoucherNumber("Stock Purchase");
                }
                else
                {
                    MessageBox.Show("Failed to save purchase voucher!", "Error", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving purchase: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateProductStock()
{
    foreach (var item in purchaseItems)
    {
        // First, check if product exists, if not create it
        string checkProductSql = "SELECT COUNT(*) FROM products WHERE name = @productName";
        using (var checkCmd = new SQLiteCommand(checkProductSql, dbManager.GetConnection()))
        {
            checkCmd.Parameters.AddWithValue("@productName", item.ProductName);
            var exists = Convert.ToInt32(checkCmd.ExecuteScalar()) > 0;

            if (!exists)
            {
                // Create new product
                string insertProductSql = @"INSERT INTO products (name, code, price, stock, unit, min_stock) 
                                          VALUES (@name, @code, @price, @stock, @unit, @minStock)";
                using (var insertCmd = new SQLiteCommand(insertProductSql, dbManager.GetConnection()))
                {
                    // Generate code from product name
                    string code = GenerateProductCode(item.ProductName);
                    
                    insertCmd.Parameters.AddWithValue("@name", item.ProductName);
                    insertCmd.Parameters.AddWithValue("@code", code);
                    insertCmd.Parameters.AddWithValue("@price", item.UnitPrice);
                    insertCmd.Parameters.AddWithValue("@stock", item.Quantity); // Initial stock
                    insertCmd.Parameters.AddWithValue("@unit", item.Unit);
                    insertCmd.Parameters.AddWithValue("@minStock", 10); // Default minimum stock
                    insertCmd.ExecuteNonQuery();
                }
            }
            else
            {
                // Update existing product stock and price
                string updateSql = @"UPDATE products SET stock = stock + @quantity, price = @price 
                                  WHERE name = @productName";
                using (var updateCmd = new SQLiteCommand(updateSql, dbManager.GetConnection()))
                {
                    updateCmd.Parameters.AddWithValue("@quantity", item.Quantity);
                    updateCmd.Parameters.AddWithValue("@price", item.UnitPrice);
                    updateCmd.Parameters.AddWithValue("@productName", item.ProductName);
                    updateCmd.ExecuteNonQuery();
                }
            }
        }

        // Add stock transaction record
        string transactionSql = @"INSERT INTO stock_transactions 
                                (product_name, transaction_type, quantity, unit_price, total_amount, voucher_number, notes)
                                VALUES (@productName, 'PURCHASE', @quantity, @unitPrice, @totalAmount, @voucherNumber, @notes)";

        using (var cmd = new SQLiteCommand(transactionSql, dbManager.GetConnection()))
        {
            cmd.Parameters.AddWithValue("@productName", item.ProductName);
            cmd.Parameters.AddWithValue("@quantity", item.Quantity);
            cmd.Parameters.AddWithValue("@unitPrice", item.UnitPrice);
            cmd.Parameters.AddWithValue("@totalAmount", item.TotalAmount);
            cmd.Parameters.AddWithValue("@voucherNumber", voucherNumberTxt.Text);
            cmd.Parameters.AddWithValue("@notes", $"Purchase from {supplierTxt.Text.Trim()}");
            cmd.ExecuteNonQuery();
        }
    }
}

private string GenerateProductCode(string productName)
{
    // Generate a simple code from product name
    string code = productName.Replace(" ", "").ToUpper();
    if (code.Length > 6)
    {
        code = code.Substring(0, 6);
    }
    
    // Add random numbers to make it unique
    Random random = new Random();
    code += random.Next(100, 999).ToString();
    
    return code;
}

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            supplierTxt.Clear();
            purchaseItems.Clear();
            itemsGrid.Rows.Clear();
            totalAmountTxt.Text = "0.00";
            datePicker.Value = DateTime.Now;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(300, 250);
            this.Name = "StockPurchaseForm";
            this.ResumeLayout(false);
        }
    }

    public class StockPurchaseItem
    {
        public string ProductName { get; set; } = "";
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "PCS";
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount => Quantity * UnitPrice;
    }
}