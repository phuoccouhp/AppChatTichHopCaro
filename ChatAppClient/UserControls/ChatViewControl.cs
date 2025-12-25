using ChatApp.Shared;
using ChatAppClient.Forms;
using ChatAppClient.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatAppClient.UserControls
{
    public partial class ChatViewControl : UserControl
    {
        private string _friendId;
        private string _friendName;
        private Form _parentForm;
        private string _myId = "";

        // Dictionary l?u bong bóng game ?? c?p nh?t tr?ng thái
        private Dictionary<string, GameInviteBubble> _gameInviteBubbles = new Dictionary<string, GameInviteBubble>();

        public ChatViewControl(string friendId, string friendName, Form parentForm)
        {
            InitializeComponent();
            _friendId = friendId;
            _friendName = friendName;
            _parentForm = parentForm;
            lblFriendName.Text = _friendName;

            // C?u hình FlowLayoutPanel
            flpMessages.AutoScroll = true;
            flpMessages.FlowDirection = FlowDirection.TopDown;
            flpMessages.WrapContents = false;
            flpMessages.HorizontalScroll.Visible = false;

            // Ch?ng nháy (Flicker)
            typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(flpMessages, true, null);
        }

        private void ChatViewControl_Load(object sender, EventArgs e)
        {
            _myId = NetworkManager.Instance.UserID ?? "";

            // Gán s? ki?n
            btnSend.Click += BtnSend_Click;
            btnStartGame.Click += BtnStartGame_Click;
            this.Resize += ChatViewControl_Resize;
            btnSendImage.Click += BtnSendImage_Click;
            btnSendFile.Click += BtnSendFile_Click;
            btnEmoji.Click += BtnEmoji_Click;
            btnAttach.Click += BtnAttach_Click;

            // C?u hình TextBox
            txtMessage.Multiline = true;
            txtMessage.ScrollBars = ScrollBars.None;
            txtMessage.WordWrap = true;
            txtMessage.KeyDown += TxtMessage_KeyDown;

            // Menu ch?n game
            ContextMenuStrip gameMenu = new ContextMenuStrip();
            gameMenu.Items.Add("Ch?i Caro", null, (s, ev) => InviteGame(GameType.Caro));
            gameMenu.Items.Add("Ch?i Tank Game", null, (s, ev) => InviteGame(GameType.Tank));
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
                    // Server ?ã reverse list r?i, nên ? ?ây duy?t xuôi
                    foreach (var msg in response.Messages)
                    {
                        bool isMe = (msg.SenderID == _myId);
                        MessageType type = isMe ? MessageType.Outgoing : MessageType.Incoming;

                        if (msg.MessageType == "GameInvite")
                        {
                            string senderName = isMe ? (NetworkManager.Instance.UserName ?? "B?n") : _friendName;
                            GameType gType = msg.FileName == "Tank" ? GameType.Tank : GameType.Caro;

                            var invite = new GameInvitePacket { SenderID = msg.SenderID, SenderName = senderName, ReceiverID = msg.ReceiverID };
                            DisplayGameInvite(invite, gType, type, msg.MessageContent);
                        }
                        else if (msg.MessageType == "Text")
                        {
                            // ? [FIX] ??m b?o n?i dung tin nh?n không r?ng
                            string content = msg.MessageContent;
                            if (!string.IsNullOrWhiteSpace(content))
                            {
                                AddMessageBubble(content, type, msg.CreatedAt);
                            }
                        }
                        else if (msg.MessageType == "File" || msg.MessageType == "Image")
                        {
                            // Hi?n th? thông báo file trong l?ch s? (vì không t?i binary ngay)
                            AddSystemMessage($"[L?ch s?] {msg.MessageType}: {msg.FileName}");
                        }
                    }
                    ScrollToBottom();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"L?i load l?ch s?: {ex.Message}");
            }
        }

        #region === H? TH?NG LAYOUT & HI?N TH? (CORE) ===

        // Thêm tin nh?n Text
        private void AddMessageBubble(string message, MessageType type, DateTime time)
        {
            // ? [FIX] ??m b?o message không null
            if (string.IsNullOrWhiteSpace(message))
            {
                message = "(Tin nh?n tr?ng)";
            }
            
            var bubble = new ChatMessageBubble();
            bubble.SetData(message, type, time);
            AddControlToLayout(bubble, type);
        }

        // Thêm tin nh?n File/Image (QUAN TR?NG: Logic tích h?p v?i FileBubble c?a b?n)
        public void ReceiveFileMessage(FilePacket p, MessageType type)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => ReceiveFileMessage(p, type))); return; }

            Control bubbleToAdd;

            if (p.IsImage)
            {
                var imgBubble = new ImageBubble();
                try
                {
                    using (var ms = new MemoryStream(p.FileData))
                    {
                        // ? [FIX] Clone image ?? tránh memory leak và l?i khi stream b? dispose
                        using (var originalImg = Image.FromStream(ms))
                        {
                            Image clonedImg = new Bitmap(originalImg);
                            imgBubble.SetImage(clonedImg, p.FileName, p.FileData, type);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"L?i load ?nh: {ex.Message}");
                    AddSystemMessage($"[L?i hi?n th? ?nh: {p.FileName}]");
                    return;
                }

                imgBubble.OnForwardRequested += (s, data) => ShowForwardDialog(null, data.fileName, data.fileData);
                bubbleToAdd = imgBubble;
            }
            else
            {
                var fileBubble = new FileBubble();
                fileBubble.SetMessage(p.FileName, p.FileData, type, flpMessages.ClientSize.Width);
                fileBubble.OnForwardRequested += (s, data) => ShowForwardDialog(null, data.fileName, data.fileData);
                bubbleToAdd = fileBubble;
            }

            AddControlToLayout(bubbleToAdd, type);
        }

        // Hàm c?n trái/ph?i dùng Container Panel
        private void AddControlToLayout(Control ctrl, MessageType type)
        {
            Panel container = new Panel();
            container.AutoSize = false;
            int scrollWidth = SystemInformation.VerticalScrollBarWidth;
            container.Width = flpMessages.ClientSize.Width - scrollWidth - 5;

            // Chi?u cao = chi?u cao control + margin
            container.Height = ctrl.Height + 10;
            container.BackColor = Color.Transparent;
            container.Margin = new Padding(0, 0, 0, 5);

            if (type == MessageType.Outgoing)
            {
                // C?n ph?i
                ctrl.Location = new Point(container.Width - ctrl.Width - 10, 0);
                ctrl.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            }
            else
            {
                // C?n trái
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

        #region === S? KI?N G?I TIN ===

        private void BtnSend_Click(object sender, EventArgs e)
        {
            string content = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(content)) return;

            // ? [FIX] ??m b?o content không r?ng tr??c khi g?i
            if (string.IsNullOrWhiteSpace(content))
            {
                return;
            }

            // Hi?n th? tin nh?n ngay l?p t?c
            AddMessageBubble(content, MessageType.Outgoing, DateTime.Now);

            // G?i packet
            var packet = new TextPacket 
            { 
                SenderID = _myId ?? "", 
                ReceiverID = _friendId ?? "", 
                MessageContent = content 
            };
            
            if (!NetworkManager.Instance.SendPacket(packet))
            {
                // N?u g?i th?t b?i, có th? hi?n th? thông báo
                System.Diagnostics.Debug.WriteLine("Không th? g?i tin nh?n");
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
                // ? [FIX] Ki?m tra kích th??c file tr??c khi g?i
                FileInfo fileInfo = new FileInfo(dialog.FileName);
                const long MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB max
                const long MAX_IMAGE_SIZE = 5 * 1024 * 1024; // 5MB max cho ?nh
                
                long maxSize = isImage ? MAX_IMAGE_SIZE : MAX_FILE_SIZE;
                if (fileInfo.Length > maxSize)
                {
                    string sizeLimit = isImage ? "5MB" : "10MB";
                    MessageBox.Show($"File quá l?n! Gi?i h?n: {sizeLimit}\nKích th??c file: {fileInfo.Length / (1024.0 * 1024.0):F2}MB", 
                        "L?i", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                string selectedFile = dialog.FileName;
                
                // ? [FIX] G?i file async ?? không block UI
                _ = Task.Run(() =>
                {
                    try
                    {
                        byte[] fileData = File.ReadAllBytes(selectedFile);
                        string fileName = Path.GetFileName(selectedFile);

                        var packet = new FilePacket 
                        { 
                            SenderID = _myId, 
                            ReceiverID = _friendId, 
                            FileName = fileName, 
                            FileData = fileData, 
                            IsImage = isImage 
                        };
                        
                        // G?i packet
                        bool sent = NetworkManager.Instance.SendPacket(packet);
                        
                        // Hi?n th? trên UI thread
                        this.Invoke(new Action(() =>
                        {
                            if (sent)
                            {
                                ReceiveFileMessage(packet, MessageType.Outgoing);
                            }
                            else
                            {
                                MessageBox.Show("Không th? g?i file. Ki?m tra k?t n?i m?ng.", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }));
                    }
                    catch (Exception ex)
                    {
                        this.Invoke(new Action(() =>
                        {
                            MessageBox.Show("L?i g?i file: " + ex.Message, "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }));
                    }
                });
            }
        }

        #endregion

        #region === X? LÝ GAME INVITE ===

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
                // Hi?n th? bong bóng Outgoing
                if (sent) ReceiveGameInvite(packet, type, MessageType.Outgoing);
            }
            else
            {
                var packet = new TankInvitePacket { SenderID = _myId, SenderName = senderName, ReceiverID = _friendId };
                sent = NetworkManager.Instance.SendPacket(packet);
                // Map sang GameInvite ?? dùng chung hàm hi?n th?
                if (sent) ReceiveGameInvite(new GameInvitePacket { SenderID = _myId, SenderName = senderName, ReceiverID = _friendId }, type, MessageType.Outgoing);
            }

            if (sent)
            {
                btnStartGame.Enabled = false;
                btnStartGame.Text = "...";
            }
            else
            {
                MessageBox.Show("Không th? g?i l?i m?i. Ki?m tra k?t n?i.");
            }
        }

        // Hàm nh?n Invite t? Server (Incoming) HO?C hi?n th? Invite v?a g?i (Outgoing)
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
                if (statusContent.Contains("ch?p nh?n")) bubble.UpdateStatus(GameInviteStatus.Accepted);
                else if (statusContent.Contains("t? ch?i")) bubble.UpdateStatus(GameInviteStatus.Declined);
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
                _gameInviteBubbles[_friendId] = bubble; // L?u ?? update khi ??i ph??ng tr? l?i
            }

            AddControlToLayout(bubble, mType);
        }

        public void UpdateGameInviteStatus(string senderID, bool accepted, GameType gType)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => UpdateGameInviteStatus(senderID, accepted, gType))); return; }

            // Tìm bubble outgoing t??ng ?ng
            if (_gameInviteBubbles.TryGetValue(_friendId, out var bubble))
            {
                bubble.UpdateStatus(accepted ? GameInviteStatus.Accepted : GameInviteStatus.Declined);
            }
            else
            {
                AddSystemMessage(accepted ? $"??i th? ?ã ch?p nh?n ch?i {gType}!" : $"??i th? ?ã t? ch?i l?i m?i {gType}.");
            }
        }

        public void ResetGameButton()
        {
            if (this.InvokeRequired) { this.Invoke(new Action(ResetGameButton)); return; }
            btnStartGame.Enabled = true;
            btnStartGame.Text = "??";
        }

        public void HandleGameInviteDeclined()
        {
            if (this.InvokeRequired) { this.Invoke(new Action(HandleGameInviteDeclined)); return; }
            ResetGameButton();
        }

        #endregion

        #region === CÁC S? KI?N PH? ===

        public void ReceiveMessage(string content)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => ReceiveMessage(content))); return; }
            
            // ? [FIX] ??m b?o content không null và không r?ng
            if (string.IsNullOrWhiteSpace(content))
            {
                content = "(Tin nh?n tr?ng)";
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

            if (friends.Count == 0) { MessageBox.Show("Không có b?n bè ?? chuy?n ti?p."); return; }

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
                    MessageBox.Show("?ã chuy?n ti?p!");
                }
            }
        }

        private void ChatViewControl_Resize(object sender, EventArgs e)
        {
            // Khi resize, update l?i width cho các container panel
            int w = flpMessages.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 5;
            foreach (Control c in flpMessages.Controls)
            {
                if (c is Panel p)
                {
                    p.Width = w;
                    // Update l?i v? trí control con bên trong n?u là Outgoing (c?n ph?i)
                    if (p.Controls.Count > 0)
                    {
                        Control child = p.Controls[0];
                        // N?u ?ang ? bên ph?i (g?n l? ph?i), thì d?i theo
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
            string[] emojis = { "??", "??", "??", "??", "??", "??", "??", "??", "??", "??", "??", "??" };
            foreach (string emoji in emojis)
            {
                // ? [FIX] Màu button emoji ?en nh? GroupChat
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
            // ? [FIX] Không v? gradient n?a, dùng màu ?en nh? GroupChat
            // Header ?ã ???c set BackColor trong Designer r?i
        }

        private void PnlInput_Paint(object sender, PaintEventArgs e) { /* Custom border */ }
        private void PnlEmojiPicker_Paint(object sender, PaintEventArgs e) { /* Custom border */ }

        #endregion
    }
}
