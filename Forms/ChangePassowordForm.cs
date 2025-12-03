using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SQLite;
using BillingSoftware.Modules;

namespace BillingSoftware.Forms
{
    public partial class ChangePasswordForm : Form
    {
        private DatabaseManager dbManager;
        private TextBox oldPasswordTxt, newPasswordTxt, confirmPasswordTxt;
        private Button saveBtn, cancelBtn;

        public ChangePasswordForm()
        {
            InitializeComponent();
            dbManager = new DatabaseManager();
            CreateChangePasswordUI();
        }

        private void CreateChangePasswordUI()
        {
            this.Text = "Change Password";
            this.Size = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(248, 249, 250);

            Label titleLabel = new Label();
            titleLabel.Text = "🔐 Change Password";
            titleLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(44, 62, 80);
            titleLabel.Location = new Point(20, 20);
            titleLabel.Size = new Size(300, 30);
            this.Controls.Add(titleLabel);

            // Old Password
            CreateLabel("Old Password:", 20, 70);
            oldPasswordTxt = CreateTextBox(150, 70, 200);
            oldPasswordTxt.PasswordChar = '•';

            // New Password
            CreateLabel("New Password:", 20, 110);
            newPasswordTxt = CreateTextBox(150, 110, 200);
            newPasswordTxt.PasswordChar = '•';

            // Confirm Password
            CreateLabel("Confirm Password:", 20, 150);
            confirmPasswordTxt = CreateTextBox(150, 150, 200);
            confirmPasswordTxt.PasswordChar = '•';

            // Buttons
            saveBtn = CreateButton("💾 Save", Color.FromArgb(46, 204, 113), new Point(100, 190));
            saveBtn.Click += SaveBtn_Click;

            cancelBtn = CreateButton("❌ Cancel", Color.FromArgb(149, 165, 166), new Point(220, 190));
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

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(oldPasswordTxt.Text))
            {
                MessageBox.Show("Please enter old password!", "Validation Error");
                return;
            }

            if (string.IsNullOrWhiteSpace(newPasswordTxt.Text))
            {
                MessageBox.Show("Please enter new password!", "Validation Error");
                return;
            }

            if (newPasswordTxt.Text.Length < 6)
            {
                MessageBox.Show("New password must be at least 6 characters!", "Validation Error");
                return;
            }

            if (newPasswordTxt.Text != confirmPasswordTxt.Text)
            {
                MessageBox.Show("New password and confirmation do not match!", "Validation Error");
                return;
            }

            try
            {
                // Verify old password
                string verifySql = "SELECT password FROM users WHERE username = @username";
                using (var cmd = new SQLiteCommand(verifySql, dbManager.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@username", Program.CurrentUser);
                    string storedPassword = cmd.ExecuteScalar()?.ToString();

                    if (storedPassword != oldPasswordTxt.Text)
                    {
                        MessageBox.Show("Old password is incorrect!", "Validation Error");
                        return;
                    }
                }

                // Update password
                string updateSql = "UPDATE users SET password = @newPassword WHERE username = @username";
                using (var cmd = new SQLiteCommand(updateSql, dbManager.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@newPassword", newPasswordTxt.Text);
                    cmd.Parameters.AddWithValue("@username", Program.CurrentUser);

                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        MessageBox.Show("Password changed successfully!", "Success");
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error changing password: {ex.Message}", "Error");
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(300, 200);
            this.Name = "ChangePasswordForm";
            this.ResumeLayout(false);
        }
    }
}