using System;
using System.Drawing;
using System.Windows.Forms;
using ChatAppClient.Helpers;

namespace ChatAppClient.UserControls
{
    public partial class GroupListItem : UserControl
    {
        public string GroupID { get; private set; }
        public string GroupName { get; private set; }
        public int MemberCount { get; private set; }

        private Label lblGroupName;
        private Label lblMemberCount;
        private Label lblLastMessage;
        private Panel pnlGroupIcon;
        private Label lblNewMessageBadge;

        public GroupListItem()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(280, 70);
            this.BackColor = Color.FromArgb(30, 33, 45);
            this.Cursor = Cursors.Hand;
            this.Padding = new Padding(10);

            // Group Icon (Circle with initials)
            pnlGroupIcon = new Panel();
            pnlGroupIcon.Size = new Size(45, 45);
            pnlGroupIcon.Location = new Point(10, 12);
            pnlGroupIcon.BackColor = Color.FromArgb(88, 101, 242);
            pnlGroupIcon.Paint += (s, e) => {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    path.AddEllipse(0, 0, 44, 44);
                    e.Graphics.SetClip(path);
                    e.Graphics.Clear(Color.FromArgb(88, 101, 242));
                    
                    // Draw group icon
                    using (var font = new Font("Segoe UI", 16, FontStyle.Bold))
                    using (var brush = new SolidBrush(Color.White))
                    {
                        string initial = GroupName?.Length > 0 ? GroupName.Substring(0, 1).ToUpper() : "G";
                        var size = e.Graphics.MeasureString(initial, font);
                        e.Graphics.DrawString(initial, font, brush, 
                            (44 - size.Width) / 2, (44 - size.Height) / 2);
                    }
                }
            };
            this.Controls.Add(pnlGroupIcon);

            // Group Name
            lblGroupName = new Label();
            lblGroupName.AutoSize = false;
            lblGroupName.Size = new Size(180, 22);
            lblGroupName.Location = new Point(65, 10);
            lblGroupName.Font = new Font("Segoe UI Semibold", 11);
            lblGroupName.ForeColor = Color.White;
            lblGroupName.Text = "Group Name";
            this.Controls.Add(lblGroupName);

            // Member Count
            lblMemberCount = new Label();
            lblMemberCount.AutoSize = true;
            lblMemberCount.Location = new Point(65, 32);
            lblMemberCount.Font = new Font("Segoe UI", 9);
            lblMemberCount.ForeColor = Color.Gray;
            lblMemberCount.Text = "0 thành viên";
            this.Controls.Add(lblMemberCount);

            // Last Message
            lblLastMessage = new Label();
            lblLastMessage.AutoSize = false;
            lblLastMessage.Size = new Size(180, 18);
            lblLastMessage.Location = new Point(65, 50);
            lblLastMessage.Font = new Font("Segoe UI", 8);
            lblLastMessage.ForeColor = Color.DarkGray;
            lblLastMessage.Text = "";
            this.Controls.Add(lblLastMessage);

            // New Message Badge
            lblNewMessageBadge = new Label();
            lblNewMessageBadge.AutoSize = false;
            lblNewMessageBadge.Size = new Size(20, 20);
            lblNewMessageBadge.Location = new Point(255, 25);
            lblNewMessageBadge.BackColor = Color.Red;
            lblNewMessageBadge.ForeColor = Color.White;
            lblNewMessageBadge.Text = "!";
            lblNewMessageBadge.TextAlign = ContentAlignment.MiddleCenter;
            lblNewMessageBadge.Font = new Font("Segoe UI", 8, FontStyle.Bold);
            lblNewMessageBadge.Visible = false;
            this.Controls.Add(lblNewMessageBadge);

            // Hover effect
            this.MouseEnter += (s, e) => ApplyHoverEffect(true);
            this.MouseLeave += (s, e) => ApplyHoverEffect(false);
     
            foreach (Control ctrl in this.Controls)
            {
                ctrl.MouseEnter += (s, e) => ApplyHoverEffect(true);
                ctrl.MouseLeave += (s, e) => ApplyHoverEffect(false);
            }
        }
        
        private void ApplyHoverEffect(bool isHovered)
        {
            bool isDark = ThemeManager.IsDarkMode;
            if (isHovered)
         {
this.BackColor = isDark ? Color.FromArgb(40, 43, 55) : Color.FromArgb(230, 235, 245);
 }
   else
     {
        this.BackColor = isDark ? Color.FromArgb(30, 33, 45) : Color.FromArgb(245, 245, 250);
   }
        }

        public void SetData(string groupId, string groupName, int memberCount, string lastMessage = null)
        {
            GroupID = groupId;
            GroupName = groupName;
            MemberCount = memberCount;

            lblGroupName.Text = groupName;
            lblMemberCount.Text = $"{memberCount} thành viên";
            lblLastMessage.Text = lastMessage ?? "";
     
          pnlGroupIcon.Invalidate();
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
       
            lblGroupName.ForeColor = isDarkMode ? Color.White : Color.Black;
            lblMemberCount.ForeColor = ThemeManager.TextMuted;
     lblLastMessage.ForeColor = ThemeManager.TextMuted;
        }
    }
}


