using ChatAppClient.Helpers;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ChatAppClient.UserControls
{
    public partial class FriendListItem : UserControl
    {
        public string FriendID { get; private set; }
        public string FriendName { get; private set; }
        public string FriendStatus { get; private set; }
        public bool IsOnline { get; private set; }
        private bool _hasNewMessage = false;

        public FriendListItem()
        {
            InitializeComponent();
            ApplyDarkTheme();
            this.Size = new Size(300, 70);
            // Xử lý sự kiện Hover
            this.MouseEnter += (s, e) => this.BackColor = Color.FromArgb(50, 50, 60);
            this.MouseLeave += (s, e) => this.BackColor = Color.Transparent;

            foreach (Control c in this.Controls)
            {
                c.MouseEnter += (s, e) => this.BackColor = Color.FromArgb(50, 50, 60);
                c.MouseLeave += (s, e) => this.BackColor = Color.Transparent;
                c.Click += (s, e) => this.OnClick(e);
            }
        }
        private void ApplyDarkTheme()
        {
            this.BackColor = Color.Transparent;
            this.lblFriendName.ForeColor = Color.White;
            this.lblStatus.ForeColor = Color.Gray;
            this.pnlStatusDot.Visible = false;
        }
        private void UpdateBackground()
        {
            if (_hasNewMessage)
                this.BackColor = Color.FromArgb(70, 60, 40); // Nền hơi sáng nếu có tin mới
            else
                this.BackColor = Color.Transparent; // Nền trong suốt bình thường
        }
        public void SetNewMessageAlert(bool hasNewMessage)
        {
            _hasNewMessage = hasNewMessage;
            UpdateBackground(); // Cập nhật màu nền
            this.Invalidate();
        }
        public void SetData(string id, string name, string status, bool isOnline)
        {
            FriendID = id;
            FriendName = name;
            FriendStatus = status;
            IsOnline = isOnline;

            lblFriendName.Text = name;
            lblFriendName.ForeColor = Color.White;
            lblStatus.Text = status;
            // Căn chỉnh vị trí chữ Online thấp xuống (Y = 40)
            lblStatus.Location = new Point(88, 42);

            if (isOnline)
            {
                lblStatus.ForeColor = Color.FromArgb(0, 255, 128); // Xanh lá
            }
            else
            {
                lblStatus.ForeColor = Color.Gray;
            }

            this.Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Color dotColor = IsOnline ? Color.FromArgb(0, 255, 128) : Color.Gray;

            using (SolidBrush brush = new SolidBrush(dotColor))
            {
                e.Graphics.FillEllipse(brush, 73, 44, 10, 10);
            }
            if (_hasNewMessage)
            {
                int badgeSize = 12; // Kích thước chấm đỏ
                int xPos = this.Width - badgeSize - 70; // Vị trí từ lề phải
                int yPos = (this.Height - badgeSize) / 2; // Căn giữa theo chiều dọc

                using (SolidBrush badgeBrush = new SolidBrush(Color.Crimson)) // Màu đỏ thắm
                {
                    e.Graphics.FillEllipse(badgeBrush, xPos, yPos, badgeSize, badgeSize);
                }
            }
        }
    }
}