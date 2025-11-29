using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SQLite;
using BillingSoftware.Modules;

namespace BillingSoftware.Forms
{
    public partial class SettingsForm : Form
    {
        private DatabaseManager dbManager;
        private TextBox companyNameTxt, addressTxt, phoneTxt, emailTxt, gstTxt, currencyTxt;
        private Button saveBtn, backupBtn, restoreBtn;

        public SettingsForm()
        {
            InitializeComponent();
            dbManager = new DatabaseManager();
            CreateSettingsUI();
            LoadCurrentSettings();
        }

        private void CreateSettingsUI()
        {
            // Form setup
            this.Text = "Application Settings";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(248, 249, 250);

            // Title
            Label titleLabel = new Label();
            titleLabel.Text = "⚙️ Application Settings";
            titleLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(44, 62, 80);
            titleLabel.Location = new Point(20, 20);
            titleLabel.Size = new Size(300, 30);
            this.Controls.Add(titleLabel);

            // Company Settings Group
            GroupBox companyGroup = new GroupBox();
            companyGroup.Text = "Company Information";
            companyGroup.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            companyGroup.Location = new Point(20, 70);
            companyGroup.Size = new Size(550, 250);
            companyGroup.BackColor = Color.White;

            // Company Name
            CreateLabel("Company Name:", 20, 40, companyGroup);
            companyNameTxt = CreateTextBox(150, 40, 350, companyGroup);

            // Address
            CreateLabel("Address:", 20, 80, companyGroup);
            addressTxt = CreateTextBox(150, 80, 350, companyGroup);
            addressTxt.Multiline = true;
            addressTxt.Height = 40;

            // Phone
            CreateLabel("Phone:", 20, 140, companyGroup);
            phoneTxt = CreateTextBox(150, 140, 200, companyGroup);

            // Email
            CreateLabel("Email:", 20, 180, companyGroup);
            emailTxt = CreateTextBox(150, 180, 200, companyGroup);

            // GST Number
            CreateLabel("GST Number:", 20, 220, companyGroup);
            gstTxt = CreateTextBox(150, 220, 200, companyGroup);

            // Currency
            CreateLabel("Currency:", 370, 140, companyGroup);
            currencyTxt = CreateTextBox(450, 140, 50, companyGroup);
            currencyTxt.Text = "₹";

            this.Controls.Add(companyGroup);

            // Buttons
            saveBtn = CreateButton("Save Settings", Color.FromArgb(46, 204, 113), new Point(20, 340));
            saveBtn.Click += SaveBtn_Click;

            backupBtn = CreateButton("Backup Data", Color.FromArgb(52, 152, 219), new Point(150, 340));
            backupBtn.Click += BackupBtn_Click;

            restoreBtn = CreateButton("Restore Data", Color.FromArgb(241, 196, 15), new Point(280, 340));
            restoreBtn.Click += RestoreBtn_Click;

            this.Controls.Add(saveBtn);
            this.Controls.Add(backupBtn);
            this.Controls.Add(restoreBtn);

            // Status label
            Label statusLabel = new Label();
            statusLabel.Text = "Note: Changes will be applied immediately after saving.";
            statusLabel.Font = new Font("Segoe UI", 9);
            statusLabel.ForeColor = Color.Gray;
            statusLabel.Location = new Point(20, 390);
            statusLabel.Size = new Size(400, 20);
            this.Controls.Add(statusLabel);
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

        private void LoadCurrentSettings()
        {
            try
            {
                string sql = "SELECT * FROM company_settings WHERE id = 1";
                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        companyNameTxt.Text = reader["company_name"].ToString();
                        addressTxt.Text = reader["address"].ToString();
                        phoneTxt.Text = reader["phone"].ToString();
                        emailTxt.Text = reader["email"].ToString();
                        gstTxt.Text = reader["gst_number"].ToString();
                        currencyTxt.Text = reader["currency"].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveBtn_Click(object sender, EventArgs e)
        {
            try
            {
                string sql = @"INSERT OR REPLACE INTO company_settings 
                              (id, company_name, address, phone, email, gst_number, currency) 
                              VALUES (1, @company, @address, @phone, @email, @gst, @currency)";

                using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@company", companyNameTxt.Text);
                    cmd.Parameters.AddWithValue("@address", addressTxt.Text);
                    cmd.Parameters.AddWithValue("@phone", phoneTxt.Text);
                    cmd.Parameters.AddWithValue("@email", emailTxt.Text);
                    cmd.Parameters.AddWithValue("@gst", gstTxt.Text);
                    cmd.Parameters.AddWithValue("@currency", currencyTxt.Text);

                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        MessageBox.Show("Settings saved successfully!", "Success", 
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BackupBtn_Click(object sender, EventArgs e)
        {
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "SQLite Database Files (*.db)|*.db|All files (*.*)|*.*";
                saveDialog.FileName = $"billing_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        System.IO.File.Copy("billing.db", saveDialog.FileName, true);
                        MessageBox.Show($"Backup created successfully!\nLocation: {saveDialog.FileName}", 
                                      "Backup Complete", 
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Backup failed: {ex.Message}", "Error", 
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void RestoreBtn_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Restoring data will replace all current data. Continue?", 
                                       "Confirm Restore", 
                                       MessageBoxButtons.YesNo, 
                                       MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                using (var openDialog = new OpenFileDialog())
                {
                    openDialog.Filter = "SQLite Database Files (*.db)|*.db|All files (*.*)|*.*";
                    
                    if (openDialog.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            // Close current connection
                            dbManager.CloseConnection();
                            
                            // Replace current database with backup
                            System.IO.File.Copy(openDialog.FileName, "billing.db", true);
                            
                            // Reopen connection
                            dbManager = new DatabaseManager();
                            
                            MessageBox.Show("Data restored successfully!", "Restore Complete", 
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Restore failed: {ex.Message}", "Error", 
                                          MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(300, 250);
            this.Name = "SettingsForm";
            this.ResumeLayout(false);
        }
    }
}