using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatAppServer
{
    public partial class frmServer : Form
    {
        private Server _server;
        private const int PORT = 9000;

        public frmServer()
        {
            InitializeComponent();
        }

        private void frmServer_Load(object sender, EventArgs e)
        {
            // Đăng ký nhận Log từ Logger
            Logger.OnLogReceived += Logger_OnLogReceived;
            rtbLog.ReadOnly = true;
            rtbLog.ForeColor = Color.White; // Thêm màu chữ trắng để dễ nhìn trên nền đen
            rtbLog.Font = new Font("Consolas", 10);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            lblStatus.Text = "Server: Running...";
            lblStatus.ForeColor = Color.Black;

            _server = new Server(PORT);

            // Đăng ký nhận thay đổi danh sách User
            _server.OnUserListChanged += Server_OnUserListChanged;

            // Chạy Server trên luồng phụ để không đơ giao diện
            Task.Run(async () =>
            {
                Logger.Success($"Server khởi động tại port {PORT}...");
                await _server.StartAsync();
            });
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            // Cách đơn giản nhất để dừng là khởi động lại app hoặc đóng
            Application.Restart();
        }

        // Hàm nhận Log và đưa lên màn hình (An toàn luồng)
        private void Logger_OnLogReceived(string message, Color color)
        {
            if (rtbLog.IsDisposed) return;
            if (rtbLog.InvokeRequired)
            {
                rtbLog.Invoke(new Action(() => Logger_OnLogReceived(message, color)));
                return;
            }

            rtbLog.SelectionStart = rtbLog.TextLength;
            rtbLog.SelectionLength = 0;
            rtbLog.SelectionColor = color;
            rtbLog.AppendText(message + Environment.NewLine);
            rtbLog.ScrollToCaret();
        }

        // Hàm cập nhật danh sách User (An toàn luồng)
        private void Server_OnUserListChanged(List<string> users)
        {
            if (lstUsers.IsDisposed) return;
            if (lstUsers.InvokeRequired)
            {
                lstUsers.Invoke(new Action(() => Server_OnUserListChanged(users)));
                return;
            }

            lstUsers.Items.Clear();
            foreach (var user in users)
            {
                lstUsers.Items.Add(user);
            }
        }
    }
}