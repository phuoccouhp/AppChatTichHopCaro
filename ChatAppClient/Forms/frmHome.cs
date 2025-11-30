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
        private List<UserStatus> _initialUsers;
        private ChatViewControl _currentChatControl = null;

        // Thêm nút Setting vào giao diện bằng Code
        private Button btnSettings;

        public frmHome(List<UserStatus> initialUsers)
        {
            InitializeComponent();
            openChatControls = new Dictionary<string, ChatViewControl>();
            openGameForms = new Dictionary<string, frmCaroGame>();
            _initialUsers = initialUsers;

            // Tạo nút Setting thủ công (để không cần sửa designer file)
            btnSettings = new Button();
            btnSettings.Text = "⚙️";
            btnSettings.Font = new Font("Segoe UI Emoji", 12F);
            btnSettings.Size = new Size(40, 40);
            btnSettings.Location = new Point(pnlHeader.Width - 50, 10); // Góc phải header
            btnSettings.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSettings.Click += btnSettings_Click;
            pnlHeader.Controls.Add(btnSettings);
        }

        private void frmHome_Load(object sender, EventArgs e)
        {
            NetworkManager.Instance.RegisterHomeForm(this);
            lblWelcome.Text = $"Chào mừng, {NetworkManager.Instance.UserName}!";
            LoadInitialFriendList();
            lblMainWelcome.Visible = true;
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
                chatControl = new ChatViewControl(friendId, friendName, this);
                chatControl.Name = friendId;
                chatControl.Dock = DockStyle.Fill;
                openChatControls.Add(friendId, chatControl);
                pnlMain.Controls.Add(chatControl);
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
            if (!openChatControls.TryGetValue(packet.SenderID, out chatControl))
            {
                string senderName = packet.SenderID;
                FriendListItem friendItem = null;
                foreach (Control ctrl in flpFriendsList.Controls) { if (ctrl is FriendListItem item && item.FriendID == packet.SenderID) { senderName = item.FriendName; friendItem = item; break; } }

                chatControl = new ChatViewControl(packet.SenderID, senderName, this) { Name = packet.SenderID, Dock = DockStyle.Fill, Visible = false };
                openChatControls.Add(packet.SenderID, chatControl);
                pnlMain.Controls.Add(chatControl);
            }
            chatControl.ReceiveMessage(packet.MessageContent);

            if (_currentChatControl == null || _currentChatControl.Name != packet.SenderID)
            {
                FriendListItem itemToAlert = null;
                foreach (Control ctrl in flpFriendsList.Controls) { if (ctrl is FriendListItem item && item.FriendID == packet.SenderID) { itemToAlert = item; break; } }
                itemToAlert?.SetNewMessageAlert(true);
            }
        }

        public void HandleIncomingFileMessage(FilePacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleIncomingFileMessage(packet))); return; }

            ChatViewControl chatControl;
            if (!openChatControls.TryGetValue(packet.SenderID, out chatControl))
            {
                string senderName = packet.SenderID;
                foreach (Control ctrl in flpFriendsList.Controls) { if (ctrl is FriendListItem item && item.FriendID == packet.SenderID) { senderName = item.FriendName; break; } }
                chatControl = new ChatViewControl(packet.SenderID, senderName, this) { Name = packet.SenderID, Dock = DockStyle.Fill, Visible = false };
                openChatControls.Add(packet.SenderID, chatControl); pnlMain.Controls.Add(chatControl);
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

            DialogResult result = MessageBox.Show($"{invite.SenderName} muốn thách đấu Caro. Đồng ý?", "Lời Mời", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            NetworkManager.Instance.SendPacket(new GameResponsePacket { SenderID = NetworkManager.Instance.UserID, ReceiverID = invite.SenderID, Accepted = (result == DialogResult.Yes) });
        }

        public void HandleGameResponse(GameResponsePacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleGameResponse(packet))); return; }
            if (!packet.Accepted && openChatControls.TryGetValue(packet.SenderID, out var chatControl)) chatControl.HandleGameInviteDeclined();
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
            if (openGameForms.TryGetValue(packet.GameID, out var gameForm)) gameForm.HandleRematchRequest(packet);
        }

        public void HandleRematchResponse(RematchResponsePacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleRematchResponse(packet))); return; }
            if (openGameForms.TryGetValue(packet.GameID, out var gameForm)) gameForm.HandleRematchResponse(packet);
        }

        public void HandleGameReset(GameResetPacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleGameReset(packet))); return; }
            if (openGameForms.TryGetValue(packet.GameID, out var gameForm)) gameForm.HandleGameReset(packet);
        }

        #endregion
    }
}