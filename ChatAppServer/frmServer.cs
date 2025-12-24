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
        private ContextMenuStrip _ctxUserMenu; // Menu chuột phải cho danh sách user

        public frmServer()
        {
            InitializeComponent();
            InitializeContextMenu(); // Tạo menu chuột phải

            // Đăng ký sự kiện MouseDown để chọn user khi click chuột phải
            lstUsers.MouseDown += lstUsers_MouseDown;
        }

        // Tạo menu chuột phải bằng code
        private void InitializeContextMenu()
        {
            _ctxUserMenu = new ContextMenuStrip();

            var itemInfo = new ToolStripMenuItem("🔍 Xem thông tin User");
            itemInfo.Click += (s, e) => ShowUserInfo();

            var itemKick = new ToolStripMenuItem("👢 Kick (Ngắt kết nối)");
            itemKick.ForeColor = Color.Red;
            itemKick.Click += (s, e) => KickSelectedUser();

            _ctxUserMenu.Items.Add(itemInfo);
            _ctxUserMenu.Items.Add(new ToolStripSeparator());
            _ctxUserMenu.Items.Add(itemKick);

            lstUsers.ContextMenuStrip = _ctxUserMenu;
        }

        private void frmServer_Load(object sender, EventArgs e)
        {
            // Đăng ký nhận log
            Logger.OnLogReceived += Logger_OnLogReceived;

            // Cấu hình RichTextBox Log
            rtbLog.ReadOnly = true;
            rtbLog.BackColor = Color.FromArgb(30, 30, 30);
            rtbLog.ForeColor = Color.White;
            rtbLog.Font = new Font("Consolas", 9, FontStyle.Regular);
            rtbLog.WordWrap = true;

            // Hiển thị IP ngay khi mở form
            UpdateServerIPDisplay();

            // Kiểm tra trạng thái Firewall ngay lập tức để cập nhật màu nút
            CheckFirewallStatusForButton();
        }

        private void UpdateServerIPDisplay()
        {
            string localIPs = GetLocalIPAddresses();
            string wifiIP = null;

            if (localIPs.Contains(","))
            {
                var parts = localIPs.Split(',');
                if (parts.Length > 1) wifiIP = parts[1].Trim();
            }

            if (!string.IsNullOrEmpty(wifiIP))
            {
                lblServerIP.Text = $"IP LAN/WiFi: {wifiIP} (Port: {PORT})";
                lblServerIP.ForeColor = Color.Blue;
            }
            else
            {
                lblServerIP.Text = $"IP Local: {localIPs} (Port: {PORT})";
                lblServerIP.ForeColor = Color.OrangeRed;
            }
        }

        private void CheckFirewallStatusForButton()
        {
            // Kiểm tra nhẹ (không delay) để set màu nút
            bool isOpen = FirewallHelper.IsPortOpen(PORT, "ChatAppServer");
            if (isOpen)
            {
                btnOpenFirewall.Text = "✓ Firewall OK";
                btnOpenFirewall.BackColor = Color.Green;
                // Vẫn cho phép click để mở lại nếu cần (trường hợp lỗi profile)
            }
            else
            {
                btnOpenFirewall.Text = "🔓 Mở Firewall";
                btnOpenFirewall.BackColor = Color.Orange;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            // 1. Kiểm tra Port có bị chiếm không
            if (FirewallHelper.IsPortInUse(PORT))
            {
                Logger.Error($"Port {PORT} đang bị chiếm dụng bởi phần mềm khác!");
                MessageBox.Show($"Port {PORT} đang bận. Vui lòng tắt ứng dụng đang chạy port này (hoặc server đang chạy ngầm).",
                    "Lỗi Port", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            btnStart.Enabled = false;
            btnStop.Enabled = true;
            lblStatus.Text = "Status: Starting...";
            lblStatus.ForeColor = Color.Orange;

            // 2. Khởi tạo và chạy Server
            try
            {
                _server = new Server(PORT);
                _server.OnUserListChanged += Server_OnUserListChanged;

                // Chạy Async để không treo UI
                Task.Run(async () =>
                {
                    try
                    {
                        await _server.StartAsync();

                        // Cập nhật UI khi start thành công
                        Invoke(new Action(() =>
                        {
                            lblStatus.Text = "Status: Running ✓";
                            lblStatus.ForeColor = Color.Green;
                        }));
                    }
                    catch (SocketException sockEx)
                    {
                        Logger.Error($"✗ Lỗi Socket khi khởi động server: {sockEx.SocketErrorCode} - {sockEx.Message}", sockEx);
                        // Khôi phục lại button
                        try
                        {
                            if (InvokeRequired)
                            {
                                Invoke(new Action(() =>
                                {
                                    btnStart.Enabled = true;
                                    btnStop.Enabled = false;
                                    lblStatus.Text = "Server: Socket Error";
                                    lblStatus.ForeColor = Color.Red;
                                }));
                            }
                        }
                        catch (Exception invokeEx)
                        {
                            Logger.Error($"Lỗi khi khôi phục UI: {invokeEx.Message}");
                        }
                    }
                    catch (InvalidOperationException ioEx)
                    {
                        Logger.Error($"✗ Lỗi InvalidOperation khi khởi động server: {ioEx.Message}", ioEx);
                        // Khôi phục lại button
                        try
                        {
                            if (InvokeRequired)
                            {
                                Invoke(new Action(() =>
                                {
                                    btnStart.Enabled = true;
                                    btnStop.Enabled = false;
                                    lblStatus.Text = "Server: Error";
                                    lblStatus.ForeColor = Color.Red;
                                }));
                            }
                        }
                        catch (Exception invokeEx)
                        {
                            Logger.Error($"Lỗi khi khôi phục UI: {invokeEx.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
<<<<<<< HEAD
                        Logger.Error($"✗ Lỗi khi khởi động server: {ex.GetType().Name} - {ex.Message}", ex);
                        // Khôi phục lại button
                        try
=======
                        Logger.Error($"Server crash: {ex.Message}");
                        Invoke(new Action(() =>
>>>>>>> b682e7f49d0e9c32014cb9ee85c85a1bd8884fdf
                        {
                            btnStart.Enabled = true;
                            btnStop.Enabled = false;
                            lblStatus.Text = "Status: Error";
                            lblStatus.ForeColor = Color.Red;
                        }));
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Error($"Không thể tạo Server: {ex.Message}");
                btnStart.Enabled = true;
                btnStop.Enabled = false;
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
            Logger.Warning("Server đã dừng.");

            lstUsers.Items.Clear();
        }

        // === XỬ LÝ NÚT MỞ FIREWALL (INBOUND + OUTBOUND) ===
        private void btnOpenFirewall_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                $"Bạn có muốn mở Port {PORT} (TCP) cho cả Inbound và Outbound không?\n\n" +
                "Thao tác này sẽ chạy lệnh CMD quyền Admin để cấu hình Windows Firewall.\n" +
                "Vui lòng nhấn YES khi hộp thoại UAC hiện lên.",
                "Cấu hình Firewall",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                btnOpenFirewall.Enabled = false;
                btnOpenFirewall.Text = "Đang xử lý...";

                Task.Run(() =>
                {
                    // Gọi hàm Helper đã có sẵn
                    bool success = FirewallHelper.OpenPortAsAdmin(PORT, "ChatAppServer");

                    Invoke(new Action(() =>
                    {
                        btnOpenFirewall.Enabled = true;
                        if (success)
                        {
                            btnOpenFirewall.Text = "✓ Firewall OK";
                            btnOpenFirewall.BackColor = Color.Green;
                            MessageBox.Show("Đã mở Firewall thành công!\nClient từ máy khác có thể kết nối ngay bây giờ.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            btnOpenFirewall.Text = "Thử lại";
                            btnOpenFirewall.BackColor = Color.Red;
                            MessageBox.Show("Không thể mở Firewall tự động.\n\n" +
                                "Nguyên nhân có thể:\n" +
                                "1. Bạn đã từ chối quyền Admin (UAC).\n" +
                                "2. Antivirus chặn script.\n\n" +
                                "Hãy mở thủ công trong Windows Defender Firewall.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }));
                });
            }
        }

        // === XỬ LÝ NÚT TEST KẾT NỐI ===
        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            // Kiểm tra toàn diện
            bool isListening = FirewallHelper.IsPortListening(PORT);
            bool isFirewallOpen = FirewallHelper.IsPortOpen(PORT, "ChatAppServer");
            bool isInUse = FirewallHelper.IsPortInUse(PORT);
            string ip = GetLocalIPAddresses();

            string statusMsg = "=== KẾT QUẢ CHẨN ĐOÁN ===\n\n";

            // 1. Kiểm tra Server chạy chưa
            statusMsg += $"1. Server Listening (Port {PORT}): " + (isListening ? "✅ ĐANG CHẠY" : "❌ CHƯA CHẠY") + "\n";
            if (!isListening) statusMsg += "   -> Hãy nhấn nút 'Start Server' trước.\n";

            // 2. Kiểm tra Firewall
            statusMsg += $"2. Windows Firewall Rule: " + (isFirewallOpen ? "✅ ĐÃ CÓ RULE" : "⚠️ CHƯA CÓ RULE") + "\n";
            if (!isFirewallOpen) statusMsg += "   -> Hãy nhấn nút 'Mở Firewall'.\n";

            // 3. Kiểm tra IP
            statusMsg += $"3. IP Máy Chủ: {ip}\n";

            // 4. Lời khuyên
            statusMsg += "\n=== LỜI KHUYÊN NẾU CLIENT KHÔNG KẾT NỐI ĐƯỢC ===\n";
            statusMsg += "- Đảm bảo Client nhập đúng IP (ưu tiên IP Wifi 192.168.x.x).\n";
            statusMsg += "- Đảm bảo 2 máy PING thấy nhau.\n";
            statusMsg += "- Tắt tạm thời phần mềm diệt virus (Kaspersky, McAfee...) để test.\n";
            statusMsg += "- Nếu dùng Wifi công cộng (Cafe/Trường học), họ thường chặn kết nối giữa các máy (AP Isolation). Hãy thử phát Wifi từ điện thoại (4G) rồi cho 2 máy kết nối vào.";

            MessageBox.Show(statusMsg, "Chẩn đoán kết nối", MessageBoxButtons.OK, isListening ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }

        private void btnShowHelp_Click(object sender, EventArgs e)
        {
            string help = "HƯỚNG DẪN:\n\n" +
                          "1. Nhấn 'Mở Firewall' -> Chọn Yes -> Đồng ý Admin.\n" +
                          "2. Nhấn 'Start Server'.\n" +
                          "3. Gửi IP (dòng chữ xanh) cho bạn bè nhập vào Client.\n\n" +
                          "LƯU Ý: Nếu Client báo lỗi kết nối, hãy thử tắt hẳn Firewall của Windows ở cả 2 máy để test.";
            MessageBox.Show(help, "Trợ giúp");
        }

        #region Helper Methods (IP, UI Update)

        private string GetLocalIPAddresses()
        {
            string wifiIP = null;
            try
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    var endPoint = socket.LocalEndPoint as IPEndPoint;
                    wifiIP = endPoint?.Address.ToString();
                }
            }
            catch { }

            if (!string.IsNullOrEmpty(wifiIP)) return $"127.0.0.1, {wifiIP}";

            // Fallback
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var ips = host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).Select(ip => ip.ToString());
            return string.Join(", ", ips);
        }

        private void Logger_OnLogReceived(string message, Color color)
        {
            if (rtbLog.IsDisposed) return;
            if (rtbLog.InvokeRequired)
            {
                rtbLog.Invoke(new Action(() => Logger_OnLogReceived(message, color)));
                return;
            }

            // Giới hạn dòng log
            if (rtbLog.Lines.Length > 1000)
            {
                rtbLog.Select(0, rtbLog.GetFirstCharIndexFromLine(100));
                rtbLog.SelectedText = "";
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
                Invoke(new Action(() => Server_OnUserListChanged(users)));
                return;
            }
            lstUsers.Items.Clear();
            foreach (var u in users) lstUsers.Items.Add(u);
        }

        #endregion

        #region Xử lý Menu Chuột Phải

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
            // Format: "Tên (ID)" -> Cắt lấy ID
            string itemText = lstUsers.SelectedItem.ToString();
            int open = itemText.LastIndexOf('(');
            int close = itemText.LastIndexOf(')');
            if (open != -1 && close != -1)
            {
                return itemText.Substring(open + 1, close - open - 1);
            }
            return null;
        }

        private void ShowUserInfo()
        {
            string? id = GetSelectedUserID();
            if (id != null && _server != null)
            {
                MessageBox.Show(_server.GetUserInfo(id), "Thông tin User");
            }
        }

        private void KickSelectedUser()
        {
            string? id = GetSelectedUserID();
            if (id != null && _server != null)
            {
                if (MessageBox.Show($"Bạn chắc chắn muốn kick user '{id}'?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    _server.KickUser(id);
                }
            }
        }

        #endregion
    }
}