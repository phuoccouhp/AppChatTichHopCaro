using ChatApp.Shared;
using ChatAppClient.Helpers;
using System;
using System.Collections.Generic; // Cần cho List
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
            this.btnLogin.Click += BtnLogin_Click;
            this.btnRegister.Click += BtnRegister_Click;

            // Thiết lập placeholder cho TextBox
            SetTextBoxPlaceholder(txtUsername, "Tên đăng nhập");
            SetTextBoxPlaceholder(txtPassword, "Mật khẩu");

            // Tự động điền (để test cho nhanh)
            txtUsername.Text = "user1";
            txtUsername.ForeColor = Color.Black;
            txtPassword.Text = "123";
            txtPassword.PasswordChar = '●';
            txtPassword.ForeColor = Color.Black;
        }

        // Sự kiện click nút Đăng nhập (đã chuyển sang async)
        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            // 1. Kiểm tra đầu vào
            if (string.IsNullOrEmpty(username) || username == "Tên đăng nhập" ||
                string.IsNullOrEmpty(password) || password == "Mật khẩu")
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Vô hiệu hóa UI
            btnLogin.Enabled = false;
            btnLogin.Text = "Đang kết nối...";

            try
            {
                // 3. Kết nối đến Server
                // (Địa chỉ 127.0.0.1 là localhost - tức máy chủ chạy trên cùng máy)
                bool connected = await NetworkManager.Instance.ConnectAsync("127.0.0.1", 9000);

                if (!connected)
                {
                    throw new Exception("Không thể kết nối đến máy chủ. Vui lòng kiểm tra lại Server!");
                }

                // 4. Gửi gói tin Login và đợi phản hồi
                btnLogin.Text = "Đang đăng nhập...";
                var loginPacket = new LoginPacket { Username = username, Password = password };

                // Gọi hàm async mới trong NetworkManager
                LoginResultPacket result = await NetworkManager.Instance.LoginAsync(loginPacket);

                // 5. Xử lý kết quả
                ProcessLoginResult(result);
            }
            catch (Exception ex)
            {
                // Bất kỳ lỗi nào (kết nối, timeout, server sập) sẽ bị bắt ở đây
                MessageBox.Show($"Đăng nhập thất bại: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnLogin.Enabled = true;
                btnLogin.Text = "Đăng Nhập";
            }
        }

        // 6. Xử lý kết quả (chạy trên luồng UI)
        private void ProcessLoginResult(LoginResultPacket result)
        {
            if (result.Success)
            {
                // Đăng nhập thành công!

                // Lưu thông tin user vào NetworkManager
                NetworkManager.Instance.SetUserCredentials(result.UserID, result.UserName);

                // Mở Form Home và truyền danh sách bạn bè
                frmHome homeForm = new frmHome(result.OnlineUsers);
                homeForm.Show();
                this.Hide(); // Ẩn form login
            }
            else
            {
                // Đăng nhập thất bại (do Server báo)
                MessageBox.Show($"Đăng nhập thất bại: {result.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnLogin.Enabled = true;
                btnLogin.Text = "Đăng Nhập";
            }
        }

        // Chức năng Đăng ký (chưa làm)
        private void BtnRegister_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Chức năng đăng ký đang được phát triển!", "Thông báo");
        }

        // Đóng ứng dụng khi tắt Form Login
        private void frmLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        #region TextBox Placeholder Helpers
        // Thủ thuật để tạo placeholder cho TextBox
        private void SetTextBoxPlaceholder(TextBox textBox, string placeholder)
        {
            textBox.Text = placeholder;
            textBox.ForeColor = Color.Gray;
            if (textBox == txtPassword)
            {
                textBox.PasswordChar = '\0'; // Hiện chữ
            }

            textBox.Enter += (s, e) =>
            {
                if (textBox.Text == placeholder)
                {
                    textBox.Text = "";
                    textBox.ForeColor = Color.Black;
                    if (textBox == txtPassword)
                    {
                        textBox.PasswordChar = '●'; // Ẩn chữ
                    }
                }
            };

            textBox.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = placeholder;
                    textBox.ForeColor = Color.Gray;
                    if (textBox == txtPassword)
                    {
                        textBox.PasswordChar = '\0'; // Hiện chữ
                    }
                }
            };
        }
        #endregion
    }
}