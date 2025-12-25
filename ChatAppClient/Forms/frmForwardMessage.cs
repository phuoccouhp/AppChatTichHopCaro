using ChatAppClient.UserControls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace ChatAppClient.Forms
{
    public partial class frmForwardMessage : Form
    {
        private ListBox lstFriends;
        private Button btnForward;
        private Button btnCancel;
        private Label lblTitle;
        private Panel pnlHeader;
        private Panel pnlContent;
        private Panel pnlFooter;

        public string? SelectedFriendID { get; private set; }
        public string? SelectedFriendName { get; private set; }

        private List<(string id, string name)> _friends;

        public frmForwardMessage(List<(string id, string name)> friends)
        {
            _friends = friends;
            InitializeComponent();
            LoadFriends(friends);
        }

        private void InitializeComponent()
        {
            this.pnlHeader = new Panel();
            this.lblTitle = new Label();
            this.pnlContent = new Panel();
            this.lstFriends = new ListBox();
            this.pnlFooter = new Panel();
            this.btnForward = new Button();
            this.btnCancel = new Button();
            this.SuspendLayout();

            // Form
            this.BackColor = Color.FromArgb(54, 57, 63);
            this.ClientSize = new Size(400, 450);
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmForwardMessage";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Chuy?n ti?p tin nh?n";

            // pnlHeader
            this.pnlHeader.BackColor = Color.FromArgb(47, 49, 54);
            this.pnlHeader.Dock = DockStyle.Top;
            this.pnlHeader.Size = new Size(400, 60);
            this.pnlHeader.Padding = new Padding(15);

            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.White;
            this.lblTitle.Location = new Point(15, 16);
            this.lblTitle.Text = "?? Chuy?n ti?p ??n...";
            this.pnlHeader.Controls.Add(this.lblTitle);

            // Close button
            Button btnClose = new Button();
            btnClose.Text = "?";
            btnClose.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnClose.ForeColor = Color.Gray;
            btnClose.BackColor = Color.Transparent;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 80, 80);
            btnClose.Size = new Size(40, 40);
            btnClose.Location = new Point(350, 10);
            btnClose.Cursor = Cursors.Hand;
            btnClose.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.pnlHeader.Controls.Add(btnClose);

            // pnlContent
            this.pnlContent.BackColor = Color.FromArgb(54, 57, 63);
            this.pnlContent.Dock = DockStyle.Fill;
            this.pnlContent.Padding = new Padding(15, 10, 15, 10);

            // lstFriends
            this.lstFriends.BackColor = Color.FromArgb(64, 68, 75);
            this.lstFriends.ForeColor = Color.White;
            this.lstFriends.Font = new Font("Segoe UI", 11F);
            this.lstFriends.BorderStyle = BorderStyle.None;
            this.lstFriends.ItemHeight = 40;
            this.lstFriends.Dock = DockStyle.Fill;
            this.lstFriends.DrawMode = DrawMode.OwnerDrawFixed;
            this.lstFriends.DrawItem += LstFriends_DrawItem;
            this.lstFriends.DoubleClick += (s, e) => BtnForward_Click(s, e);
            this.pnlContent.Controls.Add(this.lstFriends);

            // pnlFooter
            this.pnlFooter.BackColor = Color.FromArgb(47, 49, 54);
            this.pnlFooter.Dock = DockStyle.Bottom;
            this.pnlFooter.Size = new Size(400, 70);
            this.pnlFooter.Padding = new Padding(15);

            // btnForward
            this.btnForward.BackColor = Color.FromArgb(88, 101, 242);
            this.btnForward.FlatStyle = FlatStyle.Flat;
            this.btnForward.FlatAppearance.BorderSize = 0;
            this.btnForward.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            this.btnForward.ForeColor = Color.White;
            this.btnForward.Size = new Size(120, 40);
            this.btnForward.Location = new Point(155, 15);
            this.btnForward.Text = "Chuy?n ti?p";
            this.btnForward.Cursor = Cursors.Hand;
            this.btnForward.Click += BtnForward_Click;
            this.pnlFooter.Controls.Add(this.btnForward);

            // btnCancel
            this.btnCancel.BackColor = Color.FromArgb(64, 68, 75);
            this.btnCancel.FlatStyle = FlatStyle.Flat;
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.Font = new Font("Segoe UI", 11F);
            this.btnCancel.ForeColor = Color.White;
            this.btnCancel.Size = new Size(90, 40);
            this.btnCancel.Location = new Point(285, 15);
            this.btnCancel.Text = "H?y";
            this.btnCancel.Cursor = Cursors.Hand;
            this.btnCancel.Click += BtnCancel_Click;
            this.pnlFooter.Controls.Add(this.btnCancel);

            // Add panels
            this.Controls.Add(this.pnlContent);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlHeader);

            // Enable dragging
            this.pnlHeader.MouseDown += Form_MouseDown;
            this.lblTitle.MouseDown += Form_MouseDown;

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // Custom draw cho ListBox
        private void LstFriends_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            // Background
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color bgColor = isSelected ? Color.FromArgb(88, 101, 242) : Color.FromArgb(64, 68, 75);
            
            using (SolidBrush bgBrush = new SolidBrush(bgColor))
            {
                e.Graphics.FillRectangle(bgBrush, e.Bounds);
            }

            // Hover effect
            if (!isSelected && e.Index == _hoveredIndex)
            {
                using (SolidBrush hoverBrush = new SolidBrush(Color.FromArgb(79, 84, 92)))
                {
                    e.Graphics.FillRectangle(hoverBrush, e.Bounds);
                }
            }

            // Get friend info
            var friend = _friends[e.Index];
            
            // Avatar circle
            Rectangle avatarRect = new Rectangle(e.Bounds.X + 10, e.Bounds.Y + 5, 30, 30);
            using (SolidBrush avatarBrush = new SolidBrush(GetAvatarColor(friend.id)))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(avatarBrush, avatarRect);
            }
            
            // Avatar letter
            using (Font letterFont = new Font("Segoe UI", 12, FontStyle.Bold))
            using (SolidBrush letterBrush = new SolidBrush(Color.White))
            {
                string letter = friend.name.Length > 0 ? friend.name[0].ToString().ToUpper() : "?";
                SizeF letterSize = e.Graphics.MeasureString(letter, letterFont);
                float letterX = avatarRect.X + (avatarRect.Width - letterSize.Width) / 2;
                float letterY = avatarRect.Y + (avatarRect.Height - letterSize.Height) / 2;
                e.Graphics.DrawString(letter, letterFont, letterBrush, letterX, letterY);
            }

            // Name
            using (Font nameFont = new Font("Segoe UI", 11))
            using (SolidBrush textBrush = new SolidBrush(Color.White))
            {
                e.Graphics.DrawString(friend.name, nameFont, textBrush, e.Bounds.X + 50, e.Bounds.Y + 10);
            }
        }

        private int _hoveredIndex = -1;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            lstFriends.MouseMove += (s, ev) =>
            {
                int index = lstFriends.IndexFromPoint(ev.Location);
                if (index != _hoveredIndex)
                {
                    _hoveredIndex = index;
                    lstFriends.Invalidate();
                }
            };
            lstFriends.MouseLeave += (s, ev) =>
            {
                _hoveredIndex = -1;
                lstFriends.Invalidate();
            };
        }

        private Color GetAvatarColor(string id)
        {
            Color[] colors = {
                Color.FromArgb(88, 101, 242),  // Blue
                Color.FromArgb(87, 242, 135),  // Green
                Color.FromArgb(254, 231, 92),  // Yellow
                Color.FromArgb(237, 66, 69),   // Red
                Color.FromArgb(235, 69, 158),  // Pink
            };
            int hash = id.GetHashCode();
            return colors[Math.Abs(hash) % colors.Length];
        }

        private void LoadFriends(List<(string id, string name)> friends)
        {
            lstFriends.Items.Clear();
            foreach (var friend in friends)
            {
                lstFriends.Items.Add(friend.name);
            }
        }

        private void BtnForward_Click(object? sender, EventArgs e)
        {
            if (lstFriends.SelectedIndex < 0)
            {
                // Flash effect
                lstFriends.BackColor = Color.FromArgb(237, 66, 69);
                System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
                timer.Interval = 200;
                timer.Tick += (s, ev) =>
                {
                    lstFriends.BackColor = Color.FromArgb(64, 68, 75);
                    timer.Stop();
                    timer.Dispose();
                };
                timer.Start();
                return;
            }

            var friend = _friends[lstFriends.SelectedIndex];
            SelectedFriendID = friend.id;
            SelectedFriendName = friend.name;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // Dragging support
        private Point _dragStart;
        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _dragStart = new Point(e.X, e.Y);
                this.pnlHeader.MouseMove += Form_MouseMove;
                this.pnlHeader.MouseUp += Form_MouseUp;
            }
        }

        private void Form_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Left += e.X - _dragStart.X;
                this.Top += e.Y - _dragStart.Y;
            }
        }

        private void Form_MouseUp(object sender, MouseEventArgs e)
        {
            this.pnlHeader.MouseMove -= Form_MouseMove;
            this.pnlHeader.MouseUp -= Form_MouseUp;
        }
    }
}
