using ChatAppClient.Helpers;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ChatAppClient.Forms
{
    public partial class frmCaroGame : Form
    {
        private const int BOARD_SIZE = 20; // Bàn cờ 20x20
        private const int CELL_SIZE = 30;  // Mỗi ô 30x30 pixels
        private int[,] _board; // Mảng 2D lưu trạng thái bàn cờ (0=trống, 1=X, 2=O)

        private bool _isMyTurn = false; // Server sẽ quyết định ai đi trước
        private int _myPiece; // Quân cờ của tôi (1=X, 2=O)
        private int _currentPlayerPiece = 1; // Luôn bắt đầu là X (quân 1)

        private bool _isGameEnded = false;

        private Pen _gridPen = new Pen(Color.LightGray, 1);
        private Pen _xPen = new Pen(AppColors.Primary, 3);
        private Pen _oPen = new Pen(Color.DarkOrange, 3);

        private string _gameId;
        private string _opponentId;

        public frmCaroGame(string gameId, string opponentId, bool startsFirst)
        {
            InitializeComponent();
            _gameId = gameId;
            _opponentId = opponentId;

            // Thiết lập kích thước cố định cho bàn cờ
            int boardPixelSize = BOARD_SIZE * CELL_SIZE + 1;
            pnlBoard.Size = new Size(boardPixelSize, boardPixelSize);
            // Tự điều chỉnh kích thước Form
            this.ClientSize = new Size(boardPixelSize, boardPixelSize + pnlHeader.Height + pnlControls.Height);

            // Xác định lượt chơi
            _myPiece = startsFirst ? 1 : 2; // Nếu đi trước là X (1), ngược lại là O (2)
            _isMyTurn = startsFirst;
        }

        private void frmCaroGame_Load(object sender, EventArgs e)
        {
            // Gán sự kiện
            pnlBoard.Paint += pnlBoard_Paint;
            pnlBoard.MouseClick += pnlBoard_MouseClick;
            btnNewGame.Click += BtnNewGame_Click;

            InitializeGame();
        }

        private void InitializeGame()
        {
            _board = new int[BOARD_SIZE, BOARD_SIZE];
            _currentPlayerPiece = 1; // X luôn đi trước
            _isGameEnded = false;

            // Cập nhật lại lượt đi ban đầu
            _isMyTurn = (_myPiece == 1);

            UpdateTurnLabel();
            pnlBoard.Invalidate(); // Vẽ lại bàn cờ
        }

        private void UpdateTurnLabel()
        {
            if (_isGameEnded)
            {
                lblTurn.Text = "Trò chơi kết thúc!";
                return;
            }

            string player = (_currentPlayerPiece == 1) ? "X" : "O";
            lblTurn.Text = _isMyTurn ? $"Đến lượt bạn ({player})" : $"Đến lượt đối thủ ({player})";
            lblTurn.ForeColor = _isMyTurn ? AppColors.Primary : AppColors.TextSecondary;
        }

        private void BtnNewGame_Click(object sender, EventArgs e)
        {
            // TODO: Gửi yêu cầu "Chơi Lại" lên Server
            // NetworkManager.Instance.SendRematchRequest(_gameId);
            MessageBox.Show("Đã gửi yêu cầu chơi lại!");
            // Chỉ reset khi đối phương đồng ý
        }

        private void pnlBoard_MouseClick(object sender, MouseEventArgs e)
        {
            if (_isGameEnded || !_isMyTurn)
                return; // Không phải lượt của bạn, hoặc game đã kết thúc

            // Tính toán vị trí (hàng, cột) từ tọa độ (x, y) của chuột
            int col = e.X / CELL_SIZE;
            int row = e.Y / CELL_SIZE;

            // Kiểm tra vị trí hợp lệ
            if (row < 0 || row >= BOARD_SIZE || col < 0 || col >= BOARD_SIZE)
                return;

            // Kiểm tra ô đã được đánh chưa
            if (_board[row, col] == 0)
            {
                // 1. Đánh dấu nước đi
                _board[row, col] = _myPiece;

                // 2. Vô hiệu hóa lượt đi
                _isMyTurn = false;

                // 3. TODO: Gửi nước đi (row, col) lên Server
                // NetworkManager.Instance.SendGameMove(_gameId, row, col);

                // 4. Cập nhật UI ngay lập tức
                pnlBoard.Invalidate(); // Vẽ lại nước đi của mình

                // 5. Kiểm tra thắng
                if (CheckWin(row, col, _myPiece))
                {
                    EndGame(_myPiece);
                }
                else
                {
                    // Chuyển lượt (UI)
                    _currentPlayerPiece = (_currentPlayerPiece == 1) ? 2 : 1;
                    UpdateTurnLabel();
                }
            }
        }

        // Hàm này được gọi khi Server gửi nước đi của đối thủ
        public void ReceiveOpponentMove(int row, int col)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ReceiveOpponentMove(row, col)));
                return;
            }

            if (_isGameEnded) return;

            int opponentPiece = (_myPiece == 1) ? 2 : 1;
            _board[row, col] = opponentPiece;

            pnlBoard.Invalidate(); // Vẽ lại bàn cờ với nước đi của đối thủ

            if (CheckWin(row, col, opponentPiece))
            {
                EndGame(opponentPiece);
            }
            else
            {
                // Bắt đầu lượt của bạn
                _isMyTurn = true;
                _currentPlayerPiece = _myPiece;
                UpdateTurnLabel();
            }
        }

        private void EndGame(int winningPiece)
        {
            _isGameEnded = true;
            _isMyTurn = false;
            UpdateTurnLabel();

            string message = (winningPiece == _myPiece) ? "Bạn đã thắng!" : "Bạn đã thua!";
            MessageBox.Show(message, "Kết thúc trò chơi");
        }

        // Logic kiểm tra thắng (5 ô liên tiếp, không bị chặn 2 đầu)
        private bool CheckWin(int r, int c, int piece)
        {
            // Kiểm tra ngang, dọc, và 2 đường chéo
            if (CountInLine(r, c, 1, 0, piece) >= 5) return true; // Dọc
            if (CountInLine(r, c, 0, 1, piece) >= 5) return true; // Ngang
            if (CountInLine(r, c, 1, 1, piece) >= 5) return true; // Chéo \
            if (CountInLine(r, c, 1, -1, piece) >= 5) return true; // Chéo /
            return false;
        }

        // Đếm số quân liên tiếp theo 1 hướng (dr, dc)
        private int CountInLine(int r, int c, int dr, int dc, int piece)
        {
            int count = 0;
            // Đếm theo hướng (dr, dc)
            for (int i = 0; i < 5; i++)
            {
                int nr = r + i * dr;
                int nc = c + i * dc;
                if (IsSafe(nr, nc) && _board[nr, nc] == piece) count++;
                else break;
            }
            // Đếm theo hướng ngược lại (-dr, -dc), bỏ qua ô trung tâm (đã đếm)
            for (int i = 1; i < 5; i++)
            {
                int nr = r - i * dr;
                int nc = c - i * dc;
                if (IsSafe(nr, nc) && _board[nr, nc] == piece) count++;
                else break;
            }

            // TODO: Bổ sung logic "chặn 2 đầu" (luật Caro chuẩn)
            // Tạm thời chỉ cần 5 quân là thắng

            return count;
        }

        private bool IsSafe(int r, int c)
        {
            return r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE;
        }


        // Hàm vẽ bàn cờ
        private void pnlBoard_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 1. Vẽ các đường kẻ
            for (int i = 0; i <= BOARD_SIZE; i++)
            {
                g.DrawLine(_gridPen, 0, i * CELL_SIZE, BOARD_SIZE * CELL_SIZE, i * CELL_SIZE); // Ngang
                g.DrawLine(_gridPen, i * CELL_SIZE, 0, i * CELL_SIZE, BOARD_SIZE * CELL_SIZE); // Dọc
            }

            // 2. Vẽ các quân cờ (X và O)
            for (int r = 0; r < BOARD_SIZE; r++)
            {
                for (int c = 0; c < BOARD_SIZE; c++)
                {
                    int piece = _board[r, c];
                    int padding = 5; // Khoảng đệm để X, O nhỏ hơn ô
                    int x = c * CELL_SIZE + padding;
                    int y = r * CELL_SIZE + padding;
                    int size = CELL_SIZE - (padding * 2);
                    Rectangle rect = new Rectangle(x, y, size, size);


                    if (piece == 1) // X
                    {
                        g.DrawLine(_xPen, x, y, x + size, y + size);
                        g.DrawLine(_xPen, x + size, y, x, y + size);
                    }
                    else if (piece == 2) // O
                    {
                        g.DrawEllipse(_oPen, rect);
                    }
                }
            }
        }
    }
}