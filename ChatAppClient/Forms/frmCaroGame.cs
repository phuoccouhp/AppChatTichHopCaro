using ChatApp.Shared; // <-- Phải có
using ChatAppClient.Helpers;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ChatAppClient.Forms
{
    public partial class frmCaroGame : Form
    {
        // Định nghĩa bàn cờ
        private const int BOARD_SIZE = 20;
        private const int CELL_SIZE = 30;
        private int[,] _board;

        // Trạng thái ván game
        private bool _isMyTurn = false;
        private int _myPiece; // Quân của tôi (1=X, 2=O)
        private int _currentPlayerPiece = 1; // Quân đang đi (Luôn là X)
        private bool _isGameEnded = false;

        // Thông tin ván game
        private string _gameId;
        private string _opponentId;
        private string _myId;

        // Dụng cụ vẽ
        private Pen _gridPen = new Pen(Color.LightGray, 1);
        private Pen _xPen = new Pen(AppColors.Primary, 3);
        private Pen _oPen = new Pen(Color.DarkOrange, 3);

        // Hàm tạo (Constructor) - Đã sửa
        public frmCaroGame(string gameId, string opponentId, bool startsFirst)
        {
            InitializeComponent();
            _gameId = gameId;
            _opponentId = opponentId;
            _myId = NetworkManager.Instance.UserID; // Lấy ID của tôi

            // Thiết lập kích thước
            int boardPixelSize = BOARD_SIZE * CELL_SIZE + 1;
            pnlBoard.Size = new Size(boardPixelSize, boardPixelSize);
            this.ClientSize = new Size(boardPixelSize, boardPixelSize + pnlHeader.Height + pnlControls.Height);

            // Xác định lượt chơi
            _myPiece = startsFirst ? 1 : 2; // Nếu đi trước là X (1)
            _isMyTurn = startsFirst;
        }

        private void frmCaroGame_Load(object sender, EventArgs e)
        {
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
            _isMyTurn = (_myPiece == 1); // Quyền đi đầu tiên

            UpdateTurnLabel();
            pnlBoard.Invalidate();
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
            MessageBox.Show("Chức năng 'Chơi Lại' chưa được phát triển.", "Thông báo");
            // TODO: Gửi gói tin RematchPacket
        }

        // GỬI NƯỚC ĐI
        private void pnlBoard_MouseClick(object sender, MouseEventArgs e)
        {
            if (_isGameEnded || !_isMyTurn)
                return;

            int col = e.X / CELL_SIZE;
            int row = e.Y / CELL_SIZE;

            if (!IsSafe(row, col) || _board[row, col] != 0)
                return;

            // 1. Đánh dấu
            _board[row, col] = _myPiece;

            // 2. Vô hiệu hóa lượt
            _isMyTurn = false;

            // 3. TẠO GÓI TIN NƯỚC ĐI
            var movePacket = new GameMovePacket
            {
                GameID = _gameId,
                SenderID = _myId,
                Row = row,
                Col = col
            };

            // 4. GỬI GÓI TIN
            NetworkManager.Instance.SendPacket(movePacket);

            // 5. Cập nhật UI
            pnlBoard.Invalidate();

            // 6. Kiểm tra thắng
            if (CheckWin(row, col, _myPiece))
            {
                EndGame(_myPiece);
                // TODO: Gửi GameEndPacket
            }
            else
            {
                // Chuyển lượt (UI)
                _currentPlayerPiece = (_currentPlayerPiece == 1) ? 2 : 1;
                UpdateTurnLabel();
            }
        }

        // NHẬN NƯỚC ĐI (Được gọi từ frmHome)
        public void ReceiveOpponentMove(int row, int col)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ReceiveOpponentMove(row, col)));
                return;
            }

            if (_isGameEnded) return;

            int opponentPiece = (_myPiece == 1) ? 2 : 1;

            if (IsSafe(row, col) && _board[row, col] == 0)
            {
                _board[row, col] = opponentPiece;
                pnlBoard.Invalidate(); // Vẽ nước đi của đối thủ

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
        }

        private void EndGame(int winningPiece)
        {
            _isGameEnded = true;
            _isMyTurn = false;
            UpdateTurnLabel();

            string message = (winningPiece == _myPiece) ? "Bạn đã thắng!" : "Bạn đã thua!";
            MessageBox.Show(message, "Kết thúc trò chơi");
        }

        #region Logic Game (Không đổi)

        private bool CheckWin(int r, int c, int piece)
        {
            if (CountInLine(r, c, 1, 0, piece) >= 5) return true; // Dọc
            if (CountInLine(r, c, 0, 1, piece) >= 5) return true; // Ngang
            if (CountInLine(r, c, 1, 1, piece) >= 5) return true; // Chéo \
            if (CountInLine(r, c, 1, -1, piece) >= 5) return true; // Chéo /
            return false;
        }

        private int CountInLine(int r, int c, int dr, int dc, int piece)
        {
            int count = 0;
            for (int i = 0; i < 5; i++)
            {
                int nr = r + i * dr;
                int nc = c + i * dc;
                if (IsSafe(nr, nc) && _board[nr, nc] == piece) count++;
                else break;
            }
            for (int i = 1; i < 5; i++)
            {
                int nr = r - i * dr;
                int nc = c - i * dc;
                if (IsSafe(nr, nc) && _board[nr, nc] == piece) count++;
                else break;
            }
            return count;
        }

        private bool IsSafe(int r, int c)
        {
            return r >= 0 && r < BOARD_SIZE && c >= 0 && c < BOARD_SIZE;
        }

        private void pnlBoard_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            for (int i = 0; i <= BOARD_SIZE; i++)
            {
                g.DrawLine(_gridPen, 0, i * CELL_SIZE, BOARD_SIZE * CELL_SIZE, i * CELL_SIZE);
                g.DrawLine(_gridPen, i * CELL_SIZE, 0, i * CELL_SIZE, BOARD_SIZE * CELL_SIZE);
            }

            for (int r = 0; r < BOARD_SIZE; r++)
            {
                for (int c = 0; c < BOARD_SIZE; c++)
                {
                    int piece = _board[r, c];
                    int padding = 5;
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

        #endregion
    }
}