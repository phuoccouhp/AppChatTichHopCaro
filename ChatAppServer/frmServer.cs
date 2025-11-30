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
        private ContextMenuStrip _ctxUserMenu; // Menu chuột phải

        public frmServer()
        {
            InitializeComponent();
            InitializeContextMenu(); // Tạo menu

            // Gán sự kiện MouseDown cho ListBox bằng code (để bạn không cần chỉnh Designer)
            lstUsers.MouseDown += lstUsers_MouseDown;
        }

        // Tạo menu chuột phải bằng code
        private void InitializeContextMenu()
        {
            _ctxUserMenu = new ContextMenuStrip();

            var itemInfo = new ToolStripMenuItem("🔍 Quan sát / Thông tin");
            itemInfo.Click += (s, e) => ShowUserInfo();

            var itemKick = new ToolStripMenuItem("👢 Kick người dùng này");
            itemKick.ForeColor = Color.Red;
            itemKick.Click += (s, e) => KickSelectedUser();

            _ctxUserMenu.Items.Add(itemInfo);
            _ctxUserMenu.Items.Add(new ToolStripSeparator());
            _ctxUserMenu.Items.Add(itemKick);

            lstUsers.ContextMenuStrip = _ctxUserMenu;
        }

        private void frmServer_Load(object sender, EventArgs e)
        {
            Logger.OnLogReceived += Logger_OnLogReceived;
            rtbLog.ReadOnly = true;
            rtbLog.ForeColor = Color.White;
            rtbLog.Font = new Font("Consolas", 10);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            lblStatus.Text = "Server: Running...";
            lblStatus.ForeColor = Color.Green;

            _server = new Server(PORT);
            _server.OnUserListChanged += Server_OnUserListChanged;

            Task.Run(async () =>
            {
                Logger.Success($"Server khởi động tại port {PORT}...");
                await _server.StartAsync();
            });
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

        #region Xử lý Menu Chuột Phải

        // Khi nhấn chuột phải, tự động chọn dòng đó
        private void lstUsers_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int index = lstUsers.IndexFromPoint(e.Location);
                if (index != ListBox.NoMatches)
                {
                    lstUsers.SelectedIndex = index;
                }
            }
        }

        private string GetSelectedUserID()
        {
            if (lstUsers.SelectedItem == null) return null;
            // Format: "Tên (ID)" -> Cắt lấy ID trong ngoặc
            string itemText = lstUsers.SelectedItem.ToString();
            int openParen = itemText.LastIndexOf('(');
            int closeParen = itemText.LastIndexOf(')');
            if (openParen != -1 && closeParen != -1)
            {
                return itemText.Substring(openParen + 1, closeParen - openParen - 1);
            }
            return null;
        }

        private void ShowUserInfo()
        {
            string userId = GetSelectedUserID();
            if (userId != null && _server != null)
            {
                string info = _server.GetUserInfo(userId);
                MessageBox.Show(info, "Thông tin User", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void KickSelectedUser()
        {
            string userId = GetSelectedUserID();
            if (userId != null && _server != null)
            {
                var confirm = MessageBox.Show($"KICK user '{userId}'? Họ sẽ bị ngắt kết nối ngay lập tức.",
                    "Xác nhận Kick", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (confirm == DialogResult.Yes)
                {
                    _server.KickUser(userId);
                }
            }
        }

        #endregion

        #region Cập nhật Giao diện (Thread-Safe)

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

        private void Server_OnUserListChanged(List<string> users)
        {
            if (lstUsers.IsDisposed) return;
            if (lstUsers.InvokeRequired)
            {
                lstUsers.Invoke(new Action(() => Server_OnUserListChanged(users)));
                return;
            }
            lstUsers.Items.Clear();
            foreach (var user in users) lstUsers.Items.Add(user);
        }

        #endregion
    }
}