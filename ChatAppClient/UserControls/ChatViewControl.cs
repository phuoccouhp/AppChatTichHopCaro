using ChatApp.Shared;
using ChatAppClient.Forms;
using ChatAppClient.Helpers;
using ChatAppClient.UserControls;
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

        public ChatViewControl(string friendId, string friendName, Form parentForm)
        {
            InitializeComponent();
            _friendId = friendId;
            _friendName = friendName;
            _parentForm = parentForm;
            lblFriendName.Text = _friendName;
        }

        private void ChatViewControl_Load(object sender, EventArgs e)
        {
            _myId = NetworkManager.Instance.UserID ?? "";

            // Gán sự kiện
            btnSend.Click += BtnSend_Click;
            btnStartGame.Click += BtnStartGame_Click;
            this.Resize += new System.EventHandler(this.ChatViewControl_Resize);
            btnSendImage.Click += BtnSendImage_Click;
            btnSendFile.Click += BtnSendFile_Click;
            btnEmoji.Click += BtnEmoji_Click;
            btnAttach.Click += BtnAttach_Click; // Thêm event handler cho button attach
            
            // Căn chỉnh TextBox
            txtMessage.Multiline = true;
            txtMessage.ScrollBars = ScrollBars.None;
            txtMessage.AcceptsReturn = true;
            txtMessage.WordWrap = true;
            
            // Thêm context menu cho nút game để chọn loại game
            ContextMenuStrip gameMenu = new ContextMenuStrip();
            gameMenu.Items.Add("Chơi Caro", null, (s, e) => InviteCaroGame());
            gameMenu.Items.Add("Chơi Tank Game", null, (s, e) => InviteTankGame());
            btnStartGame.ContextMenuStrip = gameMenu;

            LoadEmojis();
            
            // Load lịch sử chat từ database
            LoadChatHistory();
        }

        private async void LoadChatHistory()
        {
            if (string.IsNullOrEmpty(_myId) || string.IsNullOrEmpty(_friendId))
                return;

            try
            {
                var response = await NetworkManager.Instance.RequestChatHistoryAsync(_friendId, 100);
                
                if (response.Success && response.Messages != null && response.Messages.Count > 0)
                {
                    // Hiển thị lịch sử chat (từ cũ đến mới)
                    foreach (var msg in response.Messages)
                    {
                        if (msg.MessageType == "GameInvite")
                        {
                            // Xử lý game invite message
                            bool isOutgoing = msg.SenderID == _myId;
                            string senderName = isOutgoing ? NetworkManager.Instance.UserName ?? "Bạn" : _friendName;
                            GameType gameType = msg.FileName == "Tank" ? GameType.Tank : GameType.Caro;
                            
                            var invite = new GameInvitePacket 
                            { 
                                SenderID = msg.SenderID, 
                                SenderName = senderName, 
                                ReceiverID = msg.ReceiverID 
                            };
                            
                            var bubble = new GameInviteBubble();
                            bubble.SetInvite(senderName, isOutgoing ? MessageType.Outgoing : MessageType.Incoming, gameType, msg.MessageID);
                            
                            // Kiểm tra xem có status trong message content không (đã được cập nhật)
                            string content = msg.MessageContent ?? "";
                            if (content.Contains("✓ Đã chấp nhận"))
                            {
                                bubble.UpdateStatus(GameInviteStatus.Accepted);
                            }
                            else if (content.Contains("✗ Đã từ chối"))
                            {
                                bubble.UpdateStatus(GameInviteStatus.Declined);
                            }
                            
                            // Lưu bubble để có thể cập nhật sau
                            string key = isOutgoing ? _friendId : msg.SenderID;
                            if (!_gameInviteBubbles.ContainsKey(key))
                            {
                                _gameInviteBubbles[key] = bubble;
                            }
                            
                            // Chỉ cho phép response nếu là incoming và chưa có response
                            if (!isOutgoing && bubble.Status == GameInviteStatus.Pending)
                            {
                                bubble.OnResponse += (s, accepted) =>
                                {
                                    string? myId = NetworkManager.Instance.UserID;
                                    if (myId != null)
                                    {
                                        if (gameType == GameType.Caro)
                                        {
                                            NetworkManager.Instance.SendPacket(new GameResponsePacket 
                                            { 
                                                SenderID = myId, 
                                                ReceiverID = msg.SenderID, 
                                                Accepted = accepted 
                                            });
                                        }
                                        else
                                        {
                                            NetworkManager.Instance.SendPacket(new TankResponsePacket
                                            {
                                                SenderID = myId,
                                                ReceiverID = msg.SenderID,
                                                Accepted = accepted
                                            });
                                        }
                                    }
                                    bubble.UpdateStatus(accepted ? GameInviteStatus.Accepted : GameInviteStatus.Declined);
                                };
                            }
                            else if (isOutgoing)
                            {
                                // Cho phép mời lại nếu đã bị từ chối
                                bubble.OnReinvite += (s, e) =>
                                {
                                    HandleReinvite(bubble.CurrentGameType);
                                };
                            }
                            
                            flpMessages.Controls.Add(bubble);
                        }
                        else if (msg.SenderID == _myId)
                        {
                            // Tin nhắn của mình
                            if (msg.MessageType == "Text")
                            {
                                AddMessage(msg.MessageContent ?? "", MessageType.Outgoing);
                            }
                            else if (msg.MessageType == "Image" || msg.MessageType == "File")
                            {
                                // File/Image - chỉ hiển thị thông báo vì không có dữ liệu file
                                string displayText = msg.MessageType == "Image" 
                                    ? $"Đã gửi ảnh: {msg.FileName}" 
                                    : $"Đã gửi file: {msg.FileName}";
                                AddMessage(displayText, MessageType.Outgoing);
                            }
                        }
                        else
                        {
                            // Tin nhắn từ bạn
                            if (msg.MessageType == "Text")
                            {
                                AddMessage(msg.MessageContent ?? "", MessageType.Incoming);
                            }
                            else if (msg.MessageType == "Image" || msg.MessageType == "File")
                            {
                                // File/Image - chỉ hiển thị thông báo
                                string displayText = msg.MessageType == "Image" 
                                    ? $"Đã nhận ảnh: {msg.FileName}" 
                                    : $"Đã nhận file: {msg.FileName}";
                                AddMessage(displayText, MessageType.Incoming);
                            }
                        }
                    }
                    
                    // Scroll xuống cuối để xem tin nhắn mới nhất
                    if (flpMessages.Controls.Count > 0)
                    {
                        var lastControl = flpMessages.Controls[flpMessages.Controls.Count - 1];
                        ScrollToBottom(lastControl);
                    }
                }
            }
            catch (Exception ex)
            {
                // Không hiển thị lỗi, chỉ log
                System.Diagnostics.Debug.WriteLine($"Lỗi load lịch sử chat: {ex.Message}");
            }
        }

        #region == GỬI TIN (TEXT, FILE, GAME) ==

        private void BtnSend_Click(object sender, EventArgs e)
        {
            string message = txtMessage.Text.Trim();
            if (!string.IsNullOrEmpty(message))
            {
                AddMessage(message, MessageType.Outgoing);
                var textPacket = new TextPacket { SenderID = _myId, ReceiverID = _friendId, MessageContent = message };
                NetworkManager.Instance.SendPacket(textPacket);
                txtMessage.Clear();
                txtMessage.Focus();
            }
        }

        private void BtnStartGame_Click(object sender, EventArgs e)
        {
            // Hiển thị context menu để chọn loại game
            if (btnStartGame.ContextMenuStrip != null)
            {
                btnStartGame.ContextMenuStrip.Show(btnStartGame, new Point(0, btnStartGame.Height));
            }
            else
            {
                // Fallback: mặc định mời chơi Caro nếu không có menu
                InviteCaroGame();
            }
        }

        private void InviteCaroGame()
        {
            // Kiểm tra UserID và kết nối trước khi gửi
            if (string.IsNullOrEmpty(_myId))
            {
                // Cập nhật lại _myId từ NetworkManager
                _myId = NetworkManager.Instance.UserID ?? "";
                if (string.IsNullOrEmpty(_myId))
                {
                    MessageBox.Show("Bạn chưa đăng nhập. Vui lòng đăng nhập trước khi gửi lời mời chơi game.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            string? senderName = NetworkManager.Instance.UserName ?? "User";
            var invite = new GameInvitePacket { SenderID = _myId, SenderName = senderName, ReceiverID = _friendId };
            
            try
            {
                if (NetworkManager.Instance.SendPacket(invite))
                {
                    btnStartGame.Enabled = false;
                    btnStartGame.Text = "...";
                    // Hiển thị game invite như tin nhắn
                    ReceiveGameInvite(invite, GameType.Caro, MessageType.Outgoing);
                }
                else
                {
                    MessageBox.Show("Không thể gửi lời mời. Vui lòng kiểm tra kết nối.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi gửi lời mời: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InviteTankGame()
        {
            // Kiểm tra UserID và kết nối trước khi gửi
            if (string.IsNullOrEmpty(_myId))
            {
                // Cập nhật lại _myId từ NetworkManager
                _myId = NetworkManager.Instance.UserID ?? "";
                if (string.IsNullOrEmpty(_myId))
                {
                    MessageBox.Show("Bạn chưa đăng nhập. Vui lòng đăng nhập trước khi gửi lời mời chơi game.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            string? senderName = NetworkManager.Instance.UserName ?? "User";
            var invite = new TankInvitePacket { SenderID = _myId, SenderName = senderName, ReceiverID = _friendId };
            
            try
            {
                if (NetworkManager.Instance.SendPacket(invite))
                {
                    btnStartGame.Enabled = false;
                    btnStartGame.Text = "...";
                    // Hiển thị game invite như tin nhắn
                    var gameInvite = new GameInvitePacket { SenderID = _myId, SenderName = senderName, ReceiverID = _friendId };
                    ReceiveGameInvite(gameInvite, GameType.Tank, MessageType.Outgoing);
                }
                else
                {
                    MessageBox.Show("Không thể gửi lời mời. Vui lòng kiểm tra kết nối.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi gửi lời mời: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSendImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp" };
            if (dialog.ShowDialog() == DialogResult.OK) SendFile(dialog.FileName, true);
        }

        private void BtnSendFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog { Filter = "All Files|*.*" };
            if (dialog.ShowDialog() == DialogResult.OK) SendFile(dialog.FileName, false);
        }

        private void BtnAttach_Click(object sender, EventArgs e)
        {
            // Hiển thị context menu khi click button attach
            if (ctxAttachMenu != null)
            {
                ctxAttachMenu.Show(btnAttach, new Point(0, btnAttach.Height));
            }
        }

        private void SendFile(string filePath, bool isImage)
        {
            try
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                string fileName = Path.GetFileName(filePath);
                var filePacket = new FilePacket { SenderID = _myId, ReceiverID = _friendId, FileName = fileName, FileData = fileData, IsImage = isImage };
                NetworkManager.Instance.SendPacket(filePacket);
                AddFileBubble(filePacket, MessageType.Outgoing); // Hiển thị ngay
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể gửi file: {ex.Message}", "Lỗi");
            }
        }

        #endregion

        #region == NHẬN TIN (TEXT, FILE) ==

        public void ReceiveFileMessage(FilePacket packet, MessageType type = MessageType.Incoming)
        {
            if (_parentForm != null && _parentForm.InvokeRequired) _parentForm.Invoke(new Action(() => AddFileBubble(packet, type)));
            else AddFileBubble(packet, type);
        }

        private void AddFileBubble(FilePacket packet, MessageType type)
        {
            int usableWidth = GetUsableWidth();
            Control bubbleToAdd = null;

            if (packet.IsImage)
            {
                var bubble = new ImageBubble();
                bubble.SetMessage(packet.FileData, type, usableWidth);
                bubble.OnForwardRequested += (s, data) => ShowForwardDialog(null, null, data);
                bubbleToAdd = bubble;
            }
            else
            {
                var bubble = new FileBubble();
                bubble.SetMessage(packet.FileName, packet.FileData, type, usableWidth);
                bubble.OnForwardRequested += (s, tuple) => ShowForwardDialog(null, tuple.fileName, tuple.fileData);
                bubbleToAdd = bubble;
            }

            if (bubbleToAdd != null)
            {
                flpMessages.Controls.Add(bubbleToAdd);
                ScrollToBottom(bubbleToAdd);
            }
        }

        private void ShowForwardDialog(string? textMessage, string? fileName, byte[]? fileData)
        {
            // Lấy danh sách bạn bè từ frmHome
            var friends = new List<(string id, string name)>();
            if (_parentForm is frmHome homeForm)
            {
                friends = homeForm.GetFriendsList().Where(f => f.id != _friendId).ToList(); // Loại trừ người đang chat
            }

            if (friends.Count == 0)
            {
                MessageBox.Show("Không có bạn bè để chuyển tiếp.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var forwardForm = new frmForwardMessage(friends))
            {
                if (forwardForm.ShowDialog() == DialogResult.OK && forwardForm.SelectedFriendID != null)
                {
                    try
                    {
                        string? myId = NetworkManager.Instance.UserID;
                        if (string.IsNullOrEmpty(myId))
                        {
                            MessageBox.Show("Bạn chưa đăng nhập.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        if (!string.IsNullOrEmpty(textMessage))
                        {
                            // Forward text message
                            var textPacket = new TextPacket
                            {
                                SenderID = myId,
                                ReceiverID = forwardForm.SelectedFriendID,
                                MessageContent = textMessage
                            };
                            NetworkManager.Instance.SendPacket(textPacket);
                        }
                        else if (fileData != null && !string.IsNullOrEmpty(fileName))
                        {
                            // Forward file
                            var filePacket = new FilePacket
                            {
                                SenderID = myId,
                                ReceiverID = forwardForm.SelectedFriendID,
                                FileName = fileName,
                                FileData = fileData,
                                IsImage = fileName.ToLower().EndsWith(".png") || fileName.ToLower().EndsWith(".jpg") ||
                                         fileName.ToLower().EndsWith(".jpeg") || fileName.ToLower().EndsWith(".gif") ||
                                         fileName.ToLower().EndsWith(".bmp")
                            };
                            NetworkManager.Instance.SendPacket(filePacket);
                        }

                        MessageBox.Show($"Đã chuyển tiếp đến {forwardForm.SelectedFriendName}!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Lỗi khi chuyển tiếp: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        public void ReceiveMessage(string message)
        {
            if (_parentForm != null && _parentForm.InvokeRequired) _parentForm.Invoke(new Action(() => AddMessage(message, MessageType.Incoming)));
            else AddMessage(message, MessageType.Incoming);
        }

        public void AddMessage(string message, MessageType type)
        {
            var bubble = new ChatMessageBubble();
            bubble.SetMessage(message, type, GetUsableWidth());
            bubble.OnForwardRequested += Bubble_OnForwardTextRequested;
            flpMessages.Controls.Add(bubble);
            ScrollToBottom(bubble);
        }

        private void Bubble_OnForwardTextRequested(object? sender, string message)
        {
            ShowForwardDialog(message, null, null);
        }

        // Dictionary để lưu game invite bubbles theo senderID để có thể cập nhật sau
        private Dictionary<string, GameInviteBubble> _gameInviteBubbles = new Dictionary<string, GameInviteBubble>();

        public void ReceiveGameInvite(GameInvitePacket invite, GameType gameType, MessageType type)
        {
            if (_parentForm != null && _parentForm.InvokeRequired)
            {
                _parentForm.Invoke(new Action(() => ReceiveGameInvite(invite, gameType, type)));
                return;
            }

            int usableWidth = GetUsableWidth();
            var bubble = new GameInviteBubble();
            string senderName = type == MessageType.Outgoing ? NetworkManager.Instance.UserName ?? "Bạn" : invite.SenderName;
            bubble.SetInvite(senderName, type, gameType);
            
            // Lưu bubble để có thể cập nhật sau
            string key = type == MessageType.Outgoing ? _friendId : invite.SenderID;
            if (!_gameInviteBubbles.ContainsKey(key))
            {
                _gameInviteBubbles[key] = bubble;
            }
            else
            {
                // Nếu đã có bubble cũ, xóa nó
                var oldBubble = _gameInviteBubbles[key];
                if (flpMessages.Controls.Contains(oldBubble))
                {
                    flpMessages.Controls.Remove(oldBubble);
                }
                _gameInviteBubbles[key] = bubble;
            }

            // Xử lý sự kiện response
            if (type == MessageType.Incoming)
            {
                bubble.OnResponse += (s, accepted) =>
                {
                    string? myId = NetworkManager.Instance.UserID;
                    if (myId != null)
                    {
                        if (gameType == GameType.Caro)
                        {
                            NetworkManager.Instance.SendPacket(new GameResponsePacket 
                            { 
                                SenderID = myId, 
                                ReceiverID = invite.SenderID, 
                                Accepted = accepted 
                            });
                        }
                        else
                        {
                            NetworkManager.Instance.SendPacket(new TankResponsePacket
                            {
                                SenderID = myId,
                                ReceiverID = invite.SenderID,
                                Accepted = accepted
                            });
                        }
                    }
                    // Cập nhật bubble ngay lập tức
                    bubble.UpdateStatus(accepted ? GameInviteStatus.Accepted : GameInviteStatus.Declined);
                };
            }
            else // Outgoing - thêm xử lý mời lại
            {
                bubble.OnReinvite += (s, e) =>
                {
                    HandleReinvite(bubble.CurrentGameType);
                };
            }

            flpMessages.Controls.Add(bubble);
            ScrollToBottom(bubble);
        }

        public void UpdateGameInviteStatus(string senderID, bool accepted, GameType gameType)
        {
            if (_parentForm != null && _parentForm.InvokeRequired)
            {
                _parentForm.Invoke(new Action(() => UpdateGameInviteStatus(senderID, accepted, gameType)));
                return;
            }

            Logger.Info($"[ChatViewControl] UpdateGameInviteStatus called for senderID={senderID}, accepted={accepted}, friendId={_friendId}");
            Logger.Info($"[ChatViewControl] Dictionary keys: [{string.Join(", ", _gameInviteBubbles.Keys)}]");

            bool found = false;

            // Thử tìm theo senderID (khi senderID = friendID = người phản hồi)
            if (_gameInviteBubbles.TryGetValue(senderID, out var bubble))
            {
                Logger.Info($"[ChatViewControl] Found invite bubble by key={senderID}");
                bubble.UpdateStatus(accepted ? GameInviteStatus.Accepted : GameInviteStatus.Declined);
                found = true;
            }
            // Thử tìm theo _friendId (đây là key được dùng khi tạo outgoing bubble)
            else if (_gameInviteBubbles.TryGetValue(_friendId, out var bubbleByFriend))
            {
                Logger.Info($"[ChatViewControl] Found invite bubble by friendId key={_friendId}");
                bubbleByFriend.UpdateStatus(accepted ? GameInviteStatus.Accepted : GameInviteStatus.Declined);
                found = true;
            }
            
            if (!found)
            {
                Logger.Info($"[ChatViewControl] Invite bubble not found in dictionary, searching all controls...");
                // Fallback: tìm bubble outgoing có status Pending (từ cuối lên đầu để tìm cái mới nhất)
                for (int i = flpMessages.Controls.Count - 1; i >= 0; i--)
                {
                    if (flpMessages.Controls[i] is GameInviteBubble gameBubble && gameBubble.Status == GameInviteStatus.Pending)
                    {
                        Logger.Info($"[ChatViewControl] Found pending invite bubble in controls, updating status");
                        gameBubble.UpdateStatus(accepted ? GameInviteStatus.Accepted : GameInviteStatus.Declined);
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
            {
                Logger.Warning($"[ChatViewControl] Could not find any invite bubble to update!");
            }
        }

        private void HandleReinvite(GameType gameType)
        {
            if (string.IsNullOrEmpty(_myId))
            {
                _myId = NetworkManager.Instance.UserID ?? "";
                if (string.IsNullOrEmpty(_myId))
                {
                    MessageBox.Show("Bạn chưa đăng nhập. Vui lòng đăng nhập trước khi gửi lời mời chơi game.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            string? senderName = NetworkManager.Instance.UserName ?? "User";

            try
            {
                if (gameType == GameType.Caro)
                {
                    var invite = new GameInvitePacket { SenderID = _myId, SenderName = senderName, ReceiverID = _friendId };
                    if (NetworkManager.Instance.SendPacket(invite))
                    {
                        btnStartGame.Enabled = false;
                        btnStartGame.Text = "...";
                    }
                    else
                    {
                        MessageBox.Show("Không thể gửi lời mời. Vui lòng kiểm tra kết nối.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    var invite = new TankInvitePacket { SenderID = _myId, SenderName = senderName, ReceiverID = _friendId };
                    if (NetworkManager.Instance.SendPacket(invite))
                    {
                        btnStartGame.Enabled = false;
                        btnStartGame.Text = "...";
                    }
                    else
                    {
                        MessageBox.Show("Không thể gửi lời mời. Vui lòng kiểm tra kết nối.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi gửi lời mời lại: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region == UI & TIỆN ÍCH ==

        public void HandleGameInviteDeclined()
        {
            if (this.InvokeRequired) { this.Invoke(new Action(HandleGameInviteDeclined)); return; }
            
            // Reset game button để có thể mời lại
            ResetGameButtonInternal();
            
            // Bubble đã hiển thị thông báo "✗ Đã từ chối" nên không cần MessageBox
            // Người dùng có thể nhấn nút "Mời lại" trên bubble
            Logger.Info($"[ChatViewControl] {_friendName} đã từ chối lời mời game. Button đã được reset.");
        }

        public void ResetGameButton()
        {
            if (this.InvokeRequired) { this.Invoke(new Action(ResetGameButton)); return; }
            ResetGameButtonInternal();
        }

        private void ResetGameButtonInternal()
        {
            btnStartGame.Enabled = true;
            btnStartGame.Text = "🎲";
        }

        private int GetUsableWidth()
        {
            int width = flpMessages.ClientSize.Width - (flpMessages.Padding.Left + flpMessages.Padding.Right);

            if (flpMessages.VerticalScroll.Visible)
            {
                width -= SystemInformation.VerticalScrollBarWidth;
            }
            return (width > 0) ? width : this.Width;
        }

        private void ChatViewControl_Resize(object sender, EventArgs e)
        {
            int usableWidth = GetUsableWidth();
            if (usableWidth <= 0) return;

            foreach (Control control in flpMessages.Controls)
            {
                if (control is ChatMessageBubble textBubble) textBubble.UpdateMargins(usableWidth);
                else if (control is ImageBubble imgBubble) imgBubble.UpdateMargins(usableWidth);
                else if (control is FileBubble fileBubble) fileBubble.UpdateMargins(usableWidth);
            }
        }

        private void ScrollToBottom(Control control)
        {
            this.BeginInvoke((MethodInvoker)delegate {
                flpMessages.ScrollControlIntoView(control);
            });
        }

        #region Logic Emoji (Không đổi)
        private void LoadEmojis()
        {
            string[] emojis = { "😊", "😂", "❤️", "👍", "🤔", "😢", "😠", "😮", "😎", "😶‍🌫️", "😥", "🤐", "😭", "💀", "💩" };
            foreach (string emoji in emojis)
            {
                Button btn = new Button { Text = emoji, Font = new Font("Segoe UI Emoji", 18), Size = new Size(42, 42), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(245, 245, 245);
                btn.BackColor = Color.Transparent;
                btn.Click += EmojiButton_Click;
                pnlEmojiPicker.Controls.Add(btn);
            }
        }
        private void BtnEmoji_Click(object sender, EventArgs e)
        {
            pnlEmojiPicker.Visible = !pnlEmojiPicker.Visible;
            if (pnlEmojiPicker.Visible) pnlEmojiPicker.BringToFront();
        }
        private void EmojiButton_Click(object sender, EventArgs e)
        {
            txtMessage.AppendText(((Button)sender).Text);
            pnlEmojiPicker.Visible = false;
            txtMessage.Focus();
        }
        #endregion

        #region Paint Events
        private void PnlHeader_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel) return;

            Rectangle rect = panel.ClientRectangle;
            using (LinearGradientBrush brush = new LinearGradientBrush(
                rect,
                AppColors.HeaderGradientStart,
                AppColors.HeaderGradientEnd,
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, rect);
            }

            // Vẽ shadow dưới header
            using (Pen pen = new Pen(Color.FromArgb(30, 0, 0, 0), 1))
            {
                e.Graphics.DrawLine(pen, 0, rect.Height - 1, rect.Width, rect.Height - 1);
            }
        }

        private void PnlInput_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel) return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Vẽ border trên cùng
            Rectangle rect = panel.ClientRectangle;
            using (Pen pen = new Pen(Color.FromArgb(230, 230, 230), 1))
            {
                e.Graphics.DrawLine(pen, 0, 0, rect.Width, 0);
            }

            // Vẽ rounded border cho textbox area
            if (txtMessage != null && txtMessage.Visible)
            {
                Rectangle txtRect = new Rectangle(
                    txtMessage.Left - 5,
                    txtMessage.Top - 5,
                    txtMessage.Width + 10,
                    txtMessage.Height + 10
                );
                using (GraphicsPath path = DrawingHelper.CreateRoundedRectPath(txtRect, 22))
                {
                    // Vẽ background
                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(245, 247, 250)))
                    {
                        e.Graphics.FillPath(brush, path);
                    }
                    // Vẽ border
                    using (Pen pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                }
            }
        }

        private void PnlEmojiPicker_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not FlowLayoutPanel panel) return;

            Rectangle rect = panel.ClientRectangle;
            using (GraphicsPath path = DrawingHelper.CreateRoundedRectPath(rect, 12))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                
                // Vẽ background
                using (SolidBrush brush = new SolidBrush(Color.White))
                {
                    e.Graphics.FillPath(brush, path);
                }

                // Vẽ border và shadow
                using (Pen pen = new Pen(Color.FromArgb(220, 220, 220), 1))
                {
                    e.Graphics.DrawPath(pen, path);
                }

                // Vẽ shadow effect
                Rectangle shadowRect = new Rectangle(rect.X + 2, rect.Y + 2, rect.Width, rect.Height);
                using (GraphicsPath shadowPath = DrawingHelper.CreateRoundedRectPath(shadowRect, 12))
                using (SolidBrush shadowBrush = new SolidBrush(Color.FromArgb(20, 0, 0, 0)))
                {
                    e.Graphics.FillPath(shadowBrush, shadowPath);
                }
            }
        }
        #endregion

        #endregion
    }
}