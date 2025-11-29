using System;
using System.Drawing;
using System.Windows.Forms;
using BillingSoftware.Forms.Vouchers;

namespace BillingSoftware.Forms.Vouchers
{
    public partial class StockPurchaseItemForm : Form
    {
        public StockPurchaseItem PurchaseItem { get; private set; }
        
        private TextBox productNameTxt, quantityTxt, unitPriceTxt, unitTxt;
        private Button saveBtn, cancelBtn;

        public StockPurchaseItemForm()
        {
            InitializeComponent();
            PurchaseItem = new StockPurchaseItem();
            CreateItemFormUI();
        }

        private void CreateItemFormUI()
        {
            this.Text = "Add Purchase Item";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(248, 249, 250);

            // Title
            Label titleLabel = new Label();
            titleLabel.Text = "Add Purchase Item";
            titleLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(44, 62, 80);
            titleLabel.Location = new Point(20, 20);
            titleLabel.Size = new Size(200, 25);
            this.Controls.Add(titleLabel);

            // Product Name
            CreateLabel("Product Name:", 20, 60);
            productNameTxt = CreateTextBox(120, 60, 200);

            // Quantity
            CreateLabel("Quantity:", 20, 100);
            quantityTxt = CreateTextBox(120, 100, 100);
            quantityTxt.KeyPress += NumericTextBox_KeyPress;

            // Unit
            CreateLabel("Unit:", 20, 140);
            unitTxt = CreateTextBox(120, 140, 80);
            unitTxt.Text = "PCS";

            // Unit Price
            CreateLabel("Unit Price:", 20, 180);
            unitPriceTxt = CreateTextBox(120, 180, 100);
            unitPriceTxt.KeyPress += NumericTextBox_KeyPress;

            // Buttons
            saveBtn = CreateButton("Save", Color.FromArgb(46, 204, 113), new Point(80, 220));
            saveBtn.Click += SaveBtn_Click;

            cancelBtn = CreateButton("Cancel", Color.FromArgb(149, 165, 166), new Point(200, 220));
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

        private void NumericTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                e.Handled = true;
            
            // Allow only one decimal point
            if (e.KeyChar == '.' && (sender as TextBox).Text.IndexOf('.') > -1)
                e.Handled = true;
        }

        private void SaveBtn_Click(object sender, EventArgs e)
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

            if (!decimal.TryParse(unitPriceTxt.Text, out decimal unitPrice) || unitPrice < 0)
            {
                MessageBox.Show("Please enter valid unit price!", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            PurchaseItem = new StockPurchaseItem
            {
                ProductName = productNameTxt.Text.Trim(),
                Quantity = quantity,
                Unit = string.IsNullOrWhiteSpace(unitTxt.Text) ? "PCS" : unitTxt.Text.Trim(),
                UnitPrice = unitPrice
            };

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(300, 250);
            this.Name = "StockPurchaseItemForm";
            this.ResumeLayout(false);
        }
    }
}