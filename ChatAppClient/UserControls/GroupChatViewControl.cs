using ChatApp.Shared;
using ChatAppClient.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ChatAppClient.UserControls
{
    public partial class GroupChatViewControl : UserControl
    {
        private string _groupId;
        private string _groupName;
        private string _myId;
        private List<GroupMemberInfo> _members;

        private Panel pnlHeader;
        private Label lblGroupName;
        private Label lblMemberCount;
        private Button btnInvite;
        private FlowLayoutPanel flpMessages;
        private Panel pnlInput;
        private TextBox txtMessage;
        private Button btnSend;
        private Button btnAttach;

        public GroupChatViewControl(string groupId, string groupName, List<GroupMemberInfo> members)
        {
            _groupId = groupId;
            _groupName = groupName;
            _myId = NetworkManager.Instance.UserID;
            _members = members ?? new List<GroupMemberInfo>();

            InitializeComponent();
            LoadHistoryAsync();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(54, 57, 63);
            this.Dock = DockStyle.Fill;

            // Header
            pnlHeader = new Panel();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 60;
            pnlHeader.BackColor = Color.FromArgb(47, 49, 54);
            pnlHeader.Padding = new Padding(15, 10, 15, 10);
            this.Controls.Add(pnlHeader);

            lblGroupName = new Label();
            lblGroupName.Text = "# " + _groupName;
            lblGroupName.Font = new Font("Segoe UI Semibold", 14);
            lblGroupName.ForeColor = Color.White;
            lblGroupName.AutoSize = true;
            lblGroupName.Location = new Point(15, 8);
            pnlHeader.Controls.Add(lblGroupName);

            lblMemberCount = new Label();
            lblMemberCount.Text = $"{_members.Count} thành viên";
            lblMemberCount.Font = new Font("Segoe UI", 9);
            lblMemberCount.ForeColor = Color.Gray;
            lblMemberCount.AutoSize = true;
            lblMemberCount.Location = new Point(15, 35);
            pnlHeader.Controls.Add(lblMemberCount);

            btnInvite = new Button();
            btnInvite.Text = "+ Mời";
            btnInvite.Font = new Font("Segoe UI", 9);
            btnInvite.Size = new Size(60, 30);
            btnInvite.Location = new Point(this.Width - 90, 15);
            btnInvite.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnInvite.BackColor = Color.FromArgb(88, 101, 242);
            btnInvite.ForeColor = Color.White;
            btnInvite.FlatStyle = FlatStyle.Flat;
            btnInvite.FlatAppearance.BorderSize = 0;
            btnInvite.Cursor = Cursors.Hand;
            btnInvite.Click += BtnInvite_Click;
            pnlHeader.Controls.Add(btnInvite);

            // Messages Area
            flpMessages = new FlowLayoutPanel();
            flpMessages.Dock = DockStyle.Fill;
            flpMessages.AutoScroll = true;
            flpMessages.FlowDirection = FlowDirection.TopDown;
            flpMessages.WrapContents = false;
            flpMessages.BackColor = Color.FromArgb(54, 57, 63);
            flpMessages.Padding = new Padding(10);
            this.Controls.Add(flpMessages);

            // Input Area
            pnlInput = new Panel();
            pnlInput.Dock = DockStyle.Bottom;
            pnlInput.Height = 60;
            pnlInput.BackColor = Color.FromArgb(64, 68, 75);
            pnlInput.Padding = new Padding(15, 10, 15, 10);
            this.Controls.Add(pnlInput);

            btnAttach = new Button();
            btnAttach.Text = "📎";
            btnAttach.Font = new Font("Segoe UI", 12);
            btnAttach.Size = new Size(40, 40);
            btnAttach.Location = new Point(10, 10);
            btnAttach.BackColor = Color.FromArgb(54, 57, 63);
            btnAttach.ForeColor = Color.White;
            btnAttach.FlatStyle = FlatStyle.Flat;
            btnAttach.FlatAppearance.BorderSize = 0;
            btnAttach.Cursor = Cursors.Hand;
            btnAttach.Click += BtnAttach_Click;
            pnlInput.Controls.Add(btnAttach);

            txtMessage = new TextBox();
            txtMessage.Font = new Font("Segoe UI", 11);
            txtMessage.Location = new Point(60, 12);
            txtMessage.Size = new Size(this.Width - 170, 35);
            txtMessage.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            txtMessage.BackColor = Color.FromArgb(54, 57, 63);
            txtMessage.ForeColor = Color.White;
            txtMessage.BorderStyle = BorderStyle.FixedSingle;
            txtMessage.KeyDown += TxtMessage_KeyDown;
            pnlInput.Controls.Add(txtMessage);

            btnSend = new Button();
            btnSend.Text = "Gửi";
            btnSend.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnSend.Size = new Size(60, 40);
            btnSend.Location = new Point(this.Width - 90, 10);
            btnSend.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnSend.BackColor = Color.FromArgb(88, 101, 242);
            btnSend.ForeColor = Color.White;
            btnSend.FlatStyle = FlatStyle.Flat;
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.Cursor = Cursors.Hand;
            btnSend.Click += BtnSend_Click;
            pnlInput.Controls.Add(btnSend);

            // Ensure order
            this.Controls.SetChildIndex(pnlInput, 0);
            this.Controls.SetChildIndex(pnlHeader, 2);
            this.Controls.SetChildIndex(flpMessages, 1);
        }

        private void TxtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                BtnSend_Click(sender, e);
            }
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            string message = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(message)) return;

            var packet = new GroupTextPacket
            {
                GroupID = _groupId,
                SenderID = _myId,
                SenderName = NetworkManager.Instance.UserName,
                MessageContent = message,
                SentAt = DateTime.Now
            };

            if (NetworkManager.Instance.SendPacket(packet))
            {
                AddMessageBubble(_myId, NetworkManager.Instance.UserName, message, MessageType.Outgoing, DateTime.Now);
                txtMessage.Clear();
            }
        }

        private void BtnAttach_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "All Files|*.*|Images|*.png;*.jpg;*.jpeg;*.gif;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        byte[] data = File.ReadAllBytes(ofd.FileName);
                        string fileName = Path.GetFileName(ofd.FileName);
                        string ext = Path.GetExtension(fileName).ToLower();
                        bool isImage = ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".gif" || ext == ".bmp";

                        var packet = new GroupFilePacket
                        {
                            GroupID = _groupId,
                            SenderID = _myId,
                            SenderName = NetworkManager.Instance.UserName,
                            FileName = fileName,
                            FileData = data,
                            IsImage = isImage
                        };

                        if (NetworkManager.Instance.SendPacket(packet))
                        {
                            AddSystemMessage($"Đã gửi {(isImage ? "ảnh" : "file")}: {fileName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi gửi file: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnInvite_Click(object sender, EventArgs e)
        {
            // Mở dialog để mời thêm thành viên
            var inviteForm = new frmInviteGroupMembers(_groupId, _groupName, _members);
            if (inviteForm.ShowDialog() == DialogResult.OK && inviteForm.SelectedMembers != null && inviteForm.SelectedMembers.Count > 0)
            {
                var packet = new GroupInvitePacket
                {
                    GroupID = _groupId,
                    GroupName = _groupName,
                    InviterID = _myId,
                    InviterName = NetworkManager.Instance.UserName,
                    InviteeIDs = inviteForm.SelectedMembers
                };
                
                if (NetworkManager.Instance.SendPacket(packet))
                {
                    AddSystemMessage($"Đã gửi lời mời cho {inviteForm.SelectedMembers.Count} người");
                }
            }
        }

        private async void LoadHistoryAsync()
        {
            AddSystemMessage($"Chào mừng đến nhóm {_groupName}!");
            
            try
            {
                // Yêu cầu lịch sử chat nhóm từ server
                var request = new GroupHistoryRequestPacket
                {
                    GroupID = _groupId,
                    UserID = _myId,
                    Limit = 100
                };
                
                if (NetworkManager.Instance.SendPacket(request))
                {
                    // Chờ response từ server thông qua event
                    // Response sẽ được xử lý qua HandleGroupHistoryResponse
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi load lịch sử nhóm: {ex.Message}");
            }
        }
        
        // Xử lý khi nhận lịch sử từ server
        public void LoadHistory(List<GroupMessageHistory> messages)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => LoadHistory(messages)));
                return;
            }
            
            if (messages == null) return;
            
            foreach (var msg in messages)
            {
                MessageType type = (msg.SenderID == _myId) ? MessageType.Outgoing : MessageType.Incoming;
                
                if (msg.MessageType == "Text")
                {
                    AddMessageBubble(msg.SenderID, msg.SenderName ?? msg.SenderID, msg.MessageContent ?? "", type, msg.CreatedAt);
                }
                else if (msg.MessageType == "File" || msg.MessageType == "Image")
                {
                    AddSystemMessage($"[{msg.SenderName}] đã gửi {msg.MessageType}: {msg.FileName}");
                }
            }
        }

        public void ReceiveMessage(GroupTextPacket packet)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ReceiveMessage(packet)));
                return;
            }

            MessageType type = (packet.SenderID == _myId) ? MessageType.Outgoing : MessageType.Incoming;
            AddMessageBubble(packet.SenderID, packet.SenderName, packet.MessageContent, type, packet.SentAt);
        }

        public void ReceiveFile(GroupFilePacket packet)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ReceiveFile(packet)));
                return;
            }

            AddSystemMessage($"{packet.SenderName} đã gửi {(packet.IsImage ? "ảnh" : "file")}: {packet.FileName}");
        }

        private void AddMessageBubble(string senderId, string senderName, string message, MessageType type, DateTime time)
        {
            // Create a panel to hold sender name + bubble
            Panel container = new Panel();
            container.AutoSize = false;
            int scrollWidth = SystemInformation.VerticalScrollBarWidth;
            container.Width = flpMessages.ClientSize.Width - scrollWidth - 5;
            container.BackColor = Color.Transparent;
            container.Margin = new Padding(0, 0, 0, 5);

            int yOffset = 0;

            // For incoming messages, show sender name
            if (type == MessageType.Incoming)
            {
                Label lblSender = new Label();
                lblSender.Text = senderName;
                lblSender.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                lblSender.ForeColor = GetSenderColor(senderId);
                lblSender.AutoSize = true;
                lblSender.Location = new Point(5, 0);
                container.Controls.Add(lblSender);
                yOffset = 20;
            }

            // Message bubble
            var bubble = new ChatMessageBubble();
            bubble.SetData(message, type, time);
            
            if (type == MessageType.Outgoing)
            {
                bubble.Location = new Point(container.Width - bubble.Width - 10, yOffset);
            }
            else
            {
                bubble.Location = new Point(5, yOffset);
            }
            
            container.Controls.Add(bubble);
            container.Height = yOffset + bubble.Height + 10;

            flpMessages.Controls.Add(container);
            ScrollToBottom();
        }

        private Color GetSenderColor(string senderId)
        {
            // Generate consistent color based on user ID
            int hash = senderId.GetHashCode();
            Color[] colors = {
                Color.FromArgb(114, 137, 218), // Blurple
                Color.FromArgb(67, 181, 129),  // Green
                Color.FromArgb(250, 166, 26),  // Yellow
                Color.FromArgb(240, 71, 71),   // Red
                Color.FromArgb(158, 132, 237), // Purple
                Color.FromArgb(246, 146, 230), // Pink
            };
            return colors[Math.Abs(hash) % colors.Length];
        }

        private void AddSystemMessage(string text)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.ForeColor = Color.Gray;
            lbl.Font = new Font("Segoe UI", 9, FontStyle.Italic);
            lbl.AutoSize = false;
            lbl.TextAlign = ContentAlignment.MiddleCenter;
            lbl.Width = flpMessages.ClientSize.Width - 20;
            lbl.Height = 25;
            lbl.Margin = new Padding(0, 5, 0, 5);
            flpMessages.Controls.Add(lbl);
            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            flpMessages.VerticalScroll.Value = flpMessages.VerticalScroll.Maximum;
            flpMessages.PerformLayout();
            flpMessages.VerticalScroll.Value = flpMessages.VerticalScroll.Maximum;
        }

        public void UpdateMembers(List<GroupMemberInfo> members)
        {
            _members = members;
            lblMemberCount.Text = $"{_members.Count} thành viên";
        }

        public void OnMemberJoined(string userName)
        {
            AddSystemMessage($"👋 {userName} đã tham gia nhóm");
        }

        public void OnMemberLeft(string userName)
        {
            AddSystemMessage($"👋 {userName} đã rời nhóm");
        }
    }
}

