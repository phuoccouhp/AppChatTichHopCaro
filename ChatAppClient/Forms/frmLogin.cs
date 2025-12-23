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
            // Lưu ý: RoundedTextBox thường dùng thuộc tính .Text giống TextBox thường
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
                    "- Xem IP hiển thị trên form", 
                    "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    string helpText = $"Không thể kết nối đến server tại {serverIp}:9000\n\n" +
                        "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                        "KIỂM TRA TRÊN MÁY SERVER:\n" +
                        "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                        "□ Server đã Start chưa? (Phải thấy 'Server: Running...')\n" +
                        "□ Đã mở Firewall chưa? (Click '🔓 Mở Firewall' hoặc chạy OpenFirewall.bat)\n" +
                        "□ IP hiển thị trên Server là gì? (Copy chính xác IP đó)\n\n" +
                        "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                        "KIỂM TRA TRÊN MÁY CLIENT (MÁY NÀY):\n" +
                        "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                        "□ Đã mở Firewall chưa? (Chạy OpenFirewall.bat với quyền Admin)\n" +
                        "□ Đã ping được Server chưa?\n" +
                        "  → Mở CMD: ping " + serverIp + "\n" +
                        "  → Nếu 'Request timed out' = KHÔNG CÙNG MẠNG\n" +
                        "  → Nếu 'Reply from...' = Mạng OK, kiểm tra Firewall\n\n" +
                        "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                        "KIỂM TRA MẠNG:\n" +
                        "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n" +
                        "□ Hai máy có cùng tên WiFi không?\n" +
                        "□ IP có cùng subnet không? (3 số đầu giống nhau)\n" +
                        "  Ví dụ: 10.215.204.194 và 10.215.204.110 = OK ✓\n" +
                        "         10.215.204.194 và 10.215.210.103 = SAI ✗\n\n" +
                        "Xem file CHECKLIST_KET_NOI.md để kiểm tra chi tiết!";
                    
                    throw new Exception(helpText);
                }

                // 4. Gửi gói tin Login
                btnLogin.Text = "Logging in...";
                var loginPacket = new LoginPacket 
                { 
                    Username = useEmail ? null : usernameOrEmail,
                    Email = useEmail ? usernameOrEmail : null,
                    Password = password,
                    UseEmailLogin = useEmail
                };

                // Gọi hàm async trong NetworkManager
                LoginResultPacket result = await NetworkManager.Instance.LoginAsync(loginPacket);

                // 5. Xử lý kết quả (chỉ xử lý nếu form vẫn còn visible và chưa đóng)
                if (!this.IsDisposed && this.Visible)
                {
                    ProcessLoginResult(result);
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
                NetworkManager.Instance.SetUserCredentials(result.UserID, result.UserName);

                // Mở Form Home
                frmHome homeForm = new frmHome(result.OnlineUsers);
                homeForm.Show();

                // Ẩn form login
                this.Hide();
            }
            else
            {
                // Đăng nhập thất bại - chỉ hiển thị nếu form vẫn còn visible
                if (!this.IsDisposed && this.Visible)
                {
                    MessageBox.Show($"Login Failed: {result.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    }
}