using ChatApp.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ChatAppClient.Forms
{
    public class frmTankMultiplayerLobby : Form
    {
        private Panel pnlHeader;
        private Label lblTitle;
        private Button btnCreateRoom;
        private Button btnRefresh;
        private Button btnClose;
        private Panel pnlRoomList;
        private FlowLayoutPanel flpRooms;
        private Label lblNoRooms;

        private string _myId;
        private string _myName;
        private TankRoomInfo _currentRoom;

        // Room detail panel (shown when in a room)
        private Panel pnlRoomDetail;
        private Label lblRoomName;
        private FlowLayoutPanel flpPlayers;
        private Button btnReady;
        private Button btnStartGame;
        private Button btnLeaveRoom;

        public frmTankMultiplayerLobby()
        {
            _myId = NetworkManager.Instance.UserID ?? "";
            _myName = NetworkManager.Instance.UserName ?? "Player";
            InitializeComponent();
            RegisterEvents();
            RefreshRoomList();
        }

        private void InitializeComponent()
        {
            this.Text = "Tank Multiplayer - Lobby";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(32, 34, 37);

            // Header
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 70,
                BackColor = Color.FromArgb(47, 49, 54)
            };
            this.Controls.Add(pnlHeader);

            lblTitle = new Label
            {
                Text = "\U0001F3AE Tank Multiplayer",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 18)
            };
            pnlHeader.Controls.Add(lblTitle);

            btnClose = new Button
            {
                Text = "\u2715",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(40, 40),
                Location = new Point(545, 15),
                BackColor = Color.Transparent,
                ForeColor = Color.Gray,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 80, 80);
            btnClose.Click += (s, e) => this.Close();
            pnlHeader.Controls.Add(btnClose);

            // Action buttons
            btnCreateRoom = new Button
            {
                Text = "+ Create Room",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(130, 35),
                Location = new Point(20, 80),
                BackColor = Color.FromArgb(88, 101, 242),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCreateRoom.FlatAppearance.BorderSize = 0;
            btnCreateRoom.Click += BtnCreateRoom_Click;
            this.Controls.Add(btnCreateRoom);

            btnRefresh = new Button
            {
                Text = "\U0001F504 Refresh",
                Font = new Font("Segoe UI", 10),
                Size = new Size(100, 35),
                Location = new Point(160, 80),
                BackColor = Color.FromArgb(64, 68, 75),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) => RefreshRoomList();
            this.Controls.Add(btnRefresh);

            // Room list panel
            pnlRoomList = new Panel
            {
                Location = new Point(20, 130),
                Size = new Size(545, 310),
                BackColor = Color.FromArgb(54, 57, 63)
            };
            this.Controls.Add(pnlRoomList);

            flpRooms = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.FromArgb(54, 57, 63),
                Padding = new Padding(10)
            };
            pnlRoomList.Controls.Add(flpRooms);

            lblNoRooms = new Label
            {
                Text = "No rooms available.\nCreate a new room to start!",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Visible = true
            };
            flpRooms.Controls.Add(lblNoRooms);

            // Room detail panel (hidden initially)
            InitializeRoomDetailPanel();
        }

        private void InitializeRoomDetailPanel()
        {
            pnlRoomDetail = new Panel
            {
                Location = new Point(20, 130),
                Size = new Size(545, 310),
                BackColor = Color.FromArgb(54, 57, 63),
                Visible = false
            };
            this.Controls.Add(pnlRoomDetail);

            lblRoomName = new Label
            {
                Text = "Room: ",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(15, 15)
            };
            pnlRoomDetail.Controls.Add(lblRoomName);

            Label lblPlayers = new Label
            {
                Text = "Players:",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.LightGray,
                AutoSize = true,
                Location = new Point(15, 55)
            };
            pnlRoomDetail.Controls.Add(lblPlayers);

            flpPlayers = new FlowLayoutPanel
            {
                Location = new Point(15, 85),
                Size = new Size(515, 140),
                BackColor = Color.FromArgb(47, 49, 54),
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(10)
            };
            pnlRoomDetail.Controls.Add(flpPlayers);

            btnReady = new Button
            {
                Text = "Ready",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(120, 40),
                Location = new Point(15, 240),
                BackColor = Color.FromArgb(67, 181, 129),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnReady.FlatAppearance.BorderSize = 0;
            btnReady.Click += BtnReady_Click;
            pnlRoomDetail.Controls.Add(btnReady);

            btnStartGame = new Button
            {
                Text = "Start Game",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(130, 40),
                Location = new Point(145, 240),
                BackColor = Color.FromArgb(88, 101, 242),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Visible = false
            };
            btnStartGame.FlatAppearance.BorderSize = 0;
            btnStartGame.Click += BtnStartGame_Click;
            pnlRoomDetail.Controls.Add(btnStartGame);

            btnLeaveRoom = new Button
            {
                Text = "Leave Room",
                Font = new Font("Segoe UI", 10),
                Size = new Size(110, 40),
                Location = new Point(420, 240),
                BackColor = Color.FromArgb(237, 66, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnLeaveRoom.FlatAppearance.BorderSize = 0;
            btnLeaveRoom.Click += BtnLeaveRoom_Click;
            pnlRoomDetail.Controls.Add(btnLeaveRoom);
        }

        private void RegisterEvents()
        {
            NetworkManager.Instance.OnTankRoomListReceived += HandleRoomListReceived;
            NetworkManager.Instance.OnTankCreateRoomResult += HandleCreateRoomResult;
            NetworkManager.Instance.OnTankJoinRoomResult += HandleJoinRoomResult;
            NetworkManager.Instance.OnTankRoomUpdate += HandleRoomUpdate;
            NetworkManager.Instance.OnTankMultiplayerStarted += HandleGameStarted;
        }

        private void UnregisterEvents()
        {
            NetworkManager.Instance.OnTankRoomListReceived -= HandleRoomListReceived;
            NetworkManager.Instance.OnTankCreateRoomResult -= HandleCreateRoomResult;
            NetworkManager.Instance.OnTankJoinRoomResult -= HandleJoinRoomResult;
            NetworkManager.Instance.OnTankRoomUpdate -= HandleRoomUpdate;
            NetworkManager.Instance.OnTankMultiplayerStarted -= HandleGameStarted;
        }

        private void RefreshRoomList()
        {
            NetworkManager.Instance.SendPacket(new TankRequestRoomListPacket { PlayerID = _myId });
        }

        private void HandleRoomListReceived(TankRoomListPacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleRoomListReceived(packet))); return; }

            flpRooms.Controls.Clear();

            if (packet.Rooms == null || packet.Rooms.Count == 0)
            {
                flpRooms.Controls.Add(lblNoRooms);
                lblNoRooms.Visible = true;
                return;
            }

            lblNoRooms.Visible = false;

            foreach (var room in packet.Rooms)
            {
                var roomPanel = CreateRoomPanel(room);
                flpRooms.Controls.Add(roomPanel);
            }
        }

        private Panel CreateRoomPanel(TankRoomInfo room)
        {
            var panel = new Panel
            {
                Size = new Size(510, 70),
                BackColor = Color.FromArgb(64, 68, 75),
                Margin = new Padding(0, 0, 0, 8),
                Cursor = Cursors.Hand
            };

            var lblName = new Label
            {
                Text = room.RoomName,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(15, 12)
            };
            panel.Controls.Add(lblName);

            var lblHost = new Label
            {
                Text = $"Host: {room.HostName}",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.LightGray,
                AutoSize = true,
                Location = new Point(15, 40)
            };
            panel.Controls.Add(lblHost);

            var lblPlayers = new Label
            {
                Text = $"{room.CurrentPlayers}/{room.MaxPlayers}",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = room.CurrentPlayers < room.MaxPlayers ? Color.FromArgb(67, 181, 129) : Color.FromArgb(237, 66, 69),
                AutoSize = true,
                Location = new Point(350, 20)
            };
            panel.Controls.Add(lblPlayers);

            var btnJoin = new Button
            {
                Text = "Join",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Size = new Size(70, 35),
                Location = new Point(420, 17),
                BackColor = Color.FromArgb(88, 101, 242),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Enabled = room.CurrentPlayers < room.MaxPlayers
            };
            btnJoin.FlatAppearance.BorderSize = 0;
            btnJoin.Click += (s, e) => JoinRoom(room.RoomID);
            panel.Controls.Add(btnJoin);

            // Hover effect
            panel.MouseEnter += (s, e) => panel.BackColor = Color.FromArgb(79, 84, 92);
            panel.MouseLeave += (s, e) => panel.BackColor = Color.FromArgb(64, 68, 75);

            return panel;
        }

        private void BtnCreateRoom_Click(object sender, EventArgs e)
        {
            using (var dialog = new frmCreateTankRoom())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var packet = new TankCreateRoomPacket
                    {
                        HostID = _myId,
                        HostName = _myName,
                        RoomName = dialog.RoomName,
                        MaxPlayers = dialog.MaxPlayers,
                        GameMode = dialog.GameMode
                    };
                    NetworkManager.Instance.SendPacket(packet);
                }
            }
        }

        private void HandleCreateRoomResult(TankCreateRoomResultPacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleCreateRoomResult(packet))); return; }

            if (packet.Success && packet.Room != null)
            {
                _currentRoom = packet.Room;
                ShowRoomDetail();
            }
            else
            {
                MessageBox.Show(packet.Message ?? "Failed to create room", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void JoinRoom(string roomId)
        {
            var packet = new TankJoinRoomPacket
            {
                RoomID = roomId,
                PlayerID = _myId,
                PlayerName = _myName
            };
            NetworkManager.Instance.SendPacket(packet);
        }

        private void HandleJoinRoomResult(TankJoinRoomResultPacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleJoinRoomResult(packet))); return; }

            if (packet.Success && packet.Room != null)
            {
                _currentRoom = packet.Room;
                ShowRoomDetail();
            }
            else
            {
                MessageBox.Show(packet.Message ?? "Failed to join room", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HandleRoomUpdate(TankRoomUpdatePacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleRoomUpdate(packet))); return; }

            if (_currentRoom == null || _currentRoom.RoomID != packet.RoomID) return;

            if (packet.UpdateType == "RoomClosed")
            {
                MessageBox.Show("Room has been closed.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _currentRoom = null;
                ShowRoomList();
                RefreshRoomList();
                return;
            }

            if (packet.Room != null)
            {
                _currentRoom = packet.Room;
                UpdateRoomDetailUI();
            }
        }

        private void ShowRoomDetail()
        {
            pnlRoomList.Visible = false;
            btnCreateRoom.Visible = false;
            btnRefresh.Visible = false;
            pnlRoomDetail.Visible = true;
            UpdateRoomDetailUI();
        }

        private void ShowRoomList()
        {
            pnlRoomDetail.Visible = false;
            pnlRoomList.Visible = true;
            btnCreateRoom.Visible = true;
            btnRefresh.Visible = true;
        }

        private void UpdateRoomDetailUI()
        {
            if (_currentRoom == null) return;

            lblRoomName.Text = $"Room: {_currentRoom.RoomName} ({_currentRoom.CurrentPlayers}/{_currentRoom.MaxPlayers})";

            flpPlayers.Controls.Clear();

            foreach (var player in _currentRoom.Players)
            {
                var playerPanel = CreatePlayerPanel(player);
                flpPlayers.Controls.Add(playerPanel);
            }

            // Show start button only for host
            bool isHost = _currentRoom.HostID == _myId;
            btnStartGame.Visible = isHost;

            // Update ready button
            var myPlayer = _currentRoom.Players.Find(p => p.PlayerID == _myId);
            if (myPlayer != null)
            {
                btnReady.Text = myPlayer.IsReady ? "Not Ready" : "Ready";
                btnReady.BackColor = myPlayer.IsReady ? Color.FromArgb(237, 66, 69) : Color.FromArgb(67, 181, 129);
                btnReady.Visible = !isHost; // Host doesn't need ready
            }
        }

        private Panel CreatePlayerPanel(TankPlayerInfo player)
        {
            var panel = new Panel
            {
                Size = new Size(115, 110),
                BackColor = Color.FromArgb(64, 68, 75),
                Margin = new Padding(5)
            };

            // Player slot color
            Color slotColor = GetSlotColor(player.SlotIndex);

            var lblSlot = new Label
            {
                Text = $"P{player.SlotIndex + 1}",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = slotColor,
                AutoSize = true,
                Location = new Point(35, 10)
            };
            panel.Controls.Add(lblSlot);

            var lblName = new Label
            {
                Text = player.PlayerName.Length > 10 ? player.PlayerName.Substring(0, 10) + "..." : player.PlayerName,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.White,
                AutoSize = false,
                Size = new Size(105, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(5, 50)
            };
            panel.Controls.Add(lblName);

            // Status
            string status = player.IsHost ? "\U0001F451 Host" : (player.IsReady ? "\u2705 Ready" : "\u23F3 Waiting");
            var lblStatus = new Label
            {
                Text = status,
                Font = new Font("Segoe UI", 8),
                ForeColor = player.IsHost ? Color.Gold : (player.IsReady ? Color.LightGreen : Color.Gray),
                AutoSize = false,
                Size = new Size(105, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(5, 75)
            };
            panel.Controls.Add(lblStatus);

            return panel;
        }

        private Color GetSlotColor(int slot)
        {
            return slot switch
            {
                0 => Color.FromArgb(88, 101, 242),  // Blue
                1 => Color.FromArgb(237, 66, 69),   // Red
                2 => Color.FromArgb(67, 181, 129),  // Green
                3 => Color.FromArgb(250, 166, 26),  // Yellow
                _ => Color.White
            };
        }

        private void BtnReady_Click(object sender, EventArgs e)
        {
            if (_currentRoom == null) return;

            var myPlayer = _currentRoom.Players.Find(p => p.PlayerID == _myId);
            if (myPlayer == null) return;

            var packet = new TankPlayerReadyPacket
            {
                RoomID = _currentRoom.RoomID,
                PlayerID = _myId,
                IsReady = !myPlayer.IsReady
            };
            NetworkManager.Instance.SendPacket(packet);

            // Optimistic update
            myPlayer.IsReady = !myPlayer.IsReady;
            UpdateRoomDetailUI();
        }

        private void BtnStartGame_Click(object sender, EventArgs e)
        {
            if (_currentRoom == null || _currentRoom.HostID != _myId) return;

            var packet = new TankStartMultiplayerPacket
            {
                RoomID = _currentRoom.RoomID,
                HostID = _myId
            };
            NetworkManager.Instance.SendPacket(packet);
        }

        private void BtnLeaveRoom_Click(object sender, EventArgs e)
        {
            if (_currentRoom == null) return;

            var packet = new TankLeaveRoomPacket
            {
                RoomID = _currentRoom.RoomID,
                PlayerID = _myId
            };
            NetworkManager.Instance.SendPacket(packet);

            _currentRoom = null;
            ShowRoomList();
            RefreshRoomList();
        }

        private void HandleGameStarted(TankMultiplayerStartedPacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleGameStarted(packet))); return; }

            // Open multiplayer game form
            var gameForm = new frmTankMultiplayer(packet);
            gameForm.Show();

            _currentRoom = null;
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Leave room if in one
            if (_currentRoom != null)
            {
                var packet = new TankLeaveRoomPacket
                {
                    RoomID = _currentRoom.RoomID,
                    PlayerID = _myId
                };
                NetworkManager.Instance.SendPacket(packet);
            }

            UnregisterEvents();
            base.OnFormClosing(e);
        }
    }

    // Dialog for creating a room
    public class frmCreateTankRoom : Form
    {
        public string RoomName { get; private set; } = "";
        public int MaxPlayers { get; private set; } = 4;
        public int GameMode { get; private set; } = 0;

        private TextBox txtRoomName;
        private ComboBox cmbMaxPlayers;
        private ComboBox cmbGameMode;

        public frmCreateTankRoom()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Create Room";
            this.Size = new Size(350, 280);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(47, 49, 54);

            var lblName = new Label
            {
                Text = "Room Name:",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 25)
            };
            this.Controls.Add(lblName);

            txtRoomName = new TextBox
            {
                Font = new Font("Segoe UI", 11),
                Size = new Size(290, 30),
                Location = new Point(20, 55),
                BackColor = Color.FromArgb(64, 68, 75),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(txtRoomName);

            var lblPlayers = new Label
            {
                Text = "Max Players:",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 95)
            };
            this.Controls.Add(lblPlayers);

            cmbMaxPlayers = new ComboBox
            {
                Font = new Font("Segoe UI", 11),
                Size = new Size(100, 30),
                Location = new Point(20, 125),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(64, 68, 75),
                ForeColor = Color.White
            };
            cmbMaxPlayers.Items.AddRange(new object[] { "2", "3", "4" });
            cmbMaxPlayers.SelectedIndex = 2;
            this.Controls.Add(cmbMaxPlayers);

            var lblMode = new Label
            {
                Text = "Game Mode:",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(150, 95)
            };
            this.Controls.Add(lblMode);

            cmbGameMode = new ComboBox
            {
                Font = new Font("Segoe UI", 11),
                Size = new Size(160, 30),
                Location = new Point(150, 125),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(64, 68, 75),
                ForeColor = Color.White
            };
            cmbGameMode.Items.AddRange(new object[] { "Free For All", "Team Battle" });
            cmbGameMode.SelectedIndex = 0;
            this.Controls.Add(cmbGameMode);

            var btnCreate = new Button
            {
                Text = "Create",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(120, 40),
                Location = new Point(70, 180),
                BackColor = Color.FromArgb(88, 101, 242),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCreate.FlatAppearance.BorderSize = 0;
            btnCreate.Click += BtnCreate_Click;
            this.Controls.Add(btnCreate);

            var btnCancel = new Button
            {
                Text = "Cancel",
                Font = new Font("Segoe UI", 11),
                Size = new Size(100, 40),
                Location = new Point(200, 180),
                BackColor = Color.FromArgb(64, 68, 75),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancel);
        }

        private void BtnCreate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtRoomName.Text))
            {
                MessageBox.Show("Please enter a room name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            RoomName = txtRoomName.Text.Trim();
            MaxPlayers = int.Parse(cmbMaxPlayers.SelectedItem.ToString());
            GameMode = cmbGameMode.SelectedIndex;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
