using ChatAppClient.Helpers;
using ChatAppClient.UserControls; // Thêm
using ChatApp.Shared; // Thêm
using System.Drawing;
using System.Windows.Forms;
using System;
using System.IO;
using ChatApp.Shared;
namespace ChatAppClient.UserControls
{
    public partial class ChatViewControl : UserControl
    {
        private string _friendId;
        private string _friendName;
        private Form _parentForm;

        // ID CỦA TÔI - Lấy từ GlobalState hoặc frmLogin
        private string _myId = "user1"; // Tạm thời

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
            // Gán sự kiện
            btnSend.Click += BtnSend_Click;
            btnStartGame.Click += BtnStartGame_Click;
            this.Resize += new System.EventHandler(this.ChatViewControl_Resize);

            // Gán sự kiện cho các nút mới
            btnSendImage.Click += BtnSendImage_Click;
            btnSendFile.Click += BtnSendFile_Click;
            btnEmoji.Click += BtnEmoji_Click;

            // Tải danh sách Emoji
            LoadEmojis();
        }

        #region Logic Gửi Ảnh & File

        // Khi nhấn nút Gửi Hình Ảnh
        private void BtnSendImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SendFile(dialog.FileName, true);
            }
        }

        // Khi nhấn nút Gửi File
        private void BtnSendFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "All Files|*.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SendFile(dialog.FileName, false);
            }
        }

        // Hàm chung để đọc file và gửi
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

                // TODO: Gửi gói tin này qua NetworkManager
                // NetworkManager.Instance.Send(filePacket);

                // Hiển thị ngay cho người gửi (Giả lập)
                ReceiveFileMessage(filePacket, MessageType.Outgoing);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể gửi file: {ex.Message}", "Lỗi");
            }
        }

        #endregion

        #region Logic Nhận Tin Nhắn (Text, Ảnh, File)

        // HÀM MỚI (Public): Được gọi từ NetworkManager/frmHome khi có gói tin File
        public void ReceiveFileMessage(FilePacket packet, MessageType type = MessageType.Incoming)
        {
            // Đảm bảo chạy trên luồng UI
            if (_parentForm != null && _parentForm.InvokeRequired)
            {
                _parentForm.Invoke(new Action(() => AddFileBubble(packet, type)));
            }
            else
            {
                AddFileBubble(packet, type);
            }
        }

        // Hàm này thêm bong bóng File/Ảnh vào UI
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

        // Hàm nhận Text (sửa lại từ code cũ)
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

        // Hàm thêm Text
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

        #region Logic Emoji

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
            // Hiển thị/Ẩn bảng emoji
            pnlEmojiPicker.Visible = !pnlEmojiPicker.Visible;
            if (pnlEmojiPicker.Visible)
            {
                pnlEmojiPicker.BringToFront();
            }
        }

        private void EmojiButton_Click(object sender, EventArgs e)
        {
            txtMessage.Text += ((Button)sender).Text;
            pnlEmojiPicker.Visible = false; // Ẩn đi sau khi chọn
            txtMessage.Focus();
        }

        #endregion

        #region Sự kiện & Hàm Tiện Ích

        private void ChatViewControl_Resize(object sender, EventArgs e)
        {
            int usableWidth = flpMessages.ClientSize.Width - 10;
            if (usableWidth <= 0) return;

            // Cập nhật lề cho TẤT CẢ các loại bong bóng
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

        private void BtnStartGame_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"Đã gửi lời mời chơi Caro đến {_friendName}!", "Thông báo");
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            string message = txtMessage.Text.Trim();
            if (!string.IsNullOrEmpty(message))
            {
                AddMessage(message, MessageType.Outgoing);
                // TODO: Gửi tin nhắn Text qua NetworkManager
                // NetworkManager.Instance.Send(new TextPacket(...));
                txtMessage.Clear();
                txtMessage.Focus();
            }
        }

        private void ScrollToBottom(Control control)
        {
            flpMessages.AutoScrollPosition = new Point(0, flpMessages.VerticalScroll.Maximum);
            flpMessages.ScrollControlIntoView(control);
        }

        #endregion
    }
}