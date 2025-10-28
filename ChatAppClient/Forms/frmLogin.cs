using ChatAppClient.Helpers;
using System;
using System.Drawing;
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
            // Gán sự kiện
            this.btnLogin.Click += BtnLogin_Click;
            this.btnRegister.Click += BtnRegister_Click;

            // Làm đẹp TextBox (một chút)
            SetTextBoxPlaceholder(txtUsername, "Tên đăng nhập");
            SetTextBoxPlaceholder(txtPassword, "Mật khẩu");
        }


        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || username == "Tên đăng nhập" ||
                string.IsNullOrEmpty(password) || password == "Mật khẩu")
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // TODO: Gửi username và password lên Server để xác thực
            // NetworkManager.Instance.Login(username, password);

            // Giả lập đăng nhập thành công
            // Sau khi Server phản hồi thành công, bạn làm:
            MessageBox.Show("Đăng nhập thành công! (Giả lập)", "Thành công");

            // TODO: Lưu thông tin user (ví dụ: GlobalState.CurrentUser = ...)

            frmHome homeForm = new frmHome();
            homeForm.Show();
            this.Hide(); // Ẩn form login đi
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Chức năng đăng ký đang được phát triển!", "Thông báo");
            // TODO: Mở form Đăng Ký (frmRegister)
        }

        private void frmLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Khi form Login bị tắt, tắt toàn bộ ứng dụng
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