using ChatApp.Shared; 
using ChatAppClient.Helpers;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ChatAppClient.Forms
{
    public partial class frmCaroGame : Form
    {
        private const int BOARD_SIZE = 20;
        private const int CELL_SIZE = 30;
        private int[,] _board;
        private bool _isMyTurn = false;
        private int _myPiece; 
        private int _currentPlayerPiece = 1; 
        private bool _isGameEnded = false;
        private string _gameId;
        private string _opponentId;
        private string _myId;
        private Pen _gridPen = new Pen(Color.LightGray, 1);
        private Pen _xPen = new Pen(AppColors.Primary, 3);
        private Pen _oPen = new Pen(Color.DarkOrange, 3);
        public frmCaroGame(string gameId, string opponentId, bool startsFirst)
        {
            InitializeComponent();
            _gameId = gameId;
            _opponentId = opponentId;
            _myId = NetworkManager.Instance.UserID ?? ""; 
            int boardPixelSize = BOARD_SIZE * CELL_SIZE + 1;
            pnlBoard.Size = new Size(boardPixelSize, boardPixelSize);
            this.ClientSize = new Size(boardPixelSize, boardPixelSize + pnlHeader.Height + pnlControls.Height);

            _myPiece = startsFirst ? 1 : 2; 
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
            _currentPlayerPiece = 1;
            _isGameEnded = false;
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
            if (!_isGameEnded) return;

            var request = new RematchRequestPacket
            {
                GameID = _gameId,
                SenderID = _myId
            };
            NetworkManager.Instance.SendPacket(request);
            btnNewGame.Enabled = false;
            btnNewGame.Text = "Đang chờ...";
        }
        public void HandleRematchRequest(RematchRequestPacket request)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleRematchRequest(request))); return; }
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
            if (accepted)
            {
                btnNewGame.Enabled = false;
                btnNewGame.Text = "Đang chờ...";
            }
        }
        public void HandleRematchResponse(RematchResponsePacket response)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleRematchResponse(response))); return; }

            if (!response.Accepted)
            {
                MessageBox.Show("Đối thủ đã từ chối chơi lại.", "Thông báo");
                btnNewGame.Enabled = true; 
                btnNewGame.Text = "Chơi Lại";
            }
        }
        public void HandleGameReset(GameResetPacket packet)
        {
            if (this.InvokeRequired) { this.Invoke(new Action(() => HandleGameReset(packet))); return; }

            Logger.Success("Bắt đầu ván mới!"); 

            _board = new int[BOARD_SIZE, BOARD_SIZE];
            _currentPlayerPiece = 1;
            _isGameEnded = false;
            _isMyTurn = packet.StartsFirst;

            UpdateTurnLabel();
            pnlBoard.Invalidate(); 
            btnNewGame.Enabled = true;
            btnNewGame.Text = "Chơi Lại";
        }
        
        private void pnlBoard_MouseClick(object sender, MouseEventArgs e)
        {
            if (_isGameEnded || !_isMyTurn)
                return;

            int col = e.X / CELL_SIZE;
            int row = e.Y / CELL_SIZE;

            if (!IsSafe(row, col) || _board[row, col] != 0)
                return;

            _board[row, col] = _myPiece;

            _isMyTurn = false;

            var movePacket = new GameMovePacket
            {
                GameID = _gameId,
                SenderID = _myId,
                Row = row,
                Col = col
            };

            NetworkManager.Instance.SendPacket(movePacket);

            pnlBoard.Invalidate();

            if (CheckWin(row, col, _myPiece))
            {
                EndGame(_myPiece);
            }
            else
            {
                _currentPlayerPiece = (_currentPlayerPiece == 1) ? 2 : 1;
                UpdateTurnLabel();
            }
        }

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
                pnlBoard.Invalidate(); 

                if (CheckWin(row, col, opponentPiece))
                {
                    EndGame(opponentPiece);
                }
                else
                {
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

        #region Logic Game 

        private bool CheckWin(int r, int c, int piece)
        {
            if (CountInLine(r, c, 1, 0, piece) >= 5) return true; 
            if (CountInLine(r, c, 0, 1, piece) >= 5) return true; 
            if (CountInLine(r, c, 1, 1, piece) >= 5) return true; 
            if (CountInLine(r, c, 1, -1, piece) >= 5) return true;
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