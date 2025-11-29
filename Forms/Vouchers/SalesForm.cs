using System;
using System.Drawing;
using System.Windows.Forms;
using BillingSoftware.Modules;
using BillingSoftware.Models;

namespace BillingSoftware.Forms.Vouchers
{
    public partial class SalesForm : Form
    {
        private VoucherManager voucherManager;
        private TextBox voucherNumberTxt, partyTxt, amountTxt, descriptionTxt;
        private DateTimePicker datePicker;
        private Button saveBtn, clearBtn;

        public SalesForm()
        {
            InitializeComponent();
            voucherManager = new VoucherManager();
            CreateSalesFormUI();
        }

        private void CreateSalesFormUI()
        {
            this.Text = "Sales Voucher";
            this.Size = new Size(500, 400);
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
            mainGroup.Size = new Size(450, 250);
            mainGroup.BackColor = Color.White;

            // Voucher Number
            CreateLabel("Voucher Number:", 20, 40, mainGroup);
            voucherNumberTxt = CreateTextBox(150, 40, 200, mainGroup);
            voucherNumberTxt.Text = voucherManager.GenerateVoucherNumber("Sales");
            voucherNumberTxt.ReadOnly = true;
            voucherNumberTxt.BackColor = Color.FromArgb(240, 240, 240);

            // Date
            CreateLabel("Date:", 20, 80, mainGroup);
            datePicker = new DateTimePicker();
            datePicker.Location = new Point(150, 80);
            datePicker.Size = new Size(200, 25);
            datePicker.Font = new Font("Segoe UI", 9);
            datePicker.Value = DateTime.Now;
            mainGroup.Controls.Add(datePicker);

            // Party
            CreateLabel("Customer Name:", 20, 120, mainGroup);
            partyTxt = CreateTextBox(150, 120, 250, mainGroup);

            // Amount
            CreateLabel("Amount:", 20, 160, mainGroup);
            amountTxt = CreateTextBox(150, 160, 150, mainGroup);
            amountTxt.KeyPress += AmountTxt_KeyPress;

            // Description
            CreateLabel("Description:", 20, 200, mainGroup);
            descriptionTxt = CreateTextBox(150, 200, 250, mainGroup);
            descriptionTxt.Multiline = true;
            descriptionTxt.Height = 60;

            this.Controls.Add(mainGroup);

            // Buttons
            saveBtn = CreateButton("Save Sales", Color.FromArgb(46, 204, 113), new Point(20, 340));
            saveBtn.Click += SaveBtn_Click;

            clearBtn = CreateButton("Clear", Color.FromArgb(149, 165, 166), new Point(150, 340));
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

        private void AmountTxt_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only numbers, decimal point, and control characters
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // Allow only one decimal point
            if (e.KeyChar == '.' && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(partyTxt.Text) || string.IsNullOrWhiteSpace(amountTxt.Text))
            {
                MessageBox.Show("Please enter customer name and amount!", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(amountTxt.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid amount greater than 0!", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var salesVoucher = new Voucher
            {
                Type = "Sales",
                Number = voucherNumberTxt.Text,
                Date = datePicker.Value,
                Party = partyTxt.Text.Trim(),
                Amount = amount,
                Description = descriptionTxt.Text,
                Status = "Active"
            };

            if (voucherManager.AddVoucher(salesVoucher))
            {
                MessageBox.Show("Sales voucher saved successfully!", "Success", 
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                voucherNumberTxt.Text = voucherManager.GenerateVoucherNumber("Sales");
            }
            else
            {
                MessageBox.Show("Failed to save sales voucher!", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            partyTxt.Clear();
            amountTxt.Clear();
            descriptionTxt.Clear();
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
}