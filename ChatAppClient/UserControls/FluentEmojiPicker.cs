using ChatAppClient.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ChatAppClient.UserControls
{
    /// <summary>
    /// Bộ chọn Emoji với icon màu sắc đẹp
    /// </summary>
    public class FluentEmojiPicker : UserControl
    {
        // Events
        public event EventHandler<string>? EmojiSelected;

        // UI Components
        private FlowLayoutPanel _categoryPanel;
        private FlowLayoutPanel _emojiPanel;
 private Label _lblCategory;
        private Panel _headerPanel;

    // State
        private string _currentCategory = "Smileys";
        private Dictionary<string, Button> _categoryButtons = new Dictionary<string, Button>();

        // Colors
   private readonly Color _bgColor = Color.FromArgb(47, 49, 54);
     private readonly Color _panelBgColor = Color.FromArgb(54, 57, 63);
        private readonly Color _categoryBgColor = Color.FromArgb(32, 34, 37);
   private readonly Color _hoverColor = Color.FromArgb(88, 101, 242);
        private readonly Color _selectedColor = Color.FromArgb(114, 137, 218);

        // Emoji data với màu sắc
        private readonly Dictionary<string, List<(string text, Color color, string name)>> _emojiCategories = new()
        {
            { "Smileys", new List<(string, Color, string)> {
  (":)", Color.Yellow, "smile"),
            (":D", Color.Yellow, "grin"),
        (":(", Color.Yellow, "sad"),
     (";)", Color.Yellow, "wink"),
      (":P", Color.Yellow, "tongue"),
       ("XD", Color.Yellow, "laugh"),
        (":O", Color.Yellow, "surprise"),
        (":|", Color.Yellow, "neutral"),
       (":'(", Color.FromArgb(100, 180, 255), "cry"),
  (":3", Color.FromArgb(255, 182, 193), "cute"),
    ("^_^", Color.Yellow, "happy"),
            ("-_-", Color.Yellow, "annoyed"),
    ("o_O", Color.Yellow, "confused"),
        (">:(", Color.FromArgb(255, 100, 100), "angry"),
  ("B)", Color.FromArgb(100, 200, 255), "cool"),
             (":$", Color.FromArgb(255, 150, 150), "blush"),
            (":/", Color.Yellow, "skeptic"),
  ("D:", Color.Yellow, "shock"),
  (":*", Color.FromArgb(255, 100, 150), "kiss"),
       ("<3", Color.FromArgb(255, 80, 120), "love")
      }},
   { "Hearts", new List<(string, Color, string)> {
      ("<3", Color.Red, "red heart"),
("<3", Color.Orange, "orange heart"),
            ("<3", Color.Yellow, "yellow heart"),
       ("<3", Color.LimeGreen, "green heart"),
         ("<3", Color.DeepSkyBlue, "blue heart"),
          ("<3", Color.MediumPurple, "purple heart"),
      ("<3", Color.Black, "black heart"),
          ("<3", Color.White, "white heart"),
      ("<3", Color.SaddleBrown, "brown heart"),
                ("<3", Color.HotPink, "pink heart"),
 ("</3", Color.Red, "broken heart"),
         ("♥", Color.Red, "heart suit"),
        ("♡", Color.HotPink, "heart outline"),
      ("❤", Color.Red, "heavy heart"),
    ("💕", Color.HotPink, "two hearts")
          }},
            { "Stars", new List<(string, Color, string)> {
     ("★", Color.Gold, "star"),
         ("☆", Color.Gold, "star outline"),
            ("✦", Color.Gold, "4-point star"),
        ("✧", Color.Silver, "sparkle"),
       ("⭐", Color.Gold, "glowing star"),
      ("✨", Color.Gold, "sparkles"),
("💫", Color.Gold, "dizzy star"),
    ("🌟", Color.Gold, "bright star"),
     ("*", Color.Yellow, "asterisk"),
      ("✴", Color.OrangeRed, "8-point star"),
        ("✵", Color.Orange, "outlined star"),
          ("✶", Color.Gold, "6-point star"),
    ("✷", Color.Silver, "8-point outline"),
            ("✸", Color.Gold, "heavy star"),
       ("✹", Color.Orange, "12-point star")
 }},
   { "Symbols", new List<(string, Color, string)> {
  ("!", Color.Red, "exclaim"),
      ("?", Color.DeepSkyBlue, "question"),
       ("!?", Color.Orange, "interrobang"),
          ("✓", Color.LimeGreen, "check"),
    ("✗", Color.Red, "cross"),
      ("✔", Color.LimeGreen, "heavy check"),
 ("✖", Color.Red, "heavy cross"),
   ("+", Color.LimeGreen, "plus"),
          ("-", Color.Red, "minus"),
       ("±", Color.Orange, "plus minus"),
           ("∞", Color.DeepSkyBlue, "infinity"),
         ("@", Color.DeepSkyBlue, "at"),
   ("#", Color.DeepSkyBlue, "hash"),
       ("$", Color.LimeGreen, "dollar"),
   ("%", Color.Orange, "percent")
            }},
          { "Arrows", new List<(string, Color, string)> {
     ("→", Color.DeepSkyBlue, "right"),
   ("←", Color.DeepSkyBlue, "left"),
            ("↑", Color.LimeGreen, "up"),
                ("↓", Color.Red, "down"),
     ("↔", Color.Orange, "left right"),
 ("↕", Color.MediumPurple, "up down"),
    ("➔", Color.DeepSkyBlue, "arrow right"),
      ("➜", Color.LimeGreen, "arrow bold"),
 ("➤", Color.Orange, "pointer"),
       ("►", Color.DeepSkyBlue, "play"),
    ("◄", Color.DeepSkyBlue, "reverse"),
           ("▲", Color.LimeGreen, "triangle up"),
      ("▼", Color.Red, "triangle down"),
     ("⇒", Color.DeepSkyBlue, "double right"),
      ("⇐", Color.DeepSkyBlue, "double left")
        }},
         { "Weather", new List<(string, Color, string)> {
         ("☀", Color.Gold, "sun"),
     ("☁", Color.LightGray, "cloud"),
           ("☂", Color.DeepSkyBlue, "umbrella"),
("⚡", Color.Yellow, "lightning"),
                ("❄", Color.LightBlue, "snowflake"),
          ("☃", Color.White, "snowman"),
                ("🌙", Color.Gold, "moon"),
          ("⭐", Color.Gold, "star"),
    ("🔥", Color.OrangeRed, "fire"),
    ("💧", Color.DeepSkyBlue, "droplet"),
      ("🌈", Color.Red, "rainbow"),
                ("💨", Color.LightGray, "wind"),
      ("☔", Color.DeepSkyBlue, "rain"),
       ("⛅", Color.Gold, "partly cloudy"),
       ("🌊", Color.DeepSkyBlue, "wave")
            }},
      { "Objects", new List<(string, Color, string)> {
     ("♪", Color.DeepSkyBlue, "note"),
        ("♫", Color.MediumPurple, "notes"),
 ("♬", Color.HotPink, "beamed notes"),
   ("☎", Color.Red, "phone"),
           ("✉", Color.SaddleBrown, "envelope"),
        ("✏", Color.Gold, "pencil"),
     ("✂", Color.Silver, "scissors"),
     ("☕", Color.SaddleBrown, "coffee"),
          ("⌚", Color.Silver, "watch"),
        ("⌛", Color.Gold, "hourglass"),
     ("⚙", Color.Gray, "gear"),
      ("⚠", Color.Gold, "warning"),
           ("⛔", Color.Red, "no entry"),
       ("🔒", Color.Gold, "lock"),
        ("🔑", Color.Gold, "key")
 }},
         { "Faces", new List<(string, Color, string)> {
            ("☺", Color.Yellow, "smile face"),
             ("☻", Color.Yellow, "black smile"),
       ("☹", Color.Yellow, "sad face"),
     ("(╯°□°)╯", Color.Red, "table flip"),
          ("¯\\_(ツ)_/¯", Color.Yellow, "shrug"),
    ("(ノಠ益ಠ)ノ", Color.Red, "angry flip"),
         ("(◕‿◕)", Color.LimeGreen, "happy"),
      ("(ಥ﹏ಥ)", Color.DeepSkyBlue, "crying"),
        ("(¬‿¬)", Color.MediumPurple, "smirk"),
           ("(•_•)", Color.Yellow, "look"),
     ("( ͡° ͜ʖ ͡°)", Color.Yellow, "lenny"),
            ("(づ｡◕‿‿◕｡)づ", Color.HotPink, "hug"),
   ("ಠ_ಠ", Color.Yellow, "disapproval"),
 ("(ง'̀-'́)ง", Color.Orange, "fight"),
     ("(☞ﾟヮﾟ)☞", Color.DeepSkyBlue, "point")
  }}
        };

     // Category icons với màu
        private readonly Dictionary<string, (string icon, Color color)> _categoryLabels = new()
        {
            { "Smileys", (":)", Color.Yellow) },
            { "Hearts", ("♥", Color.Red) },
            { "Stars", ("★", Color.Gold) },
      { "Symbols", ("✓", Color.LimeGreen) },
   { "Arrows", ("→", Color.DeepSkyBlue) },
{ "Weather", ("☀", Color.Gold) },
            { "Objects", ("♪", Color.DeepSkyBlue) },
            { "Faces", ("☺", Color.Yellow) }
        };

        public FluentEmojiPicker()
 {
            InitializeComponents();
            LoadEmojiCategory(_currentCategory);
  }

        private void InitializeComponents()
        {
       this.Size = new Size(380, 420);
            this.BackColor = _bgColor;
          this.BorderStyle = BorderStyle.FixedSingle;
            this.DoubleBuffered = true;

            // === Header Panel ===
            _headerPanel = new Panel
          {
      Dock = DockStyle.Top,
            Height = 40,
             BackColor = _categoryBgColor,
           Padding = new Padding(10, 5, 10, 5)
         };

            _lblCategory = new Label
          {
        Text = "Smileys",
             Font = new Font("Segoe UI", 12, FontStyle.Bold),
      ForeColor = Color.White,
     AutoSize = false,
             Dock = DockStyle.Fill,
          TextAlign = ContentAlignment.MiddleLeft
        };
            _headerPanel.Controls.Add(_lblCategory);

       // === Emoji Panel (main area) ===
          _emojiPanel = new FlowLayoutPanel
            {
           Dock = DockStyle.Fill,
   AutoScroll = true,
 FlowDirection = FlowDirection.LeftToRight,
 WrapContents = true,
 BackColor = _panelBgColor,
      Padding = new Padding(10)
   };

            // === Category Panel (Bottom) ===
          _categoryPanel = new FlowLayoutPanel
            {
    Dock = DockStyle.Bottom,
    Height = 50,
     FlowDirection = FlowDirection.LeftToRight,
       WrapContents = false,
        AutoScroll = true,
   BackColor = _categoryBgColor,
 Padding = new Padding(5, 5, 5, 5)
            };

      CreateCategoryButtons();

  // Add controls in order (bottom to top for docking)
  this.Controls.Add(_emojiPanel);
        this.Controls.Add(_categoryPanel);
       this.Controls.Add(_headerPanel);
    }

        private void CreateCategoryButtons()
        {
       foreach (var cat in _categoryLabels)
  {
   var btn = new Button
    {
            Text = cat.Value.icon,
         Font = new Font("Segoe UI", 14),
   Size = new Size(42, 42),
  FlatStyle = FlatStyle.Flat,
    BackColor = cat.Key == _currentCategory ? _selectedColor : Color.Transparent,
         ForeColor = cat.Value.color,
         Cursor = Cursors.Hand,
  Tag = cat.Key,
       Margin = new Padding(2),
            TextAlign = ContentAlignment.MiddleCenter
        };
       btn.FlatAppearance.BorderSize = 0;
    btn.FlatAppearance.MouseOverBackColor = _hoverColor;
         btn.Click += CategoryBtn_Click;

  var tooltip = new ToolTip();
      tooltip.SetToolTip(btn, cat.Key);

     _categoryPanel.Controls.Add(btn);
      _categoryButtons[cat.Key] = btn;
        }
      }

        private void CategoryBtn_Click(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is string category)
            {
           // Update selection
            foreach (var kvp in _categoryButtons)
      {
   kvp.Value.BackColor = kvp.Key == category ? _selectedColor : Color.Transparent;
      }

          _currentCategory = category;
       LoadEmojiCategory(category);
         }
  }

        private void LoadEmojiCategory(string category)
        {
            // Update header với màu
         if (_categoryLabels.TryGetValue(category, out var catInfo))
            {
       _lblCategory.Text = $"{catInfo.icon} {category}";
                _lblCategory.ForeColor = catInfo.color;
        }

            // Clear and reload emojis
            _emojiPanel.SuspendLayout();
      _emojiPanel.Controls.Clear();

       if (_emojiCategories.TryGetValue(category, out var emojis))
        {
         foreach (var emoji in emojis)
     {
        var btn = CreateColorfulEmojiButton(emoji.text, emoji.color, emoji.name);
  _emojiPanel.Controls.Add(btn);
  }
            }

        _emojiPanel.ResumeLayout();
        }

      private Button CreateColorfulEmojiButton(string emoji, Color color, string name)
    {
   var btn = new Button
   {
       Text = emoji,
     Font = new Font("Segoe UI", 11, FontStyle.Bold),
    Size = new Size(50, 45),
           FlatStyle = FlatStyle.Flat,
     BackColor = Color.FromArgb(64, 68, 75),
      ForeColor = color,
                Cursor = Cursors.Hand,
      Tag = emoji,
                Margin = new Padding(3),
          TextAlign = ContentAlignment.MiddleCenter,
      Padding = new Padding(0)
            };
   btn.FlatAppearance.BorderSize = 1;
        btn.FlatAppearance.BorderColor = Color.FromArgb(80, 85, 95);
 btn.FlatAppearance.MouseOverBackColor = _hoverColor;
            btn.FlatAppearance.MouseDownBackColor = _selectedColor;
      btn.Click += EmojiBtn_Click;

          // Tooltip với tên
      var tooltip = new ToolTip();
      tooltip.SetToolTip(btn, name);

   return btn;
        }

        private void EmojiBtn_Click(object? sender, EventArgs e)
        {
            if (sender is Button btn && btn.Tag is string emoji)
            {
    EmojiSelected?.Invoke(this, emoji);
          }
        }
    }
}
