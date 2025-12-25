using ChatApp.Shared;
using ChatAppClient.UserControls;
using System;
using System.Collections.Generic;
using System.Drawing; // Thêm
using System.Windows.Forms;

namespace ChatAppClient.Forms
{
    public partial class frmHome : Form
    {
        private Dictionary<string, ChatViewControl> openChatControls;
        private Dictionary<string, frmCaroGame> openGameForms;
        private Dictionary<string, frmTankGame> openTankGameForms;
        private List<UserStatus> _initialUsers;
        private ChatViewControl? _currentChatControl = null;

        public frmHome(List<UserStatus> initialUsers)
        {
            InitializeComponent();
            openChatControls = new Dictionary<string, ChatViewControl>();
            openGameForms = new Dictionary<string, frmCaroGame>();
            openTankGameForms = new Dictionary<string, frmTankGame>();
            _initialUsers = initialUsers;
            btnSettings.MouseEnter += (s, e) =>
            {
                btnSettings.BackColor = Color.FromArgb(80, 83, 95); // Màu xám sáng
            };
            btnSettings.MouseLeave += (s, e) =>
            {
                btnSettings.BackColor = Color.FromArgb(55, 58, 70); // Màu xám tối ban đầu
            };
            Color separatorColor = Color.FromArgb(32, 34, 37);

            // 1. Tạo các đường kẻ (Nếu chưa có)
            // Kẻ dưới Header chính
            Panel lineHeader = new Panel { Height = 2, Dock = DockStyle.Bottom, BackColor = separatorColor };
            pnlHeader.Controls.Add(lineHeader);

            // Kẻ dọc ngăn cách Sidebar
            Panel lineVertical = new Panel { Width = 2, Dock = DockStyle.Right, BackColor = separatorColor };
            pnlSidebar.Controls.Add(lineVertical);

            // Kẻ ngăn cách Tiêu đề và Tìm kiếm
            Panel lineSidebarTitle = new Panel { Height = 2, Dock = DockStyle.Top, BackColor = separatorColor };

            // 2. [QUAN TRỌNG] XÓA HẾT VÀ ADD LẠI THEO ĐÚNG THỨ TỰ MONG MUỐN
            // Để đảm bảo không bao giờ bị nhảy lung tung, ta gỡ hết ra và gắn lại.

            pnlSidebar.Controls.Clear(); // Xóa sạch Sidebar tạm thời

            // THỨ TỰ ADD: Cái nào nằm DƯỚI CÙNG thì Add TRƯỚC (Quy tắc Dock)

            // B1: Add Kẻ dọc (Nằm bên phải cùng)
            pnlSidebar.Controls.Add(lineVertical);

            // B2: Add Danh sách bạn bè (Fill - Lấp đầy khoảng trống còn lại)
            pnlSidebar.Controls.Add(flpFriendsList);

            // B3: Add Thanh tìm kiếm (Dock Top - Nằm ngay trên danh sách)
            if (pnlSearchBox != null)
            {
                pnlSearchBox.Dock = DockStyle.Top; // Đảm bảo Dock đúng
                pnlSidebar.Controls.Add(pnlSearchBox);
            }

            // B4: Add Đường kẻ ngang (Dock Top - Nằm trên thanh tìm kiếm)
            pnlSidebar.Controls.Add(lineSidebarTitle);

            // B5: Add Tiêu đề (Dock Top - Nằm trên cùng, đỉnh chóp)
            // Cần chỉnh lại Title một chút để không bị co cụm
            lblFriendsTitle.AutoSize = false; // Tắt tự co giãn để chiếm hết chiều ngang
            lblFriendsTitle.Height = 50;      // Chiều cao cố định
            lblFriendsTitle.Dock = DockStyle.Top;
            lblFriendsTitle.TextAlign = ContentAlignment.MiddleLeft; // Căn giữa dọc, trái ngang
            pnlSidebar.Controls.Add(lblFriendsTitle);

            // ✅ Thêm nút Tạo Nhóm
            Button btnCreateGroup = new Button();
            btnCreateGroup.Text = "+ Nhóm";
            btnCreateGroup.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnCreateGroup.Size = new Size(70, 30);
            btnCreateGroup.Location = new Point(130, 10);
            btnCreateGroup.BackColor = Color.FromArgb(88, 101, 242);
            btnCreateGroup.ForeColor = Color.White;
            btnCreateGroup.FlatStyle = FlatStyle.Flat;
            btnCreateGroup.FlatAppearance.BorderSize = 0;
            btnCreateGroup.Cursor = Cursors.Hand;
            btnCreateGroup.Click += (s, e) => CreateNewGroup();
            lblFriendsTitle.Controls.Add(btnCreateGroup);

            // ✅ Thêm nút Tank Multiplayer
            Button btnTankMP = new Button();
            btnTankMP.Text = "Tank MP";
            btnTankMP.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnTankMP.Size = new Size(70, 30);
            btnTankMP.Location = new Point(205, 10);
            btnTankMP.BackColor = Color.FromArgb(67, 181, 129);
            btnTankMP.ForeColor = Color.White;
            btnTankMP.FlatStyle = FlatStyle.Flat;
            btnTankMP.FlatAppearance.BorderSize = 0;
            btnTankMP.Cursor = Cursors.Hand;
            btnTankMP.Click += (s, e) => OpenTankMultiplayerLobby();
            lblFriendsTitle.Controls.Add(btnTankMP);
        }

        private void frmHome_Load(object sender, EventArgs e)
        {
            NetworkManager.Instance.RegisterHomeForm(this);
            string? userName = NetworkManager.Instance.UserName ?? "User";
            lblWelcome.Text = $"Chào mừng, {userName}!";
            LoadInitialFriendList();
            lblMainWelcome.Visible = true;
            LoadMyAvatar();
        }
        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim().ToLower();

            // Nếu đang là chữ "Tìm kiếm..." (placeholder) thì coi như rỗng
            if (keyword == "tìm kiếm...") keyword = "";

            // Tạm dừng vẽ để không bị nháy hình
            flpFriendsList.SuspendLayout();

            foreach (Control c in flpFriendsList.Controls)
            {
                if (c is ChatAppClient.UserControls.FriendListItem item)
                {
                    // Nếu ô tìm kiếm trống -> Hiện tất cả
                    if (string.IsNullOrEmpty(keyword))
                    {
                        item.Visible = true;
                    }
                    else
                    {
                        // Kiểm tra xem Tên bạn bè có chứa từ khóa không (bỏ qua hoa thường)
                        bool isMatch = item.FriendName.ToLower().Contains(keyword);
                        item.Visible = isMatch;
                    }
                }
            }

            // Vẽ lại danh sách
            flpFriendsList.ResumeLayout();
        }

        // 2. Sự kiện khi bấm vào ô tìm kiếm (Xóa chữ "Tìm kiếm...")
        private void txtSearch_Enter(object sender, EventArgs e)
        {
            if (txtSearch.Text == "Tìm kiếm...")
            {
                txtSearch.Text = "";
                txtSearch.ForeColor = Color.White; // Chữ khi gõ màu trắng
            }
        }

        // 3. Sự kiện khi rời khỏi ô tìm kiếm (Nếu trống thì hiện lại chữ "Tìm kiếm...")
        private void txtSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = "Tìm kiếm...";
                txtSearch.ForeColor = Color.Gray; // Chữ placeholder màu xám
            }
        }
        private void LoadMyAvatar()
        {
            // [TEST] Đặt màu đỏ để biết chắc chắn PictureBox đang nằm ở đó
            pbUserAvatar.BackColor = Color.White;

            string? myId = NetworkManager.Instance.UserID;
            if (string.IsNullOrEmpty(myId)) return;
            string imagePath = System.IO.Path.Combine("Images", $"{myId}.png");

            // ... (code load ảnh cũ của bạn) ...
            if (System.IO.File.Exists(imagePath))
            {
                using (var bmp = new Bitmap(imagePath))
                {
                    pbUserAvatar.Image = new Bitmap(bmp);
                    pbUserAvatar.BackColor = Color.Transparent; // Nếu có ảnh thì bỏ màu đỏ đi
                }
            }

            // Bo tròn
            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
            gp.AddEllipse(0, 0, pbUserAvatar.Width, pbUserAvatar.Height);
            pbUserAvatar.Region = new Region(gp);
        }
        private void LoadInitialFriendList()
        {
            if (_initialUsers == null) return;
            foreach (var user in _initialUsers)
            {
                AddFriendToList(user.UserID, user.UserName,
                    user.IsOnline ? "Online" : "Offline", user.IsOnline);
            }
        }

        private void FriendItem_Click(FriendListItem item)
        {
            // Kiểm tra xem đã login chưa (UserID phải được set)
            if (string.IsNullOrEmpty(NetworkManager.Instance.UserID))
            {
                MessageBox.Show("Vui lòng đợi quá trình đăng nhập hoàn tất.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string friendId = item.FriendID;
            string friendName = item.FriendName;
            item.SetNewMessageAlert(false);
            lblMainWelcome.Visible = false;

            if (_currentChatControl != null && _currentChatControl.Name == friendId) return;
            if (_currentChatControl != null) _currentChatControl.Visible = false;

            ChatViewControl chatControl;
            if (openChatControls.ContainsKey(friendId))
            {
                chatControl = openChatControls[friendId];
                chatControl.Visible = true;
            }
            else
            {
                try
                {
                    chatControl = new ChatViewControl(friendId, friendName, this);
                    chatControl.Name = friendId;
                    chatControl.Dock = DockStyle.Fill;
                    openChatControls.Add(friendId, chatControl);
                    pnlMain.Controls.Add(chatControl);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi tạo cửa sổ chat: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            _currentChatControl = chatControl;
            chatControl.BringToFront();
        }

        public void AddFriendToList(string id, string name, string status, bool isOnline)
        {
            FriendListItem item = new FriendListItem();
            item.SetData(id, name, status, isOnline);

            item.Click += (sender, e) => FriendItem_Click(item);
            foreach (Control control in item.Controls) control.Click += (sender, e) => FriendItem_Click(item);
            flpFriendsList.Controls.Add(item);
        }

        public List<(string id, string name)> GetFriendsList()
        {
            var friends = new List<(string id, string name)>();
            foreach (Control ctrl in flpFriendsList.Controls)
            {
                if (ctrl is FriendListItem item)
                {
                    friends.Add((item.FriendID, item.FriendName));
                }
            }
            return friends;
        }

        private void frmHome_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        // --- SỰ KIỆN NÚT SETTING ---
        private void btnSettings_Click(object sender, EventArgs e)
        {
            frmSettings settings = new frmSettings();
            settings.ShowDialog();
        }

        #region == XỬ LÝ GÓI TIN ==

        // --- HÀM MỚI: XỬ LÝ CẬP NHẬT PROFILE ---
        public void HandleUpdateProfile(UpdateProfilePacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleUpdateProfile(packet))); return; }

            // 1. Lưu ảnh mới
            if (packet.HasNewAvatar && packet.NewAvatarData != null)
            {
                try
                {
                    if (!System.IO.Directory.Exists("Images")) System.IO.Directory.CreateDirectory("Images");
                    string imagePath = System.IO.Path.Combine("Images", $"{packet.UserID}.png");
                    // Xóa file cũ (nếu có) để tránh lỗi
                    if (System.IO.File.Exists(imagePath))
                    {
                        try { System.IO.File.Delete(imagePath); } catch { }
                    }
                    System.IO.File.WriteAllBytes(imagePath, packet.NewAvatarData);
                }
                catch (Exception ex) { Console.WriteLine("Lỗi lưu avatar: " + ex.Message); }
            }

            // 2. Cập nhật danh sách bạn bè
            foreach (Control ctrl in flpFriendsList.Controls)
            {
                if (ctrl is FriendListItem item && item.FriendID == packet.UserID)
                {
                    string status = item.FriendStatus;
                    bool isOnline = (item.FriendStatus == "Online");
                    item.SetData(packet.UserID, packet.NewDisplayName, status, isOnline);
                    break;
                }
            }

            // 3. Cập nhật bản thân
            if (packet.UserID == NetworkManager.Instance.UserID)
            {
                NetworkManager.Instance.SetUserCredentials(packet.UserID, packet.NewDisplayName);
                lblWelcome.Text = $"Chào mừng, {packet.NewDisplayName}!";
                LoadMyAvatar();
            }
        }

        public void HandleUserStatusUpdate(UserStatusPacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleUserStatusUpdate(packet))); return; }

            FriendListItem existingItem = null;
            foreach (Control ctrl in flpFriendsList.Controls)
            {
                if (ctrl is FriendListItem item && item.FriendID == packet.UserID)
                {
                    existingItem = item; break;
                }
            }

            if (packet.IsOnline)
            {
                if (existingItem == null) AddFriendToList(packet.UserID, packet.UserName, "Online", true);
                else existingItem.SetData(packet.UserID, packet.UserName, "Online", true);
            }
            else if (existingItem != null)
            {
                existingItem.SetData(packet.UserID, packet.UserName, "Offline", false);
            }
        }

        public void HandleIncomingTextMessage(TextPacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleIncomingTextMessage(packet))); return; }

            ChatViewControl chatControl;
            string senderName = packet.SenderID;
            
            if (!openChatControls.TryGetValue(packet.SenderID, out chatControl))
            {
                FriendListItem friendItem = null;
                foreach (Control ctrl in flpFriendsList.Controls) { if (ctrl is FriendListItem item && item.FriendID == packet.SenderID) { senderName = item.FriendName; friendItem = item; break; } }

                chatControl = new ChatViewControl(packet.SenderID, senderName, this) { Name = packet.SenderID, Dock = DockStyle.Fill, Visible = false };
                openChatControls.Add(packet.SenderID, chatControl);
                pnlMain.Controls.Add(chatControl);
            }
            else
            {
                // Get sender name from friend list
                foreach (Control ctrl in flpFriendsList.Controls) { if (ctrl is FriendListItem item && item.FriendID == packet.SenderID) { senderName = item.FriendName; break; } }
            }
            
            // ✅ [FIX] Đảm bảo MessageContent không null
            string messageContent = packet.MessageContent ?? "";
            if (!string.IsNullOrWhiteSpace(messageContent))
            {
                chatControl.ReceiveMessage(messageContent);
            }

            // ✅ THÔNG BÁO TIN NHẮN ĐẾN nếu không đang xem chat này
            if (_currentChatControl == null || _currentChatControl.Name != packet.SenderID)
            {
                FriendListItem itemToAlert = null;
                foreach (Control ctrl in flpFriendsList.Controls) { if (ctrl is FriendListItem item && item.FriendID == packet.SenderID) { itemToAlert = item; break; } }
                itemToAlert?.SetNewMessageAlert(true);
                
                // ✅ Hiện thông báo Toast/Popup
                ShowMessageNotification(senderName, packet.MessageContent);
            }
        }

        // ✅ Hiện thông báo tin nhắn mới với Toast popup
        private void ShowMessageNotification(string senderName, string message)
        {
            try
            {
                // Truncate message if too long
                string displayMessage = message?.Length > 50 ? message.Substring(0, 47) + "..." : (message ?? "");
                
                // Play sound
                System.Media.SystemSounds.Asterisk.Play();
                
                // Flash taskbar
                FlashWindow(this.Handle, true);
                
                // ✅ Hiện Toast notification popup
                ShowToastNotification(senderName, displayMessage);
            }
            catch { }
        }
        
        // ✅ Toast notification popup
        private Form _toastForm;
        private System.Windows.Forms.Timer _toastTimer;
        
        private void ShowToastNotification(string title, string message)
        {
            // Đóng toast cũ nếu có
            if (_toastForm != null && !_toastForm.IsDisposed)
            {
                _toastForm.Close();
            }
            
            _toastForm = new Form();
            _toastForm.FormBorderStyle = FormBorderStyle.None;
            _toastForm.StartPosition = FormStartPosition.Manual;
            _toastForm.ShowInTaskbar = false;
            _toastForm.TopMost = true;
            _toastForm.BackColor = Color.FromArgb(45, 48, 60);
            _toastForm.Size = new Size(300, 80);
            
            // Vị trí góc dưới phải màn hình
            var screen = Screen.PrimaryScreen.WorkingArea;
            _toastForm.Location = new Point(screen.Right - _toastForm.Width - 20, screen.Bottom - _toastForm.Height - 20);
            
            // Border bo tròn
            var path = Helpers.DrawingHelper.CreateRoundedRectPath(new Rectangle(0, 0, _toastForm.Width, _toastForm.Height), 10);
            _toastForm.Region = new Region(path);
            
            // Icon tin nhắn
            Label lblIcon = new Label();
            lblIcon.Text = "💬";
            lblIcon.Font = new Font("Segoe UI Emoji", 20);
            lblIcon.AutoSize = true;
            lblIcon.Location = new Point(10, 15);
            _toastForm.Controls.Add(lblIcon);
            
            // Tên người gửi
            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.Font = new Font("Segoe UI Semibold", 11);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(55, 10);
            _toastForm.Controls.Add(lblTitle);
            
            // Nội dung tin nhắn
            Label lblMessage = new Label();
            lblMessage.Text = message;
            lblMessage.Font = new Font("Segoe UI", 9);
            lblMessage.ForeColor = Color.LightGray;
            lblMessage.AutoSize = false;
            lblMessage.Size = new Size(230, 35);
            lblMessage.Location = new Point(55, 35);
            _toastForm.Controls.Add(lblMessage);
            
            // Click để đóng
            _toastForm.Click += (s, e) => _toastForm.Close();
            foreach (Control ctrl in _toastForm.Controls)
            {
                ctrl.Click += (s, e) => _toastForm.Close();
            }
            
            _toastForm.Show();
            
            // Tự đóng sau 4 giây
            _toastTimer = new System.Windows.Forms.Timer();
            _toastTimer.Interval = 4000;
            _toastTimer.Tick += (s, e) =>
            {
                _toastTimer.Stop();
                if (_toastForm != null && !_toastForm.IsDisposed)
                    _toastForm.Close();
            };
            _toastTimer.Start();
        }
        
        // Win32 API để flash taskbar
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool FlashWindow(IntPtr hwnd, bool bInvert);

        public void HandleIncomingFileMessage(FilePacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleIncomingFileMessage(packet))); return; }

            ChatViewControl chatControl;
            if (!openChatControls.TryGetValue(packet.SenderID, out chatControl))
            {
                string senderName = packet.SenderID;
                foreach (Control ctrl in flpFriendsList.Controls) { if (ctrl is FriendListItem item && item.FriendID == packet.SenderID) { senderName = item.FriendName; break; } }
                chatControl = new ChatViewControl(packet.SenderID, senderName, this) { Name = packet.SenderID, Dock = DockStyle.Fill, Visible = false };
                openChatControls.Add(packet.SenderID, chatControl);
                pnlMain.Controls.Add(chatControl);
            }
            chatControl.ReceiveFileMessage(packet, MessageType.Incoming);

            if (_currentChatControl == null || _currentChatControl.Name != packet.SenderID)
            {
                FriendListItem itemToAlert = null;
                foreach (Control ctrl in flpFriendsList.Controls) { if (ctrl is FriendListItem item && item.FriendID == packet.SenderID) { itemToAlert = item; break; } }
                itemToAlert?.SetNewMessageAlert(true);
            }
        }

        public void HandleIncomingGameInvite(GameInvitePacket invite)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleIncomingGameInvite(invite))); return; }

            // Hiển thị game invite như tin nhắn trong chat
            ChatViewControl chatControl;
            if (!openChatControls.TryGetValue(invite.SenderID, out chatControl))
            {
                string senderName = invite.SenderName;
                FriendListItem friendItem = null;
                foreach (Control ctrl in flpFriendsList.Controls) 
                { 
                    if (ctrl is FriendListItem item && item.FriendID == invite.SenderID) 
                    { 
                        senderName = item.FriendName; 
                        friendItem = item; 
                        break; 
                    } 
                }

                chatControl = new ChatViewControl(invite.SenderID, senderName, this) 
                { 
                    Name = invite.SenderID, 
                    Dock = DockStyle.Fill, 
                    Visible = false 
                };
                openChatControls.Add(invite.SenderID, chatControl);
                pnlMain.Controls.Add(chatControl);
            }
            chatControl.ReceiveGameInvite(invite, GameType.Caro, MessageType.Incoming);

            if (_currentChatControl == null || _currentChatControl.Name != invite.SenderID)
            {
                FriendListItem itemToAlert = null;
                foreach (Control ctrl in flpFriendsList.Controls) 
                { 
                    if (ctrl is FriendListItem item && item.FriendID == invite.SenderID) 
                    { 
                        itemToAlert = item; 
                        break; 
                    } 
                }
                itemToAlert?.SetNewMessageAlert(true);
            }
        }

        public void HandleGameResponse(GameResponsePacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleGameResponse(packet))); return; }

            // packet.SenderID là người phản hồi (invitee), packet.ReceiverID là người đã gửi lời mời (inviter)
            // Tại client của inviter, cần tìm chat control theo ID của người trả lời (senderID) vì chat controls được key bởi friend ID
            string chatKey = packet.SenderID; // responder ID
            if (openChatControls.TryGetValue(chatKey, out var chatControl))
            {
                // Cập nhật game invite bubble trong chat
                chatControl.UpdateGameInviteStatus(packet.SenderID, packet.Accepted, GameType.Caro);

                if (!packet.Accepted)
                {
                    chatControl.HandleGameInviteDeclined();
                }
                else
                {
                    // Nếu chấp nhận, reset button để chờ GameStartPacket
                    chatControl.ResetGameButton();
                }
            }
            else
            {
                // Nếu chat control chưa mở (không có), vẫn có thể notify bằng MessageBox
                if (!packet.Accepted)
                {
                    MessageBox.Show($"Lời mời của bạn đã bị từ chối bởi {packet.SenderID}.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        public void HandleGameStart(GameStartPacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleGameStart(packet))); return; }
            if (openChatControls.TryGetValue(packet.OpponentID, out var chatControl)) chatControl.ResetGameButton();

            frmCaroGame gameForm = new frmCaroGame(packet.GameID, packet.OpponentID, packet.StartsFirst);
            openGameForms.Add(packet.GameID, gameForm);
            gameForm.FormClosed += (s, e) => { openGameForms.Remove(packet.GameID); };
            gameForm.Show();
        }

        public void HandleGameMove(GameMovePacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleGameMove(packet))); return; }
            if (openGameForms.TryGetValue(packet.GameID, out var gameForm)) gameForm.ReceiveOpponentMove(packet.Row, packet.Col);
        }

        // --- XỬ LÝ CHƠI LẠI (REMATCH) ---
        public void HandleRematchRequest(RematchRequestPacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleRematchRequest(packet))); return; }
            
            // Kiểm tra xem là Caro Game hay Tank Game
            if (openGameForms.TryGetValue(packet.GameID, out var gameForm))
            {
                gameForm.HandleRematchRequest(packet);
            }
            else if (openTankGameForms.TryGetValue(packet.GameID, out var tankGameForm))
            {
                tankGameForm.HandleRematchRequest(packet);
            }
        }

        public void HandleRematchResponse(RematchResponsePacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleRematchResponse(packet))); return; }
            
            // Kiểm tra xem là Caro Game hay Tank Game
            if (openGameForms.TryGetValue(packet.GameID, out var gameForm))
            {
                gameForm.HandleRematchResponse(packet);
            }
            else if (openTankGameForms.TryGetValue(packet.GameID, out var tankGameForm))
            {
                tankGameForm.HandleRematchResponse(packet);
            }
        }

        public void HandleGameReset(GameResetPacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleGameReset(packet))); return; }
            if (openGameForms.TryGetValue(packet.GameID, out var gameForm)) gameForm.HandleGameReset(packet);
        }
        
        // Xử lý Tank Game Reset (dùng TankStartPacket để reset)
        public void HandleTankGameReset(TankStartPacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleTankGameReset(packet))); return; }
            if (string.IsNullOrEmpty(packet.GameID)) return;
            if (openTankGameForms.TryGetValue(packet.GameID, out var tankGameForm))
            {
                tankGameForm.HandleTankGameReset(packet);
            }
        }

        // --- XỬ LÝ TANK GAME ---
        public void HandleTankInvite(TankInvitePacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleTankInvite(packet))); return; }
            
            // Hiển thị tank invite như tin nhắn trong chat
            ChatViewControl chatControl;
            if (!openChatControls.TryGetValue(packet.SenderID, out chatControl))
            {
                string senderName = packet.SenderName;
                FriendListItem friendItem = null;
                foreach (Control ctrl in flpFriendsList.Controls) 
                { 
                    if (ctrl is FriendListItem item && item.FriendID == packet.SenderID) 
                    { 
                        senderName = item.FriendName; 
                        friendItem = item; 
                        break; 
                    } 
                }

                chatControl = new ChatViewControl(packet.SenderID, senderName, this) 
                { 
                    Name = packet.SenderID, 
                    Dock = DockStyle.Fill, 
                    Visible = false 
                };
                openChatControls.Add(packet.SenderID, chatControl);
                pnlMain.Controls.Add(chatControl);
            }
            
            // Tạo GameInvitePacket tương đương để sử dụng cùng logic
            var invite = new GameInvitePacket 
            { 
                SenderID = packet.SenderID, 
                SenderName = packet.SenderName, 
                ReceiverID = packet.ReceiverID 
            };
            chatControl.ReceiveGameInvite(invite, GameType.Tank, MessageType.Incoming);

            if (_currentChatControl == null || _currentChatControl.Name != packet.SenderID)
            {
                FriendListItem itemToAlert = null;
                foreach (Control ctrl in flpFriendsList.Controls) 
                { 
                    if (ctrl is FriendListItem item && item.FriendID == packet.SenderID) 
                    { 
                        itemToAlert = item; 
                        break; 
                    } 
                }
                itemToAlert?.SetNewMessageAlert(true);
            }
        }

        public void HandleTankResponse(TankResponsePacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleTankResponse(packet))); return; }
            
            // packet.SenderID là người phản hồi (invitee), packet.ReceiverID là người đã gửi lời mời (inviter)
            // Tại client của inviter, cần tìm chat control theo ID của người trả lời (senderID) vì chat controls được key bởi friend ID
            string chatKey = packet.SenderID; // responder ID - giống như HandleGameResponse
            if (openChatControls.TryGetValue(chatKey, out var chatControl))
            {
                // Cập nhật game invite bubble trong chat
                chatControl.UpdateGameInviteStatus(packet.SenderID, packet.Accepted, GameType.Tank);
                
                if (!packet.Accepted)
                {
                    chatControl.HandleGameInviteDeclined();
                }
                else
                {
                    // Nếu chấp nhận, reset button để chờ TankStartPacket
                    chatControl.ResetGameButton();
                }
            }
            else
            {
                // Nếu chat control chưa mở, vẫn có thể notify
                if (!packet.Accepted)
                {
                    MessageBox.Show($"Lời mời Tank Game của bạn đã bị từ chối bởi {packet.SenderID}.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        public void HandleTankStart(TankStartPacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleTankStart(packet))); return; }
            
            if (string.IsNullOrEmpty(packet.GameID) || string.IsNullOrEmpty(packet.OpponentID))
            {
                MessageBox.Show("Lỗi: Thông tin game không hợp lệ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            // Reset button trong chat control
            if (openChatControls.TryGetValue(packet.OpponentID, out var chatControl))
            {
                chatControl.ResetGameButton();
            }

            // Kiểm tra xem game đã tồn tại chưa (rematch) hay là game mới
            if (openTankGameForms.TryGetValue(packet.GameID, out var existingForm))
            {
                // Game đã tồn tại - reset game
                existingForm.HandleTankGameReset(packet);
            }
            else
            {
                // Game mới - tạo form mới
                try
                {
                    frmTankGame tankGameForm = new frmTankGame(packet.GameID, packet.OpponentID, packet.StartsFirst);
                    openTankGameForms.Add(packet.GameID, tankGameForm);
                    tankGameForm.FormClosed += (s, e) => { openTankGameForms.Remove(packet.GameID); };
                    tankGameForm.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi tạo form game: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        public void HandleTankAction(TankActionPacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleTankAction(packet))); return; }
            if (string.IsNullOrEmpty(packet.GameID)) return;
            if (openTankGameForms.TryGetValue(packet.GameID, out var gameForm))
            {
                gameForm.ReceiveOpponentAction(packet);
            }
        }

        public void HandleTankHit(TankHitPacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleTankHit(packet))); return; }
            if (string.IsNullOrEmpty(packet.GameID)) return;
            if (openTankGameForms.TryGetValue(packet.GameID, out var gameForm))
            {
                gameForm.ReceiveHit(packet);
            }
        }

        #endregion

        private void pnlMain_Paint(object sender, PaintEventArgs e)
        {

        }

        private void lblFriendsTitle_Click(object sender, EventArgs e)
        {

        }

        public void HandleOnlineListUpdate(OnlineListPacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleOnlineListUpdate(packet))); return; }

            // Clear current list
            flpFriendsList.Controls.Clear();

            // Add all online users
            foreach (var user in packet.OnlineUsers)
            {
                AddFriendToList(user.UserID, user.UserName, user.IsOnline ? "Online" : "Offline", user.IsOnline);
            }
        }

        #region === XỬ LÝ NHÓM CHAT ===

        private Dictionary<string, GroupChatViewControl> openGroupChatControls = new Dictionary<string, GroupChatViewControl>();
        private GroupChatViewControl? _currentGroupChatControl = null;

        public void HandleCreateGroupResult(CreateGroupResultPacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleCreateGroupResult(packet))); return; }

            if (packet.Success)
            {
                MessageBox.Show($"Đã tạo nhóm '{packet.GroupName}' thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Add group to list and open chat
                AddGroupToList(packet.GroupID, packet.GroupName, packet.Members.Count);
                OpenGroupChat(packet.GroupID, packet.GroupName, packet.Members);
            }
            else
            {
                MessageBox.Show(packet.Message ?? "Không thể tạo nhóm", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void HandleGroupText(GroupTextPacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleGroupText(packet))); return; }

            bool isCurrentChat = _currentGroupChatControl != null && 
                                 _currentGroupChatControl.Name == "group_" + packet.GroupID &&
                                 _currentGroupChatControl.Visible;

            if (openGroupChatControls.TryGetValue(packet.GroupID, out var chatControl))
            {
                chatControl.ReceiveMessage(packet);
            }
            
            // Hiển thị thông báo nếu không đang xem chat nhóm này
            if (!isCurrentChat)
            {
                SetGroupNewMessageAlert(packet.GroupID, true);
                
                // ✅ Hiện thông báo Toast cho tin nhắn nhóm
                string groupName = GetGroupName(packet.GroupID);
                ShowMessageNotification($"[{groupName}] {packet.SenderName}", packet.MessageContent ?? "");
            }
        }
        
        private string GetGroupName(string groupId)
        {
            foreach (Control ctrl in flpFriendsList.Controls)
            {
                if (ctrl is GroupListItem item && item.GroupID == groupId)
                {
                    return item.GroupName;
                }
            }
            return "Nhóm";
        }

        public void HandleGroupFile(GroupFilePacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleGroupFile(packet))); return; }

            if (openGroupChatControls.TryGetValue(packet.GroupID, out var chatControl))
            {
                chatControl.ReceiveFile(packet);
            }
            else
            {
                SetGroupNewMessageAlert(packet.GroupID, true);
            }
        }

        public void HandleGroupInviteNotification(GroupInviteNotificationPacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleGroupInviteNotification(packet))); return; }

            // Add group to list
            AddGroupToList(packet.GroupID, packet.GroupName, packet.Members.Count);
            
            // Show notification
            MessageBox.Show($"Bạn đã được {packet.InviterName} mời vào nhóm '{packet.GroupName}'!", 
                "Nhóm mới", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void HandleGroupMemberUpdate(GroupMemberUpdatePacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleGroupMemberUpdate(packet))); return; }

            if (openGroupChatControls.TryGetValue(packet.GroupID, out var chatControl))
            {
                if (packet.Joined)
                    chatControl.OnMemberJoined(packet.UserName);
                else
                    chatControl.OnMemberLeft(packet.UserName);
            }
        }

        public void HandleGroupList(GroupListPacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleGroupList(packet))); return; }

            // Add all groups to sidebar
            foreach (var group in packet.Groups)
            {
                AddGroupToList(group.GroupID, group.GroupName, group.MemberCount, group.LastMessage);
            }
        }

        public void HandleGroupHistoryResponse(GroupHistoryResponsePacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleGroupHistoryResponse(packet))); return; }

            if (packet.Success && openGroupChatControls.TryGetValue(packet.GroupID, out var chatControl))
            {
                chatControl.LoadHistory(packet.Messages);
            }
        }

        private void AddGroupToList(string groupId, string groupName, int memberCount, string lastMessage = null)
        {
            // Check if already exists
            foreach (Control ctrl in flpFriendsList.Controls)
            {
                if (ctrl is GroupListItem item && item.GroupID == groupId)
                {
                    item.SetData(groupId, groupName, memberCount, lastMessage);
                    return;
                }
            }

            var groupItem = new GroupListItem();
            groupItem.SetData(groupId, groupName, memberCount, lastMessage);
            groupItem.Click += (s, e) => GroupItem_Click(groupItem);
            foreach (Control ctrl in groupItem.Controls)
            {
                ctrl.Click += (s, e) => GroupItem_Click(groupItem);
            }
            
            // Insert groups at the top
            flpFriendsList.Controls.Add(groupItem);
            flpFriendsList.Controls.SetChildIndex(groupItem, 0);
        }

        private void GroupItem_Click(GroupListItem item)
        {
            item.SetNewMessageAlert(false);
            lblMainWelcome.Visible = false;

            // Hide current chat
            if (_currentChatControl != null) _currentChatControl.Visible = false;
            if (_currentGroupChatControl != null) _currentGroupChatControl.Visible = false;

            OpenGroupChat(item.GroupID, item.GroupName, null);
        }

        private void OpenGroupChat(string groupId, string groupName, List<GroupMemberInfo> members)
        {
            GroupChatViewControl chatControl;
            
            if (openGroupChatControls.ContainsKey(groupId))
            {
                chatControl = openGroupChatControls[groupId];
                chatControl.Visible = true;
            }
            else
            {
                chatControl = new GroupChatViewControl(groupId, groupName, members ?? new List<GroupMemberInfo>());
                chatControl.Name = "group_" + groupId;
                chatControl.Dock = DockStyle.Fill;
                openGroupChatControls.Add(groupId, chatControl);
                pnlMain.Controls.Add(chatControl);
            }

            _currentGroupChatControl = chatControl;
            _currentChatControl = null;
            chatControl.BringToFront();
        }

        private void SetGroupNewMessageAlert(string groupId, bool hasNewMessage)
        {
            foreach (Control ctrl in flpFriendsList.Controls)
            {
                if (ctrl is GroupListItem item && item.GroupID == groupId)
                {
                    item.SetNewMessageAlert(hasNewMessage);
                    break;
                }
            }
        }

        // Hàm mở Tank Multiplayer Lobby
        private void OpenTankMultiplayerLobby()
        {
            var lobbyForm = new frmTankMultiplayerLobby();
            lobbyForm.Show();
        }

        // Hàm để tạo nhóm mới - gọi từ UI
        public void CreateNewGroup()
        {
            // Get online users for selection
            var availableUsers = new List<UserStatus>();
            foreach (Control ctrl in flpFriendsList.Controls)
            {
                if (ctrl is FriendListItem item)
                {
                    availableUsers.Add(new UserStatus 
                    { 
                        UserID = item.FriendID, 
                        UserName = item.FriendName,
                        IsOnline = item.FriendStatus == "Online"
                    });
                }
            }

            if (availableUsers.Count == 0)
            {
                MessageBox.Show("Không có người dùng nào để thêm vào nhóm!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var form = new frmCreateGroup(availableUsers);
            if (form.ShowDialog() == DialogResult.OK && form.Result != null)
            {
                NetworkManager.Instance.SendPacket(form.Result);
            }
        }

        #endregion
    }
}