using ChatApp.Shared;
using ChatAppClient.Forms; // Thường không cần using này ở đây
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
            _myId = NetworkManager.Instance.UserID; // Lấy ID

            // Gán sự kiện
            btnSend.Click += BtnSend_Click;
            btnStartGame.Click += BtnStartGame_Click;
            this.Resize += new System.EventHandler(this.ChatViewControl_Resize);
            btnSendImage.Click += BtnSendImage_Click;
            btnSendFile.Click += BtnSendFile_Click;
            btnEmoji.Click += BtnEmoji_Click;

            LoadEmojis();

            // Tin nhắn mẫu (có thể xóa)
            // AddMessage($"Bắt đầu trò chuyện với {_friendName}", MessageType.Incoming);
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
            var invite = new GameInvitePacket { SenderID = _myId, SenderName = NetworkManager.Instance.UserName, ReceiverID = _friendId };
            NetworkManager.Instance.SendPacket(invite);
            btnStartGame.Enabled = false;
            btnStartGame.Text = "...";
            MessageBox.Show($"Đã gửi lời mời chơi Caro đến {_friendName}!\nĐang chờ phản hồi...", "Thông báo");
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
            if (_parentForm != null && _parentForm.InvokeRequired) _parentForm.Invoke(new Action(() => AddMessage(message, MessageType.Incoming)));
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

        // Hàm được gọi từ frmHome khi bị từ chối game
        public void HandleGameInviteDeclined()
        {
            if (this.InvokeRequired) { this.Invoke(new Action(HandleGameInviteDeclined)); return; }
            MessageBox.Show($"{_friendName} đã từ chối lời mời.", "Tiếc quá!");
            ResetGameButtonInternal(); // Gọi hàm nội bộ
        }

        // Hàm được gọi từ frmHome khi game bắt đầu (chỉ reset nút)
        public void ResetGameButton()
        {
            if (this.InvokeRequired) { this.Invoke(new Action(ResetGameButton)); return; }
            ResetGameButtonInternal(); // Gọi hàm nội bộ
        }

        // Hàm nội bộ để reset nút game (tránh lặp code)
        private void ResetGameButtonInternal()
        {
            btnStartGame.Enabled = true;
            btnStartGame.Text = "🎲";
        }

        private int GetUsableWidth()
        {
            int width = flpMessages.ClientSize.Width - (flpMessages.Padding.Left + flpMessages.Padding.Right);
            // Trừ thêm khoảng cách cho thanh cuộn nếu có
            if (flpMessages.VerticalScroll.Visible)
            {
                width -= SystemInformation.VerticalScrollBarWidth;
            }
            return (width > 0) ? width : this.Width; // Đảm bảo không âm
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
            // Sử dụng BeginInvoke để đảm bảo việc cuộn xảy ra sau khi control được vẽ xong
            this.BeginInvoke((MethodInvoker)delegate {
                flpMessages.ScrollControlIntoView(control);
                // Có thể cần cuộn thêm một chút để đảm bảo thấy rõ bubble cuối cùng
                // flpMessages.AutoScrollPosition = new Point(0, flpMessages.VerticalScroll.Maximum);
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
            txtMessage.AppendText(((Button)sender).Text); // Dùng AppendText tốt hơn
            pnlEmojiPicker.Visible = false;
            txtMessage.Focus();
        }
        #endregion

        #endregion
    }
}