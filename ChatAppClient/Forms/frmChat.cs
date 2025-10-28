using ChatAppClient.Helpers;
using ChatAppClient.UserControls;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChatAppClient.Forms
{
    public partial class frmChat : Form
    {
        private string _friendId;
        private string _friendName;

        public frmChat(string friendId, string friendName)
        {
            InitializeComponent();
            _friendId = friendId;
            _friendName = friendName;

            lblFriendName.Text = _friendName;
            this.Text = $"Chat với {_friendName}";
        }

        private void frmChat_Load(object sender, EventArgs e)
        {
            btnSend.Click += BtnSend_Click;
            btnStartGame.Click += BtnStartGame_Click;

            // **THÊM MỚI:** Gán sự kiện Resize
            this.Resize += new System.EventHandler(this.frmChat_Resize);

            // Tin nhắn mẫu
            AddMessage("Chào cậu!", MessageType.Incoming);
            AddMessage("Chào, khỏe không?", MessageType.Outgoing);
        }

        // **HÀM MỚI:** Xử lý khi Form bị thay đổi kích thước
        private void frmChat_Resize(object sender, EventArgs e)
        {
            // Khi Form resize, ta phải tính toán lại lề cho TẤT CẢ bubble

            // Chiều rộng khả dụng = Chiều rộng bên trong Panel - 10px (Padding 5 trái + 5 phải)
            int usableWidth = flpMessages.ClientSize.Width - 10;
            if (usableWidth <= 0) return;

            foreach (Control control in flpMessages.Controls)
            {
                if (control is ChatMessageBubble bubble)
                {
                    // Gọi hàm UpdateMargins của bubble
                    bubble.UpdateMargins(usableWidth);
                }
            }
        }

        private void BtnStartGame_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"Đã gửi lời mời chơi Caro đến {_friendName}!", "Thông báo");
            // ... (Code mời chơi game của bạn)
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            string message = txtMessage.Text.Trim();
            if (!string.IsNullOrEmpty(message))
            {
                AddMessage(message, MessageType.Outgoing);
                // ... (Code gửi tin nhắn của bạn)
                txtMessage.Clear();
                txtMessage.Focus();
            }
        }

        // **SỬA LẠI** hàm AddMessage
        public void AddMessage(string message, MessageType type)
        {
            ChatMessageBubble bubble = new ChatMessageBubble();

            // Chiều rộng khả dụng = Chiều rộng bên trong Panel - 10px (Padding 5 trái + 5 phải)
            int usableWidth = flpMessages.ClientSize.Width - 10;
            if (usableWidth <= 0) usableWidth = this.Width; // Fix lỗi khi form thu nhỏ

            // Truyền chiều rộng vào cho bubble
            bubble.SetMessage(message, type, usableWidth);

            flpMessages.Controls.Add(bubble);

            // Cuộn xuống dưới cùng
            flpMessages.AutoScrollPosition = new Point(0, flpMessages.VerticalScroll.Maximum);
            flpMessages.ScrollControlIntoView(bubble);
        }

        // Hàm ReceiveMessage (Giữ nguyên)
        public void ReceiveMessage(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => AddMessage(message, MessageType.Incoming)));
            }
            else
            {
                AddMessage(message, MessageType.Incoming);
            }
        }
    }
}