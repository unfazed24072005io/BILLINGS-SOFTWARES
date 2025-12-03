using System;
using System.Drawing;
using System.Windows.Forms;

namespace BillingSoftware.Utilities
{
    public static class TallyUIStyles
    {
        // Tally-like Color Scheme
        public static Color TallyBlue = Color.FromArgb(0, 51, 102);       // Dark Blue
        public static Color TallyGreen = Color.FromArgb(0, 102, 51);      // Dark Green
        public static Color TallyOrange = Color.FromArgb(255, 102, 0);    // Orange
        public static Color TallyGray = Color.FromArgb(240, 240, 240);    // Light Gray
        public static Color TallyDarkGray = Color.FromArgb(64, 64, 64);   // Dark Gray
        public static Color TallyWhite = Color.White;
        public static Color TallyRed = Color.FromArgb(204, 0, 0);         // Red
        public static Color TallyYellow = Color.FromArgb(255, 204, 0);    // Yellow
        public static Color TallyPurple = Color.FromArgb(155, 89, 182);   // Purple
        public static Color DarkText = Color.FromArgb(44, 62, 80);        // Dark Text
        public static Color White = Color.White;
        
        // Tally-like Fonts
        public static Font TitleFont = new Font("Segoe UI", 14, FontStyle.Bold);
        public static Font HeaderFont = new Font("Segoe UI", 11, FontStyle.Bold);
        public static Font NormalFont = new Font("Segoe UI", 9);
        public static Font SmallFont = new Font("Segoe UI", 8);
        public static Font MonospaceFont = new Font("Consolas", 9);
        
        /// <summary>
        /// Creates Tally-style Panel Container
        /// </summary>
        public static Panel CreateTallyPanel(string title, Point location, Size size, bool withBorder = true)
        {
            Panel panel = new Panel
            {
                Location = location,
                Size = size,
                BackColor = TallyGray,
                BorderStyle = withBorder ? BorderStyle.FixedSingle : BorderStyle.None
            };
            
            if (!string.IsNullOrEmpty(title))
            {
                Label titleLabel = new Label
                {
                    Text = title,
                    Font = HeaderFont,
                    ForeColor = TallyBlue,
                    Location = new Point(10, 10),
                    Size = new Size(size.Width - 20, 25),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                panel.Controls.Add(titleLabel);
            }
            
            return panel;
        }
        
        /// <summary>
        /// Creates Tally-style DataGridView
        /// </summary>
        public static DataGridView CreateTallyGrid(Point location, Size size)
        {
            DataGridView grid = new DataGridView
            {
                Location = location,
                Size = size,
                BackgroundColor = TallyWhite,
                BorderStyle = BorderStyle.FixedSingle,
                Font = MonospaceFont,
                ReadOnly = true,
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                GridColor = Color.FromArgb(200, 200, 200),
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Padding = new Padding(3),
                    SelectionBackColor = Color.FromArgb(220, 220, 255),
                    SelectionForeColor = TallyDarkGray
                },
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(248, 248, 248)
                }
            };
            
            // Tally-style Headers
            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = TallyBlue,
                ForeColor = TallyWhite,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Padding = new Padding(3)
            };
            
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            grid.ColumnHeadersHeight = 30;
            
            return grid;
        }
        
        /// <summary>
        /// Creates Tally-style Button
        /// </summary>
        public static Button CreateTallyButton(string text, Color color, Point location, Size? size = null)
        {
            Button btn = new Button
            {
                Text = text,
                BackColor = color,
                ForeColor = TallyWhite,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Size = size ?? new Size(120, 30),
                Location = location,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            
            btn.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(color, 0.2f);
            btn.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(color, 0.2f);
            
            return btn;
        }
        
        /// <summary>
        /// Creates Tally-style TextBox
        /// </summary>
        public static TextBox CreateTallyTextBox(Point location, Size size, string placeholder = "")
        {
            TextBox txt = new TextBox
            {
                Location = location,
                Size = size,
                Font = NormalFont,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = TallyWhite
            };
            
            if (!string.IsNullOrEmpty(placeholder))
            {
                SetTallyPlaceholder(txt, placeholder);
            }
            
            return txt;
        }
        
        /// <summary>
        /// Creates Tally-style ComboBox
        /// </summary>
        public static ComboBox CreateTallyComboBox(Point location, Size size)
        {
            ComboBox combo = new ComboBox
            {
                Location = location,
                Size = size,
                Font = NormalFont,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat
            };
            
            return combo;
        }
        
        /// <summary>
        /// Creates Tally-style Label
        /// </summary>
        public static Label CreateTallyLabel(string text, Point location, Size size, bool bold = false)
        {
            return new Label
            {
                Text = text,
                Location = location,
                Size = size,
                Font = bold ? new Font("Segoe UI", 9, FontStyle.Bold) : NormalFont,
                ForeColor = TallyDarkGray,
                TextAlign = ContentAlignment.MiddleLeft
            };
        }
        
        /// <summary>
        /// Sets placeholder text in Tally style
        /// </summary>
        public static void SetTallyPlaceholder(TextBox textBox, string placeholder)
        {
            textBox.Enter += (s, e) =>
            {
                if (textBox.Text == placeholder)
                {
                    textBox.Text = "";
                    textBox.ForeColor = SystemColors.WindowText;
                }
            };
            
            textBox.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = placeholder;
                    textBox.ForeColor = SystemColors.GrayText;
                }
            };
            
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = placeholder;
                textBox.ForeColor = SystemColors.GrayText;
            }
        }
        
        /// <summary>
        /// Creates Tally-style Status Strip
        /// </summary>
        public static StatusStrip CreateTallyStatusStrip()
        {
            StatusStrip statusStrip = new StatusStrip
            {
                BackColor = TallyBlue,
                ForeColor = TallyWhite,
                Font = SmallFont
            };
            
            return statusStrip;
        }
        
        /// <summary>
        /// Creates Tally-style Menu Strip
        /// </summary>
        public static MenuStrip CreateTallyMenuStrip()
        {
            MenuStrip menuStrip = new MenuStrip
            {
                BackColor = TallyBlue,
                ForeColor = TallyWhite,
                Font = new Font("Segoe UI", 10)
            };
            
            return menuStrip;
        }
        
        /// <summary>
        /// Creates Tally-style Tab Control
        /// </summary>
        public static TabControl CreateTallyTabControl()
        {
            TabControl tabControl = new TabControl
            {
                Appearance = TabAppearance.Normal,
                SizeMode = TabSizeMode.Fixed,
                ItemSize = new Size(100, 25),
                Font = new Font("Segoe UI", 9),
                Padding = new Point(10, 5)
            };
            
            return tabControl;
        }
        
        /// <summary>
        /// Creates Tally-style Group Box
        /// </summary>
        public static GroupBox CreateTallyGroupBox(string text, Point location, Size size)
        {
            GroupBox groupBox = new GroupBox
            {
                Text = text,
                Location = location,
                Size = size,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = TallyBlue,
                BackColor = TallyGray
            };
            
            return groupBox;
        }
        
        /// <summary>
        /// Creates Tally-style Numeric UpDown
        /// </summary>
        public static NumericUpDown CreateTallyNumericUpDown(Point location, Size size)
        {
            NumericUpDown numeric = new NumericUpDown
            {
                Location = location,
                Size = size,
                Font = NormalFont,
                BorderStyle = BorderStyle.FixedSingle,
                ThousandsSeparator = true
            };
            
            return numeric;
        }
        
        /// <summary>
        /// Creates Tally-style DateTimePicker
        /// </summary>
        public static DateTimePicker CreateTallyDateTimePicker(Point location, Size size)
        {
            DateTimePicker datePicker = new DateTimePicker
            {
                Location = location,
                Size = size,
                Font = NormalFont,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd-MMM-yyyy"
            };
            
            return datePicker;
        }
        
        /// <summary>
        /// Creates Tally-style CheckBox
        /// </summary>
        public static CheckBox CreateTallyCheckBox(string text, Point location, Size size)
        {
            CheckBox checkBox = new CheckBox
            {
                Text = text,
                Location = location,
                Size = size,
                Font = NormalFont,
                ForeColor = TallyDarkGray
            };
            
            return checkBox;
        }
    }
}