using System;
using System.Drawing;
using System.Windows.Forms;
using ChatAppClient.Helpers; 

namespace ChatAppClient.UserControls
{
    public partial class FriendListItem : UserControl
    {
        private Color _hoverColor = AppColors.LightGray;
        private Color _normalColor = Color.White;

        public string FriendName { get { return lblFriendName.Text; } }
        public string FriendStatus { get { return lblStatus.Text; } }
        public string FriendID { get; private set; }

        public FriendListItem()
        {
            InitializeComponent();
            ApplyHoverEvents(this);
            ApplyHoverEvents(lblFriendName);
            ApplyHoverEvents(lblStatus);
            ApplyHoverEvents(pbAvatar);
            ApplyHoverEvents(pnlStatusDot);

            DrawingHelper.ApplyRoundedCorners(pnlStatusDot, 5);
        }
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

        private void ApplyHoverEvents(Control control)
        {
            control.MouseEnter += (s, e) => { this.BackColor = _hoverColor; };
            control.MouseLeave += (s, e) => { this.BackColor = _normalColor; };
        }
    }
}