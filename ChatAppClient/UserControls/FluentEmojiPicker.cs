using ChatAppClient.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatAppClient.UserControls
{
    /// <summary>
    /// Bộ chọn Emoji đơn giản - hiển thị emoji Unicode trực tiếp
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

   // Emoji data - tất cả emoji được nhóm theo danh mục
  // Sử dụng các ký tự emoji thông dụng
        private readonly Dictionary<string, string[]> _emojiCategories = new Dictionary<string, string[]>
     {
         { "Smileys", new[] { ":)", ":(", ":D", ";)", ":P", "XD", ":O", ":|", ":'(", ":3", "<3", ":*", "^_^", "-_-", "o_O", ">:(", "B)", ":$", ":/", "D:" } },
     { "Hearts", new[] { "<3", "</3", "♥", "♡", "❤", "💕", "💗", "💖", "💘", "💝" } },
       { "Symbols", new[] { "★", "☆", "✓", "✗", "♪", "♫", "☀", "☁", "☂", "⚡", "❄", "☃", "⭐", "✨", "🔥", "💯", "✔", "✖", "➤", "➜" } },
            { "Arrows", new[] { "→", "←", "↑", "↓", "↔", "↕", "➔", "➜", "➤", "►", "◄", "▲", "▼", "⬆", "⬇", "⬅", "➡", "↩", "↪", "⤴" } },
            { "Math", new[] { "+", "-", "×", "÷", "=", "≠", "≈", "<", ">", "≤", "≥", "±", "∞", "√", "∑", "π", "°", "%", "‰", "№" } },
         { "Faces", new[] { "☺", "☻", "☹", "♀", "♂", "⚥", "☠", "💀", "👤", "👥" } },
            { "Hands", new[] { "👍", "👎", "👏", "🙌", "👐", "🤝", "🙏", "💪", "👋", "✋", "👌", "✌", "🤞", "🤟", "🤘", "🤙", "👈", "👉", "👆", "👇" } },
     { "Objects", new[] { "☎", "✉", "✏", "✂", "☕", "⌚", "⌛", "☰", "☷", "⚙", "⚠", "⛔", "🔒", "🔓", "🔑", "💡", "📌", "📎", "🔗", "⚓" } },
        };

        // Category labels (text thay vì emoji)
   private readonly Dictionary<string, string> _categoryLabels = new Dictionary<string, string>
 {
       { "Smileys", ":)" },
  { "Hearts", "<3" },
   { "Symbols", "★" },
            { "Arrows", "→" },
{ "Math", "+" },
    { "Faces", "☺" },
          { "Hands", "👍" },
  { "Objects", "☎" }
      };

        public FluentEmojiPicker()
  {
            InitializeComponents();
   LoadEmojiCategory(_currentCategory);
        }

  private void InitializeComponents()
        {
      this.Size = new Size(340, 380);
   this.BackColor = _bgColor;
          this.BorderStyle = BorderStyle.FixedSingle;
       this.DoubleBuffered = true;

            // === Header Panel ===
       _headerPanel = new Panel
       {
       Dock = DockStyle.Top,
     Height = 35,
       BackColor = _categoryBgColor,
 Padding = new Padding(10, 5, 10, 5)
  };

            _lblCategory = new Label
     {
      Text = "Smileys",
     Font = new Font("Segoe UI", 11, FontStyle.Bold),
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
 Padding = new Padding(8)
        };

 // === Category Panel (Bottom) ===
            _categoryPanel = new FlowLayoutPanel
 {
   Dock = DockStyle.Bottom,
       Height = 45,
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
 Text = cat.Value,
            Font = new Font("Segoe UI", 11),
 Size = new Size(36, 36),
             FlatStyle = FlatStyle.Flat,
  BackColor = cat.Key == _currentCategory ? _selectedColor : Color.Transparent,
ForeColor = Color.White,
         Cursor = Cursors.Hand,
        Tag = cat.Key,
      Margin = new Padding(1),
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
     // Update header
 _lblCategory.Text = category;

        // Clear and reload emojis
            _emojiPanel.SuspendLayout();
       _emojiPanel.Controls.Clear();

        if (_emojiCategories.TryGetValue(category, out var emojis))
   {
      foreach (var emoji in emojis)
          {
     var btn = CreateEmojiButton(emoji);
   _emojiPanel.Controls.Add(btn);
          }
 }

     _emojiPanel.ResumeLayout();
        }

        private Button CreateEmojiButton(string emoji)
        {
  var btn = new Button
 {
           Text = emoji,
   Font = new Font("Segoe UI", 12),
       Size = new Size(42, 42),
    FlatStyle = FlatStyle.Flat,
            BackColor = Color.Transparent,
      ForeColor = Color.White,
               Cursor = Cursors.Hand,
        Tag = emoji,
    Margin = new Padding(2),
      TextAlign = ContentAlignment.MiddleCenter,
      Padding = new Padding(0)
 };
       btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = _hoverColor;
 btn.Click += EmojiBtn_Click;

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
