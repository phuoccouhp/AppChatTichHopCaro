using ChatApp.Shared;
using ChatAppClient.UserControls;
using ChatAppClient; // <--- QUAN TRỌNG: Thêm dòng này để dùng MessageType
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

            ConfigureUI();
        }

        private void ConfigureUI()
        {
            Color separatorColor = Color.FromArgb(32, 34, 37);
            Panel lineVertical = new Panel { Width = 2, Dock = DockStyle.Right, BackColor = separatorColor };
            Panel lineSidebarTitle = new Panel { Height = 2, Dock = DockStyle.Top, BackColor = separatorColor };

            pnlSidebar.Controls.Clear();
            pnlSidebar.Controls.Add(lineVertical);
            pnlSidebar.Controls.Add(flpFriendsList);
            if (pnlSearchBox != null) { pnlSearchBox.Dock = DockStyle.Top; pnlSidebar.Controls.Add(pnlSearchBox); }
            pnlSidebar.Controls.Add(lineSidebarTitle);
            lblFriendsTitle.Dock = DockStyle.Top;
            lblFriendsTitle.AutoSize = false;
            lblFriendsTitle.Height = 50;
            lblFriendsTitle.TextAlign = ContentAlignment.MiddleLeft;
            pnlSidebar.Controls.Add(lblFriendsTitle);

            btnSettings.MouseEnter += (s, e) => btnSettings.BackColor = Color.FromArgb(80, 83, 95);
            btnSettings.MouseLeave += (s, e) => btnSettings.BackColor = Color.FromArgb(55, 58, 70);
        }

        private void frmHome_Load(object sender, EventArgs e)
        {
            NetworkManager.Instance.RegisterHomeForm(this);
            lblWelcome.Text = $"Chào mừng, {NetworkManager.Instance.UserName ?? "User"}!";
            LoadInitialFriendList();
            lblMainWelcome.Visible = true;
            LoadMyAvatar();
        }

        private void LoadInitialFriendList()
        {
            if (_initialUsers == null) return;
            foreach (var user in _initialUsers) AddFriendToList(user.UserID, user.UserName, user.IsOnline ? "Online" : "Offline", user.IsOnline);
        }

        public void AddFriendToList(string id, string name, string status, bool isOnline)
        {
            FriendListItem item = new FriendListItem();
            item.SetData(id, name, status, isOnline);
            item.Click += (s, e) => FriendItem_Click(item);
            foreach (Control c in item.Controls) c.Click += (s, e) => FriendItem_Click(item);
            flpFriendsList.Controls.Add(item);
        }

        public void HandleUserStatusUpdate(UserStatusPacket packet)
        {
            if (InvokeRequired) { Invoke(new Action(() => HandleUserStatusUpdate(packet))); return; }
            FriendListItem? existingItem = null;
            foreach (Control c in flpFriendsList.Controls) if (c is FriendListItem item && item.FriendID == packet.UserID) { existingItem = item; break; }

            if (packet.IsOnline)
            {
                if (existingItem == null) AddFriendToList(packet.UserID, packet.UserName, "Online", true);
                else { existingItem.SetData(packet.UserID, packet.UserName, "Online", true); flpFriendsList.Controls.SetChildIndex(existingItem, 0); }
            }
            else if (existingItem != null) existingItem.SetData(packet.UserID, packet.UserName, "Offline", false);
        }

        public void HandleOnlineListUpdate(OnlineListPacket packet)
        {
            if (InvokeRequired) { Invoke(new Action(() => HandleOnlineListUpdate(packet))); return; }
            flpFriendsList.Controls.Clear();
            foreach (var user in packet.OnlineUsers) AddFriendToList(user.UserID, user.UserName, user.IsOnline ? "Online" : "Offline", user.IsOnline);
        }

        private void FriendItem_Click(FriendListItem item)
        {
            if (string.IsNullOrEmpty(NetworkManager.Instance.UserID)) return;
            item.SetNewMessageAlert(false);
            lblMainWelcome.Visible = false;

            if (_currentChatControl != null && _currentChatControl.Name == item.FriendID) return;
            if (_currentChatControl != null) _currentChatControl.Visible = false;

            if (openChatControls.TryGetValue(item.FriendID, out var existing)) { existing.Visible = true; _currentChatControl = existing; }
            else
            {
                var newChat = new ChatViewControl(item.FriendID, item.FriendName, this) { Name = item.FriendID, Dock = DockStyle.Fill };
                openChatControls[item.FriendID] = newChat;
                pnlMain.Controls.Add(newChat);
                _currentChatControl = newChat;
            }
            _currentChatControl.BringToFront();
        }

        public void HandleIncomingTextMessage(TextPacket packet)
        {
            if (InvokeRequired) { Invoke(new Action(() => HandleIncomingTextMessage(packet))); return; }
            GetChatControl(packet.SenderID).ReceiveMessage(packet.MessageContent);
            AlertUser(packet.SenderID);
        }

        public void HandleIncomingFileMessage(FilePacket packet)
        {
            if (InvokeRequired) { Invoke(new Action(() => HandleIncomingFileMessage(packet))); return; }
            // MessageType.Incoming giờ đã được nhận diện nhờ dòng using ChatAppClient;
            GetChatControl(packet.SenderID).ReceiveFileMessage(packet, MessageType.Incoming);
            AlertUser(packet.SenderID);
        }

        public void HandleIncomingGameInvite(GameInvitePacket p)
        {
            if (InvokeRequired) { Invoke(new Action(() => HandleIncomingGameInvite(p))); return; }
            GetChatControl(p.SenderID).ReceiveGameInvite(p, GameType.Caro, MessageType.Incoming);
            AlertUser(p.SenderID);
        }

        public void HandleTankInvite(TankInvitePacket p)
        {
            if (InvokeRequired) { Invoke(new Action(() => HandleTankInvite(p))); return; }
            var invite = new GameInvitePacket { SenderID = p.SenderID, SenderName = p.SenderName, ReceiverID = p.ReceiverID };
            GetChatControl(p.SenderID).ReceiveGameInvite(invite, GameType.Tank, MessageType.Incoming);
            AlertUser(p.SenderID);
        }

        private ChatViewControl GetChatControl(string senderID)
        {
            if (openChatControls.TryGetValue(senderID, out var ctrl)) return ctrl;
            string name = senderID;
            foreach (Control c in flpFriendsList.Controls) if (c is FriendListItem item && item.FriendID == senderID) { name = item.FriendName; break; }
            var newCtrl = new ChatViewControl(senderID, name, this) { Name = senderID, Dock = DockStyle.Fill, Visible = false };
            openChatControls[senderID] = newCtrl;
            pnlMain.Controls.Add(newCtrl);
            return newCtrl;
        }

        private void AlertUser(string senderID)
        {
            if (_currentChatControl == null || _currentChatControl.Name != senderID)
            {
                foreach (Control c in flpFriendsList.Controls)
                    if (c is FriendListItem item && item.FriendID == senderID) { item.SetNewMessageAlert(true); break; }
            }
        }

        public List<(string id, string name)> GetFriendsList()
        {
            var list = new List<(string, string)>();
            foreach (Control c in flpFriendsList.Controls) if (c is FriendListItem item) list.Add((item.FriendID, item.FriendName));
            return list;
        }

        // --- Game Logic ---
        public void HandleGameResponse(GameResponsePacket p) { if (InvokeRequired) Invoke(new Action(() => HandleGameResponse(p))); else { if (openChatControls.TryGetValue(p.SenderID, out var c)) { c.UpdateGameInviteStatus(p.SenderID, p.Accepted, GameType.Caro); if (p.Accepted) c.ResetGameButton(); else c.HandleGameInviteDeclined(); } } }
        public void HandleTankResponse(TankResponsePacket p) { if (InvokeRequired) Invoke(new Action(() => HandleTankResponse(p))); else { if (openChatControls.TryGetValue(p.SenderID, out var c)) { c.UpdateGameInviteStatus(p.SenderID, p.Accepted, GameType.Tank); if (p.Accepted) c.ResetGameButton(); else c.HandleGameInviteDeclined(); } } }
        public void HandleGameStart(GameStartPacket p) { if (InvokeRequired) { Invoke(new Action(() => HandleGameStart(p))); return; } if (openChatControls.TryGetValue(p.OpponentID, out var c)) c.ResetGameButton(); var f = new frmCaroGame(p.GameID, p.OpponentID, p.StartsFirst); openGameForms[p.GameID] = f; f.FormClosed += (s, e) => openGameForms.Remove(p.GameID); f.Show(); }
        public void HandleTankStart(TankStartPacket p) { if (InvokeRequired) { Invoke(new Action(() => HandleTankStart(p))); return; } if (openChatControls.TryGetValue(p.OpponentID, out var c)) c.ResetGameButton(); if (openTankGameForms.TryGetValue(p.GameID, out var existing)) existing.HandleTankGameReset(p); else { var f = new frmTankGame(p.GameID, p.OpponentID, p.StartsFirst); openTankGameForms[p.GameID] = f; f.FormClosed += (s, e) => openTankGameForms.Remove(p.GameID); f.Show(); } }
        public void HandleGameMove(GameMovePacket p) { if (InvokeRequired) Invoke(new Action(() => HandleGameMove(p))); else if (openGameForms.TryGetValue(p.GameID, out var f)) f.ReceiveOpponentMove(p.Row, p.Col); }
        public void HandleTankAction(TankActionPacket p) { if (InvokeRequired) Invoke(new Action(() => HandleTankAction(p))); else if (openTankGameForms.TryGetValue(p.GameID, out var f)) f.ReceiveOpponentAction(p); }
        public void HandleTankHit(TankHitPacket p) { if (InvokeRequired) Invoke(new Action(() => HandleTankHit(p))); else if (openTankGameForms.TryGetValue(p.GameID, out var f)) f.ReceiveHit(p); }
        public void HandleRematchRequest(RematchRequestPacket p) { if (InvokeRequired) Invoke(new Action(() => HandleRematchRequest(p))); else { if (openGameForms.TryGetValue(p.GameID, out var f)) f.HandleRematchRequest(p); else if (openTankGameForms.TryGetValue(p.GameID, out var t)) t.HandleRematchRequest(p); } }
        public void HandleRematchResponse(RematchResponsePacket p) { if (InvokeRequired) Invoke(new Action(() => HandleRematchResponse(p))); else { if (openGameForms.TryGetValue(p.GameID, out var f)) f.HandleRematchResponse(p); else if (openTankGameForms.TryGetValue(p.GameID, out var t)) t.HandleRematchResponse(p); } }
        public void HandleGameReset(GameResetPacket p) { if (InvokeRequired) Invoke(new Action(() => HandleGameReset(p))); else if (openGameForms.TryGetValue(p.GameID, out var f)) f.HandleGameReset(p); }
        public void HandleUpdateProfile(UpdateProfilePacket p) { if (InvokeRequired) { Invoke(new Action(() => HandleUpdateProfile(p))); return; } foreach (Control c in flpFriendsList.Controls) if (c is FriendListItem item && item.FriendID == p.UserID) { item.SetData(p.UserID, p.NewDisplayName, item.FriendStatus, item.FriendStatus == "Online"); break; } if (p.UserID == NetworkManager.Instance.UserID) { lblWelcome.Text = $"Chào mừng, {p.NewDisplayName}!"; LoadMyAvatar(); } }

        private void txtSearch_TextChanged(object sender, EventArgs e) { string k = txtSearch.Text.Trim().ToLower(); if (k == "tìm kiếm...") k = ""; flpFriendsList.SuspendLayout(); foreach (Control c in flpFriendsList.Controls) { if (c is FriendListItem item) item.Visible = string.IsNullOrEmpty(k) || item.FriendName.ToLower().Contains(k); } flpFriendsList.ResumeLayout(); }
        private void txtSearch_Enter(object sender, EventArgs e) { if (txtSearch.Text == "Tìm kiếm...") { txtSearch.Text = ""; txtSearch.ForeColor = Color.White; } }
        private void txtSearch_Leave(object sender, EventArgs e) { if (string.IsNullOrWhiteSpace(txtSearch.Text)) { txtSearch.Text = "Tìm kiếm..."; txtSearch.ForeColor = Color.Gray; } }
        private void btnSettings_Click(object sender, EventArgs e) { new frmSettings().ShowDialog(); }
        private void frmHome_FormClosing(object sender, FormClosingEventArgs e) => Application.Exit();

        private void LoadMyAvatar()
        {
            string path = System.IO.Path.Combine("Images", $"{NetworkManager.Instance.UserID}.png");
            if (System.IO.File.Exists(path)) { using (var bmp = new Bitmap(path)) { pbUserAvatar.Image = new Bitmap(bmp); } }
            System.Drawing.Drawing2D.GraphicsPath gp = new System.Drawing.Drawing2D.GraphicsPath();
            gp.AddEllipse(0, 0, pbUserAvatar.Width, pbUserAvatar.Height);
            pbUserAvatar.Region = new Region(gp);
        }
    }
}