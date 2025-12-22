using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatAppServer
{
    public partial class frmServer : Form
    {
        private Server? _server;
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
            rtbLog.BackColor = Color.FromArgb(30, 30, 30); // Nền tối
            rtbLog.ForeColor = Color.White;
            rtbLog.Font = new Font("Consolas", 9, FontStyle.Regular);
            rtbLog.WordWrap = true; // Bật word wrap
            rtbLog.DetectUrls = false; // Tắt auto-detect URLs để tránh lỗi format
            
            // Hiển thị IP ngay khi form load (trước khi start server)
            string localIPs = GetLocalIPAddresses();
            lblServerIP.Text = $"IP của máy này: {localIPs}";
            lblServerIP.ForeColor = Color.Gray;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            lblStatus.Text = "Server: Running...";
            lblStatus.ForeColor = Color.Green;

            // Lấy và hiển thị địa chỉ IP local
            string localIPs = GetLocalIPAddresses();
            lblServerIP.Text = $"Server IP: {localIPs} (Port: {PORT})";
            lblServerIP.ForeColor = Color.DarkBlue;
            
            Logger.Info($"Địa chỉ IP của máy chủ: {localIPs}");
            Logger.Info($"Clients cần kết nối đến một trong các IP trên với port {PORT}");
            Logger.Info("Đảm bảo cả hai máy đều cùng mạng và firewall cho phép port 9000");

            _server = new Server(PORT);
            _server.OnUserListChanged += Server_OnUserListChanged;

            Task.Run(async () =>
            {
                Logger.Success($"Server khởi động tại port {PORT}...");
                Logger.Success($"Đang lắng nghe kết nối từ mạng chung...");
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

        private string? GetSelectedUserID()
        {
            if (lstUsers.SelectedItem == null) return null;
            // Format: "Tên (ID)" -> Cắt lấy ID trong ngoặc
            string? itemText = lstUsers.SelectedItem.ToString();
            if (itemText == null) return null;
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
            string? userId = GetSelectedUserID();
            if (userId == null)
            {
                MessageBox.Show("Vui lòng chọn một người dùng từ danh sách.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (_server != null)
            {
                string info = _server.GetUserInfo(userId);
                MessageBox.Show(info, "Thông tin User", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void KickSelectedUser()
        {
            string? userId = GetSelectedUserID();
            if (userId == null)
            {
                MessageBox.Show("Vui lòng chọn một người dùng từ danh sách.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (_server != null)
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

        #region Lấy địa chỉ IP Local

        /// <summary>
        /// Lấy tất cả địa chỉ IP local của máy (loại trừ loopback và IPv6)
        /// </summary>
        private string GetLocalIPAddresses()
        {
            try
            {
                var ipAddresses = new List<string>();
                
                // Lấy tất cả các địa chỉ IP từ tất cả các network interface
                var host = Dns.GetHostEntry(Dns.GetHostName());
                
                foreach (var ip in host.AddressList)
                {
                    // Chỉ lấy IPv4 và không phải loopback (127.0.0.1)
                    if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                    {
                        ipAddresses.Add(ip.ToString());
                    }
                }

                if (ipAddresses.Count == 0)
                {
                    // Nếu không tìm thấy IP nào, thử cách khác
                    using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                    {
                        socket.Connect("8.8.8.8", 65530);
                        var endPoint = socket.LocalEndPoint as IPEndPoint;
                        if (endPoint != null)
                        {
                            ipAddresses.Add(endPoint.Address.ToString());
                        }
                    }
                }

                return ipAddresses.Count > 0 
                    ? string.Join(", ", ipAddresses) 
                    : "Không tìm thấy IP (kiểm tra kết nối mạng)";
            }
            catch (Exception ex)
            {
                Logger.Error($"Lỗi khi lấy địa chỉ IP: {ex.Message}");
                return "Lỗi khi lấy IP";
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
            
            try
            {
                // Giới hạn số dòng để tránh quá tải (giữ tối đa 1000 dòng)
                const int MAX_LINES = 1000;
                if (rtbLog.Lines.Length > MAX_LINES)
                {
                    int linesToRemove = rtbLog.Lines.Length - MAX_LINES + 100;
                    int startIndex = 0;
                    for (int i = 0; i < linesToRemove; i++)
                    {
                        int newlineIndex = rtbLog.Text.IndexOf('\n', startIndex);
                        if (newlineIndex == -1) break;
                        startIndex = newlineIndex + 1;
                    }
                    if (startIndex > 0)
                    {
                        rtbLog.Select(0, startIndex);
                        rtbLog.SelectedText = "";
                    }
                }
                
                // Append text với format đúng
                int start = rtbLog.TextLength;
                rtbLog.AppendText(message + Environment.NewLine);
                rtbLog.Select(start, message.Length);
                rtbLog.SelectionColor = color;
                rtbLog.SelectionLength = 0; // Bỏ selection
                rtbLog.SelectionStart = rtbLog.TextLength; // Di chuyển cursor về cuối
                rtbLog.ScrollToCaret();
            }
            catch (Exception ex)
            {
                // Nếu có lỗi, log vào console (không dùng Logger để tránh vòng lặp)
                System.Diagnostics.Debug.WriteLine($"Lỗi khi append log: {ex.Message}");
            }
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