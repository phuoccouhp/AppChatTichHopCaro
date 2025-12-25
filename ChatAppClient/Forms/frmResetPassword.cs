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
            
            // Dang ky su kien ngay khi form load
            NetworkManager.Instance.OnForgotPasswordResult += HandleResetResult;
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            string newPassword = txtNewPassword.Text.Trim();
            string confirmPassword = txtConfirmPassword.Text.Trim();

            if (string.IsNullOrEmpty(newPassword))
            {
                MessageBox.Show("Vui lòng nh?p m?t kh?u m?i.", "C?nh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPassword.Length < 6)
            {
                MessageBox.Show("M?t kh?u ph?i có ít nh?t 6 ký t?.", "C?nh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("M?t kh?u xác nh?n không kh?p.", "C?nh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // G?i packet reset password
            var packet = new ResetPasswordPacket
            {
                Email = _email,
                OtpCode = _otp,
                NewPassword = newPassword
            };

            NetworkManager.Instance.SendPacket(packet);
            btnReset.Enabled = false;
            btnReset.Text = "?ang x? lý...";
        }

        private void HandleResetResult(ForgotPasswordResultPacket result)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => HandleResetResult(result)));
                return;
            }

            // H?y ??ng ký s? ki?n
            NetworkManager.Instance.OnForgotPasswordResult -= HandleResetResult;

            if (result.Success && !result.IsStep1Success)
            {
                // Reset password thành công
                MessageBox.Show(result.Message, "Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Quay v? form login
                frmLogin loginForm = new frmLogin();
                loginForm.Show();
                this.Close();
            }
            else
            {
                // L?i
                MessageBox.Show(result.Message, "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnReset.Enabled = true;
                btnReset.Text = "??i M?t Kh?u";
            }
        }

        private void LnkBack_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Huy dang ky su kien truoc khi dong
            NetworkManager.Instance.OnForgotPasswordResult -= HandleResetResult;
            
            // Quay v? form login
            frmLogin loginForm = new frmLogin();
            loginForm.Show();
            this.Close();
        }

        private void frmResetPassword_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Huy dang ky su kien
            NetworkManager.Instance.OnForgotPasswordResult -= HandleResetResult;
            
            // N?u ?óng form, quay v? form login
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // Kiem tra neu da co form login mo thi khong tao moi
                bool hasLoginForm = false;
                foreach (Form f in Application.OpenForms)
                {
                    if (f is frmLogin)
                    {
                        hasLoginForm = true;
                        f.Show();
                        break;
                    }
                }
                if (!hasLoginForm)
                {
                    frmLogin loginForm = new frmLogin();
                    loginForm.Show();
                }
            }
        }
    }
}
