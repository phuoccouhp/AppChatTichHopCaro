using ChatAppClient.Helpers;
using System.Drawing;
using System.Windows.Forms;

namespace ChatAppClient.UserControls
{
    public partial class FriendListItem : UserControl
    {
        public string FriendID { get; private set; }
        public string FriendName { get; private set; }
        public string FriendStatus { get; private set; }
        private bool _isOnline;

        public FriendListItem()
        {
            InitializeComponent();
        }

        public void SetData(string id, string name, string status, bool isOnline)
        {
            FriendID = id;
            FriendName = name;
            FriendStatus = status;
            _isOnline = isOnline;

            lblFriendName.Text = name;
            lblStatus.Text = status;

            ApplyStatusColors(isOnline);

            // Load Avatar
            string avatarPath = System.IO.Path.Combine("Images", $"{id}.png");
            if (System.IO.File.Exists(avatarPath))
            {
                try
                {
                    using (var bmp = new Bitmap(avatarPath)) pbAvatar.Image = new Bitmap(bmp);
                }
                catch { }
            }
        }

        private void ApplyStatusColors(bool isOnline)
        {
            bool isDark = ThemeManager.IsDarkMode;

            if (isOnline)
            {
                pnlStatusDot.BackColor = ThemeManager.Online;
                lblStatus.ForeColor = ThemeManager.Online;
                lblFriendName.ForeColor = ThemeManager.TextPrimary;
            }
            else
            {
                pnlStatusDot.BackColor = ThemeManager.Offline;
                lblStatus.ForeColor = ThemeManager.Offline;
                lblFriendName.ForeColor = ThemeManager.TextSecondary;
            }
        }

        public void SetNewMessageAlert(bool hasNewMessage)
        {
            lblNewMessageBadge.Visible = hasNewMessage;
            bool isDark = ThemeManager.IsDarkMode;
            this.BackColor = hasNewMessage
                ? (isDark ? Color.FromArgb(45, 48, 60) : Color.FromArgb(230, 235, 245))
                : (isDark ? Color.FromArgb(30, 33, 45) : Color.FromArgb(245, 245, 250));
        }

        /// <summary>
        /// Áp dụng theme cho item
        /// </summary>
        public void ApplyTheme(bool isDarkMode)
        {
            this.BackColor = isDarkMode
                 ? Color.FromArgb(30, 33, 45)
                    : Color.FromArgb(245, 245, 250);

            ApplyStatusColors(_isOnline);
        }
    }
}