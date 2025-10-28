using ChatApp.Shared; // <-- Phải có
using ChatAppClient.UserControls;
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

        // Control chat đang hiển thị (FIX LỖI CS0103)
        private ChatViewControl _currentChatControl = null;

        // Hàm tạo (Constructor) - Đã sửa
        public frmHome(List<UserStatus> initialUsers)
        {
            InitializeComponent();
            openChatControls = new Dictionary<string, ChatViewControl>();
            openGameForms = new Dictionary<string, frmCaroGame>();
            _initialUsers = initialUsers; // Lưu lại danh sách
        }

        // Khi Form Home được tải
        private void frmHome_Load(object sender, EventArgs e)
        {
            // 1. Đăng ký Form này với NetworkManager
            NetworkManager.Instance.RegisterHomeForm(this);

            // 2. Lấy tên user từ NetworkManager (đã lưu khi login)
            lblWelcome.Text = $"Chào mừng, {NetworkManager.Instance.UserName}!";

            // 3. Tải danh sách bạn bè ban đầu (nhận từ Server)
            LoadInitialFriendList();

            // 4. Ẩn/Hiện text chào mừng
            lblMainWelcome.Visible = true;
        }

        // Tải danh sách bạn bè ban đầu
        private void LoadInitialFriendList()
        {
            if (_initialUsers == null) return;

            foreach (var user in _initialUsers)
            {
                // Hiển thị tất cả user Server gửi về (đang online)
                AddFriendToList(user.UserID, user.UserName,
                    user.IsOnline ? "Online" : "Offline", user.IsOnline);
            }
        }

        // Khi click vào một người bạn
        // Khi click vào một người bạn
        private void FriendItem_Click(FriendListItem item)
        {
            string friendId = item.FriendID;
            string friendName = item.FriendName;

            // TẮT CHẤM ĐỎ (NẾU CÓ)
            item.SetNewMessageAlert(false);

            lblMainWelcome.Visible = false;

            if (_currentChatControl != null && _currentChatControl.Name == friendId)
            {
                return;
            }

            if (_currentChatControl != null)
            {
                _currentChatControl.Visible = false;
            }

            ChatViewControl chatControl;
            if (openChatControls.ContainsKey(friendId))
            {
                // Lấy từ cache
                chatControl = openChatControls[friendId];
                chatControl.Visible = true;
            }
            else
            {
                // Tạo mới
                chatControl = new ChatViewControl(friendId, friendName, this);
                chatControl.Name = friendId;
                chatControl.Dock = DockStyle.Fill;

                openChatControls.Add(friendId, chatControl);
                pnlMain.Controls.Add(chatControl);
            }

            _currentChatControl = chatControl;
            chatControl.BringToFront();
        }

        // Hàm thêm user vào danh sách (Không đổi)
        public void AddFriendToList(string id, string name, string status, bool isOnline)
        {
            FriendListItem item = new FriendListItem();
            item.SetData(id, name, status, isOnline);

            item.Click += (sender, e) => FriendItem_Click(item);
            foreach (Control control in item.Controls)
            {
                control.Click += (sender, e) => FriendItem_Click(item);
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

        // FIX LỖI "CLIENT 1 KHÔNG THẤY CLIENT 2"
        public void HandleUserStatusUpdate(UserStatusPacket packet)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => HandleUserStatusUpdate(packet)));
                return;
            }

            FriendListItem existingItem = null;
            foreach (Control ctrl in flpFriendsList.Controls)
            {
                if (ctrl is FriendListItem item && item.FriendID == packet.UserID)
                {
                    existingItem = item;
                    break;
                }
            }

            if (packet.IsOnline) // Nếu user vừa ONLINE
            {
                if (existingItem == null)
                {
                    // User mới online (Client 2) -> Thêm vào danh sách
                    AddFriendToList(packet.UserID, packet.UserName, "Online", true);
                }
                else
                {
                    // User cũ online trở lại
                    existingItem.SetData(packet.UserID, packet.UserName, "Online", true);
                }
            }
            else // Nếu user vừa OFFLINE
            {
                if (existingItem != null)
                {
                    existingItem.SetData(packet.UserID, packet.UserName, "Offline", false);
                }
            }
        }

        // NHẬN TIN NHẮN TEXT
        // NHẬN TIN NHẮN TEXT
        public void HandleIncomingTextMessage(TextPacket packet)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => HandleIncomingTextMessage(packet)));
                return;
            }

            ChatViewControl chatControl;

            // 1. Kiểm tra xem control chat đã được tạo (cache) chưa
            if (openChatControls.ContainsKey(packet.SenderID))
            {
                chatControl = openChatControls[packet.SenderID];
            }
            else // 2. Nếu CHƯA, hãy TẠO MỚI nó (và ẩn đi)
            {
                // Lấy Tên của người gửi từ danh sách FriendList
                string senderName = packet.SenderID; // Tên mặc định
                foreach (Control ctrl in flpFriendsList.Controls)
                {
                    if (ctrl is FriendListItem item && item.FriendID == packet.SenderID)
                    {
                        senderName = item.FriendName;
                        break;
                    }
                }

                // Tạo control mới (giống hệt như khi click)
                chatControl = new ChatViewControl(packet.SenderID, senderName, this);
                chatControl.Name = packet.SenderID;
                chatControl.Dock = DockStyle.Fill;
                chatControl.Visible = false; // QUAN TRỌNG: Ẩn nó đi

                openChatControls.Add(packet.SenderID, chatControl); // Thêm vào cache
                pnlMain.Controls.Add(chatControl); // Thêm vào panel chính
            }

            // 3. Gửi tin nhắn vào control (dù đang ẩn hay hiện)
            chatControl.ReceiveMessage(packet.MessageContent);

            // 4. Bật chấm đỏ (nếu người dùng đang không xem chat này)
            if (_currentChatControl == null || _currentChatControl.Name != packet.SenderID)
                
    {
                // Tìm FriendListItem và bật chấm đỏ
                foreach (Control ctrl in flpFriendsList.Controls)
                {
                    if (ctrl is FriendListItem item && item.FriendID == packet.SenderID)
                    {
                        item.SetNewMessageAlert(true);
                        break;
                    }
                }
            }
        }

        // NHẬN TIN NHẮN FILE/ẢNH
        public void HandleIncomingFileMessage(FilePacket packet)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => HandleIncomingFileMessage(packet)));
                return;
            }

            if (openChatControls.ContainsKey(packet.SenderID))
            {
                var chatControl = openChatControls[packet.SenderID];
                chatControl.ReceiveFileMessage(packet, MessageType.Incoming);
            }
        }

        // NHẬN LỜI MỜI GAME
        public void HandleIncomingGameInvite(GameInvitePacket invite)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => HandleIncomingGameInvite(invite)));
                return;
            }

            DialogResult result = MessageBox.Show(
                $"{invite.SenderName} muốn thách đấu Cờ Caro với bạn. Bạn có đồng ý?",
                "Lời Mời Chơi Game",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            bool accepted = (result == DialogResult.Yes);

            var response = new GameResponsePacket
            {
                SenderID = NetworkManager.Instance.UserID,
                ReceiverID = invite.SenderID,
                Accepted = accepted
            };

            // GỬI PHẢN HỒI LẠI
            NetworkManager.Instance.SendPacket(response);
        }

        // NHẬN PHẢN HỒI LỜI MỜI (BỊ TỪ CHỐI)
        public void HandleGameResponse(GameResponsePacket packet)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => HandleGameResponse(packet)));
                return;
            }

            if (!packet.Accepted)
            {
                // Tìm control chat của người đã từ chối (SenderID)
                if (openChatControls.ContainsKey(packet.SenderID))
                {
                    var chatControl = openChatControls[packet.SenderID];
                    // Gọi hàm kích hoạt lại nút 🎲
                    chatControl.HandleGameInviteDeclined();
                }
            }
        }

        // NHẬN LỆNH BẮT ĐẦU GAME (TỪ SERVER)
        public void HandleGameStart(GameStartPacket packet)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => HandleGameStart(packet)));
                return;
            }

            // Kích hoạt lại nút 🎲 trên control chat (nếu có)
            if (openChatControls.ContainsKey(packet.OpponentID))
            {
                var chatControl = openChatControls[packet.OpponentID];
                chatControl.HandleGameInviteDeclined(); // Dùng chung hàm này để bật lại nút 🎲
            }

            // Mở Form Game
            frmCaroGame gameForm = new frmCaroGame(
                packet.GameID,
                packet.OpponentID,
                packet.StartsFirst
            );

            openGameForms.Add(packet.GameID, gameForm);
            gameForm.FormClosed += (s, e) => {
                openGameForms.Remove(packet.GameID);
            };

            gameForm.Show();
        }

        // NHẬN NƯỚC ĐI CỦA ĐỐI THỦ
        public void HandleGameMove(GameMovePacket packet)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => HandleGameMove(packet)));
                return;
            }

            if (openGameForms.ContainsKey(packet.GameID))
            {
                var gameForm = openGameForms[packet.GameID];
                gameForm.ReceiveOpponentMove(packet.Row, packet.Col);
            }
        }

        #endregion
    }
}