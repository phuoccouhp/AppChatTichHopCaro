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
        private ContextMenuStrip _ctxUserMenu;

        public frmServer()
        {
            InitializeComponent();
            InitializeContextMenu();
            // Thêm handler cho nút Test Connection
            btnTestConnection.Click += BtnTestConnection_Click;

            // Xử lý chọn item trong listbox bằng chuột phải
            lstUsers.MouseDown += LstUsers_MouseDown;
        }

        private void frmServer_Load(object sender, EventArgs e)
        {
            // Setup Logger
            Logger.OnLogReceived += Logger_OnLogReceived;

            // Setup giao diện Log
            rtbLog.ReadOnly = true;
            rtbLog.BackColor = Color.FromArgb(30, 30, 30);
            rtbLog.ForeColor = Color.White;
            rtbLog.Font = new Font("Consolas", 9, FontStyle.Regular);

            // Lấy thông tin IP và trạng thái Firewall ban đầu
            UpdateServerIPDisplay();
            CheckFirewallStatusAsync();
        }

        // === 1. LOGIC START / STOP SERVER ===

        private void btnStart_Click(object sender, EventArgs e)
        {
            // Kiểm tra Port
            if (IsPortInUse(PORT))
            {
                MessageBox.Show($"Port {PORT} đang bận!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnStart.Enabled = false;
            lblStatus.Text = "Status: Starting...";
            lblStatus.ForeColor = Color.Orange;

            try
            {
                _server = new Server(PORT);
                _server.OnUserListChanged += Server_OnUserListChanged;

                // --- SỬA ĐOẠN NÀY ---
                // Gọi hàm Start mới (không cần await nữa)
                _server.Start();

                // Cập nhật lại IP hiển thị và kiểm tra port đang lắng nghe
                UpdateServerIPDisplay();
                bool listening = FirewallHelper.IsPortListening(PORT);
                if (!listening)
                {
                    Logger.Warning($"Port {PORT} không lắng nghe trên localhost. Kiểm tra firewall hoặc quyền Admin.");
                    MessageBox.Show($"Server có thể đã bắt đầu nhưng Port {PORT} không lắng nghe trên localhost.\n\nHãy thử: \n- Chạy 'Mở Firewall' và cấp quyền Admin.\n- Kiểm tra lại IP mà client nhập.\n- Kiểm tra AP Isolation trên Router.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // Cập nhật giao diện NGAY LẬP TỨC
                btnStart.Enabled = false;
                btnStop.Enabled = true;
                lblStatus.Text = "Status: Running (Port " + PORT + ")";
                lblStatus.ForeColor = Color.Green;
                // --------------------
            }
            catch (Exception ex)
            {
                Logger.Error($"Server Start Failed: {ex.Message}");
                MessageBox.Show($"Không thể Start Server: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);

                btnStart.Enabled = true;
                btnStop.Enabled = false;
                lblStatus.Text = "Status: Error";
                lblStatus.ForeColor = Color.Red;
                _server = null;
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (_server != null)
            {
                _server.Stop();
                _server = null;
            }

            btnStart.Enabled = true;
            btnStop.Enabled = false;
            lblStatus.Text = "Status: Stopped";
            lblStatus.ForeColor = Color.Red;
            lstUsers.Items.Clear();
            Logger.Warning("Server đã dừng.");
        }

        // === 2. LOGIC FIREWALL (Quan Trọng) ===

        private async void btnOpenFirewall_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                $"Hệ thống sẽ yêu cầu quyền Administrator để mở Port {PORT}.\n\n" +
                "Vui lòng chọn YES/ALLOW khi cửa sổ xác nhận hiện lên.",
                "Cấp quyền Admin",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Information);

            if (result == DialogResult.Cancel) return;

            btnOpenFirewall.Enabled = false;
            btnOpenFirewall.Text = "Đang xử lý...";

            await Task.Run(() =>
            {
                try
                {
                    // Gọi hàm Helper mới (sẽ hiện popup UAC)
                    FirewallHelper.OpenPortAsAdmin(PORT, "ChatAppServer");
                }
                catch (Exception ex)
                {
                    Invoke(new Action(() => Logger.Error($"Lỗi script firewall: {ex.Message}")));
                }
            });

            // Kiểm tra lại trạng thái sau khi chạy script
            await CheckFirewallStatusAsync();
            btnOpenFirewall.Enabled = true;
        }

        private async Task CheckFirewallStatusAsync()
        {
            bool isOpen = false;
            await Task.Run(() =>
            {
                isOpen = FirewallHelper.IsPortOpen(PORT, "ChatAppServer");
            });

            Invoke(new Action(() =>
            {
                if (isOpen)
                {
                    btnOpenFirewall.Text = "✓ Firewall đã mở";
                    btnOpenFirewall.BackColor = Color.LightGreen;
                }
                else
                {
                    btnOpenFirewall.Text = "⚠️ Mở Firewall ngay";
                    btnOpenFirewall.BackColor = Color.Orange;
                }
            }));
        }

        // === 3. CÁC HÀM TIỆN ÍCH (HELPER) ===

        private void UpdateServerIPDisplay()
        {
            try
            {
                // Lấy tất cả IP V4 của máy (Trừ localhost)
                var host = Dns.GetHostEntry(Dns.GetHostName());
                var ipList = host.AddressList
                    .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                    .Select(ip => ip.ToString())
                    .ToList();

                if (ipList.Count > 0)
                {
                    lblServerIP.Text = $"IP Client cần nhập: {string.Join("  HOẶC  ", ipList)}";
                    lblServerIP.ForeColor = Color.Blue;
                    Logger.Info($"Các địa chỉ IP khả dụng: {string.Join(", ", ipList)}");
                }
                else
                {
                    lblServerIP.Text = "Không tìm thấy IP mạng LAN. Kiểm tra kết nối Wifi/Dây.";
                    lblServerIP.ForeColor = Color.Red;
                }
            }
            catch
            {
                lblServerIP.Text = "Lỗi khi lấy IP máy.";
            }
        }

        private bool IsPortInUse(int port)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    // Thử kết nối vào port chính mình, nếu được -> Port đang bị chiếm
                    var result = client.BeginConnect("127.0.0.1", port, null, null);
                    bool success = result.AsyncWaitHandle.WaitOne(100); // Check nhanh 100ms
                    return success;
                }
            }
            catch { return false; }
        }

        // === 4. XỬ LÝ UI / THREAD SAFE ===

        private void Logger_OnLogReceived(string message, Color color)
        {
            if (rtbLog.IsDisposed) return;
            if (InvokeRequired) { Invoke(new Action(() => Logger_OnLogReceived(message, color))); return; }

            // Auto-scroll logic
            rtbLog.SelectionStart = rtbLog.TextLength;
            rtbLog.SelectionLength = 0;
            rtbLog.SelectionColor = color;
            rtbLog.AppendText(message + Environment.NewLine);
            rtbLog.ScrollToCaret();
        }

        private void Server_OnUserListChanged(List<string> users)
        {
            if (lstUsers.IsDisposed) return;
            if (InvokeRequired) { Invoke(new Action(() => Server_OnUserListChanged(users))); return; }

            lstUsers.Items.Clear();
            foreach (var u in users) lstUsers.Items.Add(u);
        }

        // === 5. CONTEXT MENU (CHUỘT PHẢI USER) ===

        private void InitializeContextMenu()
        {
            _ctxUserMenu = new ContextMenuStrip();
            var itemInfo = new ToolStripMenuItem("🔍 Xem thông tin");
            itemInfo.Click += (s, e) => ShowUserInfo();

            var itemKick = new ToolStripMenuItem("👢 Kick user");
            itemKick.ForeColor = Color.Red;
            itemKick.Click += (s, e) => KickSelectedUser();

            _ctxUserMenu.Items.Add(itemInfo);
            _ctxUserMenu.Items.Add(new ToolStripSeparator());
            _ctxUserMenu.Items.Add(itemKick);

            lstUsers.ContextMenuStrip = _ctxUserMenu;
        }

        private void LstUsers_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int index = lstUsers.IndexFromPoint(e.Location);
                if (index != ListBox.NoMatches) lstUsers.SelectedIndex = index;
            }
        }

        private void ShowUserInfo()
        {
            if (lstUsers.SelectedItem == null || _server == null) return;
            string userId = ExtractUserId(lstUsers.SelectedItem.ToString());
            if (userId != null) MessageBox.Show(_server.GetUserInfo(userId), "User Info");
        }

        private void KickSelectedUser()
        {
            if (lstUsers.SelectedItem == null || _server == null) return;
            string userId = ExtractUserId(lstUsers.SelectedItem.ToString());

            if (userId != null && MessageBox.Show($"Kick user {userId}?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                _server.KickUser(userId);
            }
        }

        private string? ExtractUserId(string? displayText)
        {
            // Format hiển thị: "TênUser (UserID)"
            if (string.IsNullOrEmpty(displayText)) return null;
            int lastOpen = displayText.LastIndexOf('(');
            int lastClose = displayText.LastIndexOf(')');
            if (lastOpen != -1 && lastClose != -1 && lastClose > lastOpen)
            {
                return displayText.Substring(lastOpen + 1, lastClose - lastOpen - 1);
            }
            return null; // Hoặc trả về chính text nếu không parse được
        }

        private void btnShowHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
               "1. Nhấn 'Mở Firewall ngay' -> Chọn Yes -> Chờ thông báo thành công.\n" +
               "2. Nhấn 'Start Server'.\n" +
               "3. Copy địa chỉ IP màu xanh dương và gửi cho Client.\n" +
               "4. Client nhập IP đó vào ô 'Server IP' để kết nối.",
               "Hướng dẫn");
        }

        private async void BtnTestConnection_Click(object sender, EventArgs e)
        {
            btnTestConnection.Enabled = false;
            btnTestConnection.Text = "Checking...";

            await Task.Run(() =>
            {
                try
                {
                    var host = Dns.GetHostEntry(Dns.GetHostName());
                    var ipList = host.AddressList
                        .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                        .Select(ip => ip.ToString())
                        .ToList();

                    if (ipList.Count == 0)
                    {
                        Logger.Warning("Không có IP mạng để kiểm tra.");
                        Invoke(new Action(() => MessageBox.Show("Không tìm thấy IP mạng để kiểm tra.", "Kiểm tra kết nối", MessageBoxButtons.OK, MessageBoxIcon.Warning)));
                        return;
                    }

                    var sb = new System.Text.StringBuilder();
                    foreach (var ip in ipList)
                    {
                        sb.AppendLine($"-- Kiểm tra IP: {ip}:{PORT} --");

                        var pingResult = FirewallHelper.Ping(ip, 2000);
                        sb.AppendLine($"Ping: {pingResult.message} ({pingResult.latencyMs}ms)");

                        var conn = FirewallHelper.TestConnection(ip, PORT, 3000);
                        sb.AppendLine(conn.message + $" (latency {conn.latencyMs}ms)");

                        bool listening = FirewallHelper.IsPortListening(PORT);
                        sb.AppendLine(listening ? $"Port {PORT} đang LISTEN trên localhost" : $"Port {PORT} KHÔNG LISTEN trên localhost");

                        bool fwOpen = FirewallHelper.IsPortOpen(PORT, "ChatAppServer");
                        sb.AppendLine(fwOpen ? "Firewall rule: OK" : "Firewall rule: MISSING or not verified");
                        sb.AppendLine();
                    }

                    string report = sb.ToString();
                    Logger.Info(report);
                    Invoke(new Action(() => MessageBox.Show(report, "Kết quả kiểm tra kết nối", MessageBoxButtons.OK, MessageBoxIcon.Information)));
                }
                catch (Exception ex)
                {
                    Logger.Error($"Lỗi khi kiểm tra kết nối: {ex.Message}");
                    Invoke(new Action(() => MessageBox.Show($"Lỗi khi kiểm tra: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)));
                }
            });

            btnTestConnection.Text = "🔍 Check Port";
            btnTestConnection.Enabled = true;
        }
    }
}