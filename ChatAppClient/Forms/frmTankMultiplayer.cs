using ChatApp.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace ChatAppClient.Forms
{
    public partial class frmTankMultiplayer : Form
    {
        private const int GAME_WIDTH = 800;
        private const int GAME_HEIGHT = 600;
        private const float TANK_SIZE = 40f;
        private const float TANK_SPEED = 3f;
        private const float ROTATION_SPEED = 5f;
        private const float BULLET_SPEED = 8f;
        private const int MAX_HEALTH = 100;

        private string _gameId;
        private string _myId;
        private int _gameMode;
        private bool _isGameEnded = false;
        private bool _isEliminated = false;

        // All players
        private Dictionary<string, PlayerState> _players = new Dictionary<string, PlayerState>();
        private List<Bullet> _bullets = new List<Bullet>();

        // Input
        private bool _keyUp, _keyDown, _keyLeft, _keyRight, _keyShoot;
        private System.Windows.Forms.Timer _gameTimer;

        // Throttling
        private float _lastSentX, _lastSentY, _lastSentAngle;
        private DateTime _lastPacketSentTime = DateTime.MinValue;
        private DateTime _lastShootTime = DateTime.MinValue;
        private const float POSITION_THRESHOLD = 8f;
        private const float ANGLE_THRESHOLD = 15f;
        private const int MIN_PACKET_INTERVAL_MS = 50;
        private const int SHOOT_COOLDOWN_MS = 300;

        // UI
        private Panel pnlScoreboard;
        private Button btnExit;
        private Label lblGameOver;

        // Player colors
        private static readonly Color[] PLAYER_COLORS = new Color[]
        {
            Color.FromArgb(88, 101, 242),   // Blue
            Color.FromArgb(237, 66, 69),    // Red
            Color.FromArgb(67, 181, 129),   // Green
            Color.FromArgb(250, 166, 26)    // Yellow
        };

        private class PlayerState
        {
            public string PlayerID { get; set; } = "";
            public string PlayerName { get; set; } = "";
            public int SlotIndex { get; set; }
            public int TeamID { get; set; }
            public float X { get; set; }
            public float Y { get; set; }
            public float Angle { get; set; }
            public int Health { get; set; } = MAX_HEALTH;
            public bool IsEliminated { get; set; }
            public int Kills { get; set; }
            public int Deaths { get; set; }
            public Color Color { get; set; } = Color.Blue;
        }

        private class Bullet
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Angle { get; set; }
            public string OwnerID { get; set; } = "";
            public Color Color { get; set; } = Color.Yellow;
        }

        public frmTankMultiplayer(TankMultiplayerStartedPacket startPacket)
        {
            _gameId = startPacket.GameID;
            _gameMode = startPacket.GameMode;
            _myId = NetworkManager.Instance.UserID ?? "";

            InitializeComponent();
            InitializePlayers(startPacket);
            RegisterEvents();

            // Set initial throttle values
            if (_players.TryGetValue(_myId, out var me))
            {
                _lastSentX = me.X;
                _lastSentY = me.Y;
                _lastSentAngle = me.Angle;
            }
        }

        private void InitializeComponent()
        {
            this.ClientSize = new Size(GAME_WIDTH, GAME_HEIGHT);
            this.Text = "Tank Multiplayer";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.KeyPreview = true;
            this.BackColor = Color.FromArgb(34, 40, 34);

            this.Load += FrmTankMultiplayer_Load;
            this.KeyDown += FrmTankMultiplayer_KeyDown;
            this.KeyUp += FrmTankMultiplayer_KeyUp;
            this.Paint += FrmTankMultiplayer_Paint;

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | 
                         ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);
            this.DoubleBuffered = true;

            // Scoreboard panel
            pnlScoreboard = new Panel
            {
                Size = new Size(180, 150),
                Location = new Point(GAME_WIDTH - 190, 10),
                BackColor = Color.FromArgb(150, 0, 0, 0)
            };
            this.Controls.Add(pnlScoreboard);

            // Exit button (hidden initially)
            btnExit = new Button
            {
                Text = "Exit",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(120, 45),
                Location = new Point((GAME_WIDTH - 120) / 2, GAME_HEIGHT - 80),
                BackColor = Color.FromArgb(237, 66, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Visible = false
            };
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.Click += (s, e) => this.Close();
            this.Controls.Add(btnExit);

            // Game over label
            lblGameOver = new Label
            {
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                Visible = false
            };
            this.Controls.Add(lblGameOver);
        }

        private void InitializePlayers(TankMultiplayerStartedPacket packet)
        {
            foreach (var player in packet.Players)
            {
                var spawn = packet.SpawnPoints.FirstOrDefault(s => s.PlayerID == player.PlayerID);
                
                var state = new PlayerState
                {
                    PlayerID = player.PlayerID,
                    PlayerName = player.PlayerName,
                    SlotIndex = player.SlotIndex,
                    TeamID = player.TeamID,
                    X = spawn?.X ?? 400,
                    Y = spawn?.Y ?? 300,
                    Angle = spawn?.Angle ?? 0,
                    Color = PLAYER_COLORS[player.SlotIndex % PLAYER_COLORS.Length]
                };

                _players[player.PlayerID] = state;
            }

            UpdateScoreboard();
        }

        private void RegisterEvents()
        {
            NetworkManager.Instance.OnTankMultiplayerAction += HandleMultiplayerAction;
            NetworkManager.Instance.OnTankMultiplayerHit += HandleMultiplayerHit;
            NetworkManager.Instance.OnTankPlayerEliminated += HandlePlayerEliminated;
            NetworkManager.Instance.OnTankMultiplayerGameOver += HandleGameOver;
        }

        private void UnregisterEvents()
        {
            NetworkManager.Instance.OnTankMultiplayerAction -= HandleMultiplayerAction;
            NetworkManager.Instance.OnTankMultiplayerHit -= HandleMultiplayerHit;
            NetworkManager.Instance.OnTankPlayerEliminated -= HandlePlayerEliminated;
            NetworkManager.Instance.OnTankMultiplayerGameOver -= HandleGameOver;
        }

        private void FrmTankMultiplayer_Load(object sender, EventArgs e)
        {
            _gameTimer = new System.Windows.Forms.Timer();
            _gameTimer.Interval = 33; // ~30 FPS
            _gameTimer.Tick += GameTimer_Tick;
            _gameTimer.Start();

            this.Focus();
            this.Activate();
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (_isGameEnded || _isEliminated) return;

            bool needsRedraw = false;
            bool positionChanged = false;

            // Process input
            if (_players.TryGetValue(_myId, out var me))
            {
                if (_keyUp) { MoveTank(me, 1); positionChanged = true; }
                if (_keyDown) { MoveTank(me, -1); positionChanged = true; }
                if (_keyLeft) { RotateTank(me, -1); positionChanged = true; }
                if (_keyRight) { RotateTank(me, 1); positionChanged = true; }

                if (positionChanged)
                {
                    TrySendPositionUpdate(me);
                    needsRedraw = true;
                }
            }

            // Update bullets
            if (_bullets.Count > 0)
            {
                UpdateBullets();
                needsRedraw = true;
            }

            if (needsRedraw)
            {
                this.Invalidate();
            }
        }

        private void MoveTank(PlayerState tank, int direction)
        {
            float rad = tank.Angle * (float)Math.PI / 180f;
            float newX = tank.X + (float)Math.Cos(rad) * TANK_SPEED * direction;
            float newY = tank.Y + (float)Math.Sin(rad) * TANK_SPEED * direction;

            if (newX >= TANK_SIZE / 2 && newX <= GAME_WIDTH - TANK_SIZE / 2 &&
                newY >= TANK_SIZE / 2 && newY <= GAME_HEIGHT - TANK_SIZE / 2)
            {
                tank.X = newX;
                tank.Y = newY;
            }
        }

        private void RotateTank(PlayerState tank, int direction)
        {
            tank.Angle += ROTATION_SPEED * direction;
            if (tank.Angle < 0) tank.Angle += 360;
            if (tank.Angle >= 360) tank.Angle -= 360;
        }

        private void TrySendPositionUpdate(PlayerState me)
        {
            var now = DateTime.Now;
            if ((now - _lastPacketSentTime).TotalMilliseconds < MIN_PACKET_INTERVAL_MS)
                return;

            float deltaX = Math.Abs(me.X - _lastSentX);
            float deltaY = Math.Abs(me.Y - _lastSentY);
            float deltaAngle = Math.Abs(me.Angle - _lastSentAngle);
            if (deltaAngle > 180) deltaAngle = 360 - deltaAngle;

            bool posChanged = deltaX >= POSITION_THRESHOLD || deltaY >= POSITION_THRESHOLD;
            bool angleChanged = deltaAngle >= ANGLE_THRESHOLD;

            if (!posChanged && !angleChanged) return;

            var packet = new TankMultiplayerActionPacket
            {
                GameID = _gameId,
                SenderID = _myId,
                ActionType = posChanged ? TankActionType.Move : TankActionType.Rotate,
                X = me.X,
                Y = me.Y,
                Angle = me.Angle
            };
            NetworkManager.Instance.SendPacket(packet);

            _lastSentX = me.X;
            _lastSentY = me.Y;
            _lastSentAngle = me.Angle;
            _lastPacketSentTime = now;
        }

        private void Shoot()
        {
            if (_isGameEnded || _isEliminated) return;
            if (!_players.TryGetValue(_myId, out var me)) return;

            var now = DateTime.Now;
            if ((now - _lastShootTime).TotalMilliseconds < SHOOT_COOLDOWN_MS)
                return;
            _lastShootTime = now;

            float rad = me.Angle * (float)Math.PI / 180f;
            float bulletX = me.X + (float)Math.Cos(rad) * TANK_SIZE / 2;
            float bulletY = me.Y + (float)Math.Sin(rad) * TANK_SIZE / 2;

            _bullets.Add(new Bullet
            {
                X = bulletX,
                Y = bulletY,
                Angle = me.Angle,
                OwnerID = _myId,
                Color = me.Color
            });

            var packet = new TankMultiplayerActionPacket
            {
                GameID = _gameId,
                SenderID = _myId,
                ActionType = TankActionType.Shoot,
                X = bulletX,
                Y = bulletY,
                Angle = me.Angle
            };
            NetworkManager.Instance.SendPacket(packet);
        }

        private void UpdateBullets()
        {
            const float COLLISION_DIST = TANK_SIZE / 2f;

            for (int i = _bullets.Count - 1; i >= 0; i--)
            {
                var bullet = _bullets[i];
                float rad = bullet.Angle * (float)Math.PI / 180f;
                bullet.X += (float)Math.Cos(rad) * BULLET_SPEED;
                bullet.Y += (float)Math.Sin(rad) * BULLET_SPEED;

                // Out of bounds
                if (bullet.X < 0 || bullet.X > GAME_WIDTH || bullet.Y < 0 || bullet.Y > GAME_HEIGHT)
                {
                    _bullets.RemoveAt(i);
                    continue;
                }

                // Collision detection (only for my bullets)
                if (bullet.OwnerID == _myId)
                {
                    foreach (var player in _players.Values)
                    {
                        if (player.PlayerID == _myId || player.IsEliminated) continue;

                        // Team check
                        if (_gameMode == 1 && _players.TryGetValue(_myId, out var me) && me.TeamID == player.TeamID)
                            continue;

                        float dx = bullet.X - player.X;
                        float dy = bullet.Y - player.Y;
                        float dist = (float)Math.Sqrt(dx * dx + dy * dy);

                        if (dist < COLLISION_DIST)
                        {
                            _bullets.RemoveAt(i);

                            // Send hit packet
                            var hitPacket = new TankMultiplayerHitPacket
                            {
                                GameID = _gameId,
                                ShooterID = _myId,
                                HitPlayerID = player.PlayerID,
                                Damage = 25
                            };
                            NetworkManager.Instance.SendPacket(hitPacket);
                            break;
                        }
                    }
                }
            }
        }

        private void HandleMultiplayerAction(TankMultiplayerActionPacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleMultiplayerAction(packet))); return; }

            if (packet.GameID != _gameId || packet.SenderID == _myId) return;
            if (!_players.TryGetValue(packet.SenderID, out var player)) return;

            switch (packet.ActionType)
            {
                case TankActionType.Move:
                case TankActionType.Rotate:
                    player.X = packet.X;
                    player.Y = packet.Y;
                    player.Angle = packet.Angle;
                    break;

                case TankActionType.Shoot:
                    _bullets.Add(new Bullet
                    {
                        X = packet.X,
                        Y = packet.Y,
                        Angle = packet.Angle,
                        OwnerID = packet.SenderID,
                        Color = player.Color
                    });
                    break;
            }

            this.Invalidate();
        }

        private void HandleMultiplayerHit(TankMultiplayerHitPacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleMultiplayerHit(packet))); return; }

            if (packet.GameID != _gameId) return;
            if (!_players.TryGetValue(packet.HitPlayerID, out var player)) return;

            player.Health = packet.RemainingHealth;
            if (player.Health < 0) player.Health = 0;

            UpdateScoreboard();
            this.Invalidate();
        }

        private void HandlePlayerEliminated(TankPlayerEliminatedPacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandlePlayerEliminated(packet))); return; }

            if (packet.GameID != _gameId) return;

            if (_players.TryGetValue(packet.EliminatedPlayerID, out var eliminated))
            {
                eliminated.IsEliminated = true;
                eliminated.Deaths++;
            }

            if (_players.TryGetValue(packet.KillerID, out var killer))
            {
                killer.Kills++;
            }

            if (packet.EliminatedPlayerID == _myId)
            {
                _isEliminated = true;
                ShowEliminatedMessage();
            }

            UpdateScoreboard();
            this.Invalidate();
        }

        private void HandleGameOver(TankMultiplayerGameOverPacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleGameOver(packet))); return; }

            if (packet.GameID != _gameId) return;

            _isGameEnded = true;
            _gameTimer?.Stop();

            ShowGameOverScreen(packet);
        }

        private void ShowEliminatedMessage()
        {
            lblGameOver.Text = "YOU WERE ELIMINATED!";
            lblGameOver.ForeColor = Color.FromArgb(237, 66, 69);
            lblGameOver.Location = new Point((GAME_WIDTH - lblGameOver.Width) / 2, 150);
            lblGameOver.Visible = true;
        }

        private void ShowGameOverScreen(TankMultiplayerGameOverPacket packet)
        {
            bool isWinner = packet.WinnerID == _myId || 
                (_gameMode == 1 && _players.TryGetValue(_myId, out var me) && me.TeamID == packet.WinnerTeam);

            lblGameOver.Text = isWinner ? "VICTORY!" : $"WINNER: {packet.WinnerName}";
            lblGameOver.ForeColor = isWinner ? Color.Gold : Color.White;
            lblGameOver.Location = new Point((GAME_WIDTH - lblGameOver.Width) / 2 - 50, 150);
            lblGameOver.Visible = true;

            btnExit.Visible = true;
            btnExit.BringToFront();

            // Show final scores
            UpdateScoreboard(packet.Scores);
        }

        private void UpdateScoreboard(List<TankGameScore> finalScores = null)
        {
            pnlScoreboard.Controls.Clear();

            var lblTitle = new Label
            {
                Text = "SCOREBOARD",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(40, 5)
            };
            pnlScoreboard.Controls.Add(lblTitle);

            int y = 30;
            var players = finalScores != null 
                ? finalScores.OrderBy(s => s.Rank).Select(s => _players.GetValueOrDefault(s.PlayerID)).Where(p => p != null)
                : _players.Values.OrderByDescending(p => p.Kills).ThenBy(p => p.Deaths);

            foreach (var player in players)
            {
                if (player == null) continue;

                var pnl = new Panel
                {
                    Size = new Size(170, 25),
                    Location = new Point(5, y),
                    BackColor = player.PlayerID == _myId ? Color.FromArgb(50, 255, 255, 255) : Color.Transparent
                };

                var lblColor = new Label
                {
                    Size = new Size(15, 15),
                    Location = new Point(5, 5),
                    BackColor = player.Color
                };
                pnl.Controls.Add(lblColor);

                string name = player.PlayerName.Length > 8 ? player.PlayerName.Substring(0, 8) : player.PlayerName;
                var lblName = new Label
                {
                    Text = name,
                    Font = new Font("Segoe UI", 9),
                    ForeColor = player.IsEliminated ? Color.Gray : Color.White,
                    AutoSize = true,
                    Location = new Point(25, 3)
                };
                pnl.Controls.Add(lblName);

                var lblStats = new Label
                {
                    Text = $"{player.Kills}K/{player.Deaths}D",
                    Font = new Font("Segoe UI", 8),
                    ForeColor = Color.LightGray,
                    AutoSize = true,
                    Location = new Point(110, 5)
                };
                pnl.Controls.Add(lblStats);

                pnlScoreboard.Controls.Add(pnl);
                y += 28;
            }

            pnlScoreboard.Height = Math.Max(y + 10, 100);
        }

        private void FrmTankMultiplayer_KeyDown(object sender, KeyEventArgs e)
        {
            if (_isGameEnded || _isEliminated) return;

            switch (e.KeyCode)
            {
                case Keys.W: case Keys.Up: _keyUp = true; break;
                case Keys.S: case Keys.Down: _keyDown = true; break;
                case Keys.A: case Keys.Left: _keyLeft = true; break;
                case Keys.D: case Keys.Right: _keyRight = true; break;
                case Keys.Space:
                    if (!_keyShoot) { _keyShoot = true; Shoot(); }
                    break;
            }
        }

        private void FrmTankMultiplayer_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W: case Keys.Up: _keyUp = false; break;
                case Keys.S: case Keys.Down: _keyDown = false; break;
                case Keys.A: case Keys.Left: _keyLeft = false; break;
                case Keys.D: case Keys.Right: _keyRight = false; break;
                case Keys.Space: _keyShoot = false; break;
            }
        }

        private void FrmTankMultiplayer_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw all players
            foreach (var player in _players.Values)
            {
                if (!player.IsEliminated)
                {
                    DrawTank(g, player);
                    DrawHealthBar(g, player);
                }
            }

            // Draw bullets
            foreach (var bullet in _bullets)
            {
                using (var brush = new SolidBrush(bullet.Color))
                {
                    g.FillEllipse(brush, bullet.X - 4, bullet.Y - 4, 8, 8);
                }
            }

            // Draw overlay when eliminated or game ended
            if (_isEliminated || _isGameEnded)
            {
                using (var brush = new SolidBrush(Color.FromArgb(128, 0, 0, 0)))
                {
                    g.FillRectangle(brush, 0, 0, GAME_WIDTH, GAME_HEIGHT);
                }
            }
        }

        private void DrawTank(Graphics g, PlayerState player)
        {
            float rad = player.Angle * (float)Math.PI / 180f;

            var matrix = g.Transform;
            g.TranslateTransform(player.X, player.Y);
            g.RotateTransform(player.Angle);

            // Tank body
            using (var brush = new SolidBrush(player.Color))
            {
                g.FillRectangle(brush, -TANK_SIZE / 2, -TANK_SIZE / 2, TANK_SIZE, TANK_SIZE);
            }
            using (var pen = new Pen(Color.Black, 2))
            {
                g.DrawRectangle(pen, -TANK_SIZE / 2, -TANK_SIZE / 2, TANK_SIZE, TANK_SIZE);
            }

            g.Transform = matrix;

            // Gun barrel
            float gunX = player.X + (float)Math.Cos(rad) * TANK_SIZE / 2;
            float gunY = player.Y + (float)Math.Sin(rad) * TANK_SIZE / 2;
            using (var pen = new Pen(player.Color, 4))
            {
                g.DrawLine(pen, player.X, player.Y, gunX, gunY);
            }

            // Player name
            using (var font = new Font("Segoe UI", 8, FontStyle.Bold))
            {
                string name = player.PlayerName.Length > 10 ? player.PlayerName.Substring(0, 10) : player.PlayerName;
                var size = g.MeasureString(name, font);
                g.DrawString(name, font, Brushes.White, player.X - size.Width / 2, player.Y - TANK_SIZE - 15);
            }
        }

        private void DrawHealthBar(Graphics g, PlayerState player)
        {
            int barWidth = 50;
            int barHeight = 6;
            float x = player.X - barWidth / 2;
            float y = player.Y + TANK_SIZE / 2 + 5;

            float percent = (float)player.Health / MAX_HEALTH;
            Color healthColor = percent > 0.5f ? Color.FromArgb(67, 181, 129) :
                               percent > 0.25f ? Color.FromArgb(250, 166, 26) :
                               Color.FromArgb(237, 66, 69);

            g.FillRectangle(Brushes.DarkGray, x, y, barWidth, barHeight);
            using (var brush = new SolidBrush(healthColor))
            {
                g.FillRectangle(brush, x, y, barWidth * percent, barHeight);
            }
            g.DrawRectangle(Pens.Black, x, y, barWidth, barHeight);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _gameTimer?.Stop();
            _gameTimer?.Dispose();
            UnregisterEvents();
            base.OnFormClosing(e);
        }
    }
}
