using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ChatAppClient.Helpers
{
    /// <summary>
    /// Qu?n lý theme Dark/Light mode cho ?ng d?ng
    /// </summary>
    public static class ThemeManager
    {
        // Event khi theme thay ??i
        public static event EventHandler<bool>? ThemeChanged;

        // Tr?ng thái hi?n t?i
        private static bool _isDarkMode = true;
        public static bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    SaveThemePreference();
                    ThemeChanged?.Invoke(null, value);
                }
            }
        }

        // === DARK MODE COLORS ===
        public static class Dark
        {
            public static readonly Color Background = Color.FromArgb(54, 57, 63);
            public static readonly Color BackgroundSecondary = Color.FromArgb(47, 49, 54);
            public static readonly Color BackgroundTertiary = Color.FromArgb(32, 34, 37);
            public static readonly Color TextPrimary = Color.White;
            public static readonly Color TextSecondary = Color.FromArgb(185, 187, 190);
            public static readonly Color TextMuted = Color.FromArgb(114, 118, 125);
            public static readonly Color Primary = Color.FromArgb(88, 101, 242);
            public static readonly Color PrimaryHover = Color.FromArgb(71, 82, 196);
            public static readonly Color Success = Color.FromArgb(67, 181, 129);
            public static readonly Color Danger = Color.FromArgb(237, 66, 69);
            public static readonly Color Warning = Color.FromArgb(250, 166, 26);
            public static readonly Color InputBackground = Color.FromArgb(64, 68, 75);
            public static readonly Color InputBorder = Color.FromArgb(79, 84, 92);
            public static readonly Color MessageOutgoing = Color.FromArgb(88, 101, 242);
            public static readonly Color MessageIncoming = Color.FromArgb(64, 68, 75);
            public static readonly Color Online = Color.FromArgb(35, 165, 89);
            public static readonly Color Offline = Color.FromArgb(116, 127, 141);
            public static readonly Color Scrollbar = Color.FromArgb(32, 34, 37);
            public static readonly Color ScrollbarThumb = Color.FromArgb(79, 84, 92);
        }

        // === LIGHT MODE COLORS ===
        public static class Light
        {
            public static readonly Color Background = Color.White;
            public static readonly Color BackgroundSecondary = Color.FromArgb(242, 243, 245);
            public static readonly Color BackgroundTertiary = Color.FromArgb(227, 229, 232);
            public static readonly Color TextPrimary = Color.FromArgb(6, 6, 7);
            public static readonly Color TextSecondary = Color.FromArgb(79, 86, 96);
            public static readonly Color TextMuted = Color.FromArgb(116, 127, 141);
            public static readonly Color Primary = Color.FromArgb(88, 101, 242);
            public static readonly Color PrimaryHover = Color.FromArgb(71, 82, 196);
            public static readonly Color Success = Color.FromArgb(35, 134, 54);
            public static readonly Color Danger = Color.FromArgb(218, 55, 60);
            public static readonly Color Warning = Color.FromArgb(250, 166, 26);
            public static readonly Color InputBackground = Color.FromArgb(235, 237, 239);
            public static readonly Color InputBorder = Color.FromArgb(204, 206, 209);
            public static readonly Color MessageOutgoing = Color.FromArgb(88, 101, 242);
            public static readonly Color MessageIncoming = Color.FromArgb(235, 237, 239);
            public static readonly Color Online = Color.FromArgb(35, 165, 89);
            public static readonly Color Offline = Color.FromArgb(116, 127, 141);
            public static readonly Color Scrollbar = Color.FromArgb(227, 229, 232);
            public static readonly Color ScrollbarThumb = Color.FromArgb(185, 187, 190);
        }

        // === CURRENT THEME COLORS (tr? v? màu theo theme hi?n t?i) ===
        public static Color Background => _isDarkMode ? Dark.Background : Light.Background;
        public static Color BackgroundSecondary => _isDarkMode ? Dark.BackgroundSecondary : Light.BackgroundSecondary;
        public static Color BackgroundTertiary => _isDarkMode ? Dark.BackgroundTertiary : Light.BackgroundTertiary;
        public static Color TextPrimary => _isDarkMode ? Dark.TextPrimary : Light.TextPrimary;
        public static Color TextSecondary => _isDarkMode ? Dark.TextSecondary : Light.TextSecondary;
        public static Color TextMuted => _isDarkMode ? Dark.TextMuted : Light.TextMuted;
        public static Color Primary => _isDarkMode ? Dark.Primary : Light.Primary;
        public static Color PrimaryHover => _isDarkMode ? Dark.PrimaryHover : Light.PrimaryHover;
        public static Color Success => _isDarkMode ? Dark.Success : Light.Success;
        public static Color Danger => _isDarkMode ? Dark.Danger : Light.Danger;
        public static Color Warning => _isDarkMode ? Dark.Warning : Light.Warning;
        public static Color InputBackground => _isDarkMode ? Dark.InputBackground : Light.InputBackground;
        public static Color InputBorder => _isDarkMode ? Dark.InputBorder : Light.InputBorder;
        public static Color MessageOutgoing => _isDarkMode ? Dark.MessageOutgoing : Light.MessageOutgoing;
        public static Color MessageIncoming => _isDarkMode ? Dark.MessageIncoming : Light.MessageIncoming;
        public static Color Online => _isDarkMode ? Dark.Online : Light.Online;
        public static Color Offline => _isDarkMode ? Dark.Offline : Light.Offline;

        static ThemeManager()
        {
            LoadThemePreference();
        }

        /// <summary>
        /// Toggle gi?a Dark và Light mode
        /// </summary>
        public static void ToggleTheme()
        {
            IsDarkMode = !IsDarkMode;
        }

        /// <summary>
        /// L?u preference vào file
        /// </summary>
        private static void SaveThemePreference()
        {
            try
            {
                string folder = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ChatApp");
                System.IO.Directory.CreateDirectory(folder);
                string file = System.IO.Path.Combine(folder, "theme.txt");
                System.IO.File.WriteAllText(file, _isDarkMode ? "dark" : "light");
            }
            catch { }
        }

        /// <summary>
        /// ??c preference t? file
        /// </summary>
        private static void LoadThemePreference()
        {
            try
            {
                string file = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ChatApp", "theme.txt");
                if (System.IO.File.Exists(file))
                {
                    string theme = System.IO.File.ReadAllText(file).Trim().ToLower();
                    _isDarkMode = (theme != "light");
                }
            }
            catch
            {
                _isDarkMode = true; // Default to dark mode
            }
        }

        /// <summary>
        /// Áp d?ng theme cho m?t Form
        /// </summary>
        public static void ApplyTheme(Form form)
        {
            form.BackColor = BackgroundSecondary;
            ApplyThemeToControls(form.Controls);
        }

        /// <summary>
        /// Áp d?ng theme cho t?t c? controls
        /// </summary>
        public static void ApplyThemeToControls(Control.ControlCollection controls)
        {
            foreach (Control ctrl in controls)
            {
                ApplyThemeToControl(ctrl);
                if (ctrl.HasChildren)
                {
                    ApplyThemeToControls(ctrl.Controls);
                }
            }
        }

        /// <summary>
        /// Áp d?ng theme cho m?t control c? th?
        /// </summary>
        public static void ApplyThemeToControl(Control ctrl)
        {
            switch (ctrl)
            {
                case Label label:
                    if (label.Tag?.ToString() == "header")
                    {
                        label.ForeColor = TextPrimary;
                        label.BackColor = BackgroundTertiary;
                    }
                    else if (label.Tag?.ToString() == "muted")
                    {
                        label.ForeColor = TextMuted;
                    }
                    else
                    {
                        label.ForeColor = TextPrimary;
                    }
                    break;

                case TextBox textBox:
                    textBox.BackColor = InputBackground;
                    textBox.ForeColor = TextPrimary;
                    break;

                case Button button:
                    if (button.Tag?.ToString() == "primary")
                    {
                        button.BackColor = Primary;
                        button.ForeColor = Color.White;
                    }
                    else if (button.Tag?.ToString() == "icon")
                    {
                        button.BackColor = Color.Transparent;
                        button.ForeColor = TextPrimary;
                    }
                    break;

                case ListView listView:
                    listView.BackColor = Background;
                    listView.ForeColor = TextPrimary;
                    break;

                case ComboBox comboBox:
                    comboBox.BackColor = InputBackground;
                    comboBox.ForeColor = TextPrimary;
                    break;

                case RichTextBox richTextBox:
                    richTextBox.BackColor = InputBackground;
                    richTextBox.ForeColor = TextPrimary;
                    break;

                case Panel panel:
                    panel.BackColor = BackgroundSecondary;
                    break;
            }
        }

        /// <summary>
        /// T?o icon button cho toggle theme - S? D?NG TEXT THAY VÌ EMOJI
        /// </summary>
        public static Button CreateThemeToggleButton()
        {
            var btn = new Button
            {
                Size = new Size(50, 35),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                BackColor = _isDarkMode ? Color.FromArgb(64, 68, 75) : Color.FromArgb(220, 220, 225),
                ForeColor = _isDarkMode ? Color.White : Color.Black,
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Text = _isDarkMode ? "Light" : "Dark",
                Tag = "theme"
            };
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = _isDarkMode ? Color.FromArgb(88, 101, 242) : Color.FromArgb(150, 150, 150);
            btn.FlatAppearance.MouseOverBackColor = _isDarkMode ? Dark.InputBackground : Light.InputBackground;

            var tooltip = new ToolTip();
            tooltip.SetToolTip(btn, _isDarkMode ? "Switch to Light Mode" : "Switch to Dark Mode");

            btn.Click += (s, e) =>
            {
                ToggleTheme();
                UpdateThemeButton(btn, tooltip);
            };

            // Update button when theme changes from elsewhere
            ThemeChanged += (s, isDark) =>
            {
                if (btn.IsDisposed) return;
                UpdateThemeButton(btn, tooltip);
            };

            return btn;
        }

        private static void UpdateThemeButton(Button btn, ToolTip tooltip)
        {
            btn.Text = _isDarkMode ? "Light" : "Dark";
            btn.BackColor = _isDarkMode ? Color.FromArgb(64, 68, 75) : Color.FromArgb(220, 220, 225);
            btn.ForeColor = _isDarkMode ? Color.White : Color.Black;
            btn.FlatAppearance.BorderColor = _isDarkMode ? Color.FromArgb(88, 101, 242) : Color.FromArgb(150, 150, 150);
            btn.FlatAppearance.MouseOverBackColor = _isDarkMode ? Dark.InputBackground : Light.InputBackground;
            tooltip.SetToolTip(btn, _isDarkMode ? "Switch to Light Mode" : "Switch to Dark Mode");
        }
    }
}
