using ChatApp.Shared;
using ChatAppClient.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ChatAppClient.Forms
{
    public partial class frmResetPassword : Form
    {
        private string _email;
        private string _otp;

        public frmResetPassword(string email, string otp)
        {
            InitializeComponent();
            _email = email;
            _otp = otp;
        }

        private void frmResetPassword_Load(object sender, EventArgs e)
        {
            this.btnReset.Click += BtnReset_Click;
            this.lnkBack.LinkClicked += LnkBack_LinkClicked;
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            string newPassword = txtNewPassword.Text.Trim();
            string confirmPassword = txtConfirmPassword.Text.Trim();

            if (string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu mới.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPassword.Length < 6)
            {
                MessageBox.Show("Mật khẩu phải có ít nhất 6 ký tự.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Gửi packet reset password
            var packet = new ResetPasswordPacket
            {
                Email = _email,
                OtpCode = _otp,
                NewPassword = newPassword
            };

            NetworkManager.Instance.SendPacket(packet);
            btnReset.Enabled = false;
            btnReset.Text = "Đang xử lý...";

            // Đăng ký sự kiện để nhận kết quả
            NetworkManager.Instance.OnForgotPasswordResult += HandleResetResult;
        }

        private void HandleResetResult(ForgotPasswordResultPacket result)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => HandleResetResult(result)));
                return;
            }

            // Hủy đăng ký sự kiện
            NetworkManager.Instance.OnForgotPasswordResult -= HandleResetResult;

            if (result.Success && !result.IsStep1Success)
            {
                // Reset password thành công
                MessageBox.Show(result.Message, "Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Quay về form login
                frmLogin loginForm = new frmLogin();
                loginForm.Show();
                this.Hide();
            }
            else
            {
                // Lỗi
                MessageBox.Show(result.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnReset.Enabled = true;
                btnReset.Text = "Reset Password";
            }
        }

        private void LnkBack_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Quay về form login
            frmLogin loginForm = new frmLogin();
            loginForm.Show();
            this.Hide();
        }

        private void frmResetPassword_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Nếu đóng form, quay về form login
            if (e.CloseReason == CloseReason.UserClosing)
            {
                frmLogin loginForm = new frmLogin();
                loginForm.Show();
            }
        }
    }
}

