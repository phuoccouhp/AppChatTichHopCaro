using ChatAppClient.UserControls;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        public string? SelectedFriendID { get; private set; }
        public string? SelectedFriendName { get; private set; }

        public frmForwardMessage(List<(string id, string name)> friends)
        {
            InitializeComponent();
            LoadFriends(friends);
        }

        private void InitializeComponent()
        {
            this.lblTitle = new Label();
            this.lstFriends = new ListBox();
            this.btnForward = new Button();
            this.btnCancel = new Button();
            this.SuspendLayout();

            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.lblTitle.Location = new Point(12, 12);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Size(200, 28);
            this.lblTitle.Text = "Chọn người nhận:";

            // lstFriends
            this.lstFriends.Font = new Font("Segoe UI", 10F);
            this.lstFriends.FormattingEnabled = true;
            this.lstFriends.ItemHeight = 25;
            this.lstFriends.Location = new Point(12, 50);
            this.lstFriends.Name = "lstFriends";
            this.lstFriends.Size = new Size(360, 300);
            this.lstFriends.TabIndex = 0;

            // btnForward
            this.btnForward.BackColor = Color.FromArgb(0, 145, 255);
            this.btnForward.FlatStyle = FlatStyle.Flat;
            this.btnForward.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnForward.ForeColor = Color.White;
            this.btnForward.Location = new Point(200, 360);
            this.btnForward.Name = "btnForward";
            this.btnForward.Size = new Size(82, 35);
            this.btnForward.TabIndex = 1;
            this.btnForward.Text = "Chuyển tiếp";
            this.btnForward.UseVisualStyleBackColor = false;
            this.btnForward.Click += BtnForward_Click;

            // btnCancel
            this.btnCancel.BackColor = Color.Gray;
            this.btnCancel.FlatStyle = FlatStyle.Flat;
            this.btnCancel.Font = new Font("Segoe UI", 10F);
            this.btnCancel.ForeColor = Color.White;
            this.btnCancel.Location = new Point(290, 360);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(82, 35);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Hủy";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += BtnCancel_Click;

            // frmForwardMessage
            this.AutoScaleDimensions = new SizeF(8F, 20F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(384, 410);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnForward);
            this.Controls.Add(this.lstFriends);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmForwardMessage";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Chuyển tiếp tin nhắn";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadFriends(List<(string id, string name)> friends)
        {
            lstFriends.Items.Clear();
            foreach (var friend in friends)
            {
                lstFriends.Items.Add($"{friend.name} ({friend.id})");
            }
        }

        private void BtnForward_Click(object? sender, EventArgs e)
        {
            if (lstFriends.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn người nhận.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string selected = lstFriends.SelectedItem.ToString() ?? "";
            // Parse "Name (ID)" format
            int openParen = selected.LastIndexOf('(');
            int closeParen = selected.LastIndexOf(')');
            if (openParen != -1 && closeParen != -1)
            {
                SelectedFriendID = selected.Substring(openParen + 1, closeParen - openParen - 1);
                SelectedFriendName = selected.Substring(0, openParen).Trim();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}

