using ChatApp.Shared;
using ChatAppClient.Forms;
using ChatAppClient.Helpers;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ChatAppClient.UserControls
{
    public partial class ChatViewControl : UserControl
    {
        private string _friendId;
        private string _friendName;
        private Form _parentForm;
        private string _myId; // ID của chính user này

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
            _myId = NetworkManager.Instance.UserID;

            // Gán sự kiện
            btnSend.Click += BtnSend_Click;
            btnStartGame.Click += BtnStartGame_Click; // Nút Caro
            btnStartTank.Click += BtnStartTank_Click; // Nút Tank

            this.Resize += new System.EventHandler(this.ChatViewControl_Resize);
            btnSendImage.Click += BtnSendImage_Click;
            btnSendFile.Click += BtnSendFile_Click;
            btnEmoji.Click += BtnEmoji_Click;

            LoadEmojis();

            // Thiết lập icon mặc định cho các nút game
            btnStartGame.Text = "🎲";
            btnStartTank.Text = "🚜";
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

        // Gửi lời mời Caro
        private void BtnStartGame_Click(object sender, EventArgs e)
        {
            var invite = new GameInvitePacket
            {
                SenderID = _myId,
                SenderName = NetworkManager.Instance.UserName,
                ReceiverID = _friendId
            };
            NetworkManager.Instance.SendPacket(invite);
            btnStartGame.Enabled = false;
            btnStartGame.Text = "...";
            MessageBox.Show($"Đã gửi lời mời chơi Caro đến {_friendName}!\nĐang chờ phản hồi...", "Thông báo");
        }

        // Gửi lời mời Tank
        private void BtnStartTank_Click(object sender, EventArgs e)
        {
            var invite = new TankInvitePacket
            {
                SenderID = _myId,
                SenderName = NetworkManager.Instance.UserName,
                ReceiverID = _friendId
            };
            NetworkManager.Instance.SendPacket(invite);
            btnStartTank.Enabled = false;
            btnStartTank.Text = "...";
            MessageBox.Show($"Đã gửi lời mời bắn Tank đến {_friendName}!\nĐang chờ phản hồi...", "Thông báo");
        }

        // Gửi Ảnh
        private void BtnSendImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog { Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp" };
            if (dialog.ShowDialog() == DialogResult.OK) SendFile(dialog.FileName, true);
        }

        // Gửi File
        private void BtnSendFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog { Filter = "All Files|*.*" };
            if (dialog.ShowDialog() == DialogResult.OK) SendFile(dialog.FileName, false);
        }

        private void SendFile(string filePath, bool isImage)
        {
            try
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                string fileName = Path.GetFileName(filePath);
                var filePacket = new FilePacket
                {
                    SenderID = _myId,
                    ReceiverID = _friendId,
                    FileName = fileName,
                    FileData = fileData,
                    IsImage = isImage
                };
                NetworkManager.Instance.SendPacket(filePacket);
                AddFileBubble(filePacket, MessageType.Outgoing);
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
            if (_parentForm != null && _parentForm.InvokeRequired)
                _parentForm.Invoke(new Action(() => AddFileBubble(packet, type)));
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
                bubbleToAdd = bubble;
            }
            else
            {
                var bubble = new FileBubble();
                bubble.SetMessage(packet.FileName, packet.FileData, type, usableWidth);
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
            if (_parentForm != null && _parentForm.InvokeRequired)
                _parentForm.Invoke(new Action(() => AddMessage(message, MessageType.Incoming)));
            else AddMessage(message, MessageType.Incoming);
        }

        public void AddMessage(string message, MessageType type)
        {
            var bubble = new ChatMessageBubble();
            bubble.SetMessage(message, type, GetUsableWidth());
            flpMessages.Controls.Add(bubble);
            ScrollToBottom(bubble);
        }

        #endregion

        #region == UI & TIỆN ÍCH ==

        // Xử lý khi bị từ chối Caro
        public void HandleGameInviteDeclined()
        {
            if (this.InvokeRequired) { this.Invoke(new Action(HandleGameInviteDeclined)); return; }
            MessageBox.Show($"{_friendName} đã từ chối lời mời.", "Tiếc quá!");
            ResetGameButtonInternal(btnStartGame); // Reset Caro
            ResetGameButtonInternal(btnStartTank); // Reset Tank (nếu muốn)
        }

        public void ResetGameButton()
        {
            if (this.InvokeRequired) { this.Invoke(new Action(ResetGameButton)); return; }
            ResetGameButtonInternal(btnStartGame);
            ResetGameButtonInternal(btnStartTank);
        }

        private void ResetGameButtonInternal(Button button)
        {
            button.Enabled = true;
            if (button == btnStartGame) button.Text = "🎲";
            else if (button == btnStartTank) button.Text = "🚜";
            else button.Text = "...";
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
                switch (control)
                {
                    case ChatMessageBubble textBubble:
                        textBubble.UpdateMargins(usableWidth);
                        break;
                    case ImageBubble imgBubble:
                        imgBubble.UpdateMargins(usableWidth);
                        break;
                    case FileBubble fileBubble:
                        fileBubble.UpdateMargins(usableWidth);
                        break;
                }
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
            string[] emojis = { "😊", "😂", "❤️", "👍", "🤔", "😢", "😠", "😮" };
            foreach (string emoji in emojis)
            {
                Button btn = new Button { Text = emoji, Font = new Font("Segoe UI Emoji", 12), Size = new Size(40, 40), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand };
                btn.FlatAppearance.BorderSize = 0;
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

        #endregion
    }
}
