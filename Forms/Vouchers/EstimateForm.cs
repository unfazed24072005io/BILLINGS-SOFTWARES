using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Data.SQLite;
using BillingSoftware.Modules;
using BillingSoftware.Models;

namespace BillingSoftware.Forms.Vouchers
{
    public partial class EstimateForm : Form
    {
        private VoucherManager voucherManager;
        private DatabaseManager dbManager;
        
        private TextBox estimateNumberTxt, customerTxt, totalAmountTxt, discountTxt, taxTxt, grandTotalTxt;
        private DateTimePicker datePicker, expiryDatePicker;
        private DataGridView itemsGrid;
        private ComboBox productSearchCombo;
        private Button saveBtn, clearBtn, addItemBtn, removeItemBtn;
        private List<EstimateItem> estimateItems;
        
        private decimal discountPercent = 0;
        private decimal taxPercent = 0;

        public EstimateForm()
        {
            InitializeComponent();
            voucherManager = new VoucherManager();
            dbManager = new DatabaseManager();
            estimateItems = new List<EstimateItem>();
            CreateEstimateFormUI();
            LoadProductSearchList();
        }

        private void CreateEstimateFormUI()
        {
            this.Text = "Estimate / Quotation";
            this.Size = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(248, 249, 250);

            // Title
            Label titleLabel = new Label();
            titleLabel.Text = "📋 Estimate / Quotation";
            titleLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(44, 62, 80);
            titleLabel.Location = new Point(20, 20);
            titleLabel.Size = new Size(300, 30);
            this.Controls.Add(titleLabel);

            // Main container
            GroupBox mainGroup = new GroupBox();
            mainGroup.Text = "Estimate Details";
            mainGroup.Font = new Font("Segoe UI", 10);
            mainGroup.Location = new Point(20, 70);
            mainGroup.Size = new Size(850, 500);
            mainGroup.BackColor = Color.White;

            // Estimate Number
            CreateLabel("Estimate Number:", 20, 40, mainGroup);
            estimateNumberTxt = CreateTextBox(150, 40, 200, mainGroup);
            estimateNumberTxt.Text = $"EST-{DateTime.Now:yyyyMMdd}-{new Random().Next(100, 999)}";
            estimateNumberTxt.ReadOnly = true;

            // Date
            CreateLabel("Estimate Date:", 20, 80, mainGroup);
            datePicker = new DateTimePicker();
            datePicker.Location = new Point(150, 80);
            datePicker.Size = new Size(200, 25);
            datePicker.Value = DateTime.Now;
            mainGroup.Controls.Add(datePicker);

            // Expiry Date
            CreateLabel("Valid Until:", 20, 120, mainGroup);
            expiryDatePicker = new DateTimePicker();
            expiryDatePicker.Location = new Point(150, 120);
            expiryDatePicker.Size = new Size(200, 25);
            expiryDatePicker.Value = DateTime.Now.AddDays(30);
            mainGroup.Controls.Add(expiryDatePicker);

            // Customer
            CreateLabel("Customer:", 20, 160, mainGroup);
            customerTxt = CreateTextBox(150, 160, 250, mainGroup);
            customerTxt.PlaceholderText = "Enter customer name";

            // Product Search
            CreateLabel("Search Product:", 420, 40, mainGroup);
            productSearchCombo = new ComboBox();
            productSearchCombo.Location = new Point(540, 40);
            productSearchCombo.Size = new Size(250, 25);
            productSearchCombo.DropDownStyle = ComboBoxStyle.DropDown;
            productSearchCombo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            productSearchCombo.AutoCompleteSource = AutoCompleteSource.ListItems;
            productSearchCombo.SelectedIndexChanged += ProductSearchCombo_SelectedIndexChanged;
            mainGroup.Controls.Add(productSearchCombo);

            // Items Grid
            itemsGrid = new DataGridView();
            itemsGrid.Location = new Point(20, 200);
            itemsGrid.Size = new Size(810, 200);
            itemsGrid.BackgroundColor = Color.White;
            itemsGrid.AllowUserToAddRows = false;
            itemsGrid.RowHeadersVisible = false;
            
            // Add columns
            itemsGrid.Columns.Add("Product", "Product Name");
            itemsGrid.Columns.Add("Code", "Code");
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
            addItemBtn = CreateButton("➕ Add Item", Color.FromArgb(52, 152, 219), new Point(20, 410));
            addItemBtn.Click += AddItemBtn_Click;
            mainGroup.Controls.Add(addItemBtn);

            removeItemBtn = CreateButton("➖ Remove Item", Color.FromArgb(231, 76, 60), new Point(150, 410));
            removeItemBtn.Click += RemoveItemBtn_Click;
            mainGroup.Controls.Add(removeItemBtn);

            // Totals Section
            CreateLabel("Subtotal:", 420, 410, mainGroup);
            totalAmountTxt = CreateTextBox(520, 410, 150, mainGroup);
            totalAmountTxt.ReadOnly = true;
            totalAmountTxt.Text = "0.00";

            CreateLabel("Discount (%):", 420, 440, mainGroup);
            discountTxt = CreateTextBox(520, 440, 100, mainGroup);
            discountTxt.Text = "0";
            discountTxt.KeyPress += NumericTextBox_KeyPress;
            discountTxt.TextChanged += DiscountTxt_TextChanged;

            CreateLabel("Tax (%):", 420, 470, mainGroup);
            taxTxt = CreateTextBox(520, 470, 100, mainGroup);
            taxTxt.Text = "0";
            taxTxt.KeyPress += NumericTextBox_KeyPress;
            taxTxt.TextChanged += TaxTxt_TextChanged;

            CreateLabel("Grand Total:", 420, 500, mainGroup);
            grandTotalTxt = CreateTextBox(520, 500, 150, mainGroup);
            grandTotalTxt.ReadOnly = true;
            grandTotalTxt.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            grandTotalTxt.ForeColor = Color.Green;
            grandTotalTxt.Text = "0.00";

            this.Controls.Add(mainGroup);

            // Form Buttons
            saveBtn = CreateButton("💾 Save Estimate", Color.FromArgb(46, 204, 113), new Point(20, 590));
            saveBtn.Click += SaveBtn_Click;

            clearBtn = CreateButton("🗑️ Clear", Color.FromArgb(149, 165, 166), new Point(150, 590));
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
            label.Size = new Size(100, 20);
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

        private void LoadProductSearchList()
        {
            try
            {
                string sql = "SELECT name, code, price FROM products ORDER BY name";
                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                using (var reader = cmd.ExecuteReader())
                {
                    productSearchCombo.Items.Clear();
                    
                    while (reader.Read())
                    {
                        string productName = reader["name"].ToString();
                        string code = reader["code"].ToString();
                        decimal price = Convert.ToDecimal(reader["price"]);
                        
                        productSearchCombo.Items.Add($"{productName} [{code}] - ₹{price:N2}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading products: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ProductSearchCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (productSearchCombo.SelectedItem != null)
            {
                string selectedText = productSearchCombo.SelectedItem.ToString();
                
                // Extract product name (remove code and price)
                string productName = selectedText.Split('[')[0].Trim();
                
                // Show dialog to enter quantity
                using (var quantityForm = new QuantityForm(productName))
                {
                    if (quantityForm.ShowDialog() == DialogResult.OK)
                    {
                        AddEstimateItem(productName, quantityForm.Quantity);
                        productSearchCombo.Text = "";
                    }
                }
            }
        }

        private void AddEstimateItem(string productName, decimal quantity)
        {
            // Get product details from database
            var product = dbManager.GetProductByName(productName);
            
            if (product == null)
            {
                MessageBox.Show($"Product '{productName}' not found!", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var estimateItem = new EstimateItem
            {
                ProductName = product.Name,
                ProductCode = product.Code,
                Quantity = quantity,
                Unit = product.Unit,
                Rate = product.Price,
                Amount = quantity * product.Price
            };

            estimateItems.Add(estimateItem);
            
            // Add to grid
            itemsGrid.Rows.Add(
                estimateItem.ProductName,
                estimateItem.ProductCode,
                estimateItem.Quantity,
                estimateItem.Unit,
                estimateItem.Rate,
                estimateItem.Amount
            );
            
            // Update totals
            UpdateTotals();
        }

        private void UpdateTotals()
        {
            decimal subtotal = estimateItems.Sum(item => item.Amount);
            totalAmountTxt.Text = subtotal.ToString("N2");
            
            // Calculate discount and tax
            decimal discountAmount = subtotal * (discountPercent / 100);
            decimal taxAmount = (subtotal - discountAmount) * (taxPercent / 100);
            
            decimal grandTotal = subtotal - discountAmount + taxAmount;
            grandTotalTxt.Text = grandTotal.ToString("N2");
        }

        private void AddItemBtn_Click(object sender, EventArgs e)
        {
            // Manual add item dialog
            using (var manualForm = new ManualEstimateItemForm())
            {
                if (manualForm.ShowDialog() == DialogResult.OK)
                {
                    var product = dbManager.GetProductByName(manualForm.ProductName);
                    
                    if (product != null)
                    {
                        AddEstimateItem(product.Name, manualForm.Quantity);
                    }
                    else
                    {
                        // Add new product if not found
                        var result = MessageBox.Show($"Product '{manualForm.ProductName}' not found. Add as new product?", 
                                                   "New Product", 
                                                   MessageBoxButtons.YesNo, 
                                                   MessageBoxIcon.Question);
                        
                        if (result == DialogResult.Yes)
                        {
                            AddEstimateItem(manualForm.ProductName, manualForm.Quantity);
                        }
                    }
                }
            }
        }

        private void RemoveItemBtn_Click(object sender, EventArgs e)
        {
            if (itemsGrid.SelectedRows.Count > 0)
            {
                int selectedIndex = itemsGrid.SelectedRows[0].Index;
                if (selectedIndex < estimateItems.Count)
                {
                    estimateItems.RemoveAt(selectedIndex);
                    itemsGrid.Rows.RemoveAt(selectedIndex);
                    UpdateTotals();
                }
            }
        }

        private void NumericTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                e.Handled = true;
            
            if (e.KeyChar == '.' && (sender as TextBox).Text.IndexOf('.') > -1)
                e.Handled = true;
        }

        private void DiscountTxt_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(discountTxt.Text, out decimal discount))
            {
                discountPercent = discount;
                UpdateTotals();
            }
        }

        private void TaxTxt_TextChanged(object sender, EventArgs e)
        {
            if (decimal.TryParse(taxTxt.Text, out decimal tax))
            {
                taxPercent = tax;
                UpdateTotals();
            }
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(customerTxt.Text))
            {
                MessageBox.Show("Please enter customer name!", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (estimateItems.Count == 0)
            {
                MessageBox.Show("Please add at least one item!", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Save estimate as a voucher with type "Estimate"
                var estimateVoucher = new Voucher
                {
                    Type = "Estimate",
                    Number = estimateNumberTxt.Text,
                    Date = datePicker.Value,
                    Party = customerTxt.Text.Trim(),
                    Amount = decimal.Parse(grandTotalTxt.Text),
                    Description = $"Estimate valid until: {expiryDatePicker.Value:dd-MMM-yyyy}. " +
                                 $"Items: {estimateItems.Count}, Discount: {discountPercent}%, Tax: {taxPercent}%",
                    Status = "Active"
                };

                // Convert estimate items to voucher items
                foreach (var item in estimateItems)
                {
                    estimateVoucher.Items.Add(new VoucherItem
                    {
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.Rate
                    });
                }

                if (voucherManager.AddVoucher(estimateVoucher))
                {
                    MessageBox.Show("Estimate saved successfully!\n\n" +
                                  $"Estimate #: {estimateNumberTxt.Text}\n" +
                                  $"Customer: {customerTxt.Text}\n" +
                                  $"Amount: ₹{grandTotalTxt.Text}\n" +
                                  $"Valid Until: {expiryDatePicker.Value:dd-MMM-yyyy}", 
                                  "Success", 
                                  MessageBoxButtons.OK, 
                                  MessageBoxIcon.Information);
                    
                    ClearForm();
                    estimateNumberTxt.Text = $"EST-{DateTime.Now:yyyyMMdd}-{new Random().Next(100, 999)}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving estimate: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            customerTxt.Clear();
            estimateItems.Clear();
            itemsGrid.Rows.Clear();
            discountTxt.Text = "0";
            taxTxt.Text = "0";
            totalAmountTxt.Text = "0.00";
            grandTotalTxt.Text = "0.00";
            datePicker.Value = DateTime.Now;
            expiryDatePicker.Value = DateTime.Now.AddDays(30);
            productSearchCombo.Text = "";
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(300, 250);
            this.Name = "EstimateForm";
            this.ResumeLayout(false);
        }
    }

    // Estimate Item class
    public class EstimateItem
    {
        public string ProductName { get; set; } = "";
        public string ProductCode { get; set; } = "";
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "PCS";
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
    }

    // Quantity Form for entering quantity
    public class QuantityForm : Form
    {
        public decimal Quantity { get; private set; }
        private TextBox quantityTxt;
        private Button okBtn, cancelBtn;
        private string productName;

        public QuantityForm(string productName)
        {
            this.productName = productName;
            InitializeComponent();
            CreateQuantityFormUI();
        }

        private void CreateQuantityFormUI()
        {
            this.Text = "Enter Quantity";
            this.Size = new Size(300, 150);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(248, 249, 250);

            Label titleLabel = new Label();
            titleLabel.Text = $"Enter quantity for: {productName}";
            titleLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            titleLabel.Location = new Point(20, 20);
            titleLabel.Size = new Size(250, 25);
            this.Controls.Add(titleLabel);

            Label qtyLabel = new Label();
            qtyLabel.Text = "Quantity:";
            qtyLabel.Location = new Point(20, 60);
            qtyLabel.Size = new Size(80, 20);
            this.Controls.Add(qtyLabel);

            quantityTxt = new TextBox();
            quantityTxt.Location = new Point(100, 60);
            quantityTxt.Size = new Size(100, 25);
            quantityTxt.Text = "1";
            quantityTxt.KeyPress += QuantityTxt_KeyPress;
            this.Controls.Add(quantityTxt);

            okBtn = CreateButton("OK", Color.FromArgb(46, 204, 113), new Point(50, 100));
            okBtn.Click += OkBtn_Click;
            this.Controls.Add(okBtn);

            cancelBtn = CreateButton("Cancel", Color.FromArgb(149, 165, 166), new Point(150, 100));
            cancelBtn.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(cancelBtn);
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

        private void QuantityTxt_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                e.Handled = true;
        }

        private void OkBtn_Click(object sender, EventArgs e)
        {
            if (decimal.TryParse(quantityTxt.Text, out decimal quantity) && quantity > 0)
            {
                Quantity = quantity;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Please enter a valid quantity greater than 0!", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(300, 250);
            this.Name = "QuantityForm";
            this.ResumeLayout(false);
        }
    }

    // Manual Estimate Item Form
    public class ManualEstimateItemForm : Form
    {
        public new string ProductName { get; private set; }
        public decimal Quantity { get; private set; }
        
        private TextBox productNameTxt, quantityTxt;
        private Button okBtn, cancelBtn;

        public ManualEstimateItemForm()
        {
            InitializeComponent();
            CreateManualFormUI();
        }

        private void CreateManualFormUI()
        {
            this.Text = "Add Item Manually";
            this.Size = new Size(350, 180);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(248, 249, 250);

            Label productLabel = new Label();
            productLabel.Text = "Product Name:";
            productLabel.Location = new Point(20, 30);
            productLabel.Size = new Size(100, 20);
            this.Controls.Add(productLabel);

            productNameTxt = new TextBox();
            productNameTxt.Location = new Point(130, 30);
            productNameTxt.Size = new Size(180, 25);
            this.Controls.Add(productNameTxt);

            Label qtyLabel = new Label();
            qtyLabel.Text = "Quantity:";
            qtyLabel.Location = new Point(20, 70);
            qtyLabel.Size = new Size(100, 20);
            this.Controls.Add(qtyLabel);

            quantityTxt = new TextBox();
            quantityTxt.Location = new Point(130, 70);
            quantityTxt.Size = new Size(100, 25);
            quantityTxt.Text = "1";
            quantityTxt.KeyPress += QuantityTxt_KeyPress;
            this.Controls.Add(quantityTxt);

            okBtn = CreateButton("Add", Color.FromArgb(46, 204, 113), new Point(80, 110));
            okBtn.Click += OkBtn_Click;
            this.Controls.Add(okBtn);

            cancelBtn = CreateButton("Cancel", Color.FromArgb(149, 165, 166), new Point(180, 110));
            cancelBtn.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            this.Controls.Add(cancelBtn);
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

        private void QuantityTxt_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                e.Handled = true;
        }

        private void OkBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(productNameTxt.Text))
            {
                MessageBox.Show("Please enter product name!", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(quantityTxt.Text, out decimal quantity) || quantity <= 0)
            {
                MessageBox.Show("Please enter valid quantity greater than 0!", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ProductName = productNameTxt.Text.Trim();
            Quantity = quantity;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(300, 250);
            this.Name = "ManualEstimateItemForm";
            this.ResumeLayout(false);
        }
    }
}