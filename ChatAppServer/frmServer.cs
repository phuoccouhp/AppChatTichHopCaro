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
            string serverIPs = GetLocalIPAddresses();
            lblServerIP.Text = $"Server IP: {serverIPs} (Port: {PORT})";
            lblServerIP.ForeColor = Color.DarkBlue;
            
            Logger.Info($"Địa chỉ IP của máy chủ: {serverIPs}");
            Logger.Info($"Clients có thể kết nối đến:");
            Logger.Info($"  - 127.0.0.1:{PORT} (nếu chạy trên cùng máy - localhost)");
            // Tách IP mạng từ chuỗi (format: "127.0.0.1, 192.168.x.x")
            string networkIP = null;
            if (serverIPs.Contains(","))
            {
                var parts = serverIPs.Split(',');
                if (parts.Length > 1)
                {
                    networkIP = parts[1].Trim();
                }
            }
            if (!string.IsNullOrEmpty(networkIP) && networkIP != "127.0.0.1")
            {
                Logger.Info($"  - {networkIP}:{PORT} (nếu kết nối từ máy khác trên cùng WiFi)");
            }
            Logger.Info("Lưu ý: Chỉ nhập MỘT IP vào form Login (127.0.0.1 hoặc IP mạng)");
            Logger.Info("Đảm bảo cả hai máy đều cùng mạng WiFi và firewall cho phép port 9000");

            _server = new Server(PORT);
            _server.OnUserListChanged += Server_OnUserListChanged;

            Task.Run(async () =>
            {
                Logger.Success($"Server khởi động tại port {PORT}...");
                Logger.Success($"Đang lắng nghe kết nối từ TẤT CẢ interfaces (localhost + IP mạng)...");
                await _server.StartAsync();
            });
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

        private void btnOpenFirewall_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "Mở port 9000 trên Windows Firewall?\n\n" +
                "Điều này cho phép các máy khác trong mạng kết nối đến Server.\n" +
                "Yêu cầu quyền Administrator (sẽ hiện hộp thoại UAC).",
                "Mở Firewall",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    btnOpenFirewall.Enabled = false;
                    btnOpenFirewall.Text = "Đang mở...";

                    bool success = FirewallHelper.OpenPortAsAdmin(PORT, "ChatAppServer");

                    if (success)
                    {
                        Logger.Success($"✓ Đã mở port {PORT} trên Windows Firewall thành công!");
                        btnOpenFirewall.Text = "✓ Đã mở";
                        btnOpenFirewall.BackColor = Color.Green;
                        
                        MessageBox.Show(
                            $"Đã mở port {PORT} thành công!\n\n" +
                            "Bây giờ các máy khác có thể kết nối đến Server.\n" +
                            "Hãy đảm bảo cả hai máy cùng một mạng WiFi.",
                            "Thành công",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else
                    {
                        throw new Exception("Không thể mở port. Có thể bạn đã từ chối UAC.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Lỗi mở Firewall: {ex.Message}");
                    btnOpenFirewall.Text = "🔓 Mở Firewall";
                    btnOpenFirewall.Enabled = true;
                    
                    MessageBox.Show(
                        $"Lỗi: {ex.Message}\n\n" +
                        "Bạn có thể mở Firewall thủ công:\n" +
                        "1. Mở Windows Defender Firewall\n" +
                        "2. Chọn 'Inbound Rules' → 'New Rule'\n" +
                        "3. Chọn 'Port' → TCP → Port 9000\n" +
                        "4. Cho phép kết nối (Allow)",
                        "Lỗi mở Firewall",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
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

        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            // Hiển thị dialog để nhập IP test
            string testIP = ShowInputDialog(
                "Nhập địa chỉ IP cần test kết nối:\n\n" +
                "Ví dụ: Nếu bạn của bạn có IP 10.45.210.103,\n" +
                "hãy nhập IP đó để kiểm tra xem có thể kết nối được không.",
                "Test Kết Nối Mạng");

            if (string.IsNullOrWhiteSpace(testIP)) return;

            Logger.Info($"=== BẮT ĐẦU TEST KẾT NỐI ĐẾN {testIP} ===");

            // 1. Ping test
            Logger.Info("[1/3] Đang ping...");
            var pingResult = FirewallHelper.Ping(testIP);
            if (pingResult.success)
            {
                Logger.Success($"✓ Ping thành công: {pingResult.message}");
            }
            else
            {
                Logger.Error($"✗ Ping thất bại: {pingResult.message}");
            }

            // 2. Test port 9000 trên máy đó
            Logger.Info("[2/3] Đang test port 9000 trên máy đích...");
            var portResult = FirewallHelper.TestConnection(testIP, PORT);
            if (portResult.success)
            {
                Logger.Success($"✓ Port test thành công: {portResult.message} ({portResult.latencyMs}ms)");
            }
            else
            {
                Logger.Warning($"⚠ Port test: {portResult.message}");
            }

            // 3. Kiểm tra firewall local
            Logger.Info("[3/3] Đang kiểm tra Firewall local...");
            bool firewallOpen = FirewallHelper.IsPortOpen(PORT);
            if (firewallOpen)
            {
                Logger.Success($"✓ Firewall rule 'ChatAppServer' đã được tạo");
            }
            else
            {
                Logger.Warning($"⚠ Firewall rule 'ChatAppServer' chưa được tạo - Hãy click 'Mở Firewall'");
            }

            // 4. Hiển thị tất cả IP của máy này
            Logger.Info("--- Tất cả IP của máy này ---");
            var allIPs = FirewallHelper.GetAllLocalIPs();
            foreach (var ip in allIPs)
            {
                Logger.Info($"  • {ip}");
            }

            Logger.Info($"=== KẾT THÚC TEST ===");

            // Tổng kết
            string summary = $"KẾT QUẢ TEST ĐẾN {testIP}:\n\n" +
                $"1. Ping: {(pingResult.success ? "✓ OK" : "✗ FAIL")}\n" +
                $"2. Port 9000: {(portResult.success ? "✓ OK" : "✗ FAIL")}\n" +
                $"3. Firewall local: {(firewallOpen ? "✓ Đã mở" : "✗ Chưa mở")}\n\n";

            if (!pingResult.success)
            {
                summary += "⚠ PING THẤT BẠI:\n" +
                    "- Kiểm tra hai máy có cùng mạng WiFi không\n" +
                    "- Kiểm tra IP có đúng không\n" +
                    "- Một số mạng có AP Isolation (ngăn thiết bị giao tiếp)\n\n";
            }

            if (!portResult.success && pingResult.success)
            {
                summary += "⚠ PORT 9000 KHÔNG KẾT NỐI ĐƯỢC:\n" +
                    "- Chạy file OpenFirewall.bat với quyền Admin trên CẢ HAI máy\n" +
                    "- Hoặc tắt Windows Firewall tạm thời để test\n\n";
            }

            MessageBox.Show(summary, "Kết Quả Test", MessageBoxButtons.OK,
                pingResult.success && portResult.success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        private void btnShowHelp_Click(object sender, EventArgs e)
        {
            string helpText = @"HƯỚNG DẪN KẾT NỐI GIỮA HAI MÁY

═══════════════════════════════════════
BƯỚC 1: TRÊN MÁY SERVER (máy này)
═══════════════════════════════════════
1. Click 'Mở Firewall' → Đồng ý UAC
2. Click 'Start Server'
3. Ghi lại IP hiển thị (vd: 10.45.100.45)

═══════════════════════════════════════
BƯỚC 2: TRÊN MÁY CLIENT (máy khác)
═══════════════════════════════════════
1. Chạy file 'OpenFirewall.bat' với quyền Admin
2. Mở ChatAppClient
3. Nhập IP của máy Server (vd: 10.45.100.45)
4. Đăng nhập

═══════════════════════════════════════
NẾU VẪN KHÔNG KẾT NỐI ĐƯỢC:
═══════════════════════════════════════
1. Mở CMD trên máy Client, gõ:
   ping <IP máy Server>
   
   - Nếu 'Request timed out' → Hai máy không cùng mạng
   - Nếu 'Reply from...' → Mạng OK, kiểm tra Firewall

2. Tạm tắt Windows Firewall trên CẢ HAI máy:
   - Mở Windows Security → Firewall & network protection
   - Tắt firewall cho 'Private network'

3. Kiểm tra IP:
   - Mở CMD, gõ: ipconfig
   - Xem 'IPv4 Address' trong phần WiFi

═══════════════════════════════════════
LƯU Ý QUAN TRỌNG:
═══════════════════════════════════════
• Hai máy PHẢI cùng một mạng WiFi
• IP 127.0.0.1 chỉ dùng khi Client và Server CÙNG MÁY
• Một số mạng công ty/trường học chặn kết nối P2P
";

            MessageBox.Show(helpText, "Trợ Giúp Kết Nối", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #region Lấy địa chỉ IP Local

        /// <summary>
        /// Lấy địa chỉ IP: trả về cả 127.0.0.1 (localhost) và IP mạng (để kết nối từ máy khác)
        /// </summary>
        private string GetLocalIPAddresses()
        {
            string networkIP = null;
            
            // Lấy IP từ interface đang active (interface đang kết nối internet/WiFi)
            try
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    var endPoint = socket.LocalEndPoint as IPEndPoint;
                    if (endPoint != null)
                    {
                        networkIP = endPoint.Address.ToString();
                    }
                }
            }
            catch
            {
                // Nếu không kết nối được internet, bỏ qua và thử cách khác
            }

            // Fallback: Nếu không lấy được bằng cách trên, thử lấy từ host entry
            if (string.IsNullOrEmpty(networkIP))
            {
                try
                {
                    var host = Dns.GetHostEntry(Dns.GetHostName());
                    foreach (var ip in host.AddressList)
                    {
                        // Chỉ lấy IPv4 và không phải loopback (127.0.0.1)
                        if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                        {
                            networkIP = ip.ToString();
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Lỗi khi lấy địa chỉ IP từ host entry: {ex.Message}");
                }
            }

            // Trả về cả 127.0.0.1 và IP mạng (dùng dấu phẩy để dễ copy)
            if (!string.IsNullOrEmpty(networkIP))
            {
                return $"127.0.0.1, {networkIP}";
            }
            else
            {
                return "127.0.0.1";
            }
        }

        /// <summary>
        /// Hiển thị dialog nhập liệu đơn giản
        /// </summary>
        private string ShowInputDialog(string prompt, string title)
        {
            Form inputForm = new Form
            {
                Width = 450,
                Height = 200,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label promptLabel = new Label { Left = 20, Top = 20, Width = 400, Height = 80, Text = prompt };
            TextBox textBox = new TextBox { Left = 20, Top = 110, Width = 400 };
            Button okButton = new Button { Text = "OK", Left = 260, Width = 75, Top = 140, DialogResult = DialogResult.OK };
            Button cancelButton = new Button { Text = "Cancel", Left = 345, Width = 75, Top = 140, DialogResult = DialogResult.Cancel };

            inputForm.Controls.Add(promptLabel);
            inputForm.Controls.Add(textBox);
            inputForm.Controls.Add(okButton);
            inputForm.Controls.Add(cancelButton);
            inputForm.AcceptButton = okButton;
            inputForm.CancelButton = cancelButton;

            return inputForm.ShowDialog() == DialogResult.OK ? textBox.Text : "";
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