using ChatApp.Shared;
using ChatAppClient.Forms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
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
        private Button btnEmoji;
        private FlowLayoutPanel pnlEmojiPicker;
        private ContextMenuStrip ctxAttachMenu;

        // Unicode emoji constants
        private const string EMOJI_PAPERCLIP = "\U0001F4CE";
        private const string EMOJI_SMILE = "\U0001F60A";
        private const string EMOJI_WAVE = "\U0001F44B";

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
            lblMemberCount.Text = $"{_members.Count} members";
            lblMemberCount.Font = new Font("Segoe UI", 9);
            lblMemberCount.ForeColor = Color.Gray;
            lblMemberCount.AutoSize = true;
            lblMemberCount.Location = new Point(15, 35);
            pnlHeader.Controls.Add(lblMemberCount);

            btnInvite = new Button();
            btnInvite.Text = "+ Invite";
            btnInvite.Font = new Font("Segoe UI", 9);
            btnInvite.Size = new Size(70, 30);
            btnInvite.Location = new Point(this.Width - 100, 15);
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
            pnlInput.Padding = new Padding(10);
            this.Controls.Add(pnlInput);

            // Attach button with context menu
            btnAttach = new Button();
            btnAttach.Text = EMOJI_PAPERCLIP;
            btnAttach.Font = new Font("Segoe UI Symbol", 14);
            btnAttach.Size = new Size(40, 40);
            btnAttach.Location = new Point(10, 10);
            btnAttach.BackColor = Color.Transparent;
            btnAttach.ForeColor = Color.White;
            btnAttach.FlatStyle = FlatStyle.Flat;
            btnAttach.FlatAppearance.BorderSize = 0;
            btnAttach.FlatAppearance.MouseOverBackColor = Color.FromArgb(88, 101, 242);
            btnAttach.Cursor = Cursors.Hand;
            btnAttach.Click += BtnAttach_Click;
            pnlInput.Controls.Add(btnAttach);

            // Context menu for attach
            ctxAttachMenu = new ContextMenuStrip();
            ctxAttachMenu.BackColor = Color.FromArgb(47, 49, 54);
            ctxAttachMenu.ForeColor = Color.White;
            var menuImage = new ToolStripMenuItem("\U0001F5BC Send Image");
            menuImage.BackColor = Color.FromArgb(47, 49, 54);
            menuImage.ForeColor = Color.White;
            menuImage.Click += (s, e) => SelectAndSendFile(true);
            var menuFile = new ToolStripMenuItem("\U0001F4C4 Send File");
            menuFile.BackColor = Color.FromArgb(47, 49, 54);
            menuFile.ForeColor = Color.White;
            menuFile.Click += (s, e) => SelectAndSendFile(false);
            ctxAttachMenu.Items.Add(menuImage);
            ctxAttachMenu.Items.Add(menuFile);

            // Emoji button
            btnEmoji = new Button();
            btnEmoji.Text = EMOJI_SMILE;
            btnEmoji.Font = new Font("Segoe UI Symbol", 14);
            btnEmoji.Size = new Size(40, 40);
            btnEmoji.Location = new Point(50, 10);
            btnEmoji.BackColor = Color.Transparent;
            btnEmoji.ForeColor = Color.White;
            btnEmoji.FlatStyle = FlatStyle.Flat;
            btnEmoji.FlatAppearance.BorderSize = 0;
            btnEmoji.FlatAppearance.MouseOverBackColor = Color.FromArgb(88, 101, 242);
            btnEmoji.Cursor = Cursors.Hand;
            btnEmoji.Click += BtnEmoji_Click;
            pnlInput.Controls.Add(btnEmoji);

            // Emoji picker panel
            pnlEmojiPicker = new FlowLayoutPanel();
            pnlEmojiPicker.Size = new Size(250, 140);
            pnlEmojiPicker.Location = new Point(50, this.Height - 210);
            pnlEmojiPicker.BackColor = Color.FromArgb(47, 49, 54);
            pnlEmojiPicker.Padding = new Padding(5);
            pnlEmojiPicker.Visible = false;
            this.Controls.Add(pnlEmojiPicker);
            LoadEmojis();

            txtMessage = new TextBox();
            txtMessage.Font = new Font("Segoe UI", 11);
            txtMessage.Location = new Point(95, 12);
            txtMessage.Size = new Size(this.Width - 195, 35);
            txtMessage.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            txtMessage.BackColor = Color.FromArgb(54, 57, 63);
            txtMessage.ForeColor = Color.White;
            txtMessage.BorderStyle = BorderStyle.None;
            txtMessage.KeyDown += TxtMessage_KeyDown;
            pnlInput.Controls.Add(txtMessage);

            btnSend = new Button();
            btnSend.Text = "Send";
            btnSend.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnSend.Size = new Size(70, 40);
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
            this.Controls.SetChildIndex(pnlEmojiPicker, 3);
            pnlEmojiPicker.BringToFront();
        }

        private void LoadEmojis()
        {
            pnlEmojiPicker.Controls.Clear();
            
            string[] emojis = { 
                "\U0001F60A", "\U0001F602", "\u2764", "\U0001F44D", "\U0001F914", 
                "\U0001F622", "\U0001F620", "\U0001F62E", "\U0001F60E", "\U0001F625", 
                "\U0001F62D", "\U0001F480", "\U0001F389", "\U0001F525", "\U0001F44B", 
                "\U0001F4AF", "\u2728", "\U0001F64F", "\U0001F4AA", "\U0001F440" 
            };
            
            foreach (string emoji in emojis)
            {
                Button btn = new Button 
                { 
                    Text = emoji, 
                    Font = new Font("Segoe UI Symbol", 16), 
                    Size = new Size(45, 45), 
                    FlatStyle = FlatStyle.Flat, 
                    Cursor = Cursors.Hand, 
                    BackColor = Color.FromArgb(54, 57, 63), 
                    ForeColor = Color.White,
                    Margin = new Padding(2)
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(88, 101, 242);
                btn.Click += (s, ev) => 
                { 
                    txtMessage.AppendText(emoji); 
                    pnlEmojiPicker.Visible = false; 
                    txtMessage.Focus(); 
                };
                pnlEmojiPicker.Controls.Add(btn);
            }
        }

        private void BtnEmoji_Click(object sender, EventArgs e)
        {
            pnlEmojiPicker.Location = new Point(
                btnEmoji.Left,
                pnlInput.Top - pnlEmojiPicker.Height - 5
            );
            pnlEmojiPicker.Visible = !pnlEmojiPicker.Visible;
            if (pnlEmojiPicker.Visible)
            {
                pnlEmojiPicker.BringToFront();
            }
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
            ctxAttachMenu.Show(btnAttach, new Point(0, -ctxAttachMenu.Height));
        }

        private void SelectAndSendFile(bool isImage)
        {
            string filter = isImage ? "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp" : "All Files|*.*";
            using (var ofd = new OpenFileDialog { Filter = filter })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    // Check file size
                    FileInfo fileInfo = new FileInfo(ofd.FileName);
                    long maxSize = isImage ? 5 * 1024 * 1024 : 10 * 1024 * 1024;
                    if (fileInfo.Length > maxSize)
                    {
                        string limit = isImage ? "5MB" : "10MB";
                        MessageBox.Show($"File too large! Limit: {limit}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    _ = Task.Run(() =>
                    {
                        try
                        {
                            byte[] data = File.ReadAllBytes(ofd.FileName);
                            string fileName = Path.GetFileName(ofd.FileName);

                            var packet = new GroupFilePacket
                            {
                                GroupID = _groupId,
                                SenderID = _myId,
                                SenderName = NetworkManager.Instance.UserName,
                                FileName = fileName,
                                FileData = data,
                                IsImage = isImage
                            };

                            bool sent = NetworkManager.Instance.SendPacket(packet);

                            this.Invoke(new Action(() =>
                            {
                                if (sent)
                                {
                                    if (isImage)
                                    {
                                        AddImageBubble(_myId, NetworkManager.Instance.UserName, data, fileName, MessageType.Outgoing);
                                    }
                                    else
                                    {
                                        AddFileBubble(_myId, NetworkManager.Instance.UserName, fileName, data, MessageType.Outgoing);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Cannot send file. Check connection.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }));
                        }
                        catch (Exception ex)
                        {
                            this.Invoke(new Action(() =>
                            {
                                MessageBox.Show($"Error sending file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }));
                        }
                    });
                }
            }
        }

        private void BtnInvite_Click(object sender, EventArgs e)
        {
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
                    AddSystemMessage($"Sent invitation to {inviteForm.SelectedMembers.Count} people");
                }
            }
        }

        private async void LoadHistoryAsync()
        {
            AddSystemMessage($"Welcome to group {_groupName}!");
            
            try
            {
                var request = new GroupHistoryRequestPacket
                {
                    GroupID = _groupId,
                    UserID = _myId,
                    Limit = 100
                };
                
                NetworkManager.Instance.SendPacket(request);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading group history: {ex.Message}");
            }
        }
        
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
                    AddSystemMessage($"[{msg.SenderName}] sent {msg.MessageType}: {msg.FileName}");
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

            MessageType type = (packet.SenderID == _myId) ? MessageType.Outgoing : MessageType.Incoming;
            
            if (packet.IsImage && packet.FileData != null && packet.FileData.Length > 0)
            {
                AddImageBubble(packet.SenderID, packet.SenderName, packet.FileData, packet.FileName, type);
            }
            else if (packet.FileData != null && packet.FileData.Length > 0)
            {
                AddFileBubble(packet.SenderID, packet.SenderName, packet.FileName, packet.FileData, type);
            }
            else
            {
                AddSystemMessage($"{packet.SenderName} sent {(packet.IsImage ? "image" : "file")}: {packet.FileName}");
            }
        }

        private void AddMessageBubble(string senderId, string senderName, string message, MessageType type, DateTime time)
        {
            Panel container = new Panel();
            container.AutoSize = false;
            int scrollWidth = SystemInformation.VerticalScrollBarWidth;
            container.Width = flpMessages.ClientSize.Width - scrollWidth - 5;
            container.BackColor = Color.Transparent;
            container.Margin = new Padding(0, 0, 0, 5);

            int yOffset = 0;

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

        private void AddImageBubble(string senderId, string senderName, byte[] imageData, string fileName, MessageType type)
        {
            Panel container = new Panel();
            container.AutoSize = false;
            int scrollWidth = SystemInformation.VerticalScrollBarWidth;
            container.Width = flpMessages.ClientSize.Width - scrollWidth - 5;
            container.BackColor = Color.Transparent;
            container.Margin = new Padding(0, 0, 0, 5);

            int yOffset = 0;

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

            try
            {
                var imgBubble = new ImageBubble();
                using (var ms = new MemoryStream(imageData))
                {
                    using (var originalImg = Image.FromStream(ms))
                    {
                        Image clonedImg = new Bitmap(originalImg);
                        imgBubble.SetImage(clonedImg, fileName, imageData, type);
                    }
                }

                if (type == MessageType.Outgoing)
                {
                    imgBubble.Location = new Point(container.Width - imgBubble.Width - 10, yOffset);
                }
                else
                {
                    imgBubble.Location = new Point(5, yOffset);
                }

                container.Controls.Add(imgBubble);
                container.Height = yOffset + imgBubble.Height + 10;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error displaying image: {ex.Message}");
                AddSystemMessage($"[Error displaying image: {fileName}]");
                return;
            }

            flpMessages.Controls.Add(container);
            ScrollToBottom();
        }

        private void AddFileBubble(string senderId, string senderName, string fileName, byte[] fileData, MessageType type)
        {
            Panel container = new Panel();
            container.AutoSize = false;
            int scrollWidth = SystemInformation.VerticalScrollBarWidth;
            container.Width = flpMessages.ClientSize.Width - scrollWidth - 5;
            container.BackColor = Color.Transparent;
            container.Margin = new Padding(0, 0, 0, 5);

            int yOffset = 0;

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

            var fileBubble = new FileBubble();
            fileBubble.SetMessage(fileName, fileData, type, container.Width);

            if (type == MessageType.Outgoing)
            {
                fileBubble.Location = new Point(container.Width - fileBubble.Width - 10, yOffset);
            }
            else
            {
                fileBubble.Location = new Point(5, yOffset);
            }

            container.Controls.Add(fileBubble);
            container.Height = yOffset + fileBubble.Height + 10;

            flpMessages.Controls.Add(container);
            ScrollToBottom();
        }

        private Color GetSenderColor(string senderId)
        {
            int hash = senderId.GetHashCode();
            Color[] colors = {
                Color.FromArgb(114, 137, 218),
                Color.FromArgb(67, 181, 129),
                Color.FromArgb(250, 166, 26),
                Color.FromArgb(240, 71, 71),
                Color.FromArgb(158, 132, 237),
                Color.FromArgb(246, 146, 230),
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
            lblMemberCount.Text = $"{_members.Count} members";
        }

        public void OnMemberJoined(string userName)
        {
            AddSystemMessage($"{EMOJI_WAVE} {userName} joined the group");
        }

        public void OnMemberLeft(string userName)
        {
            AddSystemMessage($"{EMOJI_WAVE} {userName} left the group");
        }
    }
}
