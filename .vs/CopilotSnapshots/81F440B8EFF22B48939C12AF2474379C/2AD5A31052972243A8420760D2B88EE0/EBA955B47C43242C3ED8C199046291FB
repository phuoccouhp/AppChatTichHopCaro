using ChatAppClient.Helpers;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ChatAppClient.UserControls
{
    public enum MessageType { Incoming, Outgoing }

    public partial class ChatMessageBubble : UserControl
    {
        private string _messageText = "";
        private MessageType _type;
        private DateTime _time;

        // Cấu hình giao diện
        private int _maxWidth = 350; // Chiều rộng tối đa của bubble
        private int _padding = 12;   // Khoảng cách từ viền đến chữ
        private int _borderRadius = 15; // Độ bo tròn
        private Font _font = new Font("Segoe UI", 11F); // Font chữ to rõ
        private Font _timeFont = new Font("Segoe UI", 8F); // Font giờ

        public ChatMessageBubble()
        {
            InitializeComponent();
            this.DoubleBuffered = true; // Chống nháy hình
            this.BackColor = Color.Transparent;
            this.ResizeRedraw = true; // Vẽ lại khi thay đổi kích thước
        }

        public void SetData(string message, MessageType type, DateTime time)
        {
            _messageText = message;
            _type = type;
            _time = time;

            CalculateSize(); // Tính toán kích thước dựa trên nội dung
            this.Invalidate(); // Vẽ lại
        }

        private void CalculateSize()
        {
            // Đo kích thước văn bản
            Size textSize = TextRenderer.MeasureText(_messageText, _font, new Size(_maxWidth - (_padding * 2), 0), TextFormatFlags.WordBreak);

            // Tính toán kích thước Control
            int width = textSize.Width + (_padding * 2);
            int height = textSize.Height + (_padding * 2) + 15; // +15 cho dòng thời gian ở dưới

            // Đảm bảo không quá nhỏ
            if (width < 60) width = 60;

            this.Size = new Size(width, height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // 1. Xác định màu sắc
            Color bgColor = (_type == MessageType.Outgoing) ? AppColors.Primary : Color.FromArgb(230, 230, 230);
            Color textColor = (_type == MessageType.Outgoing) ? Color.White : Color.Black;
            Color timeColor = (_type == MessageType.Outgoing) ? Color.FromArgb(200, 255, 255, 255) : Color.Gray;

            // 2. Vẽ Bong bóng (Bubble)
            // Trừ đi 1 pixel để viền không bị cắt
            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 15);
            using (GraphicsPath path = CreateRoundedPath(rect, _borderRadius))
            using (SolidBrush brush = new SolidBrush(bgColor))
            {
                e.Graphics.FillPath(brush, path);
            }

            // 3. Vẽ Nội dung tin nhắn
            // Rectangle để vẽ chữ (có padding)
            Rectangle textRect = new Rectangle(_padding, _padding, this.Width - (_padding * 2), this.Height - (_padding * 2) - 15);
            TextRenderer.DrawText(e.Graphics, _messageText, _font, textRect, textColor, TextFormatFlags.WordBreak | TextFormatFlags.Left | TextFormatFlags.Top);

            // 4. Vẽ Thời gian (Góc dưới bên phải hoặc trái tùy loại)
            string timeStr = _time.ToString("HH:mm");
            Size timeSize = TextRenderer.MeasureText(timeStr, _timeFont);
            int timeX = (_type == MessageType.Outgoing) ? this.Width - timeSize.Width - 5 : 5;
            int timeY = this.Height - 15;

            // Vẽ giờ bên ngoài bubble một chút hoặc bên trong đáy
            TextRenderer.DrawText(e.Graphics, timeStr, _timeFont, new Point(timeX, timeY), Color.Gray);
        }

        private GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;

            path.AddArc(rect.X, rect.Y, d, d, 180, 90); // Top-Left
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90); // Top-Right
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90); // Bottom-Right
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90); // Bottom-Left
            path.CloseFigure();
            return path;
        }
    }
}