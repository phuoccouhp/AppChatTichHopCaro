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
            string wifiIP = null;
            if (localIPs.Contains(","))
            {
                var parts = localIPs.Split(',');
                if (parts.Length > 1)
                {
                    wifiIP = parts[1].Trim();
                }
            }
            
            if (!string.IsNullOrEmpty(wifiIP))
            {
                lblServerIP.Text = $"Server IP: {wifiIP} (Port: {PORT})";
                lblServerIP.ForeColor = Color.Gray;
            }
            else
            {
                lblServerIP.Text = $"Server IP: {localIPs} (Port: {PORT})";
                lblServerIP.ForeColor = Color.Orange;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            // Kiểm tra port có đang được sử dụng bởi process khác không
            if (FirewallHelper.IsPortInUse(PORT))
            {
                Logger.Error($"✗ Port {PORT} đang được sử dụng bởi process khác!");
                Logger.Error("Hãy đóng ứng dụng khác đang dùng port này hoặc thay đổi port.");
                
                var result = MessageBox.Show(
                    $"Port {PORT} đang được sử dụng bởi process khác!\n\n" +
                    "Bạn có muốn kiểm tra process nào đang dùng port này không?",
                    "Port đã được sử dụng",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                
                if (result == DialogResult.Yes)
                {
                    CheckPortStatus();
                }
                return;
            }

            btnStart.Enabled = false;
            btnStop.Enabled = true;
            lblStatus.Text = "Server: Starting...";
            lblStatus.ForeColor = Color.Orange;

            // Lấy và hiển thị địa chỉ IP WiFi thực tế của máy Server
            string serverIPs = GetLocalIPAddresses();
            
            // Tách IP WiFi từ chuỗi (format: "127.0.0.1, 10.45.100.45")
            string wifiIP = null;
            if (serverIPs.Contains(","))
            {
                var parts = serverIPs.Split(',');
                if (parts.Length > 1)
                {
                    wifiIP = parts[1].Trim();
                }
            }
            
            if (!string.IsNullOrEmpty(wifiIP))
            {
                lblServerIP.Text = $"Server IP: {wifiIP} (Port: {PORT})";
                lblServerIP.ForeColor = Color.Green;
            }
            else
            {
                lblServerIP.Text = $"Server IP: {serverIPs} (Port: {PORT})";
                lblServerIP.ForeColor = Color.Orange;
            }
            
            Logger.Info($"Địa chỉ IP của máy chủ: {serverIPs}");
            Logger.Info($"Clients có thể kết nối đến:");
            Logger.Info($"  - 127.0.0.1:{PORT} (nếu chạy trên cùng máy - localhost)");
            
            if (!string.IsNullOrEmpty(wifiIP) && wifiIP != "127.0.0.1")
            {
                Logger.Success($"  - {wifiIP}:{PORT} (IP WiFi của Server - dùng IP này để kết nối từ máy khác)");
            }
            Logger.Info("Đảm bảo cả hai máy đều cùng mạng WiFi và firewall cho phép port 9000");

            try
            {
                _server = new Server(PORT);
                _server.OnUserListChanged += Server_OnUserListChanged;

                // Sử dụng ContinueWith để đảm bảo exception được handle
                Task.Run(async () =>
                {
                    try
                    {
                        Logger.Success($"Server khởi động tại port {PORT}...");
                        Logger.Success($"Đang lắng nghe kết nối từ TẤT CẢ interfaces (localhost + IP mạng)...");
                        await _server.StartAsync();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"✗ Lỗi khi khởi động server: {ex.Message}", ex);
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
                }).ContinueWith(task =>
                {
                    // Đảm bảo exception từ Task.Run được handle
                    if (task.IsFaulted && task.Exception != null)
                    {
                        foreach (var ex in task.Exception.InnerExceptions)
                        {
                            Logger.Error($"✗ Exception trong Task.Run start server: {ex.GetType().Name} - {ex.Message}", ex);
                        }
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
                        catch { }
                    }
                }, TaskContinuationOptions.OnlyOnFaulted);
            }
            catch (Exception ex)
            {
                Logger.Error($"✗ Lỗi khi tạo Server object: {ex.Message}", ex);
                btnStart.Enabled = true;
                btnStop.Enabled = false;
                lblStatus.Text = "Server: Error";
                lblStatus.ForeColor = Color.Red;
                MessageBox.Show($"Không thể khởi động server: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Đợi một chút rồi kiểm tra xem server có đang listen không
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(2000); // Đợi 2 giây để server khởi động
                    
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() =>
                        {
                            try
                            {
                                if (FirewallHelper.IsPortListening(PORT))
                                {
                                    lblStatus.Text = "Server: Running ✓";
                                    lblStatus.ForeColor = Color.Green;
                                    Logger.Success($"✓ Xác nhận: Port {PORT} đang lắng nghe thành công!");
                                }
                                else
                                {
                                    // Không cần đổi status nếu server đang chạy nhưng port check thất bại
                                    // Có thể do firewall hoặc port check không chính xác
                                    Logger.Warning($"⚠ Port {PORT} check thất bại, nhưng server có thể vẫn đang chạy");
                                }
                            }
                            catch (Exception checkEx)
                            {
                                Logger.Warning($"Lỗi khi kiểm tra port: {checkEx.Message}");
                            }
                        }));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Lỗi trong task kiểm tra port: {ex.Message}");
                }
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
                    Application.DoEvents(); // Cập nhật UI ngay lập tức

                    Logger.Info($"Đang mở port {PORT} trên Windows Firewall...");
                    bool success = FirewallHelper.OpenPortAsAdmin(PORT, "ChatAppServer");

                    if (!success)
                    {
                        Logger.Warning("OpenPortAsAdmin trả về false. Kiểm tra lại rule...");
                    }

                    // Đợi một chút để rule được tạo và commit vào firewall
                    System.Threading.Thread.Sleep(2000); // Tăng lên 2 giây để đảm bảo
                    
                    // Kiểm tra lại xem rule đã được tạo chưa (với retry mechanism)
                    Logger.Info($"Đang kiểm tra lại firewall rule sau khi tạo...");
                    bool ruleExists = FirewallHelper.IsPortOpen(PORT, "ChatAppServer", retryCount: 5, delayMs: 500);
                    Logger.Info($"Kết quả kiểm tra: success={success}, ruleExists={ruleExists}");
                    
                    if (success && ruleExists)
                    {
                        Logger.Success($"✓ Đã mở port {PORT} trên Windows Firewall thành công!");
                        
                        if (InvokeRequired)
                        {
                            Invoke(new Action(() =>
                            {
                                btnOpenFirewall.Text = "✓ Đã mở";
                                btnOpenFirewall.BackColor = Color.Green;
                                btnOpenFirewall.Enabled = false; // Disable để tránh click lại
                            }));
                        }
                        else
                        {
                            btnOpenFirewall.Text = "✓ Đã mở";
                            btnOpenFirewall.BackColor = Color.Green;
                            btnOpenFirewall.Enabled = false;
                        }
                        
                        MessageBox.Show(
                            $"Đã mở port {PORT} thành công!\n\n" +
                            "Bây giờ các máy khác có thể kết nối đến Server.\n" +
                            "Hãy đảm bảo cả hai máy cùng một mạng WiFi.\n\n" +
                            "Lưu ý: Cần mở firewall trên CẢ HAI máy (Server và Client).",
                            "Thành công",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else if (ruleExists)
                    {
                        // Rule đã tồn tại nhưng OpenPortAsAdmin trả về false (có thể do đã có rule từ trước)
                        Logger.Success($"✓ Port {PORT} đã được mở (rule đã tồn tại)!");
                        
                        if (InvokeRequired)
                        {
                            Invoke(new Action(() =>
                            {
                                btnOpenFirewall.Text = "✓ Đã mở";
                                btnOpenFirewall.BackColor = Color.Green;
                                btnOpenFirewall.Enabled = false;
                            }));
                        }
                        else
                        {
                            btnOpenFirewall.Text = "✓ Đã mở";
                            btnOpenFirewall.BackColor = Color.Green;
                            btnOpenFirewall.Enabled = false;
                        }
                        
                        MessageBox.Show(
                            $"Port {PORT} đã được mở trước đó!\n\n" +
                            "Rule firewall đã tồn tại, bạn có thể tiếp tục sử dụng.",
                            "Thông báo",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else
                    {
                        // Thất bại
                        string errorDetail = "";
                        if (!success) errorDetail += "- Process mở firewall thất bại\n";
                        if (!ruleExists) errorDetail += "- Rule không tồn tại sau khi tạo\n";
                        
                        throw new Exception($"Không thể mở port {PORT}.\n\n{errorDetail}\n" +
                            "Có thể do:\n" +
                            "1. Bạn đã từ chối UAC (yêu cầu quyền Admin)\n" +
                            "2. Firewall service không chạy\n" +
                            "3. Không có quyền Administrator\n\n" +
                            "Giải pháp:\n" +
                            "1. Chạy file OpenFirewall.bat với quyền Admin (Right-click → Run as administrator)\n" +
                            "2. Hoặc mở Firewall thủ công (xem hướng dẫn bên dưới)");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Lỗi mở Firewall: {ex.Message}", ex);
                    
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() =>
                        {
                            btnOpenFirewall.Text = "🔓 Mở Firewall";
                            btnOpenFirewall.BackColor = Color.Orange;
                            btnOpenFirewall.Enabled = true;
                        }));
                    }
                    else
                    {
                        btnOpenFirewall.Text = "🔓 Mở Firewall";
                        btnOpenFirewall.BackColor = Color.Orange;
                        btnOpenFirewall.Enabled = true;
                    }
                    
                    string errorMsg = $"Lỗi: {ex.Message}\n\n" +
                        "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                        "CÁCH 1: CHẠY SCRIPT THỦ CÔNG\n" +
                        "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                        "1. Tìm file 'OpenFirewall.bat' trong thư mục project\n" +
                        "2. Right-click → 'Run as administrator'\n" +
                        "3. Hoặc chạy file 'OpenFirewall.ps1' với PowerShell (Admin)\n\n" +
                        "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                        "CÁCH 2: MỞ FIREWALL THỦ CÔNG\n" +
                        "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                        "1. Mở Windows Security → Firewall & network protection\n" +
                        "2. Advanced settings → Inbound Rules → New Rule\n" +
                        "3. Port → TCP → Port 9000 → Allow → Private/Domain\n" +
                        "4. Lặp lại với Outbound Rules\n\n" +
                        "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                        "CÁCH 3: DÙNG COMMAND LINE\n" +
                        "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                        "Mở CMD với quyền Admin, chạy:\n" +
                        $"netsh advfirewall firewall add rule name=\"ChatAppServer\" dir=in action=allow protocol=TCP localport={PORT} profile=private,domain enable=yes\n" +
                        $"netsh advfirewall firewall add rule name=\"ChatAppServer (Out)\" dir=out action=allow protocol=TCP localport={PORT} profile=private,domain enable=yes\n\n" +
                        "Xem file MO_FIREWALL_THU_CONG.md để biết chi tiết!";
                    
                    MessageBox.Show(errorMsg, "Lỗi mở Firewall", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void CheckPortStatus()
        {
            Logger.Info("=== KIỂM TRA TRẠNG THÁI PORT 9000 ===");
            
            // KIỂM TRA PORT ĐANG LẮNG NGHE TRƯỚC (để biết server có đang chạy không)
            bool portListening = FirewallHelper.IsPortListening(PORT);
            
            // 1. Kiểm tra port có đang được sử dụng bởi process khác
            // CHỈ check nếu server KHÔNG đang chạy (vì nếu server đang chạy thì port rõ ràng đang được sử dụng)
            bool portInUse = false;
            string status1;
            
            if (portListening)
            {
                // Server đang chạy, port rõ ràng đang được sử dụng bởi server này
                Logger.Success($"✓ Port {PORT} đang được sử dụng bởi SERVER (đang chạy)");
                status1 = "✓ ĐANG ĐƯỢC SỬ DỤNG (bởi Server đang chạy)";
            }
            else
            {
                // Server không chạy, kiểm tra xem có process khác đang dùng port không
                portInUse = FirewallHelper.IsPortInUse(PORT);
                if (portInUse)
                {
                    Logger.Warning($"⚠ Port {PORT} đang được sử dụng bởi process KHÁC");
                    Logger.Warning("⚠ KHÔNG THỂ START SERVER - Port đã bị chiếm!");
                    Logger.Info("Để xem process nào đang dùng port, mở CMD và chạy:");
                    Logger.Info($"  netstat -ano | findstr :{PORT}");
                    status1 = "⚠ ĐANG BỊ SỬ DỤNG (bởi process khác - KHÔNG thể start server)";
                }
                else
                {
                    Logger.Success($"✓ Port {PORT} sẵn sàng (không bị process khác sử dụng)");
                    status1 = "✓ SẴN SÀNG (có thể start server)";
                }
            }
            
            // 2. Hiển thị trạng thái server
            if (portListening)
            {
                Logger.Success($"✓ Port {PORT} đang lắng nghe - SERVER ĐANG CHẠY");
            }
            else
            {
                Logger.Info($"ℹ Port {PORT} không lắng nghe (Server chưa start hoặc đã dừng)");
            }
            
            // 3. Kiểm tra firewall
            bool firewallOpen = FirewallHelper.IsPortOpen(PORT);
            if (firewallOpen)
            {
                Logger.Success($"✓ Firewall rule đã được tạo cho port {PORT}");
            }
            else
            {
                Logger.Warning($"⚠ Firewall rule chưa được tạo - Hãy click 'Mở Firewall'");
            }
            
            Logger.Info("=== KẾT THÚC KIỂM TRA ===");
            
            string status2 = portListening 
                ? "✓ ĐANG LẮNG NGHE (Server đang chạy)" 
                : "ℹ KHÔNG LẮNG NGHE (Server chưa start)";
            
            string status3 = firewallOpen 
                ? "✓ Đã mở" 
                : "⚠ Chưa mở";
            
            string summary = $"TRẠNG THÁI PORT {PORT}:\n\n" +
                $"1. Port có sẵn: {status1}\n" +
                $"2. Server đang chạy: {status2}\n" +
                $"3. Firewall rule: {status3}";
            
            // Chỉ hiển thị warning nếu có vấn đề (port bị chiếm bởi process khác, hoặc server không chạy khi đáng lý phải chạy)
            MessageBoxIcon icon = (portInUse && !portListening) || (!portListening && btnStart.Enabled == false)
                ? MessageBoxIcon.Warning 
                : MessageBoxIcon.Information;
            
            MessageBox.Show(summary, "Kiểm Tra Port", MessageBoxButtons.OK, icon);
        }

        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            // Nếu click vào button này, hiển thị menu: Test IP khác hoặc Check Port Local
            var result = MessageBox.Show(
                "Chọn chức năng:\n\n" +
                "• YES: Kiểm tra Port 9000 (Local)\n" +
                "• NO: Test kết nối đến IP khác",
                "Test Kết Nối",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);
            
            if (result == DialogResult.Yes)
            {
                // Kiểm tra Port Local
                CheckPortStatus();
                return;
            }
            else if (result == DialogResult.Cancel)
            {
                return;
            }
            
            // Test IP khác
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
        /// Lấy IP WiFi thực tế của máy Server (KHÔNG phải Default Gateway)
        /// </summary>
        private string GetLocalIPAddresses()
        {
            string wifiIP = null;
            
            // Cách 1: Lấy IP từ socket connection (chính xác nhất)
            try
            {
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    var endPoint = socket.LocalEndPoint as IPEndPoint;
                    if (endPoint != null)
                    {
                        wifiIP = endPoint.Address.ToString();
                        Logger.Info($"[IP] Tìm thấy IP WiFi: {wifiIP}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"Không thể lấy IP WiFi từ socket: {ex.Message}");
            }

            // Cách 2: Nếu không lấy được, thử từ network interfaces
            if (string.IsNullOrEmpty(wifiIP))
            {
                try
                {
                    var networkInterfaces = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
                    foreach (var ni in networkInterfaces)
                    {
                        // Chỉ lấy interface đang hoạt động, không phải loopback
                        if (ni.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up &&
                            ni.NetworkInterfaceType != System.Net.NetworkInformation.NetworkInterfaceType.Loopback &&
                            (ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Wireless80211 ||
                             ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Ethernet))
                        {
                            var properties = ni.GetIPProperties();
                            foreach (var unicast in properties.UnicastAddresses)
                            {
                                if (unicast.Address.AddressFamily == AddressFamily.InterNetwork &&
                                    !IPAddress.IsLoopback(unicast.Address))
                                {
                                    wifiIP = unicast.Address.ToString();
                                    Logger.Info($"[IP] Tìm thấy IP từ interface {ni.Name}: {wifiIP}");
                                    break;
                                }
                            }
                            if (!string.IsNullOrEmpty(wifiIP)) break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Lỗi khi lấy IP từ network interfaces: {ex.Message}");
                }
            }

            // Cách 3: Fallback - lấy từ host entry
            if (string.IsNullOrEmpty(wifiIP))
            {
                try
                {
                    var host = Dns.GetHostEntry(Dns.GetHostName());
                    foreach (var ip in host.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                        {
                            wifiIP = ip.ToString();
                            Logger.Info($"[IP] Tìm thấy IP từ host entry: {wifiIP}");
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Lỗi khi lấy địa chỉ IP từ host entry: {ex.Message}");
                }
            }

            // Trả về cả 127.0.0.1 và IP WiFi thực tế
            if (!string.IsNullOrEmpty(wifiIP))
            {
                return $"127.0.0.1, {wifiIP}";
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