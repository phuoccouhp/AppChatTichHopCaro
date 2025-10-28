using ChatApp.Shared; // <-- Phải có
using ChatAppClient.UserControls; // <-- Phải có
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ChatAppClient.Forms
{
    public partial class frmHome : Form
    {
        // Quản lý các control chat đang mở (Key = FriendID)
        private Dictionary<string, ChatViewControl> openChatControls;
        // Quản lý các form game đang mở (Key = GameID)
        private Dictionary<string, frmCaroGame> openGameForms;

        // Danh sách bạn bè nhận được khi login
        private List<UserStatus> _initialUsers;

        // Control chat đang hiển thị
        private ChatViewControl _currentChatControl = null;

        // Hàm tạo (Constructor)
        public frmHome(List<UserStatus> initialUsers)
        {
            InitializeComponent();
            openChatControls = new Dictionary<string, ChatViewControl>();
            openGameForms = new Dictionary<string, frmCaroGame>();
            _initialUsers = initialUsers;
        }

        // Khi Form Home được tải
        private void frmHome_Load(object sender, EventArgs e)
        {
            NetworkManager.Instance.RegisterHomeForm(this); // Đăng ký
            lblWelcome.Text = $"Chào mừng, {NetworkManager.Instance.UserName}!"; // Cập nhật tên
            LoadInitialFriendList(); // Tải bạn bè
            lblMainWelcome.Visible = true; // Hiện lời chào
        }

        // Tải danh sách bạn bè ban đầu
        private void LoadInitialFriendList()
        {
            flpFriendsList.SuspendLayout(); // Tạm dừng layout để thêm nhanh hơn
            if (_initialUsers != null)
            {
                foreach (var user in _initialUsers)
                {
                    AddFriendToList(user.UserID, user.UserName, user.IsOnline ? "Online" : "Offline", user.IsOnline);
                }
            }
            flpFriendsList.ResumeLayout(); // Bật lại layout
        }

        // Khi click vào một người bạn
        private void FriendItem_Click(FriendListItem item)
        {
            string friendId = item.FriendID;
            string friendName = item.FriendName;

            item.SetNewMessageAlert(false); // TẮT CHẤM ĐỎ

            lblMainWelcome.Visible = false; // Ẩn lời chào

            // Nếu đang xem chat này rồi thì không làm gì
            if (_currentChatControl != null && _currentChatControl.Name == friendId) return;

            // Ẩn chat cũ (nếu có)
            if (_currentChatControl != null) _currentChatControl.Visible = false;

            ChatViewControl chatControl;
            // Tìm trong cache hoặc tạo mới
            if (!openChatControls.TryGetValue(friendId, out chatControl))
            {
                chatControl = new ChatViewControl(friendId, friendName, this) { Name = friendId, Dock = DockStyle.Fill };
                openChatControls.Add(friendId, chatControl);
                pnlMain.Controls.Add(chatControl);
            }

            // Hiển thị chat mới
            chatControl.Visible = true;
            chatControl.BringToFront();
            _currentChatControl = chatControl;
        }

        // Hàm thêm user vào danh sách (Không đổi)
        public void AddFriendToList(string id, string name, string status, bool isOnline)
        {
            FriendListItem item = new FriendListItem();
            item.SetData(id, name, status, isOnline);

            // Gán sự kiện click cho cả item và các control con của nó
            Action<object, EventArgs> clickAction = (sender, e) => FriendItem_Click(item);
            item.Click += new EventHandler(clickAction);
            foreach (Control control in item.Controls)
            {
                control.Click += new EventHandler(clickAction);
            }

            flpFriendsList.Controls.Add(item);
        }

        // Khi đóng Form Home
        private void frmHome_FormClosing(object sender, FormClosingEventArgs e)
        {
            // TODO: Gửi tin "Disconnect"
            Application.Exit();
        }

        #region == BỘ ĐIỀU PHỐI GÓI TIN (GỌI TỪ NETWORKMANAGER) ==

        // Xử lý khi có ai đó Online/Offline
        public void HandleUserStatusUpdate(UserStatusPacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleUserStatusUpdate(packet))); return; }

            FriendListItem existingItem = null;
            foreach (Control ctrl in flpFriendsList.Controls)
            {
                if (ctrl is FriendListItem item && item.FriendID == packet.UserID)
                {
                    existingItem = item;
                    break;
                }
            }

            if (packet.IsOnline)
            {
                if (existingItem == null) AddFriendToList(packet.UserID, packet.UserName, "Online", true);
                else existingItem.SetData(packet.UserID, packet.UserName, "Online", true);
            }
            else
            {
                if (existingItem != null) existingItem.SetData(packet.UserID, packet.UserName, "Offline", false);
            }
        }

        // Xử lý khi nhận tin nhắn TEXT
        public void HandleIncomingTextMessage(TextPacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleIncomingTextMessage(packet))); return; }

            ChatViewControl chatControl;
            // Tìm hoặc Tạo control chat
            if (!openChatControls.TryGetValue(packet.SenderID, out chatControl))
            {
                string senderName = packet.SenderID; // Tên mặc định
                FriendListItem friendItem = null;
                foreach (Control ctrl in flpFriendsList.Controls)
                {
                    if (ctrl is FriendListItem item && item.FriendID == packet.SenderID)
                    {
                        senderName = item.FriendName;
                        friendItem = item; // Lưu lại để bật chấm đỏ
                        break;
                    }
                }
                chatControl = new ChatViewControl(packet.SenderID, senderName, this) { Name = packet.SenderID, Dock = DockStyle.Fill, Visible = false };
                openChatControls.Add(packet.SenderID, chatControl);
                pnlMain.Controls.Add(chatControl);
            }

            // Thêm tin nhắn vào control (dù ẩn hay hiện)
            chatControl.ReceiveMessage(packet.MessageContent);

            // Bật chấm đỏ nếu đang không xem
            if (_currentChatControl == null || _currentChatControl.Name != packet.SenderID)
            {
                // Tìm lại FriendListItem nếu cần
                FriendListItem friendItemToAlert = null;
                if (chatControl.IsHandleCreated) // Kiểm tra xem control đã được tạo chưa
                {
                    foreach (Control ctrl in flpFriendsList.Controls)
                    {
                        if (ctrl is FriendListItem item && item.FriendID == packet.SenderID)
                        {
                            friendItemToAlert = item;
                            break;
                        }
                    }
                }
                friendItemToAlert?.SetNewMessageAlert(true); // Bật chấm đỏ
            }
        }

        // Xử lý khi nhận tin nhắn FILE/ẢNH
        public void HandleIncomingFileMessage(FilePacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleIncomingFileMessage(packet))); return; }

            ChatViewControl chatControl;
            // Tìm hoặc Tạo control chat (Tương tự như Text)
            if (!openChatControls.TryGetValue(packet.SenderID, out chatControl))
            {
                string senderName = packet.SenderID;
                FriendListItem friendItem = null;
                foreach (Control ctrl in flpFriendsList.Controls) { if (ctrl is FriendListItem item && item.FriendID == packet.SenderID) { senderName = item.FriendName; friendItem = item; break; } }
                chatControl = new ChatViewControl(packet.SenderID, senderName, this) { Name = packet.SenderID, Dock = DockStyle.Fill, Visible = false };
                openChatControls.Add(packet.SenderID, chatControl); pnlMain.Controls.Add(chatControl);
            }

            // Gửi file vào control
            chatControl.ReceiveFileMessage(packet, MessageType.Incoming);

            // Bật chấm đỏ nếu cần
            if (_currentChatControl == null || _currentChatControl.Name != packet.SenderID)
            {
                FriendListItem friendItemToAlert = null;
                if (chatControl.IsHandleCreated) { foreach (Control ctrl in flpFriendsList.Controls) { if (ctrl is FriendListItem item && item.FriendID == packet.SenderID) { friendItemToAlert = item; break; } } }
                friendItemToAlert?.SetNewMessageAlert(true);
            }
        }

        // Xử lý khi nhận lời mời GAME
        public void HandleIncomingGameInvite(GameInvitePacket invite)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleIncomingGameInvite(invite))); return; }

            DialogResult result = MessageBox.Show($"{invite.SenderName} muốn thách đấu Cờ Caro. Đồng ý?", "Lời Mời Chơi Game", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            bool accepted = (result == DialogResult.Yes);
            var response = new GameResponsePacket { SenderID = NetworkManager.Instance.UserID, ReceiverID = invite.SenderID, Accepted = accepted };
            NetworkManager.Instance.SendPacket(response); // Gửi phản hồi
        }

        // Xử lý khi nhận phản hồi lời mời GAME (Bị từ chối)
        public void HandleGameResponse(GameResponsePacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleGameResponse(packet))); return; }

            if (!packet.Accepted)
            {
                // Tìm control chat của người từ chối (SenderID)
                if (openChatControls.TryGetValue(packet.SenderID, out var chatControl))
                {
                    chatControl.HandleGameInviteDeclined(); // Gọi hàm của control đó
                }
            }
        }

        // Xử lý khi nhận lệnh BẮT ĐẦU GAME
        public void HandleGameStart(GameStartPacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleGameStart(packet))); return; }

            // Reset nút 🎲 trên control chat (nếu có)
            if (openChatControls.TryGetValue(packet.OpponentID, out var chatControl))
            {
                chatControl.ResetGameButton(); // *** ĐÃ SỬA ***
            }

            // Mở Form Game
            frmCaroGame gameForm = new frmCaroGame(packet.GameID, packet.OpponentID, packet.StartsFirst);
            openGameForms.Add(packet.GameID, gameForm); // Quản lý form
            gameForm.FormClosed += (s, e) => { openGameForms.Remove(packet.GameID); }; // Xóa khi đóng
            gameForm.Show();
        }

        // Xử lý khi nhận NƯỚC ĐI của đối thủ
        public void HandleGameMove(GameMovePacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleGameMove(packet))); return; }

            // Tìm form game đang mở và gửi nước đi vào
            if (openGameForms.TryGetValue(packet.GameID, out var gameForm))
            {
                gameForm.ReceiveOpponentMove(packet.Row, packet.Col);
            }
        }

        #endregion
    }
}