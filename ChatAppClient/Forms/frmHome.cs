using ChatApp.Shared; // <-- Phải có
using ChatAppClient.UserControls; // <-- Phải có
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ChatAppClient.Forms
{
    public partial class frmHome : Form
    {
        private Dictionary<string, ChatViewControl> openChatControls;
        private Dictionary<string, frmCaroGame> openGameForms;

        private List<UserStatus> _initialUsers;

        private ChatViewControl _currentChatControl = null;

        public frmHome(List<UserStatus> initialUsers)
        {
            InitializeComponent();
            openChatControls = new Dictionary<string, ChatViewControl>();
            openGameForms = new Dictionary<string, frmCaroGame>();
            _initialUsers = initialUsers;
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
            flpFriendsList.SuspendLayout(); 
            if (_initialUsers != null)
            {
                foreach (var user in _initialUsers)
                {
                    AddFriendToList(user.UserID, user.UserName, user.IsOnline ? "Online" : "Offline", user.IsOnline);
                }
            }
            flpFriendsList.ResumeLayout(); 
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
            if (!openChatControls.TryGetValue(friendId, out chatControl))
            {
                chatControl = new ChatViewControl(friendId, friendName, this) { Name = friendId, Dock = DockStyle.Fill };
                openChatControls.Add(friendId, chatControl);
                pnlMain.Controls.Add(chatControl);
            }

            chatControl.Visible = true;
            chatControl.BringToFront();
            _currentChatControl = chatControl;
        }

        public void AddFriendToList(string id, string name, string status, bool isOnline)
        {
            FriendListItem item = new FriendListItem();
            item.SetData(id, name, status, isOnline);

            Action<object, EventArgs> clickAction = (sender, e) => FriendItem_Click(item);
            item.Click += new EventHandler(clickAction);
            foreach (Control control in item.Controls)
            {
                control.Click += new EventHandler(clickAction);
            }

            flpFriendsList.Controls.Add(item);
        }

        private void frmHome_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        #region BỘ ĐIỀU PHỐI GÓI TIN 

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

        public void HandleIncomingTextMessage(TextPacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleIncomingTextMessage(packet))); return; }

            ChatViewControl chatControl;
            if (!openChatControls.TryGetValue(packet.SenderID, out chatControl))
            {
                string senderName = packet.SenderID; 
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
                chatControl = new ChatViewControl(packet.SenderID, senderName, this) { Name = packet.SenderID, Dock = DockStyle.Fill, Visible = false };
                openChatControls.Add(packet.SenderID, chatControl);
                pnlMain.Controls.Add(chatControl);
            }

            chatControl.ReceiveMessage(packet.MessageContent);

            if (_currentChatControl == null || _currentChatControl.Name != packet.SenderID)
            {
                FriendListItem friendItemToAlert = null;
                if (chatControl.IsHandleCreated) 
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
                friendItemToAlert?.SetNewMessageAlert(true);
            }
        }

        public void HandleIncomingFileMessage(FilePacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleIncomingFileMessage(packet))); return; }

            ChatViewControl chatControl;
            if (!openChatControls.TryGetValue(packet.SenderID, out chatControl))
            {
                string senderName = packet.SenderID;
                FriendListItem friendItem = null;
                foreach (Control ctrl in flpFriendsList.Controls) { if (ctrl is FriendListItem item && item.FriendID == packet.SenderID) { senderName = item.FriendName; friendItem = item; break; } }
                chatControl = new ChatViewControl(packet.SenderID, senderName, this) { Name = packet.SenderID, Dock = DockStyle.Fill, Visible = false };
                openChatControls.Add(packet.SenderID, chatControl); pnlMain.Controls.Add(chatControl);
            }

            chatControl.ReceiveFileMessage(packet, MessageType.Incoming);

            if (_currentChatControl == null || _currentChatControl.Name != packet.SenderID)
            {
                FriendListItem friendItemToAlert = null;
                if (chatControl.IsHandleCreated) { foreach (Control ctrl in flpFriendsList.Controls) { if (ctrl is FriendListItem item && item.FriendID == packet.SenderID) { friendItemToAlert = item; break; } } }
                friendItemToAlert?.SetNewMessageAlert(true);
            }
        }

        public void HandleIncomingGameInvite(GameInvitePacket invite)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleIncomingGameInvite(invite))); return; }

            DialogResult result = MessageBox.Show($"{invite.SenderName} muốn thách đấu Cờ Caro. Đồng ý?", "Lời Mời Chơi Game", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            bool accepted = (result == DialogResult.Yes);
            var response = new GameResponsePacket { SenderID = NetworkManager.Instance.UserID, ReceiverID = invite.SenderID, Accepted = accepted };
            NetworkManager.Instance.SendPacket(response); 
        }

        public void HandleGameResponse(GameResponsePacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleGameResponse(packet))); return; }

            if (!packet.Accepted)
            {
                if (openChatControls.TryGetValue(packet.SenderID, out var chatControl))
                {
                    chatControl.HandleGameInviteDeclined(); 
                }
            }
        }

        public void HandleGameStart(GameStartPacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleGameStart(packet))); return; }

           
            if (openChatControls.TryGetValue(packet.OpponentID, out var chatControl))
            {
                chatControl.ResetGameButton(); 
            }

            frmCaroGame gameForm = new frmCaroGame(packet.GameID, packet.OpponentID, packet.StartsFirst);
            openGameForms.Add(packet.GameID, gameForm); 
            gameForm.FormClosed += (s, e) => { openGameForms.Remove(packet.GameID); };
            gameForm.Show();
        }

        public void HandleGameMove(GameMovePacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleGameMove(packet))); return; }

            if (openGameForms.TryGetValue(packet.GameID, out var gameForm))
            {
                gameForm.ReceiveOpponentMove(packet.Row, packet.Col);
            }
        }

        #endregion
    }
}