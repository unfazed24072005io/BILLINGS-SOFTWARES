using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Data.SQLite;
using BillingSoftware.Modules;
using BillingSoftware.Models;
using BillingSoftware.Utilities;

namespace BillingSoftware.Forms.Vouchers
{
    public partial class SalesForm : Form
    {
        private VoucherManager voucherManager;
        private DatabaseManager dbManager;
        
        private TextBox voucherNumberTxt, customerTxt, totalAmountTxt;
        private DateTimePicker datePicker;
        private DataGridView itemsGrid;
        private Button saveBtn, clearBtn, addItemBtn, removeItemBtn, printBtn;
        private List<SaleItem> saleItems;

        public SalesForm()
        {
            InitializeComponent();
            voucherManager = new VoucherManager();
            dbManager = new DatabaseManager();
            saleItems = new List<SaleItem>();
            CreateSalesFormUI();
        }

        private void CreateSalesFormUI()
        {
            this.Text = "Sales Voucher";
            this.Size = new Size(800, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(248, 249, 250);

            // Title
            Label titleLabel = new Label();
            titleLabel.Text = "💰 Sales Voucher";
            titleLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(44, 62, 80);
            titleLabel.Location = new Point(20, 20);
            titleLabel.Size = new Size(300, 30);
            this.Controls.Add(titleLabel);

            // Main container
            GroupBox mainGroup = new GroupBox();
            mainGroup.Text = "Sales Details";
            mainGroup.Font = new Font("Segoe UI", 10);
            mainGroup.Location = new Point(20, 70);
            mainGroup.Size = new Size(750, 400);
            mainGroup.BackColor = Color.White;

            // Voucher Number
            CreateLabel("Sales No:", 20, 40, mainGroup);
            voucherNumberTxt = CreateTextBox(150, 40, 200, mainGroup);
            voucherNumberTxt.Text = voucherManager.GenerateVoucherNumber("Sales");
            voucherNumberTxt.ReadOnly = true;

            // Date
            CreateLabel("Date:", 20, 80, mainGroup);
            datePicker = new DateTimePicker();
            datePicker.Location = new Point(150, 80);
            datePicker.Size = new Size(200, 25);
            datePicker.Value = DateTime.Now;
            mainGroup.Controls.Add(datePicker);

            // Customer
            CreateLabel("Customer:", 20, 120, mainGroup);
            customerTxt = CreateTextBox(150, 120, 250, mainGroup);

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
            saveBtn = CreateButton("💾 Save Sales", Color.FromArgb(46, 204, 113), new Point(20, 490));
            saveBtn.Click += SaveBtn_Click;

            clearBtn = CreateButton("🗑️ Clear", Color.FromArgb(149, 165, 166), new Point(150, 490));
            clearBtn.Click += ClearBtn_Click;

            printBtn = CreateButton("🖨️ Print", Color.FromArgb(155, 89, 182), new Point(280, 490));
            printBtn.Click += PrintBtn_Click;

            this.Controls.Add(saveBtn);
            this.Controls.Add(clearBtn);
            this.Controls.Add(printBtn);
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
            using (var itemForm = new SalesItemForm())
            {
                if (itemForm.ShowDialog() == DialogResult.OK)
                {
                    var item = itemForm.SaleItem;
                    saleItems.Add(item);
                    
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
                if (selectedIndex < saleItems.Count)
                {
                    saleItems.RemoveAt(selectedIndex);
                    itemsGrid.Rows.RemoveAt(selectedIndex);
                    UpdateTotalAmount();
                }
            }
        }

        private void UpdateTotalAmount()
        {
            decimal total = 0;
            foreach (var item in saleItems)
            {
                total += item.Amount;
            }
            totalAmountTxt.Text = total.ToString("N2");
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(customerTxt.Text))
            {
                MessageBox.Show("Please enter customer name!", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (saleItems.Count == 0)
            {
                MessageBox.Show("Please add at least one item!", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check stock availability
            foreach (var item in saleItems)
            {
                var product = dbManager.GetProductByName(item.ProductName);
                if (product == null)
                {
                    MessageBox.Show($"Product '{item.ProductName}' not found!", "Error", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                if (product.Stock < item.Quantity)
                {
                    MessageBox.Show($"Insufficient stock for '{item.ProductName}'\n" +
                                  $"Available: {product.Stock}, Requested: {item.Quantity}", 
                                  "Stock Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            try
            {
                // Create voucher
                var salesVoucher = new Voucher
                {
                    Type = "Sales",
                    Number = voucherNumberTxt.Text,
                    Date = datePicker.Value,
                    Party = customerTxt.Text.Trim(),
                    Amount = decimal.Parse(totalAmountTxt.Text),
                    Description = $"Sales to {customerTxt.Text.Trim()}",
                    Status = "Active"
                };

                // Add items to voucher
                foreach (var item in saleItems)
                {
                    salesVoucher.Items.Add(new VoucherItem
                    {
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.Rate
                    });
                }

                if (voucherManager.AddVoucher(salesVoucher))
                {
                    string message = $"Sales saved successfully!\nStock updated.\n\n" +
                                   $"Sales #: {voucherNumberTxt.Text}\n" +
                                   $"Customer: {customerTxt.Text}\n" +
                                   $"Amount: ₹{totalAmountTxt.Text}\n\n" +
                                   $"Do you want to print this sales voucher?";
                    
                    var result = MessageBox.Show(message, "Success", 
                                               MessageBoxButtons.YesNo, 
                                               MessageBoxIcon.Information);
                    
                    if (result == DialogResult.Yes)
                    {
                        PrintBtn_Click(null, EventArgs.Empty);
                    }
                    
                    ClearForm();
                    voucherNumberTxt.Text = voucherManager.GenerateVoucherNumber("Sales");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving sales: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintBtn_Click(object sender, EventArgs e)
        {
            if (saleItems.Count == 0)
            {
                MessageBox.Show("Please add items to the sales before printing!", 
                              "No Items", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var salesVoucher = new Voucher
            {
                Type = "Sales",
                Number = voucherNumberTxt.Text,
                Date = datePicker.Value,
                Party = string.IsNullOrWhiteSpace(customerTxt.Text) ? "Customer" : customerTxt.Text.Trim(),
                Amount = decimal.Parse(totalAmountTxt.Text),
                Description = $"Sales to {customerTxt.Text.Trim()}"
            };

            var voucherItems = new List<VoucherItem>();
            foreach (var item in saleItems)
            {
                voucherItems.Add(new VoucherItem
                {
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitPrice = item.Rate
                });
            }

            PrintHelper printHelper = new PrintHelper();
            printHelper.PrintVoucher(salesVoucher, voucherItems);
        }

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            customerTxt.Clear();
            saleItems.Clear();
            itemsGrid.Rows.Clear();
            totalAmountTxt.Text = "0.00";
            datePicker.Value = DateTime.Now;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(300, 250);
            this.Name = "SalesForm";
            this.ResumeLayout(false);
        }
    }

    // Sales Item Form
    public class SalesItemForm : Form
    {
        public SaleItem SaleItem { get; private set; }
        
        private TextBox productNameTxt, quantityTxt, rateTxt, unitTxt;
        private Button saveBtn, cancelBtn;
        private DatabaseManager dbManager;

        public SalesItemForm()
        {
            InitializeComponent();
            dbManager = new DatabaseManager();
            SaleItem = new SaleItem();
            CreateItemFormUI();
        }

        private void CreateItemFormUI()
        {
            this.Text = "Add Sale Item";
            this.Size = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterParent;

            Label titleLabel = new Label();
            titleLabel.Text = "Add Sale Item";
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
            saveBtn = CreateButton("Save", Color.FromArgb(46, 204, 113), new Point(120, 220));
            saveBtn.Click += SaveBtn_Click;

            cancelBtn = CreateButton("Cancel", Color.FromArgb(149, 165, 166), new Point(220, 220));
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
                Cursor = Cursors.Hand
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

            SaleItem = new SaleItem
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
            this.Name = "SalesItemForm";
            this.ResumeLayout(false);
        }
    }
}