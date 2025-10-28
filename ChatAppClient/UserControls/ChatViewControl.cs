using ChatApp.Shared;
using ChatAppClient.Forms;
using ChatAppClient.Helpers;
using ChatAppClient.UserControls;
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

        // ID của chính user này
        private string _myId;

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
            // LẤY ID TỪ NETWORKMANAGER
            _myId = NetworkManager.Instance.UserID;

            // Gán sự kiện
            btnSend.Click += BtnSend_Click;
            btnStartGame.Click += BtnStartGame_Click;
            this.Resize += new System.EventHandler(this.ChatViewControl_Resize);
            btnSendImage.Click += BtnSendImage_Click;
            btnSendFile.Click += BtnSendFile_Click;
            btnEmoji.Click += BtnEmoji_Click;

            LoadEmojis();

            // Tin nhắn mẫu (có thể xóa)
            AddMessage($"Bắt đầu trò chuyện với {_friendName}", MessageType.Incoming);
        }

        #region == GỬI TIN (TEXT, FILE, GAME) ==

        // GỬI TIN NHẮN TEXT
        private void BtnSend_Click(object sender, EventArgs e)
        {
            string message = txtMessage.Text.Trim();
            if (!string.IsNullOrEmpty(message))
            {
                // 1. Hiển thị ngay
                AddMessage(message, MessageType.Outgoing);

                // 2. Tạo gói tin và GỬI
                var textPacket = new TextPacket
                {
                    SenderID = _myId,
                    ReceiverID = _friendId,
                    MessageContent = message
                };
                NetworkManager.Instance.SendPacket(textPacket);

                txtMessage.Clear();
                txtMessage.Focus();
            }
        }

        // GỬI LỜI MỜI GAME
        private void BtnStartGame_Click(object sender, EventArgs e)
        {
            var invite = new GameInvitePacket
            {
                SenderID = _myId,
                SenderName = NetworkManager.Instance.UserName,
                ReceiverID = _friendId
            };

            // GỬI GÓI TIN
            NetworkManager.Instance.SendPacket(invite);

            btnStartGame.Enabled = false;
            btnStartGame.Text = "...";

            MessageBox.Show($"Đã gửi lời mời chơi Caro đến {_friendName}!\nĐang chờ phản hồi...", "Thông báo");
        }

        // GỬI ẢNH
        private void BtnSendImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SendFile(dialog.FileName, true);
            }
        }

        // GỬI FILE
        private void BtnSendFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "All Files|*.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SendFile(dialog.FileName, false);
            }
        }

        // Hàm chung GỬI FILE/ẢNH
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

                // GỬI GÓI TIN
                NetworkManager.Instance.SendPacket(filePacket);

                // Hiển thị ngay cho người gửi
                AddFileBubble(filePacket, MessageType.Outgoing);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể gửi file: {ex.Message}", "Lỗi");
            }
        }

        #endregion

        #region == NHẬN TIN (TEXT, FILE) ==

        // Hàm này được gọi bởi frmHome khi có tin nhắn FILE
        public void ReceiveFileMessage(FilePacket packet, MessageType type = MessageType.Incoming)
        {
            if (_parentForm != null && _parentForm.InvokeRequired)
            {
                _parentForm.Invoke(new Action(() => AddFileBubble(packet, type)));
            }
            else
            {
                AddFileBubble(packet, type);
            }
        }

        // Hàm thêm bong bóng File/Ảnh vào UI
        private void AddFileBubble(FilePacket packet, MessageType type)
        {
            int usableWidth = flpMessages.ClientSize.Width - 10;
            if (usableWidth <= 0) usableWidth = this.Width;

            if (packet.IsImage)
            {
                ImageBubble bubble = new ImageBubble();
                bubble.SetMessage(packet.FileData, type, usableWidth);
                flpMessages.Controls.Add(bubble);
                ScrollToBottom(bubble);
            }
            else
            {
                FileBubble bubble = new FileBubble();
                bubble.SetMessage(packet.FileName, packet.FileData, type, usableWidth);
                flpMessages.Controls.Add(bubble);
                ScrollToBottom(bubble);
            }
        }

        // Hàm này được gọi bởi frmHome khi có tin nhắn TEXT
        public void ReceiveMessage(string message)
        {
            if (_parentForm != null && _parentForm.InvokeRequired)
            {
                _parentForm.Invoke(new Action(() => AddMessage(message, MessageType.Incoming)));
            }
            else
            {
                AddMessage(message, MessageType.Incoming);
            }
        }

        // Hàm thêm bong bóng TEXT
        public void AddMessage(string message, MessageType type)
        {
            ChatMessageBubble bubble = new ChatMessageBubble();
            int usableWidth = flpMessages.ClientSize.Width - 10;
            if (usableWidth <= 0) usableWidth = this.Width;

            bubble.SetMessage(message, type, usableWidth);
            flpMessages.Controls.Add(bubble);
            ScrollToBottom(bubble);
        }

        #endregion

        #region == UI & TIỆN ÍCH ==

        // Hàm được gọi từ frmHome khi bị từ chối game
        public void HandleGameInviteDeclined()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(HandleGameInviteDeclined));
                return;
            }

            MessageBox.Show($"{_friendName} đã từ chối lời mời.", "Tiếc quá!");
            btnStartGame.Enabled = true;
            btnStartGame.Text = "🎲";
        }

        // Xử lý Resize (Không đổi)
        private void ChatViewControl_Resize(object sender, EventArgs e)
        {
            int usableWidth = flpMessages.ClientSize.Width - 10;
            if (usableWidth <= 0) return;

            foreach (Control control in flpMessages.Controls)
            {
                if (control is ChatMessageBubble textBubble)
                    textBubble.UpdateMargins(usableWidth);
                else if (control is ImageBubble imgBubble)
                    imgBubble.UpdateMargins(usableWidth);
                else if (control is FileBubble fileBubble)
                    fileBubble.UpdateMargins(usableWidth);
            }
        }

        // Cuộn xuống dưới (Không đổi)
        private void ScrollToBottom(Control control)
        {
            flpMessages.AutoScrollPosition = new Point(0, flpMessages.VerticalScroll.Maximum);
            flpMessages.ScrollControlIntoView(control);
        }

        #region Logic Emoji (Không đổi)
        private void LoadEmojis()
        {
            string[] emojis = { "😊", "😂", "❤️", "👍", "🤔", "😢", "😠", "😮" };
            foreach (string emoji in emojis)
            {
                Button btn = new Button();
                btn.Text = emoji;
                btn.Font = new Font("Segoe UI Emoji", 12);
                btn.Size = new Size(40, 40);
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.Cursor = Cursors.Hand;
                btn.Click += EmojiButton_Click;
                pnlEmojiPicker.Controls.Add(btn);
            }
        }
        private void BtnEmoji_Click(object sender, EventArgs e)
        {
            pnlEmojiPicker.Visible = !pnlEmojiPicker.Visible;
            if (pnlEmojiPicker.Visible)
            {
                pnlEmojiPicker.BringToFront();
            }
        }
        private void EmojiButton_Click(object sender, EventArgs e)
        {
            txtMessage.Text += ((Button)sender).Text;
            pnlEmojiPicker.Visible = false;
            txtMessage.Focus();
        }
        #endregion

        #endregion
    }
}