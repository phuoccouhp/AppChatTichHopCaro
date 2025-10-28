using System;
using System.Drawing;
using System.Windows.Forms;
using ChatAppClient.Helpers; // Dùng AppColors

namespace ChatAppClient.UserControls
{
    public partial class FriendListItem : UserControl
    {
        private Color _hoverColor = AppColors.LightGray;
        private Color _normalColor = Color.White;

        public string FriendName { get { return lblFriendName.Text; } }
        public string FriendStatus { get { return lblStatus.Text; } }
        // Thêm thuộc tính để lưu ID (quan trọng khi click)
        public string FriendID { get; private set; }

        public FriendListItem()
        {
            InitializeComponent();
            ApplyHoverEvents(this);
            ApplyHoverEvents(lblFriendName);
            ApplyHoverEvents(lblStatus);
            ApplyHoverEvents(pbAvatar);
            ApplyHoverEvents(pnlStatusDot);

            // Bo tròn cái chấm status
            DrawingHelper.ApplyRoundedCorners(pnlStatusDot, 5);
            // Bo tròn Avatar
            // (Bo tròn PictureBox phức tạp, tạm thời để vuông)
        }
        // (Bên trong public partial class FriendListItem)

        // ... (Các hàm cũ SetData, ApplyHoverEvents... giữ nguyên) ...

        /// <summary>
        /// Dùng để Bật/Tắt chấm đỏ thông báo tin nhắn mới
        /// </summary>
        public void SetNewMessageAlert(bool visible)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => SetNewMessageAlert(visible)));
                return;
            }
            lblNewMessageBadge.Visible = visible;
        }
        public void SetData(string id, string name, string status, bool isOnline)
        {
            FriendID = id;
            lblFriendName.Text = name;
            lblStatus.Text = status;

            if (isOnline)
            {
                pnlStatusDot.BackColor = AppColors.Online;
                lblStatus.ForeColor = AppColors.Online;
            }
            else
            {
                pnlStatusDot.BackColor = AppColors.Offline;
                lblStatus.ForeColor = AppColors.TextSecondary;
            }
        }

        // Gán sự kiện hover cho control và các control con
        private void ApplyHoverEvents(Control control)
        {
            control.MouseEnter += (s, e) => { this.BackColor = _hoverColor; };
            control.MouseLeave += (s, e) => { this.BackColor = _normalColor; };
        }
    }
}