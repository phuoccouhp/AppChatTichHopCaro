using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using ChatAppClient.Helpers; 

namespace ChatAppClient.CustomControls
{
    public class RoundedButton : Button
    {
        private int _borderRadius = 20;
        private Color _buttonColor = AppColors.Primary;
        private Color _textColor = Color.White;

        public int BorderRadius
        {
            get { return _borderRadius; }
            set { _borderRadius = value; this.Invalidate(); }
        }

        public Color ButtonColor
        {
            get { return _buttonColor; }
            set { _buttonColor = value; _originalColor = value; this.Invalidate(); } 
        }

        public Color TextColor
        {
            get { return _textColor; }
            set { _textColor = value; this.Invalidate(); }
        }

        private Color _originalColor; 

        public RoundedButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.Size = new Size(150, 40);
            this.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.BackColor = Color.Transparent;
            this.ForeColor = Color.Transparent;
            this.DoubleBuffered = true;
            _originalColor = _buttonColor; 

            this.MouseEnter += (s, e) =>
            {
                this._buttonColor = ControlPaint.Dark(_originalColor, 0.1f);
                this.Invalidate();
            };
            this.MouseLeave += (s, e) =>
            {
                this._buttonColor = _originalColor;
                this.Invalidate();
            };
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
            if (rect.Width <= 0 || rect.Height <= 0) return;

            using (GraphicsPath path = DrawingHelper.CreateRoundedRectPath(rect, _borderRadius))
            using (SolidBrush brush = new SolidBrush(_buttonColor))
            {
                pevent.Graphics.FillPath(brush, path);
            }

            TextRenderer.DrawText(pevent.Graphics, this.Text, this.Font,
                rect, _textColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}