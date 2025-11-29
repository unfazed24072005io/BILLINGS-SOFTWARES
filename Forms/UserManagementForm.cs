using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using BillingSoftware.Modules;
using BillingSoftware.Models;

namespace BillingSoftware.Forms
{
    public partial class UserManagementForm : Form
    {
        private UserManager userManager;
        private DataGridView usersGridView;
        private Button addUserBtn, editUserBtn, deleteUserBtn, refreshBtn;
        private TextBox usernameTxt, passwordTxt;
        private ComboBox roleCombo;
        private CheckBox activeCheckBox;

        public UserManagementForm()
        {
            InitializeComponent();
            userManager = new UserManager();
            CreateUserManagementUI();
            LoadUsersData();
        }

        private void CreateUserManagementUI()
        {
            // Form setup
            this.Text = "User Management";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(248, 249, 250);

            // Title
            Label titleLabel = new Label();
            titleLabel.Text = "👥 User Management";
            titleLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            titleLabel.ForeColor = Color.FromArgb(44, 62, 80);
            titleLabel.Location = new Point(20, 20);
            titleLabel.Size = new Size(300, 30);
            this.Controls.Add(titleLabel);

            // Input panel
            GroupBox inputGroup = new GroupBox();
            inputGroup.Text = "Add/Edit User";
            inputGroup.Font = new Font("Segoe UI", 10);
            inputGroup.Location = new Point(20, 70);
            inputGroup.Size = new Size(350, 200);
            inputGroup.BackColor = Color.White;

            // Username
            Label usernameLabel = new Label();
            usernameLabel.Text = "Username:";
            usernameLabel.Location = new Point(20, 40);
            usernameLabel.Size = new Size(80, 20);
            inputGroup.Controls.Add(usernameLabel);

            usernameTxt = new TextBox();
            usernameTxt.Location = new Point(120, 40);
            usernameTxt.Size = new Size(200, 25);
            inputGroup.Controls.Add(usernameTxt);

            // Password
            Label passwordLabel = new Label();
            passwordLabel.Text = "Password:";
            passwordLabel.Location = new Point(20, 80);
            passwordLabel.Size = new Size(80, 20);
            inputGroup.Controls.Add(passwordLabel);

            passwordTxt = new TextBox();
            passwordTxt.Location = new Point(120, 80);
            passwordTxt.Size = new Size(200, 25);
            passwordTxt.UseSystemPasswordChar = true;
            inputGroup.Controls.Add(passwordTxt);

            // Role
            Label roleLabel = new Label();
            roleLabel.Text = "Role:";
            roleLabel.Location = new Point(20, 120);
            roleLabel.Size = new Size(80, 20);
            inputGroup.Controls.Add(roleLabel);

            roleCombo = new ComboBox();
            roleCombo.Location = new Point(120, 120);
            roleCombo.Size = new Size(200, 25);
            roleCombo.Items.AddRange(new string[] { "User", "Admin" });
            roleCombo.SelectedIndex = 0;
            inputGroup.Controls.Add(roleCombo);

            // Active checkbox
            activeCheckBox = new CheckBox();
            activeCheckBox.Text = "Active User";
            activeCheckBox.Location = new Point(120, 160);
            activeCheckBox.Size = new Size(120, 20);
            activeCheckBox.Checked = true;
            inputGroup.Controls.Add(activeCheckBox);

            this.Controls.Add(inputGroup);

            // Buttons
            addUserBtn = CreateButton("Add User", Color.FromArgb(46, 204, 113), new Point(20, 290));
            addUserBtn.Click += AddUserBtn_Click;

            editUserBtn = CreateButton("Edit User", Color.FromArgb(52, 152, 219), new Point(130, 290));
            editUserBtn.Click += EditUserBtn_Click;

            deleteUserBtn = CreateButton("Delete User", Color.FromArgb(231, 76, 60), new Point(240, 290));
            deleteUserBtn.Click += DeleteUserBtn_Click;

            refreshBtn = CreateButton("Refresh", Color.FromArgb(149, 165, 166), new Point(20, 330));
            refreshBtn.Click += (s, e) => LoadUsersData();

            this.Controls.Add(addUserBtn);
            this.Controls.Add(editUserBtn);
            this.Controls.Add(deleteUserBtn);
            this.Controls.Add(refreshBtn);

            // Users grid
            usersGridView = new DataGridView();
            usersGridView.Location = new Point(390, 70);
            usersGridView.Size = new Size(370, 350);
            usersGridView.BackgroundColor = Color.White;
            usersGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            usersGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            usersGridView.ReadOnly = true;
            usersGridView.Font = new Font("Segoe UI", 9);

            this.Controls.Add(usersGridView);
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
            btn.Size = new Size(100, 35);
            btn.Location = location;
            btn.Cursor = Cursors.Hand;
            return btn;
        }

        private void LoadUsersData()
        {
            var users = userManager.GetAllUsers();
            usersGridView.DataSource = users;
            usersGridView.Refresh();
        }

        private void AddUserBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(usernameTxt.Text) || string.IsNullOrWhiteSpace(passwordTxt.Text))
            {
                MessageBox.Show("Please enter username and password!", "Validation Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var newUser = new User
            {
                Username = usernameTxt.Text.Trim(),
                Password = passwordTxt.Text,
                Role = roleCombo.SelectedItem.ToString(),
                IsActive = activeCheckBox.Checked
            };

            if (userManager.AddUser(newUser))
            {
                MessageBox.Show("User added successfully!", "Success", 
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadUsersData();
            }
            else
            {
                MessageBox.Show("Failed to add user. Username might already exist.", "Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void EditUserBtn_Click(object sender, EventArgs e)
        {
            if (usersGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a user to edit!", "Selection Required", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedUser = usersGridView.SelectedRows[0].DataBoundItem as User;
            if (selectedUser != null)
            {
                // Open edit dialog or populate form
                usernameTxt.Text = selectedUser.Username;
                passwordTxt.Text = selectedUser.Password;
                roleCombo.SelectedItem = selectedUser.Role;
                activeCheckBox.Checked = selectedUser.IsActive;

                // Change add button to update
                addUserBtn.Text = "Update User";
                addUserBtn.Click -= AddUserBtn_Click;
                addUserBtn.Click += (s, e) => UpdateUser(selectedUser.Id);
            }
        }

        private void UpdateUser(int userId)
        {
            var user = new User
            {
                Id = userId,
                Username = usernameTxt.Text.Trim(),
                Password = passwordTxt.Text,
                Role = roleCombo.SelectedItem.ToString(),
                IsActive = activeCheckBox.Checked
            };

            if (userManager.UpdateUser(user))
            {
                MessageBox.Show("User updated successfully!", "Success", 
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
                LoadUsersData();
                addUserBtn.Text = "Add User";
                addUserBtn.Click -= (EventHandler)null;
                addUserBtn.Click += AddUserBtn_Click;
            }
        }

        private void DeleteUserBtn_Click(object sender, EventArgs e)
        {
            if (usersGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a user to delete!", "Selection Required", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedUser = usersGridView.SelectedRows[0].DataBoundItem as User;
            if (selectedUser != null)
            {
                var result = MessageBox.Show($"Are you sure you want to delete user '{selectedUser.Username}'?", 
                                           "Confirm Delete", 
                                           MessageBoxButtons.YesNo, 
                                           MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Soft delete by deactivating
                    selectedUser.IsActive = false;
                    if (userManager.UpdateUser(selectedUser))
                    {
                        MessageBox.Show("User deactivated successfully!", "Success", 
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadUsersData();
                    }
                }
            }
        }

        private void ClearForm()
        {
            usernameTxt.Clear();
            passwordTxt.Clear();
            roleCombo.SelectedIndex = 0;
            activeCheckBox.Checked = true;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(300, 250);
            this.Name = "UserManagementForm";
            this.ResumeLayout(false);
        }
    }
}