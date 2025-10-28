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
        private int _borderRadius = 15;

        private int _bubbleWidth = 0;

        public ChatMessageBubble()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        public void SetMessage(string message, MessageType type, int parentUsableWidth)
        {
            _type = type;
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

            this.Size = new Size(lblMessage.Width + 20, lblMessage.Height + 20);
            _bubbleWidth = this.Width; 

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
                this.Margin = new Padding(remainingSpace, 5, 0, 5);
            }
            else
            {
                this.Margin = new Padding(0, 5, remainingSpace, 5);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (this.Width <= 0 || this.Height <= 0) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);

            using (GraphicsPath path = DrawingHelper.CreateRoundedRectPath(rect, _borderRadius))
            using (SolidBrush brush = new SolidBrush(_bubbleColor))
            {
                e.Graphics.FillPath(brush, path);
            }
        }
    }
}