using System;
using System.Drawing;
using System.Windows.Forms;

namespace CaroNetClient
{
    // Không cần partial vì không dùng Designer
    public class Form1 : Form
    {
        // --- Cấu hình bàn cờ ---
        const int N = 20;      // 20x20 ô
        const int CELL = 28;   // mỗi ô 28px

        // --- Trạng thái trò chơi ---
        int[,] board = new int[N, N]; // 0: trống, 1: X, 2: O
        string myUser = "thuy";
        string mySymbol = "X";        // "X" hoặc "O"
        bool myTurn = false;          // X đi trước

        // --- Các control giao diện ---
        TextBox txtIP, txtPort, txtUser;
        Button btnConnect, btnInvite;
        Label lblStatus;

        public Form1()
        {
            // Tắt nhấp nháy khi vẽ
            this.SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint, true);
            this.UpdateStyles();

            // Kích thước cửa sổ
            int topBar = 60;
            this.ClientSize = new Size(N * CELL + 1, N * CELL + 1 + topBar);
            this.Text = "Caro Client";

            // Gắn sự kiện
            this.Paint += Form1_Paint;
            this.MouseDown += Form1_MouseDown;

            // Xây thanh điều khiển trên cùng
            BuildTopBar();

            // Đăng ký sự kiện nhận dữ liệu mạng
            Net.OnLine += HandleLine;
        }

        void BuildTopBar()
        {
            int y = 8, h = 24, x = 8;

            txtIP = new TextBox { Left = x, Top = y, Width = 130, Text = "127.0.0.1" };
            Controls.Add(txtIP); x += txtIP.Width + 6;

            txtPort = new TextBox { Left = x, Top = y, Width = 60, Text = "5555" };
            Controls.Add(txtPort); x += txtPort.Width + 6;

            txtUser = new TextBox { Left = x, Top = y, Width = 120, Text = "thuy" };
            Controls.Add(txtUser); x += txtUser.Width + 6;

            btnConnect = new Button { Left = x, Top = y - 1, Width = 90, Height = h + 4, Text = "Connect" };
            btnConnect.Click += (s, e) => ConnectToServer();
            Controls.Add(btnConnect); x += btnConnect.Width + 6;

            // Nút mời đối thủ, dùng hộp thoại tự tạo
            btnInvite = new Button { Left = x, Top = y - 1, Width = 90, Height = h + 4, Text = "Invite..." };
            btnInvite.Click += (s, e) =>
            {
                string target = Prompt.Show("Mời người chơi nào?", "Invite", "phuoc");
                if (!string.IsNullOrWhiteSpace(target))
                    Net.Send("INVITE " + target.Trim());
            };
            Controls.Add(btnInvite); x += btnInvite.Width + 6;

            lblStatus = new Label { Left = x, Top = y + 3, Width = 320, Height = h, Text = "Not connected" };
            Controls.Add(lblStatus);
        }

        int BoardOffsetY { get { return 60; } }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            int offY = BoardOffsetY;

            // Vẽ lưới
            for (int i = 0; i <= N; i++)
            {
                g.DrawLine(Pens.Gray, 0, offY + i * CELL, N * CELL, offY + i * CELL);
                g.DrawLine(Pens.Gray, i * CELL, offY, i * CELL, offY + N * CELL);
            }

            // Vẽ ký hiệu X/O
            using (var font = new Font("Segoe UI", CELL * 0.6f, FontStyle.Bold))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                for (int r = 0; r < N; r++)
                    for (int c = 0; c < N; c++)
                    {
                        if (board[r, c] == 0) continue;
                        string s = (board[r, c] == 1) ? "X" : "O";
                        var rect = new Rectangle(c * CELL, offY + r * CELL, CELL, CELL);
                        g.DrawString(s, font, Brushes.Black, rect, sf);
                    }
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Y < BoardOffsetY) return;
            if (!myTurn) return;

            int c = e.X / CELL;
            int r = (e.Y - BoardOffsetY) / CELL;

            if (r < 0 || r >= N || c < 0 || c >= N) return;
            if (board[r, c] != 0) return;

            // Đánh cờ
            board[r, c] = (mySymbol == "X") ? 1 : 2;
            Invalidate();

            // Gửi nước đi
            Net.Send(string.Format("MOVE {0} {1}", r, c));

            if (CheckWin(r, c, board[r, c]))
            {
                Net.Send("RESULT WIN");
                MessageBox.Show("Bạn thắng!");
            }

            myTurn = false;
            UpdateStatus();
        }

        void ConnectToServer()
        {
            string host = txtIP.Text.Trim();
            int port = 5555;
            int.TryParse(txtPort.Text.Trim(), out port);
            myUser = string.IsNullOrWhiteSpace(txtUser.Text) ? "thuy" : txtUser.Text.Trim();

            bool ok = Net.Connect(host, port);
            if (ok)
            {
                Net.Send("LOGIN " + myUser);
                lblStatus.Text = string.Format("Đã kết nối {0}:{1} dưới tên {2}", host, port, myUser);
            }
            else
            {
                lblStatus.Text = "Kết nối thất bại";
            }
        }

        void HandleLine(string line)
        {
            if (this.InvokeRequired) { this.BeginInvoke(new Action<string>(HandleLine), line); return; }

            // Cắt câu lệnh
            string cmd = line;
            string rest = "";
            int sp = line.IndexOf(' ');
            if (sp >= 0)
            {
                cmd = line.Substring(0, sp);
                rest = line.Substring(sp + 1);
            }

            if (cmd == "LOGIN_OK")
            {
                lblStatus.Text = "Đăng nhập thành công với tên " + myUser;
            }
            else if (cmd == "INVITE_OK")
            {
                string[] p = rest.Split(new[] { ' ' }, 2, StringSplitOptions.None);
                string opponent = p.Length > 0 ? p[0] : "";
                string symbol = p.Length > 1 ? p[1] : "X";

                mySymbol = symbol;
                myTurn = (mySymbol == "X");
                board = new int[N, N];
                Invalidate();

                lblStatus.Text = string.Format("Đấu với {0}. Bạn là {1}. {2}",
                    opponent, mySymbol, myTurn ? "Đến lượt bạn." : "Đợi đối thủ...");
            }
            else if (cmd == "OPP_MOVE")
            {
                string[] p = rest.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (p.Length >= 3)
                {
                    int r, c;
                    if (int.TryParse(p[0], out r) && int.TryParse(p[1], out c))
                    {
                        string sym = p[2];
                        board[r, c] = (sym == "X") ? 1 : 2;
                        Invalidate();
                        myTurn = (sym != mySymbol);
                        UpdateStatus();
                    }
                }
            }
            else if (cmd == "CHAT")
            {
                MessageBox.Show(rest, "Chat");
            }
            else if (cmd == "RESULT")
            {
                MessageBox.Show("Kết quả: " + rest);
            }
            else if (cmd == "ERROR")
            {
                MessageBox.Show("Lỗi từ server: " + rest, "ERROR");
            }
        }

        void UpdateStatus()
        {
            lblStatus.Text = string.Format("{0} ({1}) — {2}",
                myUser, mySymbol, myTurn ? "Lượt bạn" : "Đối thủ");
        }

        // --- Kiểm tra thắng ---
        bool CheckWin(int r, int c, int who)
        {
            Func<int, int, int> CountDir = (dr, dc) =>
            {
                int k = 0, rr = r + dr, cc = c + dc;
                while (rr >= 0 && rr < N && cc >= 0 && cc < N && board[rr, cc] == who)
                {
                    k++; rr += dr; cc += dc;
                }
                return k;
            };

            int[] drs = { 0, 1, 1, -1 };
            int[] dcs = { 1, 0, 1, 1 };
            for (int i = 0; i < 4; i++)
            {
                int total = 1 + CountDir(drs[i], dcs[i]) + CountDir(-drs[i], -dcs[i]);
                if (total >= 5) return true;
            }
            return false;
        }
    }
}
