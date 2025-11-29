using System;
using System.Drawing;
using System.Windows.Forms;
using BillingSoftware.Modules;
using BillingSoftware.Models;

namespace BillingSoftware.Forms.Vouchers
{
    public partial class EstimateForm : Form
    {
        private VoucherManager voucherManager;
        private TextBox estimateNumberTxt, customerTxt, amountTxt, descriptionTxt, validityTxt;
        private DateTimePicker datePicker, expiryDatePicker;
        private Button saveBtn, clearBtn, convertToSalesBtn;

        public EstimateForm()
        {
            InitializeComponent();
            voucherManager = new VoucherManager();
            CreateEstimateFormUI();
        }

        private void CreateEstimateFormUI()
        {
            this.Text = "Estimate/Quotation";
            this.Size = new Size(500, 500);
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
            mainGroup.Size = new Size(450, 350);
            mainGroup.BackColor = Color.White;

            // Estimate Number
            CreateLabel("Estimate Number:", 20, 40, mainGroup);
            estimateNumberTxt = CreateTextBox(150, 40, 200, mainGroup);
            estimateNumberTxt.Text = voucherManager.GenerateVoucherNumber("Estimate");
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

            // Amount
            CreateLabel("Amount:", 20, 200, mainGroup);
            amountTxt = CreateTextBox(150, 200, 150, mainGroup);
            amountTxt.KeyPress += AmountTxt_KeyPress;

            // Validity Days
            CreateLabel("Validity (Days):", 20, 240, mainGroup);
            validityTxt = CreateTextBox(150, 240, 100, mainGroup);
            validityTxt.Text = "30";
            validityTxt.KeyPress += ValidityTxt_KeyPress;
            validityTxt.TextChanged += ValidityTxt_TextChanged;

            // Description
            CreateLabel("Description:", 20, 280, mainGroup);
            descriptionTxt = CreateTextBox(150, 280, 250, mainGroup);
            descriptionTxt.Multiline = true;
            descriptionTxt.Height = 50;

            this.Controls.Add(mainGroup);

            // Buttons
            saveBtn = CreateButton("Save Estimate", Color.FromArgb(46, 204, 113), new Point(20, 440));
            saveBtn.Click += SaveBtn_Click;

            convertToSalesBtn = CreateButton("Convert to Sales", Color.FromArgb(52, 152, 219), new Point(150, 440));
            convertToSalesBtn.Click += ConvertToSalesBtn_Click;

            clearBtn = CreateButton("Clear", Color.FromArgb(149, 165, 166), new Point(280, 440));
            clearBtn.Click += ClearBtn_Click;

            this.Controls.Add(saveBtn);
            this.Controls.Add(convertToSalesBtn);
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
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                e.Handled = true;
            if (e.KeyChar == '.' && (sender as TextBox).Text.IndexOf('.') > -1)
                e.Handled = true;
        }

        private void ValidityTxt_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void ValidityTxt_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(validityTxt.Text, out int days) && days > 0)
            {
                expiryDatePicker.Value = datePicker.Value.AddDays(days);
            }
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(customerTxt.Text) || string.IsNullOrWhiteSpace(amountTxt.Text))
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

            var estimateVoucher = new Voucher
            {
                Type = "Estimate",
                Number = estimateNumberTxt.Text,
                Date = datePicker.Value,
                Party = customerTxt.Text.Trim(),
                Amount = amount,
                Description = $"Valid until: {expiryDatePicker.Value:dd-MMM-yyyy}. {descriptionTxt.Text}",
                Status = "Active"
            };

            if (voucherManager.AddVoucher(estimateVoucher))
            {
                MessageBox.Show("Estimate saved successfully!", "Success", 
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                estimateNumberTxt.Text = voucherManager.GenerateVoucherNumber("Estimate");
            }
            else
            {
                MessageBox.Show("Failed to save estimate!", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConvertToSalesBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(customerTxt.Text) || string.IsNullOrWhiteSpace(amountTxt.Text))
            {
                MessageBox.Show("Please fill customer and amount before converting to sales!", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var result = MessageBox.Show("Convert this estimate to a sales voucher?", "Confirm Conversion", 
                                       MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Create sales voucher from estimate data
                var salesVoucher = new Voucher
                {
                    Type = "Sales",
                    Number = voucherManager.GenerateVoucherNumber("Sales"),
                    Date = DateTime.Now,
                    Party = customerTxt.Text.Trim(),
                    Amount = decimal.Parse(amountTxt.Text),
                    Description = $"Converted from estimate {estimateNumberTxt.Text}. {descriptionTxt.Text}",
                    Status = "Active"
                };

                if (voucherManager.AddVoucher(salesVoucher))
                {
                    MessageBox.Show("Estimate converted to sales successfully!", "Success", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            customerTxt.Clear();
            amountTxt.Clear();
            descriptionTxt.Clear();
            validityTxt.Text = "30";
            datePicker.Value = DateTime.Now;
            expiryDatePicker.Value = DateTime.Now.AddDays(30);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(300, 250);
            this.Name = "EstimateForm";
            this.ResumeLayout(false);
        }
    }
}