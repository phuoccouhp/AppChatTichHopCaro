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

        public FriendListItem()
        {
            InitializeComponent();
        }

        public void SetData(string id, string name, string status, bool isOnline)
        {
            FriendID = id;
            FriendName = name;
            FriendStatus = status;

            lblFriendName.Text = name;
            lblStatus.Text = status;

            if (isOnline)
            {
                // Trạng thái Online: Sáng sủa
                pnlStatusDot.BackColor = Color.LimeGreen;
                lblStatus.ForeColor = Color.LimeGreen;
                lblFriendName.ForeColor = Color.White;
            }
            else
            {
                // Trạng thái Offline: Tối màu
                pnlStatusDot.BackColor = Color.Gray;
                lblStatus.ForeColor = Color.Gray;
                lblFriendName.ForeColor = Color.Silver;
            }

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

        public void SetNewMessageAlert(bool hasNewMessage)
        {
            lblNewMessageBadge.Visible = hasNewMessage;
            this.BackColor = hasNewMessage ? Color.FromArgb(45, 48, 60) : Color.FromArgb(30, 33, 45);
        }
    }
}