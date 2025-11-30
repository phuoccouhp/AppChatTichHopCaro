using ChatApp.Shared;
using ChatAppClient.Helpers; // Logger (nếu cần)
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatAppClient.Forms
{
    public partial class frmSignup : Form
    {
        public frmSignup()
        {
            InitializeComponent();
        }

        private void frmSignup_Load(object sender, EventArgs e)
        {
            // Gán sự kiện Click
            btnRegister.Click += btnRegister_Click;
        }

        private async void btnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUser.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPass.Text.Trim();
            string confirm = txtConfirm.Text.Trim();

            // 1. Validate (Kiểm tra dữ liệu)
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Username và Password.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (password != confirm)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Kết nối Server (Nếu chưa kết nối)
            // Lưu ý: Đăng ký cần kết nối mạng. Bạn có thể thêm ô nhập IP ở Form Đăng ký 
            // hoặc dùng IP mặc định/đã lưu. Ở đây mình dùng IP local mặc định hoặc yêu cầu user nhập ở Login trước.
            if (!await EnsureConnectionAsync())
            {
                return;
            }

            btnRegister.Enabled = false;
            btnRegister.Text = "Processing...";

            try
            {
                // 3. Tạo gói tin
                var registerPacket = new RegisterPacket
                {
                    Username = username,
                    Password = password,
                    Email = email
                };

                // 4. Gửi và đợi phản hồi
                RegisterResultPacket result = await NetworkManager.Instance.RegisterAsync(registerPacket);

                if (result.Success)
                {
                    MessageBox.Show("Đăng ký thành công! Bạn có thể đăng nhập ngay bây giờ.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Chuyển về form Login
                    OpenLoginForm();
                }
                else
                {
                    MessageBox.Show($"Đăng ký thất bại: {result.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi hệ thống: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnRegister.Enabled = true;
                btnRegister.Text = "Create Account";
            }
        }

        private void lnkLogin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenLoginForm();
        }

        private void OpenLoginForm()
        {
            // Mở form Login (giả sử frmLogin là form khởi động, ta chỉ cần ẩn form này đi hoặc đóng nó)
            // Cách tốt nhất:
            this.Hide();
            // Tìm Form Login đang ẩn và hiện nó lên (nếu có)
            foreach (Form f in Application.OpenForms)
            {
                if (f is frmLogin)
                {
                    f.Show();
                    return;
                }
            }
            // Nếu không tìm thấy, tạo mới
            new frmLogin().Show();
        }

        // Hàm phụ trợ để đảm bảo kết nối
        private async Task<bool> EnsureConnectionAsync()
        {
            // Tạm thời hardcode IP hoặc lấy từ biến tĩnh toàn cục nếu bạn có.
            // Để chuyên nghiệp, bạn nên thêm ô nhập IP vào Form Đăng Ký.
            string defaultIp = "127.0.0.1";
            int port = 9000;

            try
            {
                return await NetworkManager.Instance.ConnectAsync(defaultIp, port);
            }
            catch
            {
                MessageBox.Show("Không thể kết nối Server để đăng ký. Vui lòng kiểm tra lại.", "Lỗi kết nối");
                return false;
            }
        }
    }
}