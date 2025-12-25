using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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
 /// T?o nút toggle theme ??p v?i gradient và icon
        /// </summary>
        public static Button CreateThemeToggleButton()
        {
  var btn = new ThemeToggleButton();
        btn.UpdateTheme(_isDarkMode);
    
            btn.Click += (s, e) =>
          {
       ToggleTheme();
    btn.UpdateTheme(_isDarkMode);
            };

            // Update button when theme changes from elsewhere
     ThemeChanged += (s, isDark) =>
      {
          if (btn.IsDisposed) return;
       btn.UpdateTheme(isDark);
            };

      return btn;
      }
    }

    /// <summary>
    /// Custom button cho theme toggle v?i gradient và icon ??p
    /// </summary>
 public class ThemeToggleButton : Button
    {
        private bool _isDark = true;
        
        // Colors for dark mode button (shows sun - switch to light)
        private readonly Color _darkBgStart = Color.FromArgb(30, 35, 60);
        private readonly Color _darkBgEnd = Color.FromArgb(50, 55, 80);
        private readonly Color _sunColor = Color.FromArgb(255, 200, 50);
      private readonly Color _sunGlow = Color.FromArgb(255, 220, 100);
        
  // Colors for light mode button (shows moon - switch to dark)
        private readonly Color _lightBgStart = Color.FromArgb(135, 206, 250);
        private readonly Color _lightBgEnd = Color.FromArgb(200, 230, 255);
      private readonly Color _moonColor = Color.FromArgb(240, 240, 200);
        private readonly Color _moonShadow = Color.FromArgb(200, 200, 170);

public ThemeToggleButton()
   {
            this.Size = new Size(60, 32);
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.Cursor = Cursors.Hand;
      this.Text = "";
        this.Font = new Font("Segoe UI", 9);
            
            // Enable custom painting
      this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        public void UpdateTheme(bool isDark)
        {
            _isDark = isDark;
        this.Invalidate(); // Redraw
         
   var tooltip = new ToolTip();
        tooltip.SetToolTip(this, isDark ? "Switch to Light Mode" : "Switch to Dark Mode");
        }

      protected override void OnPaint(PaintEventArgs e)
  {
      var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
  
         var rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
   
            // Draw rounded background with gradient
        using (var path = CreateRoundedRectangle(rect, 16))
        {
    Color bgStart = _isDark ? _darkBgStart : _lightBgStart;
  Color bgEnd = _isDark ? _darkBgEnd : _lightBgEnd;
  
      using (var brush = new LinearGradientBrush(rect, bgStart, bgEnd, LinearGradientMode.Vertical))
            {
    g.FillPath(brush, path);
           }
   
      // Draw border
        using (var pen = new Pen(_isDark ? Color.FromArgb(80, 90, 120) : Color.FromArgb(150, 180, 220), 1))
   {
            g.DrawPath(pen, path);
                }
    }
         
      // Draw icon
     if (_isDark)
            {
  // Draw Sun (switch to light mode)
      DrawSun(g);
        }
        else
            {
  // Draw Moon (switch to dark mode)
     DrawMoon(g);
     }
     
       // Draw text
            string text = _isDark ? "Light" : "Dark";
     using (var textBrush = new SolidBrush(_isDark ? Color.White : Color.FromArgb(50, 50, 80)))
            {
   var textRect = new Rectangle(22, 0, this.Width - 24, this.Height);
    var sf = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
          g.DrawString(text, this.Font, textBrush, textRect, sf);
        }
   }

        private void DrawSun(Graphics g)
        {
    int centerX = 14;
  int centerY = this.Height / 2;
    int radius = 6;
            
      // Draw glow
        using (var glowBrush = new SolidBrush(Color.FromArgb(100, _sunGlow)))
  {
    g.FillEllipse(glowBrush, centerX - radius - 3, centerY - radius - 3, (radius + 3) * 2, (radius + 3) * 2);
            }
            
 // Draw sun circle
    using (var sunBrush = new SolidBrush(_sunColor))
    {
         g.FillEllipse(sunBrush, centerX - radius, centerY - radius, radius * 2, radius * 2);
            }
 
       // Draw rays
  using (var rayPen = new Pen(_sunColor, 2))
            {
            for (int i = 0; i < 8; i++)
     {
    double angle = i * Math.PI / 4;
    int x1 = centerX + (int)(Math.Cos(angle) * (radius + 2));
  int y1 = centerY + (int)(Math.Sin(angle) * (radius + 2));
         int x2 = centerX + (int)(Math.Cos(angle) * (radius + 5));
        int y2 = centerY + (int)(Math.Sin(angle) * (radius + 5));
        g.DrawLine(rayPen, x1, y1, x2, y2);
  }
         }
        }

        private void DrawMoon(Graphics g)
  {
            int centerX = 14;
            int centerY = this.Height / 2;
         int radius = 7;
    
    // Draw moon with crescent effect
    using (var moonBrush = new SolidBrush(_moonColor))
            {
     g.FillEllipse(moonBrush, centerX - radius, centerY - radius, radius * 2, radius * 2);
            }
         
      // Draw shadow to create crescent
            using (var shadowBrush = new SolidBrush(_lightBgEnd))
      {
     g.FillEllipse(shadowBrush, centerX - radius + 4, centerY - radius - 2, radius * 2 - 2, radius * 2 - 2);
 }
     
        // Draw small stars
      using (var starBrush = new SolidBrush(Color.FromArgb(180, 255, 255, 200)))
            {
   g.FillEllipse(starBrush, centerX + 8, centerY - 5, 3, 3);
   g.FillEllipse(starBrush, centerX + 5, centerY + 4, 2, 2);
            }
 }

        private GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
     {
    var path = new GraphicsPath();
            int d = radius * 2;
  
 path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
         
      return path;
  }

        protected override void OnMouseEnter(EventArgs e)
        {
      base.OnMouseEnter(e);
            this.Cursor = Cursors.Hand;
        }
    }
}
