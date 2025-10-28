using ChatApp.Shared;
using ChatAppClient.Helpers;
using System;
using System.Collections.Generic; 
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

            SetTextBoxPlaceholder(txtUsername, "Tên đăng nhập");
            SetTextBoxPlaceholder(txtPassword, "Mật khẩu");

            txtUsername.Text = "user1";
            txtUsername.ForeColor = Color.Black;
            txtPassword.Text = "123";
            txtPassword.PasswordChar = '●';
            txtPassword.ForeColor = Color.Black;
        }


        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            string serverIp = txtServerIp.Text.Trim(); 
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(serverIp))
            {
                MessageBox.Show("Vui lòng nhập địa chỉ IP của Server.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(username) || username == "Tên đăng nhập" ||
                string.IsNullOrEmpty(password) || password == "Mật khẩu")
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Tên đăng nhập và Mật khẩu.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnLogin.Enabled = false;
            btnLogin.Text = "Đang kết nối...";

            try
            {
                bool connected = await NetworkManager.Instance.ConnectAsync(serverIp, 9000);

                if (!connected)
                {
                    throw new Exception("Không thể kết nối đến máy chủ. Vui lòng kiểm tra lại IP Server và Firewall!");
                }

                btnLogin.Text = "Đang đăng nhập...";
                var loginPacket = new LoginPacket { Username = username, Password = password };
                LoginResultPacket result = await NetworkManager.Instance.LoginAsync(loginPacket);

                ProcessLoginResult(result);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đăng nhập thất bại: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnLogin.Enabled = true; 
                btnLogin.Text = "Đăng Nhập";
            }
        }

        private void ProcessLoginResult(LoginResultPacket result)
        {
            if (result.Success)
            {
                NetworkManager.Instance.SetUserCredentials(result.UserID, result.UserName);

                frmHome homeForm = new frmHome(result.OnlineUsers);
                homeForm.Show();
                this.Hide(); 
            }
            else
            {
                MessageBox.Show($"Đăng nhập thất bại: {result.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnLogin.Enabled = true;
                btnLogin.Text = "Đăng Nhập";
            }
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Chức năng đăng ký đang được phát triển!", "Thông báo");
        }

        private void frmLogin_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        #region TextBox Placeholder Helpers
        private void SetTextBoxPlaceholder(TextBox textBox, string placeholder)
        {
            textBox.Text = placeholder;
            textBox.ForeColor = Color.Gray;
            if (textBox == txtPassword)
            {
                textBox.PasswordChar = '\0';
            }

            textBox.Enter += (s, e) =>
            {
                if (textBox.Text == placeholder)
                {
                    textBox.Text = "";
                    textBox.ForeColor = Color.Black;
                    if (textBox == txtPassword)
                    {
                        textBox.PasswordChar = '●'; 
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
                        textBox.PasswordChar = '\0'; 
                    }
                }
            };
        }
        #endregion
    }
}