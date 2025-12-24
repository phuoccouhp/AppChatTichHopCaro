using ChatAppClient.Helpers;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ChatAppClient.UserControls
{
    public partial class ChatMessageBubble : UserControl
    {
        private string _messageText = "";
        private MessageType _type;
        private Color _bgColor;
        private Color _textColor;

        public event EventHandler<string>? OnForwardRequested;

        public ChatMessageBubble()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
            this.BackColor = Color.Transparent; // Nền trong suốt

            // Ẩn label mặc định của designer đi để ta tự vẽ
            if (this.Controls.ContainsKey("lblMessage")) this.Controls["lblMessage"].Visible = false;

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Copy", null, (s, e) => Clipboard.SetText(_messageText));
            menu.Items.Add("Chuyển tiếp", null, (s, e) => OnForwardRequested?.Invoke(this, _messageText));
            this.ContextMenuStrip = menu;
        }

        public void SetMessage(string message, MessageType type, int parentUsableWidth)
        {
            _messageText = message;
            _type = type;

            // 1. Màu sắc
            if (_type == MessageType.Outgoing)
            {
                _bgColor = Color.FromArgb(0, 132, 255); // Xanh Messenger
                _textColor = Color.White;
            }
            else
            {
                _bgColor = Color.FromArgb(230, 230, 230); // Xám
                _textColor = Color.Black;
            }

            // 2. Tính kích thước
            Font font = new Font("Segoe UI", 11F);
            int padding = 12;
            int maxWidth = parentUsableWidth * 2 / 3;

            Size textSize = TextRenderer.MeasureText(message, font, new Size(maxWidth - (padding * 2), 0), TextFormatFlags.WordBreak);

            this.Size = new Size(textSize.Width + (padding * 2) + 10, textSize.Height + (padding * 2));

            // 3. Căn lề margin
            UpdateMargins(parentUsableWidth);

            this.Invalidate(); // Vẽ lại
        }

        public void UpdateMargins(int parentUsableWidth)
        {
            int remainingSpace = parentUsableWidth - this.Width;
            if (remainingSpace < 0) remainingSpace = 0;

            if (_type == MessageType.Outgoing)
                this.Margin = new Padding(remainingSpace, 5, 0, 5); // Căn phải
            else
                this.Margin = new Padding(0, 5, remainingSpace, 5); // Căn trái
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Vẽ khối bo tròn
            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            using (GraphicsPath path = CreateRoundedPath(rect, 18))
            using (SolidBrush brush = new SolidBrush(_bgColor))
            {
                e.Graphics.FillPath(brush, path);
            }

            // Vẽ chữ
            int p = 12;
            Rectangle textRect = new Rectangle(p, p, this.Width - (p * 2), this.Height - (p * 2));
            Font font = new Font("Segoe UI", 11F);
            TextRenderer.DrawText(e.Graphics, _messageText, font, textRect, _textColor, TextFormatFlags.WordBreak | TextFormatFlags.Left | TextFormatFlags.Top);
        }

        private GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}