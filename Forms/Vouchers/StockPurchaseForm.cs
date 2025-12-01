using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Data.SQLite;
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
        private List<PurchaseItem> purchaseItems;

        public StockPurchaseForm()
        {
            InitializeComponent();
            voucherManager = new VoucherManager();
            dbManager = new DatabaseManager();
            purchaseItems = new List<PurchaseItem>();
            CreatePurchaseFormUI();
        }

        private void CreatePurchaseFormUI()
        {
            this.Text = "Purchase Voucher";
            this.Size = new Size(800, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(248, 249, 250);

            // Title
            Label titleLabel = new Label();
            titleLabel.Text = "📦 Purchase Voucher";
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
            mainGroup.Size = new Size(750, 400);
            mainGroup.BackColor = Color.White;

            // Voucher Number
            CreateLabel("Purchase No:", 20, 40, mainGroup);
            voucherNumberTxt = CreateTextBox(150, 40, 200, mainGroup);
            voucherNumberTxt.Text = voucherManager.GenerateVoucherNumber("Stock Purchase");
            voucherNumberTxt.ReadOnly = true;

            // Date
            CreateLabel("Date:", 20, 80, mainGroup);
            datePicker = new DateTimePicker();
            datePicker.Location = new Point(150, 80);
            datePicker.Size = new Size(200, 25);
            datePicker.Value = DateTime.Now;
            mainGroup.Controls.Add(datePicker);

            // Supplier
            CreateLabel("Supplier:", 20, 120, mainGroup);
            supplierTxt = CreateTextBox(150, 120, 250, mainGroup);

            // Total Amount
            CreateLabel("Total Amount:", 450, 120, mainGroup);
            totalAmountTxt = CreateTextBox(570, 120, 150, mainGroup);
            totalAmountTxt.ReadOnly = true;
            totalAmountTxt.Text = "0.00";
            totalAmountTxt.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            totalAmountTxt.ForeColor = Color.Green;

            // Items Grid
            itemsGrid = new DataGridView();
            itemsGrid.Location = new Point(20, 160);
            itemsGrid.Size = new Size(710, 150);
            itemsGrid.BackgroundColor = Color.White;
            itemsGrid.AllowUserToAddRows = false;
            itemsGrid.RowHeadersVisible = false;
            
            // Add columns
            itemsGrid.Columns.Add("Product", "Product Name");
            itemsGrid.Columns.Add("Quantity", "Qty");
            itemsGrid.Columns.Add("Unit", "Unit");
            itemsGrid.Columns.Add("Rate", "Rate");
            itemsGrid.Columns.Add("Amount", "Amount");
            
            // Format columns
            itemsGrid.Columns["Quantity"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            itemsGrid.Columns["Rate"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            itemsGrid.Columns["Amount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            itemsGrid.Columns["Rate"].DefaultCellStyle.Format = "N2";
            itemsGrid.Columns["Amount"].DefaultCellStyle.Format = "N2";
            
            itemsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            itemsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            mainGroup.Controls.Add(itemsGrid);

            // Buttons for items
            addItemBtn = CreateButton("➕ Add Item", Color.FromArgb(52, 152, 219), new Point(20, 320));
            addItemBtn.Click += AddItemBtn_Click;
            mainGroup.Controls.Add(addItemBtn);

            removeItemBtn = CreateButton("➖ Remove Item", Color.FromArgb(231, 76, 60), new Point(150, 320));
            removeItemBtn.Click += RemoveItemBtn_Click;
            mainGroup.Controls.Add(removeItemBtn);

            this.Controls.Add(mainGroup);

            // Form Buttons
            saveBtn = CreateButton("💾 Save Purchase", Color.FromArgb(46, 204, 113), new Point(20, 490));
            saveBtn.Click += SaveBtn_Click;

            clearBtn = CreateButton("🗑️ Clear", Color.FromArgb(149, 165, 166), new Point(150, 490));
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
            using (var itemForm = new PurchaseItemForm())
            {
                if (itemForm.ShowDialog() == DialogResult.OK)
                {
                    var item = itemForm.PurchaseItem;
                    purchaseItems.Add(item);
                    
                    // Add to grid
                    itemsGrid.Rows.Add(item.ProductName, item.Quantity, item.Unit, item.Rate, item.Amount);
                    
                    // Update total amount
                    UpdateTotalAmount();
                }
            }
        }

        private void RemoveItemBtn_Click(object sender, EventArgs e)
        {
            if (itemsGrid.SelectedRows.Count > 0)
            {
                int selectedIndex = itemsGrid.SelectedRows[0].Index;
                if (selectedIndex < purchaseItems.Count)
                {
                    purchaseItems.RemoveAt(selectedIndex);
                    itemsGrid.Rows.RemoveAt(selectedIndex);
                    UpdateTotalAmount();
                }
            }
        }

        private void UpdateTotalAmount()
        {
            decimal total = 0;
            foreach (var item in purchaseItems)
            {
                total += item.Amount;
            }
            totalAmountTxt.Text = total.ToString("N2");
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(supplierTxt.Text))
            {
                MessageBox.Show("Please enter supplier name!", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (purchaseItems.Count == 0)
            {
                MessageBox.Show("Please add at least one item!", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Create voucher
                var purchaseVoucher = new Voucher
                {
                    Type = "Stock Purchase",
                    Number = voucherNumberTxt.Text,
                    Date = datePicker.Value,
                    Party = supplierTxt.Text.Trim(),
                    Amount = decimal.Parse(totalAmountTxt.Text),
                    Description = $"Purchase from {supplierTxt.Text.Trim()}",
                    Status = "Active"
                };

                // Add items to voucher
                foreach (var item in purchaseItems)
                {
                    purchaseVoucher.Items.Add(new VoucherItem
                    {
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.Rate
                    });
                }

                if (voucherManager.AddVoucher(purchaseVoucher))
                {
                    MessageBox.Show("Purchase saved successfully!\nStock updated.", "Success", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    voucherNumberTxt.Text = voucherManager.GenerateVoucherNumber("Stock Purchase");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving purchase: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

    // Purchase Item Class
    public class PurchaseItem
    {
        public string ProductName { get; set; } = "";
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "PCS";
        public decimal Rate { get; set; }
        public decimal Amount => Quantity * Rate;
    }

    // Purchase Item Form
    public class PurchaseItemForm : Form
    {
        public PurchaseItem PurchaseItem { get; private set; }
        
        private TextBox productNameTxt, quantityTxt, rateTxt, unitTxt;
        private Button saveBtn, cancelBtn;
        private DatabaseManager dbManager;

        public PurchaseItemForm()
        {
            InitializeComponent();
            dbManager = new DatabaseManager();
            PurchaseItem = new PurchaseItem();
            CreateItemFormUI();
        }

        private void CreateItemFormUI()
        {
            this.Text = "Add Purchase Item";
            this.Size = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterParent;

            Label titleLabel = new Label();
            titleLabel.Text = "Add Purchase Item";
            titleLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            titleLabel.Location = new Point(20, 20);
            titleLabel.Size = new Size(200, 25);
            this.Controls.Add(titleLabel);

            // Product Name
            CreateLabel("Product Name:", 20, 60);
            productNameTxt = CreateTextBox(130, 60, 200);

            // Quantity
            CreateLabel("Quantity:", 20, 100);
            quantityTxt = CreateTextBox(130, 100, 100);
            quantityTxt.Text = "1";
            quantityTxt.KeyPress += NumericKeyPress;

            // Unit
            CreateLabel("Unit:", 20, 140);
            unitTxt = CreateTextBox(130, 140, 80);
            unitTxt.Text = "PCS";

            // Rate
            CreateLabel("Rate:", 20, 180);
            rateTxt = CreateTextBox(130, 180, 100);
            rateTxt.KeyPress += NumericKeyPress;

            // Buttons
              saveBtn = CreateButton("Save", Color.FromArgb(46, 204, 113), new Point(100, 200)); // Changed from 220 to 200
    saveBtn.Click += SaveBtn_Click;

    cancelBtn = CreateButton("Cancel", Color.FromArgb(149, 165, 166), new Point(200, 200)); // Changed from 220 to 200
    cancelBtn.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.Add(saveBtn);
            this.Controls.Add(cancelBtn);
        }

        private void CreateLabel(string text, int x, int y)
        {
            var label = new Label 
            { 
                Text = text, 
                Location = new Point(x, y), 
                Size = new Size(100, 20),
                Font = new Font("Segoe UI", 9)
            };
            this.Controls.Add(label);
        }

        private TextBox CreateTextBox(int x, int y, int width)
        {
            var txt = new TextBox 
            { 
                Location = new Point(x, y), 
                Size = new Size(width, 25),
                Font = new Font("Segoe UI", 9)
            };
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
        Cursor = Cursors.Hand,
        Font = new Font("Segoe UI", 9)
    };
}

        private void NumericKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                e.Handled = true;
            
            if (e.KeyChar == '.' && (sender as TextBox).Text.IndexOf('.') > -1)
                e.Handled = true;
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(productNameTxt.Text))
            {
                MessageBox.Show("Please enter product name!", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(quantityTxt.Text, out decimal qty) || qty <= 0)
            {
                MessageBox.Show("Please enter valid quantity!", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(rateTxt.Text, out decimal rate) || rate < 0)
            {
                MessageBox.Show("Please enter valid rate!", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            PurchaseItem = new PurchaseItem
            {
                ProductName = productNameTxt.Text.Trim(),
                Quantity = qty,
                Unit = string.IsNullOrWhiteSpace(unitTxt.Text) ? "PCS" : unitTxt.Text.Trim(),
                Rate = rate
            };

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(300, 250);
            this.Name = "PurchaseItemForm";
            this.ResumeLayout(false);
        }
    }
}