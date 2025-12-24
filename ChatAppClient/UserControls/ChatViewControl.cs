using ChatApp.Shared;
using ChatAppClient.Forms;
using ChatAppClient.Helpers;
using ChatAppClient.UserControls;
using ChatAppClient; // <--- QUAN TRỌNG: Thêm dòng này để dùng MessageType
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

        // Dictionary để lưu game invite bubbles
        private Dictionary<string, GameInviteBubble> _gameInviteBubbles = new Dictionary<string, GameInviteBubble>();

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
            btnAttach.Click += BtnAttach_Click;

            // Căn chỉnh TextBox
            txtMessage.Multiline = true;
            txtMessage.ScrollBars = ScrollBars.None;
            txtMessage.AcceptsReturn = true;
            txtMessage.WordWrap = true;

            // Context menu game
            ContextMenuStrip gameMenu = new ContextMenuStrip();
            gameMenu.Items.Add("Chơi Caro", null, (s, ev) => InviteCaroGame());
            gameMenu.Items.Add("Chơi Tank Game", null, (s, ev) => InviteTankGame());
            btnStartGame.ContextMenuStrip = gameMenu;

            LoadEmojis();
            LoadChatHistory();
        }

        private async void LoadChatHistory()
        {
            if (string.IsNullOrEmpty(_myId) || string.IsNullOrEmpty(_friendId)) return;

            try
            {
                var response = await NetworkManager.Instance.RequestChatHistoryAsync(_friendId, 100);

                if (response.Success && response.Messages != null && response.Messages.Count > 0)
                {
                    foreach (var msg in response.Messages)
                    {
                        if (msg.MessageType == "GameInvite")
                        {
                            bool isOutgoing = msg.SenderID == _myId;
                            string senderName = isOutgoing ? NetworkManager.Instance.UserName ?? "Bạn" : _friendName;
                            GameType gameType = msg.FileName == "Tank" ? GameType.Tank : GameType.Caro;

                            var invite = new GameInvitePacket { SenderID = msg.SenderID, SenderName = senderName, ReceiverID = msg.ReceiverID };
                            var bubble = new GameInviteBubble();
                            bubble.SetInvite(senderName, isOutgoing ? MessageType.Outgoing : MessageType.Incoming, gameType); // Đã bỏ msg.MessageID vì hàm cũ k có tham số này

                            string content = msg.MessageContent ?? "";
                            if (content.Contains("chấp nhận")) bubble.UpdateStatus(GameInviteStatus.Accepted);
                            else if (content.Contains("từ chối")) bubble.UpdateStatus(GameInviteStatus.Declined);

                            string key = isOutgoing ? _friendId : msg.SenderID;
                            if (!_gameInviteBubbles.ContainsKey(key)) _gameInviteBubbles[key] = bubble;

                            if (!isOutgoing && bubble.Status == GameInviteStatus.Pending)
                            {
                                bubble.OnResponse += (s, accepted) =>
                                {
                                    string? myId = NetworkManager.Instance.UserID;
                                    if (myId != null)
                                    {
                                        if (gameType == GameType.Caro) NetworkManager.Instance.SendPacket(new GameResponsePacket { SenderID = myId, ReceiverID = msg.SenderID, Accepted = accepted });
                                        else NetworkManager.Instance.SendPacket(new TankResponsePacket { SenderID = myId, ReceiverID = msg.SenderID, Accepted = accepted });
                                    }
                                    bubble.UpdateStatus(accepted ? GameInviteStatus.Accepted : GameInviteStatus.Declined);
                                };
                            }
                            else if (isOutgoing)
                            {
                                bubble.OnReinvite += (s, e) => HandleReinvite(bubble.CurrentGameType);
                            }
                            flpMessages.Controls.Add(bubble);
                        }
                        else if (msg.SenderID == _myId)
                        {
                            if (msg.MessageType == "Text") AddMessage(msg.MessageContent ?? "", MessageType.Outgoing);
                            else AddMessage($"Đã gửi {(msg.MessageType == "Image" ? "ảnh" : "file")}: {msg.FileName}", MessageType.Outgoing);
                        }
                        else
                        {
                            if (msg.MessageType == "Text") AddMessage(msg.MessageContent ?? "", MessageType.Incoming);
                            else AddMessage($"Đã nhận {(msg.MessageType == "Image" ? "ảnh" : "file")}: {msg.FileName}", MessageType.Incoming);
                        }
                    }
                    if (flpMessages.Controls.Count > 0)
                    {
                        var lastControl = flpMessages.Controls[flpMessages.Controls.Count - 1];
                        ScrollToBottom(lastControl);
                    }
                }
            }
            catch (Exception ex)
            {
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
            if (btnStartGame.ContextMenuStrip != null) btnStartGame.ContextMenuStrip.Show(btnStartGame, new Point(0, btnStartGame.Height));
            else InviteCaroGame();
        }

        private void InviteCaroGame() => SendGameInvite(GameType.Caro);
        private void InviteTankGame() => SendGameInvite(GameType.Tank);

        private void SendGameInvite(GameType type)
        {
            if (string.IsNullOrEmpty(_myId))
            {
                _myId = NetworkManager.Instance.UserID ?? "";
                if (string.IsNullOrEmpty(_myId)) { MessageBox.Show("Chưa đăng nhập."); return; }
            }

            string? senderName = NetworkManager.Instance.UserName ?? "User";
            object packet = type == GameType.Caro
                ? (object)new GameInvitePacket { SenderID = _myId, SenderName = senderName, ReceiverID = _friendId }
                : (object)new TankInvitePacket { SenderID = _myId, SenderName = senderName, ReceiverID = _friendId };

            try
            {
                if (NetworkManager.Instance.SendPacket(packet))
                {
                    btnStartGame.Enabled = false;
                    btnStartGame.Text = "...";
                    var invite = new GameInvitePacket { SenderID = _myId, SenderName = senderName, ReceiverID = _friendId };
                    ReceiveGameInvite(invite, type, MessageType.Outgoing);
                }
                else MessageBox.Show("Lỗi kết nối.");
            }
            catch (Exception ex) { MessageBox.Show("Lỗi gửi mời: " + ex.Message); }
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

        private void BtnAttach_Click(object sender, EventArgs e) => ctxAttachMenu?.Show(btnAttach, new Point(0, btnAttach.Height));

        private void SendFile(string filePath, bool isImage)
        {
            try
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                string fileName = Path.GetFileName(filePath);
                var filePacket = new FilePacket { SenderID = _myId, ReceiverID = _friendId, FileName = fileName, FileData = fileData, IsImage = isImage };
                NetworkManager.Instance.SendPacket(filePacket);
                AddFileBubble(filePacket, MessageType.Outgoing);
            }
            catch (Exception ex) { MessageBox.Show($"Lỗi gửi file: {ex.Message}"); }
        }

        #endregion

        #region == NHẬN TIN ==

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

        public void ReceiveMessage(string message)
        {
            if (_parentForm != null && _parentForm.InvokeRequired) _parentForm.Invoke(new Action(() => AddMessage(message, MessageType.Incoming)));
            else AddMessage(message, MessageType.Incoming);
        }

        public void AddMessage(string message, MessageType type)
        {
            var bubble = new ChatMessageBubble();
            bubble.SetMessage(message, type, GetUsableWidth());
            bubble.OnForwardRequested += (s, msg) => ShowForwardDialog(msg, null, null);
            flpMessages.Controls.Add(bubble);
            ScrollToBottom(bubble);
        }

        public void ReceiveGameInvite(GameInvitePacket invite, GameType gameType, MessageType type)
        {
            if (_parentForm != null && _parentForm.InvokeRequired) { _parentForm.Invoke(new Action(() => ReceiveGameInvite(invite, gameType, type))); return; }

            int usableWidth = GetUsableWidth();
            var bubble = new GameInviteBubble();
            string senderName = type == MessageType.Outgoing ? NetworkManager.Instance.UserName ?? "Bạn" : invite.SenderName;
            bubble.SetInvite(senderName, type, gameType);

            string key = type == MessageType.Outgoing ? _friendId : invite.SenderID;
            if (!_gameInviteBubbles.ContainsKey(key)) _gameInviteBubbles[key] = bubble;
            else
            {
                var old = _gameInviteBubbles[key];
                if (flpMessages.Controls.Contains(old)) flpMessages.Controls.Remove(old);
                _gameInviteBubbles[key] = bubble;
            }

            if (type == MessageType.Incoming)
            {
                bubble.OnResponse += (s, accepted) =>
                {
                    string myId = NetworkManager.Instance.UserID ?? "";
                    if (gameType == GameType.Caro) NetworkManager.Instance.SendPacket(new GameResponsePacket { SenderID = myId, ReceiverID = invite.SenderID, Accepted = accepted });
                    else NetworkManager.Instance.SendPacket(new TankResponsePacket { SenderID = myId, ReceiverID = invite.SenderID, Accepted = accepted });
                    bubble.UpdateStatus(accepted ? GameInviteStatus.Accepted : GameInviteStatus.Declined);
                };
            }
            else bubble.OnReinvite += (s, e) => HandleReinvite(bubble.CurrentGameType);

            flpMessages.Controls.Add(bubble);
            ScrollToBottom(bubble);
        }

        public void UpdateGameInviteStatus(string senderID, bool accepted, GameType gType)
        {
            if (_parentForm != null && _parentForm.InvokeRequired) { _parentForm.Invoke(new Action(() => UpdateGameInviteStatus(senderID, accepted, gType))); return; }

            // Tìm bubble theo ID
            bool found = false;
            if (_gameInviteBubbles.TryGetValue(senderID, out var bubble))
            {
                bubble.UpdateStatus(accepted ? GameInviteStatus.Accepted : GameInviteStatus.Declined);
                found = true;
            }
            else if (_gameInviteBubbles.TryGetValue(_friendId, out var bubbleByFriend))
            {
                bubbleByFriend.UpdateStatus(accepted ? GameInviteStatus.Accepted : GameInviteStatus.Declined);
                found = true;
            }

            if (!found)
            {
                for (int i = flpMessages.Controls.Count - 1; i >= 0; i--)
                {
                    if (flpMessages.Controls[i] is GameInviteBubble gb && gb.Status == GameInviteStatus.Pending)
                    {
                        gb.UpdateStatus(accepted ? GameInviteStatus.Accepted : GameInviteStatus.Declined);
                        break;
                    }
                }
            }
        }

        private void HandleReinvite(GameType gameType) { if (gameType == GameType.Caro) InviteCaroGame(); else InviteTankGame(); }
        public void ResetGameButton() { if (InvokeRequired) Invoke(new Action(ResetGameButton)); else { btnStartGame.Enabled = true; btnStartGame.Text = "🎲"; } }
        public void HandleGameInviteDeclined() { if (InvokeRequired) Invoke(new Action(HandleGameInviteDeclined)); else ResetGameButton(); }

        private int GetUsableWidth() { int w = flpMessages.ClientSize.Width - flpMessages.Padding.Left - flpMessages.Padding.Right; if (flpMessages.VerticalScroll.Visible) w -= SystemInformation.VerticalScrollBarWidth; return w > 0 ? w : Width; }
        private void ChatViewControl_Resize(object sender, EventArgs e) { int w = GetUsableWidth(); if (w > 0) foreach (Control c in flpMessages.Controls) { if (c is ChatMessageBubble t) t.UpdateMargins(w); else if (c is ImageBubble i) i.UpdateMargins(w); else if (c is FileBubble f) f.UpdateMargins(w); } }
        private void ScrollToBottom(Control c) { this.BeginInvoke((MethodInvoker)delegate { flpMessages.ScrollControlIntoView(c); }); }

        // --- Helper ---
        private void ShowForwardDialog(string? text, string? fname, byte[]? data)
        {
            var friends = new List<(string id, string name)>();
            if (_parentForm is frmHome homeForm) friends = homeForm.GetFriendsList().Where(f => f.id != _friendId).ToList();
            if (friends.Count == 0) { MessageBox.Show("Không có bạn bè để chuyển tiếp."); return; }

            using (var fwd = new frmForwardMessage(friends))
            {
                if (fwd.ShowDialog() == DialogResult.OK && fwd.SelectedFriendID != null)
                {
                    string tid = fwd.SelectedFriendID;
                    string myId = NetworkManager.Instance.UserID ?? "";
                    if (!string.IsNullOrEmpty(text)) NetworkManager.Instance.SendPacket(new TextPacket { SenderID = myId, ReceiverID = tid, MessageContent = text });
                    else if (data != null) NetworkManager.Instance.SendPacket(new FilePacket { SenderID = myId, ReceiverID = tid, FileName = fname ?? "unknown", FileData = data, IsImage = fname?.EndsWith(".png") == true });
                    MessageBox.Show("Đã chuyển tiếp!");
                }
            }
        }

        private void LoadEmojis()
        {
            string[] emojis = { "😊", "😂", "❤️", "👍", "🤔", "😢", "😠", "😮", "😎", "😥", "😭", "💀" };
            foreach (string e in emojis) { Button b = new Button { Text = e, Font = new Font("Segoe UI Emoji", 18), Size = new Size(42, 42), FlatStyle = FlatStyle.Flat }; b.FlatAppearance.BorderSize = 0; b.Click += EmojiButton_Click; pnlEmojiPicker.Controls.Add(b); }
        }
        private void BtnEmoji_Click(object sender, EventArgs e) { pnlEmojiPicker.Visible = !pnlEmojiPicker.Visible; if (pnlEmojiPicker.Visible) pnlEmojiPicker.BringToFront(); }
        private void EmojiButton_Click(object sender, EventArgs e) { txtMessage.AppendText(((Button)sender).Text); pnlEmojiPicker.Visible = false; txtMessage.Focus(); }
        private void PnlHeader_Paint(object sender, PaintEventArgs e) { if (sender is Panel p) using (LinearGradientBrush b = new LinearGradientBrush(p.ClientRectangle, AppColors.HeaderGradientStart, AppColors.HeaderGradientEnd, LinearGradientMode.Vertical)) e.Graphics.FillRectangle(b, p.ClientRectangle); }
        private void PnlInput_Paint(object sender, PaintEventArgs e) { /* Custom border */ }
        private void PnlEmojiPicker_Paint(object sender, PaintEventArgs e) { /* Custom border */ }

        #endregion
    }
}