using ChatApp.Shared;
using ChatAppClient.Forms;
using ChatAppClient.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ChatAppClient.UserControls
{
    public partial class ChatViewControl : UserControl
    {
        private string _friendId;
        private string _friendName;
        private Form _parentForm;
        private string _myId = "";

        // Dictionary lưu bong bóng game để cập nhật trạng thái
        private Dictionary<string, GameInviteBubble> _gameInviteBubbles = new Dictionary<string, GameInviteBubble>();

        public ChatViewControl(string friendId, string friendName, Form parentForm)
        {
            InitializeComponent();
            _friendId = friendId;
            _friendName = friendName;
            _parentForm = parentForm;
            lblFriendName.Text = _friendName;

            // Cấu hình FlowLayoutPanel
            flpMessages.AutoScroll = true;
            flpMessages.FlowDirection = FlowDirection.TopDown;
            flpMessages.WrapContents = false;
            flpMessages.HorizontalScroll.Visible = false;

            // Chống nháy (Flicker)
            typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(flpMessages, true, null);
        }

        private void ChatViewControl_Load(object sender, EventArgs e)
        {
            _myId = NetworkManager.Instance.UserID ?? "";

            // Gán sự kiện
            btnSend.Click += BtnSend_Click;
            btnStartGame.Click += BtnStartGame_Click;
            this.Resize += ChatViewControl_Resize;
            btnSendImage.Click += BtnSendImage_Click;
            btnSendFile.Click += BtnSendFile_Click;
            btnEmoji.Click += BtnEmoji_Click;
            btnAttach.Click += BtnAttach_Click;

            // Cấu hình TextBox
            txtMessage.Multiline = true;
            txtMessage.ScrollBars = ScrollBars.None;
            txtMessage.WordWrap = true;
            txtMessage.KeyDown += TxtMessage_KeyDown;

            // Menu chọn game
            ContextMenuStrip gameMenu = new ContextMenuStrip();
            gameMenu.Items.Add("Chơi Caro", null, (s, ev) => InviteGame(GameType.Caro));
            gameMenu.Items.Add("Chơi Tank Game", null, (s, ev) => InviteGame(GameType.Tank));
            btnStartGame.ContextMenuStrip = gameMenu;

            LoadEmojis();
            LoadChatHistory();
        }

        private void TxtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                BtnSend_Click(sender, e);
            }
        }

        private async void LoadChatHistory()
        {
            if (string.IsNullOrEmpty(_myId) || string.IsNullOrEmpty(_friendId)) return;

            try
            {
                var response = await NetworkManager.Instance.RequestChatHistoryAsync(_friendId, 100);
                if (response.Success && response.Messages != null)
                {
                    // Server đã reverse list rồi, nên ở đây duyệt xuôi
                    foreach (var msg in response.Messages)
                    {
                        bool isMe = (msg.SenderID == _myId);
                        MessageType type = isMe ? MessageType.Outgoing : MessageType.Incoming;

                        if (msg.MessageType == "GameInvite")
                        {
                            string senderName = isMe ? (NetworkManager.Instance.UserName ?? "Bạn") : _friendName;
                            GameType gType = msg.FileName == "Tank" ? GameType.Tank : GameType.Caro;

                            var invite = new GameInvitePacket { SenderID = msg.SenderID, SenderName = senderName, ReceiverID = msg.ReceiverID };
                            DisplayGameInvite(invite, gType, type, msg.MessageContent);
                        }
                        else if (msg.MessageType == "Text")
                        {
                            // ✅ [FIX] Đảm bảo nội dung tin nhắn không rỗng
                            string content = msg.MessageContent;
                            if (!string.IsNullOrWhiteSpace(content))
                            {
                                AddMessageBubble(content, type, msg.CreatedAt);
                            }
                        }
                        else if (msg.MessageType == "File" || msg.MessageType == "Image")
                        {
                            // Hiển thị thông báo file trong lịch sử (vì không tải binary ngay)
                            AddSystemMessage($"[Lịch sử] {msg.MessageType}: {msg.FileName}");
                        }
                    }
                    ScrollToBottom();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi load lịch sử: {ex.Message}");
            }
        }

        #region === HỆ THỐNG LAYOUT & HIỂN THỊ (CORE) ===

        // Thêm tin nhắn Text
        private void AddMessageBubble(string message, MessageType type, DateTime time)
        {
            // ✅ [FIX] Đảm bảo message không null
            if (string.IsNullOrWhiteSpace(message))
            {
                message = "(Tin nhắn trống)";
            }
            
            var bubble = new ChatMessageBubble();
            bubble.SetData(message, type, time);
            AddControlToLayout(bubble, type);
        }

        // Thêm tin nhắn File/Image (QUAN TRỌNG: Logic tích hợp với FileBubble của bạn)
        public void ReceiveFileMessage(FilePacket p, MessageType type)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => ReceiveFileMessage(p, type))); return; }

            Control bubbleToAdd;

            if (p.IsImage)
            {
                var imgBubble = new ImageBubble();
                using (var ms = new MemoryStream(p.FileData))
                {
                    // [FIXED] Truyền đủ 4 tham số: Ảnh, Tên file, Dữ liệu gốc, Loại tin nhắn
                    imgBubble.SetImage(Image.FromStream(ms), p.FileName, p.FileData, type);
                }

                // Sự kiện Forward giờ đã khớp kiểu dữ liệu
                imgBubble.OnForwardRequested += (s, data) => ShowForwardDialog(null, data.fileName, data.fileData);
                bubbleToAdd = imgBubble;
            }
            else
            {
                var fileBubble = new FileBubble();
                // Giữ nguyên logic FileBubble
                fileBubble.SetMessage(p.FileName, p.FileData, type, flpMessages.ClientSize.Width);
                fileBubble.OnForwardRequested += (s, data) => ShowForwardDialog(null, data.fileName, data.fileData);
                bubbleToAdd = fileBubble;
            }

            AddControlToLayout(bubbleToAdd, type);
        }

        // Hàm căn trái/phải dùng Container Panel
        private void AddControlToLayout(Control ctrl, MessageType type)
        {
            Panel container = new Panel();
            container.AutoSize = false;
            int scrollWidth = SystemInformation.VerticalScrollBarWidth;
            container.Width = flpMessages.ClientSize.Width - scrollWidth - 5;

            // Chiều cao = chiều cao control + margin
            container.Height = ctrl.Height + 10;
            container.BackColor = Color.Transparent;
            container.Margin = new Padding(0, 0, 0, 5);

            if (type == MessageType.Outgoing)
            {
                // Căn phải
                ctrl.Location = new Point(container.Width - ctrl.Width - 10, 0);
                ctrl.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            }
            else
            {
                // Căn trái
                ctrl.Location = new Point(5, 0);
                ctrl.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            }

            container.Controls.Add(ctrl);
            flpMessages.Controls.Add(container);
            ScrollToBottom();
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
            try
            {
                if (flpMessages.Controls.Count > 0)
                    flpMessages.ScrollControlIntoView(flpMessages.Controls[flpMessages.Controls.Count - 1]);
            }
            catch { }
        }

        #endregion

        #region === SỰ KIỆN GỬI TIN ===

        private void BtnSend_Click(object sender, EventArgs e)
        {
            string content = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(content)) return;

            // ✅ [FIX] Đảm bảo content không rỗng trước khi gửi
            if (string.IsNullOrWhiteSpace(content))
            {
                return;
            }

            // Hiển thị tin nhắn ngay lập tức
            AddMessageBubble(content, MessageType.Outgoing, DateTime.Now);

            // Gửi packet
            var packet = new TextPacket 
            { 
                SenderID = _myId ?? "", 
                ReceiverID = _friendId ?? "", 
                MessageContent = content 
            };
            
            if (!NetworkManager.Instance.SendPacket(packet))
            {
                // Nếu gửi thất bại, có thể hiển thị thông báo
                System.Diagnostics.Debug.WriteLine("Không thể gửi tin nhắn");
            }

            txtMessage.Clear();
            txtMessage.Focus();
        }

        private void BtnSendImage_Click(object sender, EventArgs e) => SelectAndSendFile(true);
        private void BtnSendFile_Click(object sender, EventArgs e) => SelectAndSendFile(false);

        private void SelectAndSendFile(bool isImage)
        {
            string filter = isImage ? "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp" : "All Files|*.*";
            OpenFileDialog dialog = new OpenFileDialog { Filter = filter };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    byte[] fileData = File.ReadAllBytes(dialog.FileName);
                    string fileName = Path.GetFileName(dialog.FileName);

                    var packet = new FilePacket { SenderID = _myId, ReceiverID = _friendId, FileName = fileName, FileData = fileData, IsImage = isImage };
                    NetworkManager.Instance.SendPacket(packet);

                    // Hiển thị ngay lập tức
                    ReceiveFileMessage(packet, MessageType.Outgoing);
                }
                catch (Exception ex) { MessageBox.Show("Lỗi gửi file: " + ex.Message); }
            }
        }

        #endregion

        #region === XỬ LÝ GAME INVITE ===

        private void BtnStartGame_Click(object sender, EventArgs e)
        {
            if (btnStartGame.ContextMenuStrip != null)
                btnStartGame.ContextMenuStrip.Show(btnStartGame, new Point(0, btnStartGame.Height));
            else
                InviteGame(GameType.Caro);
        }

        private void InviteGame(GameType type)
        {
            if (string.IsNullOrEmpty(_myId)) _myId = NetworkManager.Instance.UserID ?? "";
            string senderName = NetworkManager.Instance.UserName ?? "User";

            bool sent = false;
            if (type == GameType.Caro)
            {
                var packet = new GameInvitePacket { SenderID = _myId, SenderName = senderName, ReceiverID = _friendId };
                sent = NetworkManager.Instance.SendPacket(packet);
                // Hiển thị bong bóng Outgoing
                if (sent) ReceiveGameInvite(packet, type, MessageType.Outgoing);
            }
            else
            {
                var packet = new TankInvitePacket { SenderID = _myId, SenderName = senderName, ReceiverID = _friendId };
                sent = NetworkManager.Instance.SendPacket(packet);
                // Map sang GameInvite để dùng chung hàm hiển thị
                if (sent) ReceiveGameInvite(new GameInvitePacket { SenderID = _myId, SenderName = senderName, ReceiverID = _friendId }, type, MessageType.Outgoing);
            }

            if (sent)
            {
                btnStartGame.Enabled = false;
                btnStartGame.Text = "...";
            }
            else
            {
                MessageBox.Show("Không thể gửi lời mời. Kiểm tra kết nối.");
            }
        }

        // Hàm nhận Invite từ Server (Incoming) HOẶC hiển thị Invite vừa gửi (Outgoing)
        public void ReceiveGameInvite(GameInvitePacket p, GameType gType, MessageType mType)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => ReceiveGameInvite(p, gType, mType))); return; }
            DisplayGameInvite(p, gType, mType, null);
        }

        private void DisplayGameInvite(GameInvitePacket p, GameType gType, MessageType mType, string? statusContent)
        {
            var bubble = new GameInviteBubble();
            bubble.SetInvite(p.SenderName, mType, gType);

            if (!string.IsNullOrEmpty(statusContent))
            {
                if (statusContent.Contains("chấp nhận")) bubble.UpdateStatus(GameInviteStatus.Accepted);
                else if (statusContent.Contains("từ chối")) bubble.UpdateStatus(GameInviteStatus.Declined);
            }

            if (mType == MessageType.Incoming)
            {
                bubble.OnResponse += (s, accepted) =>
                {
                    if (gType == GameType.Caro)
                        NetworkManager.Instance.SendPacket(new GameResponsePacket { SenderID = _myId, ReceiverID = p.SenderID, Accepted = accepted });
                    else
                        NetworkManager.Instance.SendPacket(new TankResponsePacket { SenderID = _myId, ReceiverID = p.SenderID, Accepted = accepted });

                    bubble.UpdateStatus(accepted ? GameInviteStatus.Accepted : GameInviteStatus.Declined);
                };
            }
            else // Outgoing
            {
                bubble.OnReinvite += (s, e) => InviteGame(gType);
                _gameInviteBubbles[_friendId] = bubble; // Lưu để update khi đối phương trả lời
            }

            AddControlToLayout(bubble, mType);
        }

        public void UpdateGameInviteStatus(string senderID, bool accepted, GameType gType)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => UpdateGameInviteStatus(senderID, accepted, gType))); return; }

            // Tìm bubble outgoing tương ứng
            if (_gameInviteBubbles.TryGetValue(_friendId, out var bubble))
            {
                bubble.UpdateStatus(accepted ? GameInviteStatus.Accepted : GameInviteStatus.Declined);
            }
            else
            {
                AddSystemMessage(accepted ? $"Đối thủ đã chấp nhận chơi {gType}!" : $"Đối thủ đã từ chối lời mời {gType}.");
            }
        }

        public void ResetGameButton()
        {
            if (this.InvokeRequired) { this.Invoke(new Action(ResetGameButton)); return; }
            btnStartGame.Enabled = true;
            btnStartGame.Text = "🎲";
        }

        public void HandleGameInviteDeclined()
        {
            if (this.InvokeRequired) { this.Invoke(new Action(HandleGameInviteDeclined)); return; }
            ResetGameButton();
        }

        #endregion

        #region === CÁC SỰ KIỆN PHỤ ===

        public void ReceiveMessage(string content)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => ReceiveMessage(content))); return; }
            
            // ✅ [FIX] Đảm bảo content không null và không rỗng
            if (string.IsNullOrWhiteSpace(content))
            {
                content = "(Tin nhắn trống)";
            }
            
            AddMessageBubble(content, MessageType.Incoming, DateTime.Now);
        }

        private void ShowForwardDialog(string? text, string? fileName, byte[]? fileData)
        {
            var friends = new List<(string id, string name)>();
            if (_parentForm is frmHome homeForm)
            {
                friends = homeForm.GetFriendsList().Where(f => f.id != _friendId).ToList();
            }

            if (friends.Count == 0) { MessageBox.Show("Không có bạn bè để chuyển tiếp."); return; }

            using (var fwd = new frmForwardMessage(friends))
            {
                if (fwd.ShowDialog() == DialogResult.OK && fwd.SelectedFriendID != null)
                {
                    string target = fwd.SelectedFriendID;
                    if (!string.IsNullOrEmpty(text))
                        NetworkManager.Instance.SendPacket(new TextPacket { SenderID = _myId, ReceiverID = target, MessageContent = text });
                    else if (fileData != null)
                    {
                        bool isImg = fileName.EndsWith(".png") || fileName.EndsWith(".jpg");
                        NetworkManager.Instance.SendPacket(new FilePacket { SenderID = _myId, ReceiverID = target, FileName = fileName, FileData = fileData, IsImage = isImg });
                    }
                    MessageBox.Show("Đã chuyển tiếp!");
                }
            }
        }

        private void ChatViewControl_Resize(object sender, EventArgs e)
        {
            // Khi resize, update lại width cho các container panel
            int w = flpMessages.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 5;
            foreach (Control c in flpMessages.Controls)
            {
                if (c is Panel p)
                {
                    p.Width = w;
                    // Update lại vị trí control con bên trong nếu là Outgoing (căn phải)
                    if (p.Controls.Count > 0)
                    {
                        Control child = p.Controls[0];
                        // Nếu đang ở bên phải (gần lề phải), thì dời theo
                        if (child.Left > p.Width / 2)
                        {
                            child.Left = p.Width - child.Width - 10;
                        }
                    }
                }
            }
        }

        private void BtnAttach_Click(object sender, EventArgs e) => ctxAttachMenu?.Show(btnAttach, new Point(0, -ctxAttachMenu.Height));
        private void BtnEmoji_Click(object sender, EventArgs e) { pnlEmojiPicker.Visible = !pnlEmojiPicker.Visible; if (pnlEmojiPicker.Visible) pnlEmojiPicker.BringToFront(); }

        private void LoadEmojis()
        {
            string[] emojis = { "😊", "😂", "❤️", "👍", "🤔", "😢", "😠", "😮", "😎", "😥", "😭", "💀" };
            foreach (string emoji in emojis)
            {
                // ✅ [FIX] Màu button emoji đen như GroupChat
                Button btn = new Button 
                { 
                    Text = emoji, 
                    Font = new Font("Segoe UI Emoji", 14), 
                    Size = new Size(40, 40), 
                    FlatStyle = FlatStyle.Flat, 
                    Cursor = Cursors.Hand, 
                    BackColor = Color.FromArgb(54, 57, 63), 
                    ForeColor = Color.White 
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(88, 101, 242);
                btn.Click += (s, e) => { txtMessage.AppendText(emoji); pnlEmojiPicker.Visible = false; txtMessage.Focus(); };
                pnlEmojiPicker.Controls.Add(btn);
            }
        }

        private void PnlHeader_Paint(object sender, PaintEventArgs e)
        {
            // ✅ [FIX] Không vẽ gradient nữa, dùng màu đen như GroupChat
            // Header đã được set BackColor trong Designer rồi
        }

        private void PnlInput_Paint(object sender, PaintEventArgs e) { /* Custom border */ }
        private void PnlEmojiPicker_Paint(object sender, PaintEventArgs e) { /* Custom border */ }

        #endregion
    }
}