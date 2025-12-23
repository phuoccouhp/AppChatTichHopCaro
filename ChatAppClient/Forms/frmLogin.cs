using ChatApp.Shared;
using ChatAppClient.Helpers;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatAppClient.Forms
{
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
            // Đảm bảo IsPassword được set sau khi InitializeComponent
            txtPass.IsPassword = true;
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            // Gán sự kiện Click cho nút Đăng nhập
            this.btnLogin.Click += BtnLogin_Click;

            // Đảm bảo txtServerIP có thể nhập được
            txtServerIP.InnerTextBox.ReadOnly = false;
            txtServerIP.InnerTextBox.Enabled = true;
            txtServerIP.Enabled = true;

            // Thiết lập mặc định
            UpdateLoginFieldPlaceholder();
            // (Optional) Pre-fill for testing if you want
            // txtServerIP.Text = "127.0.0.1";
            // txtUser.Text = "user1";
            // txtPass.Text = "123";
        }

        private void RdoLoginType_CheckedChanged(object sender, EventArgs e)
        {
            UpdateLoginFieldPlaceholder();
        }

        private void UpdateLoginFieldPlaceholder()
        {
            if (rdoEmail.Checked)
            {
                txtUser.PlaceholderText = "Email";
                txtUser.Text = "Email";
            }
            else
            {
                txtUser.PlaceholderText = "Username";
                txtUser.Text = "Username";
            }
        }

        // Sự kiện click nút Đăng nhập (Async)
        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            // Lấy dữ liệu từ các RoundedTextBox
            string serverIp = txtServerIP.Text.Trim();
            string usernameOrEmail = txtUser.Text.Trim();
            string password = txtPass.Text.Trim();
            bool useEmail = rdoEmail.Checked;

            // 1. Kiểm tra đầu vào
            if (string.IsNullOrEmpty(serverIp))
            {
                MessageBox.Show("Vui lòng nhập địa chỉ IP của máy chủ.\n\n" +
                    "Lấy IP từ máy chủ:\n" +
                    "- Mở form Server\n" +
                    "- Nhấn Start Server\n" +
                    "- Xem IP hiển thị trên form\n\n" +
                    "LƯU Ý:\n" +
                    "- KHÔNG nhập 127.0.0.1 (chỉ dùng khi cùng máy)\n" +
                    "- KHÔNG nhập IP Gateway (router IP)\n" +
                    "- Phải là IP WiFi của máy Server",
                    "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate IP address format
            if (!IsValidIPAddress(serverIp))
            {
                MessageBox.Show($"Địa chỉ IP không hợp lệ: {serverIp}\n\n" +
                    "IP address phải có định dạng:\n" +
                    "  - IPv4: xxx.xxx.xxx.xxx (ví dụ: 192.168.1.100)\n" +
                    "  - Hoặc localhost: 127.0.0.1 (chỉ dùng khi cùng máy)\n\n" +
                    "LƯU Ý:\n" +
                    "- KHÔNG nhập IP Gateway (router IP)\n" +
                    "- Phải là IP WiFi của máy Server",
                    "IP không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(usernameOrEmail) || string.IsNullOrEmpty(password))
            {
                string fieldName = useEmail ? "Email" : "Username";
                MessageBox.Show($"Vui lòng nhập {fieldName} và Password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate email format nếu đăng nhập bằng email
            if (useEmail && !IsValidEmail(usernameOrEmail))
            {
                MessageBox.Show("Email không hợp lệ. Vui lòng nhập đúng định dạng email.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Vô hiệu hóa nút
            btnLogin.Enabled = false;
            btnLogin.Text = "Connecting...";

            try
            {
                // 3. Kết nối đến Server
                bool connected = await NetworkManager.Instance.ConnectAsync(serverIp, 9000);

                if (!connected)
                {
                    // Kiểm tra xem có ping được không - sử dụng async để không block UI thread
                    bool canPing = false;
                    try
                    {
                        using (var ping = new System.Net.NetworkInformation.Ping())
                        {
                            var reply = await ping.SendPingAsync(serverIp, 3000);
                            canPing = (reply.Status == System.Net.NetworkInformation.IPStatus.Success);
                        }
                    }
                    catch { }

                    string helpText = $"Không thể kết nối đến server tại {serverIp}:9000\n\n";

                    if (!canPing)
                    {
                        helpText += "🔴 KHÔNG PING ĐƯỢC - HAI MÁY KHÔNG CÙNG MẠNG!\n\n" +
                            "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                            "NGUYÊN NHÂN:\n" +
                            "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                            "• Hai máy KHÔNG cùng mạng WiFi\n" +
                            "• Khác subnet (IP khác lớp)\n" +
                            "• Router có AP Isolation\n\n" +
                            "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                            "GIẢI PHÁP (Thử theo thứ tự):\n" +
                            "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                            "1️⃣ DÙNG MOBILE HOTSPOT (Đơn giản nhất)\n" +
                            "   → Bật Hotspot trên điện thoại\n" +
                            "   → Cả hai máy kết nối WiFi từ điện thoại\n" +
                            "   → Xem lại IP mới và thử lại\n\n" +
                            "2️⃣ KIỂM TRA CÙNG WIFI\n" +
                            "   → Đảm bảo cả hai máy cùng tên WiFi\n" +
                            "   → Ngắt/kết nối lại WiFi trên cả hai máy\n" +
                            "   → Chạy ipconfig để xem IP mới\n\n" +
                            "3️⃣ KIỂM TRA SUBNET\n" +
                            "   → IP phải cùng subnet (3 số đầu giống)\n" +
                            "   → Ví dụ: 192.168.1.10 và 192.168.1.20 = OK ✓\n" +
                            "   → Ví dụ: 192.168.1.10 và 192.168.2.20 = SAI ✗\n\n" +
                            "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                            "Xem file HUONG_DAN_KHAC_MANG.md để biết chi tiết!";
                    }
                    else
                    {
                        helpText += "✅ Ping được nhưng không kết nối được port 9000\n\n" +
                            "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                            "KIỂM TRA:\n" +
                            "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                            "□ Server đã Start chưa? (Phải thấy 'Server: Running...')\n" +
                            "□ Firewall Server đã mở chưa? (Chạy OpenFirewall.bat)\n" +
                            "□ Firewall Client đã mở chưa? (Chạy OpenFirewall.bat)\n" +
                            "□ IP nhập có đúng không? (Lấy từ form Server)\n\n" +
                            "Xem file CHECKLIST_KET_NOI.md để kiểm tra chi tiết!";
                    }

                    throw new Exception(helpText);
                }

                // 4. Gửi gói tin Login
                btnLogin.Text = "Logging in...";
                var loginPacket = new LoginPacket();

                if (useEmail)
                {
                    loginPacket.Email = usernameOrEmail;
                    loginPacket.Username = null;
                    loginPacket.UseEmailLogin = true;
                }
                else
                {
                    loginPacket.Username = usernameOrEmail;
                    loginPacket.Email = null;
                    loginPacket.UseEmailLogin = false;
                }

                loginPacket.Password = password;

                // Gọi hàm async trong NetworkManager
                LoginResultPacket result = await NetworkManager.Instance.LoginAsync(loginPacket);

                // 5. Xử lý kết quả (chỉ xử lý nếu form vẫn còn visible và chưa đóng)
                // Đảm bảo chạy trên UI thread
                if (!this.IsDisposed && this.Visible)
                {
                    if (this.InvokeRequired)
                    {
                        this.Invoke(new Action(() => ProcessLoginResult(result)));
                    }
                    else
                    {
                        ProcessLoginResult(result);
                    }
                }
            }
            catch (Exception ex)
            {
                // Chỉ hiển thị message box nếu form vẫn còn visible và chưa đóng
                // (tránh hiển thị sau khi đã login thành công và chuyển sang form khác)
                if (!this.IsDisposed && this.Visible)
                {
                    // Kiểm tra xem đã login thành công chưa (UserID đã được set)
                    if (string.IsNullOrEmpty(NetworkManager.Instance.UserID))
                    {
                        MessageBox.Show($"Login Failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    // Nếu đã có UserID thì không hiển thị error (có thể đã login thành công)
                }

                // Reset trạng thái nút (chỉ nếu form vẫn còn visible)
                if (!this.IsDisposed && this.Visible)
                {
                    btnLogin.Enabled = true;
                    btnLogin.Text = "Log in";
                }
            }
        }

        // Xử lý kết quả trả về từ Server
        private void ProcessLoginResult(LoginResultPacket result)
        {
            // Kiểm tra form vẫn còn tồn tại và visible
            if (this.IsDisposed || !this.Visible) return;

            if (result.Success)
            {
                // Đăng nhập thành công!
                NetworkManager.Instance.SetUserCredentials(result.UserID ?? string.Empty, result.UserName ?? string.Empty);

                // Đảm bảo OnlineUsers không null
                var onlineUsers = result.OnlineUsers ?? new List<UserStatus>();

                // Mở Form Home
                try
                {
                    // Tạo form trước (có thể mất thời gian)
                    frmHome homeForm = new frmHome(onlineUsers);
                    
                    // Hiển thị form mới trước
                    homeForm.Show();
                    
                    // Sau đó mới ẩn form login (để tránh flicker)
                    this.Hide();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi mở giao diện chính: {ex.Message}\n\nChi tiết: {ex.StackTrace}", 
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnLogin.Enabled = true;
                    btnLogin.Text = "Log in";
                }
            }
            else
            {
                // Đăng nhập thất bại - chỉ hiển thị nếu form vẫn còn visible
                if (!this.IsDisposed && this.Visible)
                {
                    MessageBox.Show($"Login Failed: {result.Message ?? "Không xác định được lỗi"}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnLogin.Enabled = true;
                    btnLogin.Text = "Log in";
                }
            }
        }

        // Sự kiện click vào Link Đăng ký
        private void lnkSignup_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmSignup signup = new frmSignup();
            signup.Show();
            this.Hide();
        }

        // Sự kiện click vào Link Quên mật khẩu
        private void lnkForgot_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmForgotPass forgot = new frmForgotPass();
            forgot.Show();
            this.Hide();
        }

        private void frmLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        // Hàm kiểm tra email hợp lệ
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // Hàm kiểm tra IP address hợp lệ
        private bool IsValidIPAddress(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return false;

            // Cho phép localhost
            if (ipAddress == "127.0.0.1" || ipAddress == "localhost")
                return true;

            // Kiểm tra định dạng IPv4
            string[] parts = ipAddress.Split('.');
            if (parts.Length != 4)
                return false;

            foreach (string part in parts)
            {
                if (!int.TryParse(part, out int num) || num < 0 || num > 255)
                    return false;
            }

            return true;
        }
    }
}