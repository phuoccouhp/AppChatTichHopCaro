using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChatAppClient.Forms
{
    public partial class FrmTank : Form
    {
        private int tankSpeed = 5;
        private int bulletSpeed = 10;

        private bool goUp, goDown, goLeft, goRight;
        private string facing = "Up";

        // New fields to identify game and opponent
        private string _gameId;
        private string _opponentId;
        private bool _isPlayer1;

        public FrmTank()
        {
            InitializeComponent();
            SetupEvents();
        }

        // New constructor used by frmHome
        public FrmTank(string gameId, string opponentId, bool isPlayer1) : this()
        {
            _gameId = gameId;
            _opponentId = opponentId;
            _isPlayer1 = isPlayer1;

            // Optionally adjust starting position based on player
            if (isPlayer1)
            {
                tank.BackColor = Color.LimeGreen;
                tank.Location = new Point(50, 50);
            }
            else
            {
                tank.BackColor = Color.Red;
                tank.Location = new Point(this.ClientSize.Width - 100, this.ClientSize.Height - 100);
            }

            // Create an opponent tank visual
            EnsureOpponentTankExists();
        }

        private void SetupEvents()
        {
            gameTimer.Tick += GameTimer_Tick;
            this.KeyDown += KeyIsDown;
            this.KeyUp += KeyIsUp;
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            // --- Di chuyển Tank ---
            if (goLeft && tank.Left > 0)
            {
                tank.Left -= tankSpeed;
                facing = "Left";
            }
            if (goRight && tank.Right < this.ClientSize.Width)
            {
                tank.Left += tankSpeed;
                facing = "Right";
            }
            if (goUp && tank.Top > 0)
            {
                tank.Top -= tankSpeed;
                facing = "Up";
            }
            if (goDown && tank.Bottom < this.ClientSize.Height)
            {
                tank.Top += tankSpeed;
                facing = "Down";
            }

            // --- Di chuyển Bullet ---
            // Use a copy of controls to avoid modification during iteration
            var controls = new List<Control>();
            foreach (Control c in this.Controls) controls.Add(c);

            foreach (Control c in controls)
            {
                if (c is PictureBox && c.Tag?.ToString() == "bullet")
                {
                    string dir = c.Name; // ⭐ FIX: dùng Name để lưu hướng

                    if (dir == "Up") c.Top -= bulletSpeed;
                    if (dir == "Down") c.Top += bulletSpeed;
                    if (dir == "Left") c.Left -= bulletSpeed;
                    if (dir == "Right") c.Left += bulletSpeed;

                    // Xoá đạn
                    if (c.Top < 0 || c.Top > this.ClientSize.Height ||
                        c.Left < 0 || c.Left > this.ClientSize.Width)
                    {
                        c.Dispose();
                    }
                }
            }
        }

        private void KeyIsDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left) goLeft = true;
            if (e.KeyCode == Keys.Right) goRight = true;
            if (e.KeyCode == Keys.Up) goUp = true;
            if (e.KeyCode == Keys.Down) goDown = true;
        }

        private void KeyIsUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left) goLeft = false;
            if (e.KeyCode == Keys.Right) goRight = false;
            if (e.KeyCode == Keys.Up) goUp = false;
            if (e.KeyCode == Keys.Down) goDown = false;

            if (e.KeyCode == Keys.Space)
                Shoot();
        }

        private void Shoot()
        {
            PictureBox bullet = new PictureBox
            {
                Size = new Size(8, 8),
                BackColor = Color.Yellow,
                Tag = "bullet",
                Name = facing // ⭐ Hướng lưu ở Name
            };

            if (facing == "Up") bullet.Location = new Point(tank.Left + 23, tank.Top - 10);
            if (facing == "Down") bullet.Location = new Point(tank.Left + 23, tank.Bottom + 5);
            if (facing == "Left") bullet.Location = new Point(tank.Left - 10, tank.Top + 23);
            if (facing == "Right") bullet.Location = new Point(tank.Right + 5, tank.Top + 23);

            this.Controls.Add(bullet);
            bullet.BringToFront();
        }

        // Ensure there is a visual representation for the opponent tank
        private void EnsureOpponentTankExists()
        {
            if (this.Controls.ContainsKey("opponentTank")) return;

            PictureBox opponent = new PictureBox
            {
                Name = "opponentTank",
                Size = new Size(50, 50),
                BackColor = Color.Blue,
                Tag = "opponent"
            };

            // Place off-screen initially
            opponent.Location = new Point(this.ClientSize.Width - 100, this.ClientSize.Height - 100);
            this.Controls.Add(opponent);
            opponent.BringToFront();
        }

        // Method called by frmHome when receiving opponent actions
        public void UpdateOpponent(int x, int y, ChatApp.Shared.TankDirection direction, bool isShooting)
        {
            if (this.IsDisposed) return;

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => UpdateOpponent(x, y, direction, isShooting)));
                return;
            }

            EnsureOpponentTankExists();
            var opponent = this.Controls["opponentTank"] as PictureBox;
            if (opponent != null)
            {
                // Clamp within bounds
                int nx = Math.Max(0, Math.Min(this.ClientSize.Width - opponent.Width, x));
                int ny = Math.Max(0, Math.Min(this.ClientSize.Height - opponent.Height, y));
                opponent.Location = new Point(nx, ny);
            }

            if (isShooting)
            {
                // Spawn a bullet at opponent location moving in given direction
                string dir = direction.ToString();
                PictureBox bullet = new PictureBox
                {
                    Size = new Size(8, 8),
                    BackColor = Color.OrangeRed,
                    Tag = "bullet",
                    Name = dir
                };

                // Start bullet from center of opponent tank
                if (opponent != null)
                {
                    bullet.Location = new Point(opponent.Left + opponent.Width / 2, opponent.Top + opponent.Height / 2);
                }
                else
                {
                    bullet.Location = new Point(x, y);
                }

                this.Controls.Add(bullet);
                bullet.BringToFront();
            }
        }
    }
}
