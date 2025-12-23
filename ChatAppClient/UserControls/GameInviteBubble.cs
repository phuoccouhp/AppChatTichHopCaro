using ChatAppClient.Helpers;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ChatAppClient.UserControls
{
    public enum GameInviteStatus
    {
        Pending,
        Accepted,
        Declined
    }

    public enum GameType
    {
        Caro,
        Tank
    }

    public partial class GameInviteBubble : UserControl
    {
        private Color _bubbleColor = AppColors.LightGray;
        private MessageType _type = MessageType.Incoming;
        private int _borderRadius = 18;
        private GameInviteStatus _status = GameInviteStatus.Pending;
        private GameType _gameType = GameType.Caro;
        private string _senderName = "";
        private int _messageId = 0;

        public event EventHandler<bool>? OnResponse; // bool: accepted
        public event EventHandler? OnReinvite; // Event khi người dùng muốn mời lại
        public int MessageID { get; set; }
        public GameInviteStatus Status => _status;
        public GameType CurrentGameType => _gameType;

        public GameInviteBubble()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            UpdateUI();
        }

        public void SetInvite(string senderName, MessageType type, GameType gameType, int messageId = 0)
        {
            _senderName = senderName;
            _type = type;
            _gameType = gameType;
            _status = GameInviteStatus.Pending;
            MessageID = messageId;
            UpdateUI();
        }

        public void UpdateStatus(GameInviteStatus status)
        {
            _status = status;
            UpdateUI();
        }

        private void UpdateUI()
        {
            string gameName = _gameType == GameType.Caro ? "Caro" : "Tank Game";
            
            if (_type == MessageType.Outgoing)
            {
                _bubbleColor = AppColors.Primary;
                lblMessage.ForeColor = Color.White;
                lblStatus.ForeColor = Color.White;
            }
            else
            {
                _bubbleColor = AppColors.LightGray;
                lblMessage.ForeColor = Color.Black;
                lblStatus.ForeColor = Color.Black;
            }

            switch (_status)
            {
                case GameInviteStatus.Pending:
                    if (_type == MessageType.Outgoing)
                    {
                        lblMessage.Text = $"Bạn đã mời {_senderName} chơi {gameName}";
                        lblStatus.Text = "Đang chờ phản hồi...";
                        btnAccept.Visible = false;
                        btnDecline.Visible = false;
                    }
                    else
                    {
                        lblMessage.Text = $"{_senderName} mời bạn chơi {gameName}";
                        lblStatus.Text = "";
                        btnAccept.Visible = true;
                        btnDecline.Visible = true;
                    }
                    btnReinvite.Visible = false;
                    break;
                case GameInviteStatus.Accepted:
                    if (_type == MessageType.Outgoing)
                    {
                        lblMessage.Text = $"Bạn đã mời {_senderName} chơi {gameName}";
                        lblStatus.Text = "✓ Đã chấp nhận";
                    }
                    else
                    {
                        lblMessage.Text = $"{_senderName} mời bạn chơi {gameName}";
                        lblStatus.Text = "✓ Bạn đã chấp nhận";
                    }
                    btnAccept.Visible = false;
                    btnDecline.Visible = false;
                    btnReinvite.Visible = false;
                    break;
                case GameInviteStatus.Declined:
                    if (_type == MessageType.Outgoing)
                    {
                        lblMessage.Text = $"Bạn đã mời {_senderName} chơi {gameName}";
                        lblStatus.Text = "✗ Đã từ chối";
                        btnReinvite.Visible = true; // Hiển thị nút mời lại cho người gửi
                    }
                    else
                    {
                        lblMessage.Text = $"{_senderName} mời bạn chơi {gameName}";
                        lblStatus.Text = "✗ Bạn đã từ chối";
                        btnReinvite.Visible = false;
                    }
                    btnAccept.Visible = false;
                    btnDecline.Visible = false;
                    break;
            }

            this.Invalidate();
        }

        private void BtnAccept_Click(object sender, EventArgs e)
        {
            if (_status == GameInviteStatus.Pending && _type == MessageType.Incoming)
            {
                OnResponse?.Invoke(this, true);
            }
        }

        private void BtnDecline_Click(object sender, EventArgs e)
        {
            if (_status == GameInviteStatus.Pending && _type == MessageType.Incoming)
            {
                OnResponse?.Invoke(this, false);
            }
        }

        private void BtnReinvite_Click(object sender, EventArgs e)
        {
            if (_status == GameInviteStatus.Declined && _type == MessageType.Outgoing)
            {
                // Ẩn nút mời lại và cập nhật trạng thái thành đang chờ
                btnReinvite.Visible = false;
                _status = GameInviteStatus.Pending;
                UpdateUI();
                OnReinvite?.Invoke(this, EventArgs.Empty);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            using (GraphicsPath path = DrawingHelper.CreateRoundedRectPath(rect, _borderRadius))
            {
                using (SolidBrush brush = new SolidBrush(_bubbleColor))
                {
                    g.FillPath(brush, path);
                }
            }
        }
    }
}

