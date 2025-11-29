using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SQLite;
using BillingSoftware.Modules;
using BillingSoftware.Models;

namespace BillingSoftware.Forms.Vouchers
{
    public partial class JournalForm : Form
    {
        private VoucherManager voucherManager;
        private TextBox voucherNumberTxt, debitAccountTxt, creditAccountTxt, amountTxt, descriptionTxt, referenceVoucherTxt;
        private DateTimePicker datePicker;
        private Button saveBtn, clearBtn;

        public JournalForm()
        {
            InitializeComponent();
            voucherManager = new VoucherManager();
            CreateJournalFormUI();
        }

        private void CreateJournalFormUI()
        {
            this.Text = "Journal Voucher";
            this.Size = new Size(500, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(248, 249, 250);

            // Title
            Label titleLabel = new Label();
            titleLabel.Text = "📒 Journal Voucher";
            titleLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(44, 62, 80);
            titleLabel.Location = new Point(20, 20);
            titleLabel.Size = new Size(300, 30);
            this.Controls.Add(titleLabel);

            // Main container
            GroupBox mainGroup = new GroupBox();
            mainGroup.Text = "Journal Entry Details";
            mainGroup.Font = new Font("Segoe UI", 10);
            mainGroup.Location = new Point(20, 70);
            mainGroup.Size = new Size(450, 350);
            mainGroup.BackColor = Color.White;

            // Voucher Number
            CreateLabel("Journal Number:", 20, 40, mainGroup);
            voucherNumberTxt = CreateTextBox(150, 40, 200, mainGroup);
            voucherNumberTxt.Text = voucherManager.GenerateVoucherNumber("Journal");
            voucherNumberTxt.ReadOnly = true;

            // Date
            CreateLabel("Date:", 20, 80, mainGroup);
            datePicker = new DateTimePicker();
            datePicker.Location = new Point(150, 80);
            datePicker.Size = new Size(200, 25);
            datePicker.Value = DateTime.Now;
            mainGroup.Controls.Add(datePicker);

            // Debit Account
            CreateLabel("Debit Account:", 20, 120, mainGroup);
            debitAccountTxt = CreateTextBox(150, 120, 250, mainGroup);
            debitAccountTxt.PlaceholderText = "Account to debit";

            // Credit Account
            CreateLabel("Credit Account:", 20, 160, mainGroup);
            creditAccountTxt = CreateTextBox(150, 160, 250, mainGroup);
            creditAccountTxt.PlaceholderText = "Account to credit";

            // Amount
            CreateLabel("Amount:", 20, 200, mainGroup);
            amountTxt = CreateTextBox(150, 200, 150, mainGroup);
            amountTxt.KeyPress += AmountTxt_KeyPress;

            // Reference Voucher
            CreateLabel("Reference Voucher:", 20, 240, mainGroup);
            referenceVoucherTxt = CreateTextBox(150, 240, 150, mainGroup);
            referenceVoucherTxt.PlaceholderText = "Optional reference";

            // Description
            CreateLabel("Description:", 20, 280, mainGroup);
            descriptionTxt = CreateTextBox(150, 280, 250, mainGroup);
            descriptionTxt.Multiline = true;
            descriptionTxt.Height = 40;

            this.Controls.Add(mainGroup);

            // Buttons
            saveBtn = CreateButton("Save Journal", Color.FromArgb(46, 204, 113), new Point(20, 440));
            saveBtn.Click += SaveBtn_Click;

            clearBtn = CreateButton("Clear", Color.FromArgb(149, 165, 166), new Point(150, 440));
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
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                e.Handled = true;
            if (e.KeyChar == '.' && (sender as TextBox).Text.IndexOf('.') > -1)
                e.Handled = true;
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(debitAccountTxt.Text) || 
                string.IsNullOrWhiteSpace(creditAccountTxt.Text) || 
                string.IsNullOrWhiteSpace(amountTxt.Text))
            {
                MessageBox.Show("Please enter debit account, credit account and amount!", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(amountTxt.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid amount greater than 0!", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var journalVoucher = new Voucher
            {
                Type = "Journal",
                Number = voucherNumberTxt.Text,
                Date = datePicker.Value,
                Party = $"Dr: {debitAccountTxt.Text} | Cr: {creditAccountTxt.Text}",
                Amount = amount,
                Description = descriptionTxt.Text,
                Status = "Active",
                ReferenceVoucher = referenceVoucherTxt.Text.Trim()
            };

            if (voucherManager.AddVoucher(journalVoucher))
            {
                MessageBox.Show("Journal voucher saved successfully!", "Success", 
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                voucherNumberTxt.Text = voucherManager.GenerateVoucherNumber("Journal");
            }
            else
            {
                MessageBox.Show("Failed to save journal voucher!", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            debitAccountTxt.Clear();
            creditAccountTxt.Clear();
            amountTxt.Clear();
            descriptionTxt.Clear();
            referenceVoucherTxt.Clear();
            datePicker.Value = DateTime.Now;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(300, 250);
            this.Name = "JournalForm";
            this.ResumeLayout(false);
        }
    }
}