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
        
        // Score tracking
        private int _myScore = 0;
        private int _myHits = 0;

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

        private System.Windows.Forms.Timer? _gameTimer;
        private bool _keyUp, _keyDown, _keyLeft, _keyRight, _keyShoot;
        
        // Throttle để tránh gửi quá nhiều packet
        private float _lastSentX = -1;
        private float _lastSentY = -1;
        private float _lastSentAngle = -1;
        private const float POSITION_THRESHOLD = 5f; // ✅ [FIX] Tăng từ 2f lên 5f để gửi ít packet hơn
        private const float ANGLE_THRESHOLD = 10f; // ✅ [FIX] Tăng từ 5f lên 10f để gửi ít packet hơn

        public frmTankGame(string gameId, string opponentId, bool startsFirst)
        {
            InitializeComponent();
            _gameId = gameId;
            _opponentId = opponentId;
            _myId = NetworkManager.Instance.UserID ?? "";
            _isMyTurn = startsFirst;
            
            // Khởi tạo last sent values
            _lastSentX = _myTankX;
            _lastSentY = _myTankY;
            _lastSentAngle = _myTankAngle;

            this.ClientSize = new Size(GAME_WIDTH, GAME_HEIGHT);
            this.Text = $"Tank Game - vs {opponentId}";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.KeyPreview = true; // QUAN TRỌNG: Cho phép form nhận key events
            this.BackColor = Color.DarkGreen; // Đặt màu nền
            
            // QUAN TRỌNG: Kết nối Load event
            this.Load += frmTankGame_Load;
            
            // Gán KeyDown, KeyUp ngay từ constructor
            this.KeyDown += FrmTankGame_KeyDown;
            this.KeyUp += FrmTankGame_KeyUp;
            
            // Gán sự kiện cho buttons
            if (btnRematch != null)
                btnRematch.Click += BtnRematch_Click;
            if (btnExit != null)
                btnExit.Click += BtnExit_Click;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.DoubleBuffered = true;
            
            // Force paint ngay lập tức
            this.Invalidate();
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            FrmTankGame_Paint(this, e);
        }
        
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Vẽ nền trước
            e.Graphics.Clear(Color.DarkGreen);
        }

        private void frmTankGame_Load(object sender, EventArgs e)
        {
            // Khởi tạo timer
            _gameTimer = new System.Windows.Forms.Timer();
            _gameTimer.Interval = 33; // ✅ [FIX] Giảm từ 16ms xuống 33ms (30 FPS) - vẫn mượt nhưng ít lag hơn
            _gameTimer.Tick += GameTimer_Tick;
            _gameTimer.Start();

            UpdateStatusLabel();
            UpdateScoreLabels();
            
            // Focus vào form để nhận key events
            this.Focus();
            this.Activate();
            
            // Force paint ngay lập tức
            this.Invalidate();
        }
        
        private void UpdateScoreLabels()
        {
            if (lblScore != null)
                lblScore.Text = $"Score: {_myScore}";
            if (lblHits != null)
                lblHits.Text = $"Hits: {_myHits}";
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (_isGameEnded) return;
            
            bool needsRedraw = false;
            
            // Tank Game là real-time, cả 2 người có thể di chuyển đồng thời
            // Xử lý di chuyển (không cần kiểm tra _isMyTurn)
            if (_keyUp) { MoveTank(1); needsRedraw = true; }
            if (_keyDown) { MoveTank(-1); needsRedraw = true; }
            if (_keyLeft) { RotateTank(-1); needsRedraw = true; }
            if (_keyRight) { RotateTank(1); needsRedraw = true; }

            // Cập nhật đạn (chỉ khi có đạn)
            if (_bullets.Count > 0)
            {
                UpdateBullets();
                needsRedraw = true;
            }

            // ✅ [FIX] Chỉ vẽ lại khi thực sự cần thiết
            if (needsRedraw)
            {
                this.Invalidate();
            }
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

                // Chỉ gửi packet nếu vị trí thay đổi đáng kể (throttle)
                float deltaX = Math.Abs(_myTankX - _lastSentX);
                float deltaY = Math.Abs(_myTankY - _lastSentY);
                
                if (_lastSentX < 0 || _lastSentY < 0 || deltaX >= POSITION_THRESHOLD || deltaY >= POSITION_THRESHOLD)
                {
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
                    
                    _lastSentX = _myTankX;
                    _lastSentY = _myTankY;
                    _lastSentAngle = _myTankAngle;
                }
            }
        }

        private void RotateTank(int direction)
        {
            _myTankAngle += ROTATION_SPEED * direction;
            if (_myTankAngle < 0) _myTankAngle += 360;
            if (_myTankAngle >= 360) _myTankAngle -= 360;

            // Chỉ gửi packet nếu góc thay đổi đáng kể (throttle)
            float deltaAngle = Math.Abs(_myTankAngle - _lastSentAngle);
            if (deltaAngle > 180) deltaAngle = 360 - deltaAngle; // Xử lý wrap-around
            
            if (_lastSentAngle < 0 || deltaAngle >= ANGLE_THRESHOLD)
            {
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
                
                _lastSentX = _myTankX;
                _lastSentY = _myTankY;
                _lastSentAngle = _myTankAngle;
            }
        }

        private void Shoot()
        {
            if (_isGameEnded) return; // Tank Game là real-time, không cần kiểm tra turn

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
            const float TANK_HALF_SIZE = TANK_SIZE / 2f;
            const float COLLISION_DIST_SQ = TANK_HALF_SIZE * TANK_HALF_SIZE; // ✅ [FIX] Dùng distance squared để tránh sqrt tốn kém
            
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

                // ✅ [FIX] Tối ưu collision detection - dùng distance squared thay vì sqrt
                if (bullet.IsMine)
                {
                    // Kiểm tra va chạm với tank đối thủ
                    float dx = bullet.X - _opponentTankX;
                    float dy = bullet.Y - _opponentTankY;
                    float distSq = dx * dx + dy * dy; // Không cần sqrt - nhanh hơn nhiều
                    
                    if (distSq < COLLISION_DIST_SQ)
                    {
                        _bullets.RemoveAt(i);
                        _myHits++;
                        UpdateScoreLabels();
                        
                        var hitPacket = new TankHitPacket
                        {
                            GameID = _gameId,
                            HitPlayerID = _opponentId,
                            Damage = 10,
                            RemainingHealth = 0,
                            IsGameOver = false,
                            WinnerID = null
                        };
                        NetworkManager.Instance.SendPacket(hitPacket);
                        continue;
                    }
                }
                else
                {
                    // Kiểm tra va chạm với tank của tôi
                    float dx = bullet.X - _myTankX;
                    float dy = bullet.Y - _myTankY;
                    float distSq = dx * dx + dy * dy; // Không cần sqrt - nhanh hơn nhiều
                    
                    if (distSq < COLLISION_DIST_SQ)
                    {
                        _bullets.RemoveAt(i);
                        var hitPacket = new TankHitPacket
                        {
                            GameID = _gameId,
                            HitPlayerID = _myId,
                            Damage = 10,
                            RemainingHealth = 0,
                            IsGameOver = false,
                            WinnerID = null
                        };
                        NetworkManager.Instance.SendPacket(hitPacket);
                        continue;
                    }
                }
            }
        }

        private void FrmTankGame_KeyDown(object sender, KeyEventArgs e)
        {
            if (_isGameEnded) return; // Tank Game là real-time, không cần kiểm tra turn

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
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Vẽ nền (nếu chưa được vẽ trong OnPaintBackground)
            if (this.BackColor != Color.DarkGreen)
            {
                g.Clear(Color.DarkGreen);
            }

            // Vẽ tank của tôi
            DrawTank(g, _myTankX, _myTankY, _myTankAngle, Color.Blue);

            // Vẽ tank đối thủ
            DrawTank(g, _opponentTankX, _opponentTankY, _opponentTankAngle, Color.Red);

            // Vẽ đạn
            foreach (var bullet in _bullets)
            {
                if (bullet.X >= 0 && bullet.X <= GAME_WIDTH && bullet.Y >= 0 && bullet.Y <= GAME_HEIGHT)
                {
                    using (SolidBrush brush = new SolidBrush(bullet.IsMine ? Color.Yellow : Color.Orange))
                    {
                        g.FillEllipse(brush, bullet.X - 4, bullet.Y - 4, 8, 8);
                    }
                    using (Pen pen = new Pen(Color.Black, 1))
                    {
                        g.DrawEllipse(pen, bullet.X - 4, bullet.Y - 4, 8, 8);
                    }
                }
            }

            // Vẽ thanh máu (di chuyển xuống dưới để không che label)
            DrawHealthBar(g, 10, 60, _myHealth, MAX_HEALTH, Color.Blue, "Bạn");
            DrawHealthBar(g, GAME_WIDTH - 210, 60, _opponentHealth, MAX_HEALTH, Color.Red, "Đối thủ");
            
            // Vẽ overlay khi game kết thúc
            if (_isGameEnded)
            {
                using (SolidBrush overlayBrush = new SolidBrush(Color.FromArgb(128, 0, 0, 0)))
                {
                    g.FillRectangle(overlayBrush, 0, 0, GAME_WIDTH, GAME_HEIGHT);
                }
                
                // Vẽ text kết thúc
                using (Font font = new Font("Segoe UI", 24, FontStyle.Bold))
                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    string gameOverText = _myHealth > 0 ? "BẠN THẮNG!" : "BẠN THUA!";
                    SizeF textSize = g.MeasureString(gameOverText, font);
                    float x = (GAME_WIDTH - textSize.Width) / 2;
                    float y = 200;
                    g.DrawString(gameOverText, font, textBrush, x, y);
                }
            }
        }

        private void DrawTank(Graphics g, float x, float y, float angle, Color color)
        {
            // Kiểm tra bounds
            if (x < 0 || x > GAME_WIDTH || y < 0 || y > GAME_HEIGHT) return;
            
            float rad = angle * (float)Math.PI / 180f;

            // Vẽ thân tank (hình chữ nhật)
            Matrix m = g.Transform;
            g.TranslateTransform(x, y);
            g.RotateTransform(angle);
            
            // Vẽ thân tank với viền đen để dễ nhìn
            using (SolidBrush brush = new SolidBrush(color))
            {
                g.FillRectangle(brush, -TANK_SIZE / 2, -TANK_SIZE / 2, TANK_SIZE, TANK_SIZE);
            }
            using (Pen pen = new Pen(Color.Black, 2))
            {
                g.DrawRectangle(pen, -TANK_SIZE / 2, -TANK_SIZE / 2, TANK_SIZE, TANK_SIZE);
            }
            
            g.Transform = m;

            // Vẽ nòng súng
            float gunX = x + (float)Math.Cos(rad) * TANK_SIZE / 2;
            float gunY = y + (float)Math.Sin(rad) * TANK_SIZE / 2;
            using (Pen gunPen = new Pen(color, 4))
            {
                g.DrawLine(gunPen, x, y, gunX, gunY);
            }

            // Vẽ mũi tên chỉ hướng bắn
            DrawArrow(g, gunX, gunY, angle, color);
        }

        private void DrawArrow(Graphics g, float x, float y, float angle, Color color)
        {
            float rad = angle * (float)Math.PI / 180f;
            const float ARROW_SIZE = 12f;
            const float ARROW_LENGTH = 8f;

            // Tính toán 3 điểm của mũi tên (hình tam giác)
            // Điểm đầu mũi tên (hướng về phía bắn)
            float tipX = x + (float)Math.Cos(rad) * ARROW_LENGTH;
            float tipY = y + (float)Math.Sin(rad) * ARROW_LENGTH;

            // Hai điểm đuôi mũi tên
            float tailAngle1 = angle + 150f; // 150 độ từ hướng chính
            float tailAngle2 = angle - 150f; // -150 độ từ hướng chính
            float rad1 = tailAngle1 * (float)Math.PI / 180f;
            float rad2 = tailAngle2 * (float)Math.PI / 180f;

            float tail1X = x + (float)Math.Cos(rad1) * ARROW_SIZE;
            float tail1Y = y + (float)Math.Sin(rad1) * ARROW_SIZE;
            float tail2X = x + (float)Math.Cos(rad2) * ARROW_SIZE;
            float tail2Y = y + (float)Math.Sin(rad2) * ARROW_SIZE;

            // Vẽ mũi tên (hình tam giác)
            PointF[] arrowPoints = new PointF[]
            {
                new PointF(tipX, tipY),
                new PointF(tail1X, tail1Y),
                new PointF(tail2X, tail2Y)
            };

            using (SolidBrush brush = new SolidBrush(color))
            {
                g.FillPolygon(brush, arrowPoints);
            }
            using (Pen pen = new Pen(Color.Black, 1))
            {
                g.DrawPolygon(pen, arrowPoints);
            }
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
                this.Text = $"Tank Game - vs {_opponentId} | HP: {_myHealth}/{MAX_HEALTH}";
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
            
            if (packet == null || string.IsNullOrEmpty(packet.GameID) || packet.GameID != _gameId) return;
            
            // Kiểm tra SenderID phải là đối thủ
            if (packet.SenderID != _opponentId) return;

            switch (packet.ActionType)
            {
                case TankActionType.Move:
                    // Cập nhật vị trí đối thủ - kiểm tra bounds kỹ hơn
                    if (packet.X >= TANK_SIZE / 2 && packet.X <= GAME_WIDTH - TANK_SIZE / 2 &&
                        packet.Y >= TANK_SIZE / 2 && packet.Y <= GAME_HEIGHT - TANK_SIZE / 2)
                    {
                        _opponentTankX = packet.X;
                        _opponentTankY = packet.Y;
                        _opponentTankAngle = packet.Angle;
                        // Normalize angle
                        while (_opponentTankAngle < 0) _opponentTankAngle += 360;
                        while (_opponentTankAngle >= 360) _opponentTankAngle -= 360;
                        this.Invalidate();
                    }
                    break;
                case TankActionType.Rotate:
                    // Cập nhật góc xoay và vị trí đối thủ
                    if (packet.X >= TANK_SIZE / 2 && packet.X <= GAME_WIDTH - TANK_SIZE / 2 &&
                        packet.Y >= TANK_SIZE / 2 && packet.Y <= GAME_HEIGHT - TANK_SIZE / 2)
                    {
                        _opponentTankX = packet.X;
                        _opponentTankY = packet.Y;
                        _opponentTankAngle = packet.Angle;
                        // Normalize angle
                        while (_opponentTankAngle < 0) _opponentTankAngle += 360;
                        while (_opponentTankAngle >= 360) _opponentTankAngle -= 360;
                        this.Invalidate();
                    }
                    break;
                case TankActionType.Shoot:
                    // Thêm đạn của đối thủ
                    if (packet.X >= 0 && packet.X <= GAME_WIDTH && packet.Y >= 0 && packet.Y <= GAME_HEIGHT)
                    {
                        _bullets.Add(new Bullet
                        {
                            X = packet.X,
                            Y = packet.Y,
                            Angle = packet.Angle,
                            IsMine = false
                        });
                        this.Invalidate();
                    }
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

            if (packet == null || string.IsNullOrEmpty(packet.GameID) || packet.GameID != _gameId) return;

            if (packet.HitPlayerID == _myId)
            {
                _myHealth = packet.RemainingHealth;
                if (_myHealth < 0) _myHealth = 0;
            }
            else if (packet.HitPlayerID == _opponentId)
            {
                _opponentHealth = packet.RemainingHealth;
                if (_opponentHealth < 0) _opponentHealth = 0;
            }

            if (packet.IsGameOver)
            {
                _isGameEnded = true;
                bool isWinner = (packet.WinnerID != null && packet.WinnerID == _myId);
                if (isWinner)
                {
                    _myScore += 100; // Bonus điểm khi thắng
                    UpdateScoreLabels();
                }
                
                // Hiển thị buttons chơi lại và thoát
                ShowGameOverButtons();
            }

            UpdateStatusLabel();
            this.Invalidate();
        }
        
        private void ShowGameOverButtons()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(ShowGameOverButtons));
                return;
            }
            
            if (btnRematch != null)
            {
                btnRematch.Visible = true;
                btnRematch.Enabled = true;
                btnRematch.BringToFront();
            }
            if (btnExit != null)
            {
                btnExit.Visible = true;
                btnExit.BringToFront();
            }
        }
        
        private void BtnRematch_Click(object sender, EventArgs e)
        {
            if (!_isGameEnded) return;
            
            // Kiểm tra GameID và UserID hợp lệ
            if (string.IsNullOrEmpty(_gameId) || string.IsNullOrEmpty(_myId))
            {
                MessageBox.Show("Lỗi: Thông tin game không hợp lệ. Không thể gửi yêu cầu chơi lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            // Gửi rematch request
            var request = new RematchRequestPacket
            {
                GameID = _gameId,
                SenderID = _myId
            };
            
            try
            {
                if (NetworkManager.Instance.SendPacket(request))
                {
                    if (btnRematch != null)
                    {
                        btnRematch.Enabled = false;
                        btnRematch.Text = "Đang chờ...";
                    }
                }
                else
                {
                    MessageBox.Show("Không thể gửi yêu cầu chơi lại. Vui lòng kiểm tra kết nối.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    if (btnRematch != null)
                    {
                        btnRematch.Enabled = true;
                        btnRematch.Text = "Chơi Lại";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi gửi yêu cầu chơi lại: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (btnRematch != null)
                {
                    btnRematch.Enabled = true;
                    btnRematch.Text = "Chơi Lại";
                }
            }
        }
        
        private void BtnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
        public void HandleRematchRequest(RematchRequestPacket request)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => HandleRematchRequest(request)));
                return;
            }
            
            if (!_isGameEnded) return;
            
            DialogResult result = MessageBox.Show(
                "Đối thủ muốn chơi lại. Bạn có đồng ý?",
                "Yêu Cầu Chơi Lại",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);
            
            bool accepted = (result == DialogResult.Yes);
            var response = new RematchResponsePacket
            {
                GameID = _gameId,
                SenderID = _myId,
                ReceiverID = request.SenderID,
                Accepted = accepted
            };
            NetworkManager.Instance.SendPacket(response);
            
            if (accepted && btnRematch != null)
            {
                btnRematch.Enabled = false;
                btnRematch.Text = "Đang chờ...";
            }
        }
        
        public void HandleRematchResponse(RematchResponsePacket response)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => HandleRematchResponse(response)));
                return;
            }
            
            if (!response.Accepted)
            {
                MessageBox.Show("Đối thủ đã từ chối chơi lại.", "Thông báo");
                if (btnRematch != null)
                {
                    btnRematch.Enabled = true;
                    btnRematch.Text = "Chơi Lại";
                }
            }
        }
        
        public void HandleTankGameReset(TankStartPacket packet)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => HandleTankGameReset(packet)));
                return;
            }
            
            // Kiểm tra packet hợp lệ
            if (packet == null || string.IsNullOrEmpty(packet.GameID))
            {
                MessageBox.Show("Lỗi: Thông tin reset game không hợp lệ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            // Kiểm tra GameID khớp
            if (packet.GameID != _gameId)
            {
                // GameID không khớp, có thể là game mới hoặc lỗi
                return;
            }
            
            // Reset game state
            _isGameEnded = false;
            _myHealth = MAX_HEALTH;
            _opponentHealth = MAX_HEALTH;
            _myScore = 0;
            _myHits = 0;
            _bullets.Clear();
            
            // Reset vị trí tank
            _myTankX = 100f;
            _myTankY = 300f;
            _myTankAngle = 0f;
            _opponentTankX = 700f;
            _opponentTankY = 300f;
            _opponentTankAngle = 180f;
            
            // Cập nhật _isMyTurn dựa trên StartsFirst
            _isMyTurn = packet.StartsFirst;
            
            // Reset last sent values
            _lastSentX = _myTankX;
            _lastSentY = _myTankY;
            _lastSentAngle = _myTankAngle;
            
            // Ẩn buttons
            if (btnRematch != null)
            {
                btnRematch.Visible = false;
                btnRematch.Enabled = true;
                btnRematch.Text = "Chơi Lại";
            }
            if (btnExit != null)
            {
                btnExit.Visible = false;
            }
            
            // Khởi động lại timer nếu đã dừng
            if (_gameTimer != null && !_gameTimer.Enabled)
            {
                _gameTimer.Start();
            }
            
            UpdateStatusLabel();
            UpdateScoreLabels();
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
