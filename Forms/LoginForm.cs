using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SQLite;
using BillingSoftware.Modules;

namespace BillingSoftware.Forms
{
    public partial class LoginForm : Form
    {
        private DatabaseManager dbManager;
        private TextBox usernameTxt, passwordTxt;
        private Button loginBtn, exitBtn;
        private CheckBox rememberCheck;
        private Label titleLabel, errorLabel;
        
        public string LoggedInUsername { get; private set; } = "";
        public string UserRole { get; private set; } = "";

        public LoginForm()
        {
            InitializeComponent();
            dbManager = new DatabaseManager();
            CreateLoginUI();
            CreateDefaultAdmin();
        }

        private void CreateLoginUI()
        {
            this.Text = "Billing Software - Login";
            this.Size = new Size(400, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(248, 249, 250);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Padding = new Padding(20);

            // Title Panel
            Panel titlePanel = new Panel();
            titlePanel.BackColor = Color.FromArgb(52, 152, 219);
            titlePanel.Location = new Point(0, 0);
            titlePanel.Size = new Size(400, 70);
            titlePanel.Dock = DockStyle.Top;

            titleLabel = new Label();
            titleLabel.Text = "🔐 Billing Software";
            titleLabel.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.Location = new Point(20, 20);
            titleLabel.Size = new Size(360, 35);
            titlePanel.Controls.Add(titleLabel);

            this.Controls.Add(titlePanel);

            // Login Container
            Panel loginPanel = new Panel();
            loginPanel.BackColor = Color.White;
            loginPanel.Location = new Point(40, 90);
            loginPanel.Size = new Size(320, 200);
            loginPanel.Padding = new Padding(20);

            // Username
            Label userLabel = new Label();
            userLabel.Text = "👤 Username:";
            userLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            userLabel.ForeColor = Color.FromArgb(44, 62, 80);
            userLabel.Location = new Point(20, 30);
            userLabel.Size = new Size(100, 25);
            loginPanel.Controls.Add(userLabel);

            usernameTxt = new TextBox();
            usernameTxt.Location = new Point(130, 30);
            usernameTxt.Size = new Size(150, 25);
            usernameTxt.Font = new Font("Segoe UI", 10);
            usernameTxt.PlaceholderText = "Enter username";
            loginPanel.Controls.Add(usernameTxt);

            // Password
            Label passLabel = new Label();
            passLabel.Text = "🔒 Password:";
            passLabel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            passLabel.ForeColor = Color.FromArgb(44, 62, 80);
            passLabel.Location = new Point(20, 80);
            passLabel.Size = new Size(100, 25);
            loginPanel.Controls.Add(passLabel);

            passwordTxt = new TextBox();
            passwordTxt.Location = new Point(130, 80);
            passwordTxt.Size = new Size(150, 25);
            passwordTxt.Font = new Font("Segoe UI", 10);
            passwordTxt.PasswordChar = '•';
            passwordTxt.PlaceholderText = "Enter password";
            loginPanel.Controls.Add(passwordTxt);

            // Remember Me
            rememberCheck = new CheckBox();
            rememberCheck.Text = "Remember me";
            rememberCheck.Font = new Font("Segoe UI", 9);
            rememberCheck.ForeColor = Color.FromArgb(44, 62, 80);
            rememberCheck.Location = new Point(130, 115);
            rememberCheck.Size = new Size(120, 20);
            loginPanel.Controls.Add(rememberCheck);

            // Error Label
            errorLabel = new Label();
            errorLabel.Text = "";
            errorLabel.Font = new Font("Segoe UI", 9);
            errorLabel.ForeColor = Color.FromArgb(231, 76, 60);
            errorLabel.Location = new Point(20, 140);
            errorLabel.Size = new Size(280, 20);
            errorLabel.TextAlign = ContentAlignment.MiddleCenter;
            loginPanel.Controls.Add(errorLabel);

            this.Controls.Add(loginPanel);

            // Buttons
            loginBtn = new Button();
            loginBtn.Text = "🚀 Login";
            loginBtn.BackColor = Color.FromArgb(46, 204, 113);
            loginBtn.ForeColor = Color.White;
            loginBtn.FlatStyle = FlatStyle.Flat;
            loginBtn.FlatAppearance.BorderSize = 0;
            loginBtn.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            loginBtn.Size = new Size(120, 40);
            loginBtn.Location = new Point(80, 300);
            loginBtn.Cursor = Cursors.Hand;
            loginBtn.Click += LoginBtn_Click;
            this.Controls.Add(loginBtn);

            exitBtn = new Button();
            exitBtn.Text = "❌ Exit";
            exitBtn.BackColor = Color.FromArgb(149, 165, 166);
            exitBtn.ForeColor = Color.White;
            exitBtn.FlatStyle = FlatStyle.Flat;
            exitBtn.FlatAppearance.BorderSize = 0;
            exitBtn.Font = new Font("Segoe UI", 11);
            exitBtn.Size = new Size(120, 40);
            exitBtn.Location = new Point(210, 300);
            exitBtn.Cursor = Cursors.Hand;
            exitBtn.Click += (s, e) => Application.Exit();
            this.Controls.Add(exitBtn);

            // Load saved credentials
            LoadSavedCredentials();
        }

        private void CreateDefaultAdmin()
        {
            try
            {
                // Create users table if not exists
                string createTable = @"CREATE TABLE IF NOT EXISTS users (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    username TEXT UNIQUE NOT NULL,
                    password TEXT NOT NULL,
                    role TEXT DEFAULT 'User',
                    full_name TEXT,
                    email TEXT,
                    is_active INTEGER DEFAULT 1,
                    created_date TEXT DEFAULT CURRENT_TIMESTAMP
                )";

                using (var cmd = new SQLiteCommand(createTable, dbManager.GetConnection()))
                {
                    cmd.ExecuteNonQuery();
                }

                // Insert default admin if not exists
                string checkAdmin = "SELECT COUNT(*) FROM users WHERE username = 'admin'";
                using (var cmd = new SQLiteCommand(checkAdmin, dbManager.GetConnection()))
                {
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    if (count == 0)
                    {
                        string insertAdmin = @"INSERT INTO users (username, password, role, full_name) 
                                             VALUES ('admin', 'admin123', 'Admin', 'Administrator')";
                        using (var insertCmd = new SQLiteCommand(insertAdmin, dbManager.GetConnection()))
                        {
                            insertCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating default admin: {ex.Message}", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSavedCredentials()
        {
            try
            {
                if (Properties.Settings.Default.RememberMe)
                {
                    usernameTxt.Text = Properties.Settings.Default.Username;
                    passwordTxt.Text = Properties.Settings.Default.Password;
                    rememberCheck.Checked = true;
                }
            }
            catch
            {
                // Settings not configured yet
            }
        }

        private void LoginBtn_Click(object sender, EventArgs e)
{
    string username = usernameTxt.Text.Trim();
    string password = passwordTxt.Text;

    if (string.IsNullOrWhiteSpace(username))
    {
        ShowError("Please enter username!");
        return;
    }

    if (string.IsNullOrWhiteSpace(password))
    {
        ShowError("Please enter password!");
        return;
    }

    try
    {
        string sql = "SELECT username, password, role FROM users WHERE username = @username AND is_active = 1";
        using (var cmd = new SQLiteCommand(sql, dbManager.GetConnection()))
        {
            cmd.Parameters.AddWithValue("@username", username);
            
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    string storedPassword = reader["password"].ToString();
                    string role = reader["role"].ToString();

                    // Simple password check
                    if (password == storedPassword)
                    {
                        // Save credentials if remember me is checked
                        if (rememberCheck.Checked)
                        {
                            Properties.Settings.Default.Username = username;
                            Properties.Settings.Default.Password = password;
                            Properties.Settings.Default.RememberMe = true;
                            Properties.Settings.Default.Save();
                        }
                        else
                        {
                            Properties.Settings.Default.Reset();
                        }

                        LoggedInUsername = username;
                        UserRole = role;
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                    else
                    {
                        ShowError("Invalid password!");
                    }
                }
                else
                {
                    // FIX: If admin not found, create it
                    if (username == "admin")
                    {
                        CreateDefaultAdminUser();
                        ShowError("Default admin created. Please login again with admin/admin123");
                    }
                    else
                    {
                        ShowError("User not found!");
                    }
                }
            }
        }
    }
    catch (Exception ex)
    {
        ShowError($"Login error: {ex.Message}");
    }
}
private void CreateDefaultAdminUser()
{
    try
    {
        string insertAdmin = @"INSERT INTO users (username, password, role, full_name) 
                             VALUES ('admin', 'admin123', 'Admin', 'Administrator')";
        using (var cmd = new SQLiteCommand(insertAdmin, dbManager.GetConnection()))
        {
            cmd.ExecuteNonQuery();
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Error creating admin: {ex.Message}", "Error");
    }
}

        private void ShowError(string message)
        {
            errorLabel.Text = message;
            errorLabel.ForeColor = Color.FromArgb(231, 76, 60);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(300, 200);
            this.Name = "LoginForm";
            this.ResumeLayout(false);
        }
    }
}