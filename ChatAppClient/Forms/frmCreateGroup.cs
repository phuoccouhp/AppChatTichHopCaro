using ChatApp.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ChatAppClient.Forms
{
    public partial class frmCreateGroup : Form
    {
        private TextBox txtGroupName;
        private CheckedListBox clbMembers;
        private Button btnCreate;
        private Button btnCancel;
        private Label lblTitle;
        private Label lblGroupName;
        private Label lblMembers;

        private List<UserStatus> _availableUsers;

        public CreateGroupPacket Result { get; private set; }

        public frmCreateGroup(List<UserStatus> availableUsers)
        {
            _availableUsers = availableUsers;
            InitializeComponent();
            LoadUsers();
        }

        private void InitializeComponent()
        {
            this.Text = "Tạo Nhóm Chat Mới";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(30, 33, 45);

            // Title
            lblTitle = new Label();
            lblTitle.Text = "🗨️ Tạo Nhóm Chat";
            lblTitle.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(20, 20);
            this.Controls.Add(lblTitle);

            // Group Name Label
            lblGroupName = new Label();
            lblGroupName.Text = "Tên nhóm:";
            lblGroupName.Font = new Font("Segoe UI", 10);
            lblGroupName.ForeColor = Color.White;
            lblGroupName.AutoSize = true;
            lblGroupName.Location = new Point(20, 70);
            this.Controls.Add(lblGroupName);

            // Group Name TextBox
            txtGroupName = new TextBox();
            txtGroupName.Font = new Font("Segoe UI", 11);
            txtGroupName.Size = new Size(340, 30);
            txtGroupName.Location = new Point(20, 95);
            txtGroupName.BackColor = Color.FromArgb(45, 48, 60);
            txtGroupName.ForeColor = Color.White;
            txtGroupName.BorderStyle = BorderStyle.FixedSingle;
            this.Controls.Add(txtGroupName);

            // Members Label
            lblMembers = new Label();
            lblMembers.Text = "Chọn thành viên:";
            lblMembers.Font = new Font("Segoe UI", 10);
            lblMembers.ForeColor = Color.White;
            lblMembers.AutoSize = true;
            lblMembers.Location = new Point(20, 140);
            this.Controls.Add(lblMembers);

            // Members CheckedListBox
            clbMembers = new CheckedListBox();
            clbMembers.Font = new Font("Segoe UI", 10);
            clbMembers.Size = new Size(340, 250);
            clbMembers.Location = new Point(20, 165);
            clbMembers.BackColor = Color.FromArgb(45, 48, 60);
            clbMembers.ForeColor = Color.White;
            clbMembers.BorderStyle = BorderStyle.FixedSingle;
            clbMembers.CheckOnClick = true;
            this.Controls.Add(clbMembers);

            // Create Button
            btnCreate = new Button();
            btnCreate.Text = "Tạo Nhóm";
            btnCreate.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnCreate.Size = new Size(120, 35);
            btnCreate.Location = new Point(130, 420);
            btnCreate.BackColor = Color.FromArgb(88, 101, 242);
            btnCreate.ForeColor = Color.White;
            btnCreate.FlatStyle = FlatStyle.Flat;
            btnCreate.FlatAppearance.BorderSize = 0;
            btnCreate.Cursor = Cursors.Hand;
            btnCreate.Click += BtnCreate_Click;
            this.Controls.Add(btnCreate);

            // Cancel Button
            btnCancel = new Button();
            btnCancel.Text = "Hủy";
            btnCancel.Font = new Font("Segoe UI", 10);
            btnCancel.Size = new Size(80, 35);
            btnCancel.Location = new Point(260, 420);
            btnCancel.BackColor = Color.FromArgb(60, 63, 75);
            btnCancel.ForeColor = Color.White;
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancel);
        }

        private void LoadUsers()
        {
            clbMembers.Items.Clear();
            foreach (var user in _availableUsers)
            {
                string status = user.IsOnline ? "🟢" : "⚪";
                clbMembers.Items.Add($"{status} {user.UserName} ({user.UserID})", false);
            }
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            string groupName = txtGroupName.Text.Trim();
            
            if (string.IsNullOrEmpty(groupName))
            {
                MessageBox.Show("Vui lòng nhập tên nhóm!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (clbMembers.CheckedItems.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn ít nhất 1 thành viên!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get selected member IDs
            var memberIds = new List<string>();
            foreach (var item in clbMembers.CheckedItems)
            {
                string text = item.ToString();
                // Extract UserID from format "🟢 DisplayName (UserID)"
                int start = text.LastIndexOf('(') + 1;
                int end = text.LastIndexOf(')');
                if (start > 0 && end > start)
                {
                    string userId = text.Substring(start, end - start);
                    memberIds.Add(userId);
                }
            }

            Result = new CreateGroupPacket
            {
                CreatorID = NetworkManager.Instance.UserID,
                GroupName = groupName,
                MemberIDs = memberIds
            };

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}


