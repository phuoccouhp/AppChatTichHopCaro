using ChatApp.Shared;
using ChatAppClient.Helpers;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ChatAppClient.Forms
{
    public partial class frmTankGame : Form
    {
        private const int GAME_WIDTH = 800;
        private const int GAME_HEIGHT = 600;
        private const float TANK_SIZE = 40f;
        private const float TANK_SPEED = 3f;
        private const float ROTATION_SPEED = 5f;
        private const float BULLET_SPEED = 8f;
        private const int MAX_HEALTH = 100;

        private string _gameId;
        private string _opponentId;
        private string _myId;
        private bool _isMyTurn;
        private bool _isGameEnded = false;

        // Tank của tôi
        private float _myTankX = 100;
        private float _myTankY = 300;
        private float _myTankAngle = 0;
        private int _myHealth = MAX_HEALTH;

        // Tank đối thủ
        private float _opponentTankX = 700;
        private float _opponentTankY = 300;
        private float _opponentTankAngle = 180;
        private int _opponentHealth = MAX_HEALTH;

        // Đạn
        private class Bullet
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Angle { get; set; }
            public bool IsMine { get; set; }
        }
        private System.Collections.Generic.List<Bullet> _bullets = new System.Collections.Generic.List<Bullet>();

        private Timer _gameTimer;
        private bool _keyUp, _keyDown, _keyLeft, _keyRight, _keyShoot;

        public frmTankGame(string gameId, string opponentId, bool startsFirst)
        {
            InitializeComponent();
            _gameId = gameId;
            _opponentId = opponentId;
            _myId = NetworkManager.Instance.UserID;
            _isMyTurn = startsFirst;

            this.ClientSize = new Size(GAME_WIDTH, GAME_HEIGHT);
            this.Text = $"Tank Game - vs {opponentId}";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
        }

        private void frmTankGame_Load(object sender, EventArgs e)
        {
            this.Paint += FrmTankGame_Paint;
            this.KeyDown += FrmTankGame_KeyDown;
            this.KeyUp += FrmTankGame_KeyUp;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);

            _gameTimer = new Timer();
            _gameTimer.Interval = 16; // ~60 FPS
            _gameTimer.Tick += GameTimer_Tick;
            _gameTimer.Start();

            UpdateStatusLabel();
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (_isGameEnded || !_isMyTurn) return;

            // Xử lý di chuyển
            if (_keyUp) MoveTank(1);
            if (_keyDown) MoveTank(-1);
            if (_keyLeft) RotateTank(-1);
            if (_keyRight) RotateTank(1);

            // Cập nhật đạn
            UpdateBullets();

            this.Invalidate();
        }

        private void MoveTank(int direction)
        {
            float rad = _myTankAngle * (float)Math.PI / 180f;
            float newX = _myTankX + (float)Math.Cos(rad) * TANK_SPEED * direction;
            float newY = _myTankY + (float)Math.Sin(rad) * TANK_SPEED * direction;

            // Kiểm tra biên
            if (newX >= TANK_SIZE / 2 && newX <= GAME_WIDTH - TANK_SIZE / 2 &&
                newY >= TANK_SIZE / 2 && newY <= GAME_HEIGHT - TANK_SIZE / 2)
            {
                _myTankX = newX;
                _myTankY = newY;

                // Gửi action đến server
                var action = new TankActionPacket
                {
                    GameID = _gameId,
                    SenderID = _myId,
                    ActionType = TankActionType.Move,
                    X = _myTankX,
                    Y = _myTankY,
                    Angle = _myTankAngle
                };
                NetworkManager.Instance.SendPacket(action);
            }
        }

        private void RotateTank(int direction)
        {
            _myTankAngle += ROTATION_SPEED * direction;
            if (_myTankAngle < 0) _myTankAngle += 360;
            if (_myTankAngle >= 360) _myTankAngle -= 360;

            var action = new TankActionPacket
            {
                GameID = _gameId,
                SenderID = _myId,
                ActionType = TankActionType.Rotate,
                X = _myTankX,
                Y = _myTankY,
                Angle = _myTankAngle
            };
            NetworkManager.Instance.SendPacket(action);
        }

        private void Shoot()
        {
            if (_isGameEnded || !_isMyTurn) return;

            float rad = _myTankAngle * (float)Math.PI / 180f;
            float bulletX = _myTankX + (float)Math.Cos(rad) * TANK_SIZE / 2;
            float bulletY = _myTankY + (float)Math.Sin(rad) * TANK_SIZE / 2;

            _bullets.Add(new Bullet
            {
                X = bulletX,
                Y = bulletY,
                Angle = _myTankAngle,
                IsMine = true
            });

            var action = new TankActionPacket
            {
                GameID = _gameId,
                SenderID = _myId,
                ActionType = TankActionType.Shoot,
                X = bulletX,
                Y = bulletY,
                Angle = _myTankAngle
            };
            NetworkManager.Instance.SendPacket(action);
        }

        private void UpdateBullets()
        {
            for (int i = _bullets.Count - 1; i >= 0; i--)
            {
                var bullet = _bullets[i];
                float rad = bullet.Angle * (float)Math.PI / 180f;
                bullet.X += (float)Math.Cos(rad) * BULLET_SPEED;
                bullet.Y += (float)Math.Sin(rad) * BULLET_SPEED;

                // Kiểm tra biên
                if (bullet.X < 0 || bullet.X > GAME_WIDTH || bullet.Y < 0 || bullet.Y > GAME_HEIGHT)
                {
                    _bullets.RemoveAt(i);
                    continue;
                }

                // Kiểm tra va chạm với tank
                if (bullet.IsMine)
                {
                    // Kiểm tra va chạm với tank đối thủ
                    float dist = (float)Math.Sqrt(Math.Pow(bullet.X - _opponentTankX, 2) + Math.Pow(bullet.Y - _opponentTankY, 2));
                    if (dist < TANK_SIZE / 2)
                    {
                        _bullets.RemoveAt(i);
                        // Gửi hit packet đến server
                        var hitPacket = new TankHitPacket
                        {
                            GameID = _gameId,
                            HitPlayerID = _opponentId,
                            Damage = 10,
                            RemainingHealth = _opponentHealth - 10
                        };
                        NetworkManager.Instance.SendPacket(hitPacket);
                        continue;
                    }
                }
                else
                {
                    // Kiểm tra va chạm với tank của tôi
                    float dist = (float)Math.Sqrt(Math.Pow(bullet.X - _myTankX, 2) + Math.Pow(bullet.Y - _myTankY, 2));
                    if (dist < TANK_SIZE / 2)
                    {
                        _bullets.RemoveAt(i);
                        // Gửi hit packet đến server
                        var hitPacket = new TankHitPacket
                        {
                            GameID = _gameId,
                            HitPlayerID = _myId,
                            Damage = 10,
                            RemainingHealth = _myHealth - 10
                        };
                        NetworkManager.Instance.SendPacket(hitPacket);
                        continue;
                    }
                }
            }
        }

        private void FrmTankGame_KeyDown(object sender, KeyEventArgs e)
        {
            if (_isGameEnded || !_isMyTurn) return;

            switch (e.KeyCode)
            {
                case Keys.W: _keyUp = true; break;
                case Keys.S: _keyDown = true; break;
                case Keys.A: _keyLeft = true; break;
                case Keys.D: _keyRight = true; break;
                case Keys.Space:
                    if (!_keyShoot)
                    {
                        _keyShoot = true;
                        Shoot();
                    }
                    break;
            }
        }

        private void FrmTankGame_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.W: _keyUp = false; break;
                case Keys.S: _keyDown = false; break;
                case Keys.A: _keyLeft = false; break;
                case Keys.D: _keyRight = false; break;
                case Keys.Space: _keyShoot = false; break;
            }
        }

        private void FrmTankGame_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Vẽ nền
            g.Clear(Color.DarkGreen);

            // Vẽ tank của tôi
            DrawTank(g, _myTankX, _myTankY, _myTankAngle, Color.Blue);

            // Vẽ tank đối thủ
            DrawTank(g, _opponentTankX, _opponentTankY, _opponentTankAngle, Color.Red);

            // Vẽ đạn
            foreach (var bullet in _bullets)
            {
                g.FillEllipse(Brushes.Yellow, bullet.X - 3, bullet.Y - 3, 6, 6);
            }

            // Vẽ thanh máu
            DrawHealthBar(g, 10, 10, _myHealth, MAX_HEALTH, Color.Blue, "Bạn");
            DrawHealthBar(g, GAME_WIDTH - 210, 10, _opponentHealth, MAX_HEALTH, Color.Red, "Đối thủ");
        }

        private void DrawTank(Graphics g, float x, float y, float angle, Color color)
        {
            float rad = angle * (float)Math.PI / 180f;
            PointF center = new PointF(x, y);

            // Vẽ thân tank (hình chữ nhật)
            Matrix m = g.Transform;
            g.TranslateTransform(x, y);
            g.RotateTransform(angle);
            g.FillRectangle(new SolidBrush(color), -TANK_SIZE / 2, -TANK_SIZE / 2, TANK_SIZE, TANK_SIZE);
            g.Transform = m;

            // Vẽ nòng súng
            float gunX = x + (float)Math.Cos(rad) * TANK_SIZE / 2;
            float gunY = y + (float)Math.Sin(rad) * TANK_SIZE / 2;
            g.DrawLine(new Pen(color, 4), x, y, gunX, gunY);
        }

        private void DrawHealthBar(Graphics g, int x, int y, int current, int max, Color color, string label)
        {
            int barWidth = 200;
            int barHeight = 20;
            float percent = (float)current / max;

            // Nền
            g.FillRectangle(Brushes.DarkGray, x, y, barWidth, barHeight);
            // Máu
            g.FillRectangle(new SolidBrush(color), x, y, barWidth * percent, barHeight);
            // Viền
            g.DrawRectangle(Pens.Black, x, y, barWidth, barHeight);
            // Text
            g.DrawString($"{label}: {current}/{max}", SystemFonts.DefaultFont, Brushes.White, x, y + 25);
        }

        private void UpdateStatusLabel()
        {
            if (_isGameEnded)
            {
                this.Text = $"Tank Game - Kết thúc";
            }
            else
            {
                this.Text = $"Tank Game - {(_isMyTurn ? "Lượt bạn" : "Lượt đối thủ")}";
            }
        }

        // Nhận action từ đối thủ
        public void ReceiveOpponentAction(TankActionPacket packet)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ReceiveOpponentAction(packet)));
                return;
            }

            if (_isGameEnded) return;

            switch (packet.ActionType)
            {
                case TankActionType.Move:
                case TankActionType.Rotate:
                    _opponentTankX = packet.X;
                    _opponentTankY = packet.Y;
                    _opponentTankAngle = packet.Angle;
                    this.Invalidate();
                    break;
                case TankActionType.Shoot:
                    _bullets.Add(new Bullet
                    {
                        X = packet.X,
                        Y = packet.Y,
                        Angle = packet.Angle,
                        IsMine = false
                    });
                    break;
            }
        }

        // Nhận thông báo bị bắn trúng
        public void ReceiveHit(TankHitPacket packet)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ReceiveHit(packet)));
                return;
            }

            if (packet.HitPlayerID == _myId)
            {
                _myHealth = packet.RemainingHealth;
            }
            else
            {
                _opponentHealth = packet.RemainingHealth;
            }

            if (packet.IsGameOver)
            {
                _isGameEnded = true;
                _isMyTurn = false;
                string message = packet.WinnerID == _myId ? "Bạn đã thắng!" : "Bạn đã thua!";
                MessageBox.Show(message, "Kết thúc trò chơi");
            }

            UpdateStatusLabel();
            this.Invalidate();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _gameTimer?.Stop();
            _gameTimer?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
