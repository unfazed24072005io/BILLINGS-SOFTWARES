using System;
using System.Drawing;
using System.Windows.Forms;

namespace BillingSoftware.Utilities
{
    public static class UIHelper
    {
        // Modern color scheme
        public static Color PrimaryGreen = Color.FromArgb(46, 204, 113);
        public static Color PrimaryBlue = Color.FromArgb(52, 152, 219);
        public static Color PrimaryRed = Color.FromArgb(231, 76, 60);
        public static Color PrimaryOrange = Color.FromArgb(230, 126, 34);
        public static Color PrimaryPurple = Color.FromArgb(155, 89, 182);
        public static Color DarkText = Color.FromArgb(44, 62, 80);
        public static Color LightBg = Color.FromArgb(248, 249, 250);
        public static Color White = Color.White;
        public static Color BorderColor = Color.FromArgb(206, 212, 218);

        // Fonts
        public static Font TitleFont = new Font("Segoe UI", 16, FontStyle.Bold);
        public static Font SubtitleFont = new Font("Segoe UI", 12, FontStyle.Bold);
        public static Font NormalFont = new Font("Segoe UI", 9);
        public static Font SmallFont = new Font("Segoe UI", 8);

        /// <summary>
        /// Creates a modern styled button with consistent appearance
        /// </summary>
        public static Button CreateModernButton(string text, Color backgroundColor, Point location, Size? size = null)
        {
            var btn = new Button
            {
                Text = text,
                BackColor = backgroundColor,
                ForeColor = White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Size = size ?? new Size(120, 35),
                Location = location,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(backgroundColor);
            btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(backgroundColor);

            return btn;
        }

        /// <summary>
        /// Creates a textbox with consistent styling
        /// </summary>
        public static TextBox CreateTextBox(Point location, Size size, string placeholder = "", bool isMultiline = false)
        {
            var txt = new TextBox
            {
                Location = location,
                Size = size,
                Font = NormalFont,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = White
            };

            if (isMultiline)
            {
                txt.Multiline = true;
                txt.ScrollBars = ScrollBars.Vertical;
            }

            // Set placeholder text using native Windows feature if available
            if (!string.IsNullOrEmpty(placeholder))
            {
                SetPlaceholderText(txt, placeholder);
            }

            return txt;
        }

        /// <summary>
        /// Sets placeholder text for a textbox
        /// </summary>
        public static void SetPlaceholderText(TextBox textBox, string placeholder)
        {
            // For .NET Framework 4.0+, we can use this approach
            bool placeholderActive = true;

            textBox.Enter += (s, e) =>
            {
                if (placeholderActive)
                {
                    textBox.Text = "";
                    textBox.ForeColor = SystemColors.WindowText;
                    placeholderActive = false;
                }
            };

            textBox.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = placeholder;
                    textBox.ForeColor = SystemColors.GrayText;
                    placeholderActive = true;
                }
            };

            // Initialize
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = placeholder;
                textBox.ForeColor = SystemColors.GrayText;
            }
        }

        /// <summary>
        /// Creates a modern DataGridView with consistent styling
        /// </summary>
        public static DataGridView CreateModernDataGridView(Point location, Size size)
        {
            var grid = new DataGridView
            {
                Location = location,
                Size = size,
                BackgroundColor = White,
                BorderStyle = BorderStyle.None,
                Font = NormalFont,
                ReadOnly = true,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                GridColor = BorderColor
            };

            // Style the headers
            grid.ColumnHeadersDefaultCellStyle.BackColor = PrimaryBlue;
            grid.ColumnHeadersDefaultCellStyle.ForeColor = White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grid.EnableHeadersVisualStyles = false;

            // Style alternating rows
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);

            return grid;
        }

        /// <summary>
        /// Creates a label with consistent styling
        /// </summary>
        public static Label CreateLabel(string text, Point location, Size size, Font font = null, Color? foreColor = null)
        {
            return new Label
            {
                Text = text,
                Location = location,
                Size = size,
                Font = font ?? NormalFont,
                ForeColor = foreColor ?? DarkText,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }

        /// <summary>
        /// Creates a group box with consistent styling
        /// </summary>
        public static GroupBox CreateGroupBox(string text, Point location, Size size)
        {
            return new GroupBox
            {
                Text = text,
                Location = location,
                Size = size,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = White
            };
        }

        /// <summary>
        /// Shows a success message dialog
        /// </summary>
        public static void ShowSuccess(string message, string title = "Success")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Shows an error message dialog
        /// </summary>
        public static void ShowError(string message, string title = "Error")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Shows a warning message dialog
        /// </summary>
        public static void ShowWarning(string message, string title = "Warning")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Shows a confirmation dialog
        /// </summary>
        public static DialogResult ShowConfirmation(string message, string title = "Confirm")
        {
            return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        /// <summary>
        /// Centers a child form relative to its parent
        /// </summary>
        public static void CenterForm(Form childForm, Form parentForm)
        {
            childForm.StartPosition = FormStartPosition.Manual;
            childForm.Location = new Point(
                parentForm.Location.X + (parentForm.Width - childForm.Width) / 2,
                parentForm.Location.Y + (parentForm.Height - childForm.Height) / 2
            );
        }

        /// <summary>
        /// Formats a decimal value as currency
        /// </summary>
        public static string FormatCurrency(decimal amount)
        {
            return $"₹{amount:N2}";
        }

        /// <summary>
        /// Formats a date in short format
        /// </summary>
        public static string FormatDate(DateTime date)
        {
            return date.ToString("dd-MMM-yyyy");
        }

        /// <summary>
        /// Applies modern form styling
        /// </summary>
        public static void ApplyModernFormStyle(Form form)
        {
            form.BackColor = LightBg;
            form.Font = NormalFont;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MaximizeBox = false;
            form.MinimizeBox = false;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
        }
    }
}