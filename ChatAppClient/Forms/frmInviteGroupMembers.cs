using ChatApp.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ChatAppClient.Forms
{
    public class frmInviteGroupMembers : Form
    {
        private CheckedListBox clbMembers;
        private Button btnInvite;
        private Button btnCancel;
        private Label lblTitle;

        private string _groupId;
        private string _groupName;
        private List<GroupMemberInfo> _currentMembers;
        private List<UserStatus> _availableUsers;

        public List<string> SelectedMembers { get; private set; }

        public frmInviteGroupMembers(string groupId, string groupName, List<GroupMemberInfo> currentMembers)
        {
            _groupId = groupId;
            _groupName = groupName;
            _currentMembers = currentMembers ?? new List<GroupMemberInfo>();
            _availableUsers = new List<UserStatus>();

            InitializeComponent();
            LoadAvailableUsers();
        }

        private void InitializeComponent()
        {
            this.Text = "Mời thành viên";
            this.Size = new Size(350, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(30, 33, 45);

            // Title
            lblTitle = new Label();
            lblTitle.Text = $"Mời vào nhóm: {_groupName}";
            lblTitle.Font = new Font("Segoe UI Semibold", 12);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 20);
            this.Controls.Add(lblTitle);

            // Members CheckedListBox
            clbMembers = new CheckedListBox();
            clbMembers.Font = new Font("Segoe UI", 10);
            clbMembers.Size = new Size(300, 300);
            clbMembers.Location = new Point(20, 60);
            clbMembers.BackColor = Color.FromArgb(45, 48, 60);
            clbMembers.ForeColor = Color.White;
            clbMembers.BorderStyle = BorderStyle.FixedSingle;
            clbMembers.CheckOnClick = true;
            this.Controls.Add(clbMembers);

            // Invite Button
            btnInvite = new Button();
            btnInvite.Text = "Mời";
            btnInvite.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnInvite.Size = new Size(100, 35);
            btnInvite.Location = new Point(100, 370);
            btnInvite.BackColor = Color.FromArgb(88, 101, 242);
            btnInvite.ForeColor = Color.White;
            btnInvite.FlatStyle = FlatStyle.Flat;
            btnInvite.FlatAppearance.BorderSize = 0;
            btnInvite.Cursor = Cursors.Hand;
            btnInvite.Click += BtnInvite_Click;
            this.Controls.Add(btnInvite);

            // Cancel Button
            btnCancel = new Button();
            btnCancel.Text = "Hủy";
            btnCancel.Font = new Font("Segoe UI", 10);
            btnCancel.Size = new Size(80, 35);
            btnCancel.Location = new Point(210, 370);
            btnCancel.BackColor = Color.FromArgb(60, 63, 75);
            btnCancel.ForeColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancel);
        }

        private void LoadAvailableUsers()
        {
            // Lấy danh sách bạn bè từ form chính
            var homeForm = Application.OpenForms.OfType<frmHome>().FirstOrDefault();
            if (homeForm != null)
            {
                var friends = homeForm.GetFriendsList();
                var existingMemberIds = _currentMembers.Select(m => m.UserID).ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var friend in friends)
                {
                    // Chỉ hiện những người chưa trong nhóm
                    if (!existingMemberIds.Contains(friend.id))
                    {
                        clbMembers.Items.Add($"{friend.name} ({friend.id})", false);
                        _availableUsers.Add(new UserStatus { UserID = friend.id, UserName = friend.name });
                    }
                }
            }

            if (clbMembers.Items.Count == 0)
            {
                clbMembers.Items.Add("(Không có người để mời)");
                clbMembers.Enabled = false;
                btnInvite.Enabled = false;
            }
        }

        private void BtnInvite_Click(object sender, EventArgs e)
        {
            if (clbMembers.CheckedItems.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất 1 người!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SelectedMembers = new List<string>();
            foreach (var item in clbMembers.CheckedItems)
            {
                string text = item.ToString();
                // Extract UserID from format "DisplayName (UserID)"
                int start = text.LastIndexOf('(') + 1;
                int end = text.LastIndexOf(')');
                if (start > 0 && end > start)
                {
                    string userId = text.Substring(start, end - start);
                    SelectedMembers.Add(userId);
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}

