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
        private Color _bubbleColor = AppColors.LightGray;
        private MessageType _type = MessageType.Incoming;
        private int _borderRadius = 18;

        private int _bubbleWidth = 0;
        private string _messageText = "";

        public event EventHandler<string>? OnForwardRequested;

        public ChatMessageBubble()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            
            // Hover events
            this.MouseEnter += ChatMessageBubble_MouseEnter;
            this.MouseLeave += ChatMessageBubble_MouseLeave;
            lblMessage.MouseEnter += ChatMessageBubble_MouseEnter;
            lblMessage.MouseLeave += ChatMessageBubble_MouseLeave;
            
            // Forward button event
            btnForward.Click += BtnForward_Click;
        }

        private void ChatMessageBubble_MouseEnter(object? sender, EventArgs e)
        {
            btnForward.Visible = true;
            btnForward.BringToFront();
        }

        private void ChatMessageBubble_MouseLeave(object? sender, EventArgs e)
        {
            if (!btnForward.ClientRectangle.Contains(btnForward.PointToClient(Control.MousePosition)))
            {
                btnForward.Visible = false;
            }
        }

        private void BtnForward_Click(object? sender, EventArgs e)
        {
            OnForwardRequested?.Invoke(this, _messageText);
        }

        public void SetMessage(string message, MessageType type, int parentUsableWidth)
        {
            _type = type;
            _messageText = message;
            lblMessage.Text = message;

            if (type == MessageType.Outgoing)
            {
                _bubbleColor = AppColors.Primary;
                lblMessage.ForeColor = Color.White;
            }
            else
            {
                _bubbleColor = AppColors.LightGray;
                lblMessage.ForeColor = AppColors.TextPrimary;
            }

            // Tính toán kích thước bubble với padding đều
            int horizontalPadding = 12; // Padding trái và phải
            int verticalPadding = 8; // Padding trên và dưới
            
            this.Size = new Size(lblMessage.Width + (horizontalPadding * 2), lblMessage.Height + (verticalPadding * 2));
            _bubbleWidth = this.Width;
            
            // Căn giữa label theo chiều dọc
            lblMessage.Location = new Point(horizontalPadding, verticalPadding);
            
            // Đặt vị trí button forward
            if (_type == MessageType.Outgoing)
            {
                btnForward.Location = new Point(this.Width - 30, 2);
            }
            else
            {
                btnForward.Location = new Point(2, 2);
            }
            
            UpdateMargins(parentUsableWidth);

            this.Invalidate();
        }

        public void UpdateMargins(int parentUsableWidth)
        {
            if (_bubbleWidth == 0)
                _bubbleWidth = this.Width;

            int remainingSpace = parentUsableWidth - _bubbleWidth;
            if (remainingSpace < 0)
                remainingSpace = 0;

            if (_type == MessageType.Outgoing)
            {
                this.Margin = new Padding(remainingSpace, 6, 0, 6);
            }
            else
            {
                this.Margin = new Padding(0, 6, remainingSpace, 6);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (this.Width <= 0 || this.Height <= 0) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);

            using (GraphicsPath path = DrawingHelper.CreateRoundedRectPath(rect, _borderRadius))
            {
                // Vẽ shadow nhẹ
                if (_type == MessageType.Outgoing)
                {
                    Rectangle shadowRect = new Rectangle(1, 1, rect.Width, rect.Height);
                    using (GraphicsPath shadowPath = DrawingHelper.CreateRoundedRectPath(shadowRect, _borderRadius))
                    using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(15, 0, 0, 0)))
                    {
                        e.Graphics.FillPath(shadowBrush, shadowPath);
                    }
                }

                // Vẽ bubble
                using (SolidBrush brush = new SolidBrush(_bubbleColor))
                {
                    e.Graphics.FillPath(brush, path);
                }

                // Vẽ border nhẹ cho incoming messages
                if (_type == MessageType.Incoming)
                {
                    using (Pen pen = new Pen(Color.FromArgb(220, 220, 220), 0.5f))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                }
            }
        }
    }
}