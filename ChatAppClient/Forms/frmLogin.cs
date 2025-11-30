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
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            // Gán sự kiện Click cho nút Đăng nhập
            this.btnLogin.Click += BtnLogin_Click;

            // (Optional) Pre-fill for testing if you want
            // txtServerIP.Text = "127.0.0.1";
            // txtUser.Text = "user1";
            // txtPass.Text = "123";
        }

        // Sự kiện click nút Đăng nhập (Async)
        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            // Lấy dữ liệu từ các RoundedTextBox
            // Lưu ý: RoundedTextBox thường dùng thuộc tính .Text giống TextBox thường
            string serverIp = txtServerIP.Text.Trim();
            string username = txtUser.Text.Trim();
            string password = txtPass.Text.Trim();

            // 1. Kiểm tra đầu vào
            if (string.IsNullOrEmpty(serverIp))
            {
                MessageBox.Show("Please enter Server IP.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter Username and Password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    throw new Exception("Could not connect to server. Please check IP and Firewall!");
                }

                // 4. Gửi gói tin Login
                btnLogin.Text = "Logging in...";
                var loginPacket = new LoginPacket { Username = username, Password = password };

                // Gọi hàm async trong NetworkManager
                LoginResultPacket result = await NetworkManager.Instance.LoginAsync(loginPacket);

                // 5. Xử lý kết quả
                ProcessLoginResult(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login Failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Reset trạng thái nút
                btnLogin.Enabled = true;
                btnLogin.Text = "Log in";
            }
        }

        // Xử lý kết quả trả về từ Server
        private void ProcessLoginResult(LoginResultPacket result)
        {
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
                // Đăng nhập thất bại
                MessageBox.Show($"Login Failed: {result.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnLogin.Enabled = true;
                btnLogin.Text = "Log in";
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
    }
}