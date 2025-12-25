using ChatApp.Shared;
using ChatAppClient.Helpers;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatAppClient.Forms
{
    public partial class frmForgotPass : Form
    {
        private string _pendingOtp = null;
        private string _pendingEmail = null;

        public frmForgotPass()
        {
            InitializeComponent();
        }

        private void frmForgotPass_Load(object sender, EventArgs e)
        {
            // K?t n?i s? ki?n Click
            this.roundedButton1.Click += btnSendOTP_Click; // Nút Send OTP
            this.btnSend.Click += btnResetPassword_Click; // Nút Reset Pass
            this.lnkBack.LinkClicked += lnkBack_LinkClicked;

            // Cài ??t tr?ng thái ban ??u
            txtOTP.Visible = false;
            btnSend.Visible = false; // Nút Reset ?n

            // ??ng ký nh?n k?t qu? t? Server
            NetworkManager.Instance.OnForgotPasswordResult += HandleResult;
        }

        private async void btnSendOTP_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Vui lòng nh?p Email.", "C?nh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate email format
            if (!IsValidEmail(email))
            {
                MessageBox.Show("Email không h?p l?. Vui lòng nh?p ?úng ??nh d?ng email.", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // --- KI?M TRA K?T N?I ---
            // S? d?ng IP ?ã l?u t? l?n k?t n?i tr??c (n?u có)
            string serverIp = NetworkManager.Instance.CurrentServerIP ?? "127.0.0.1";
            int port = NetworkManager.Instance.CurrentServerPort;

            // Hàm ConnectAsync s? t? tr? v? true n?u ?ã k?t n?i r?i
            bool isConnected = await NetworkManager.Instance.ConnectAsync(serverIp, port);

            if (!isConnected)
            {
                MessageBox.Show("Không th? k?t n?i ??n Server. Vui lòng ki?m tra m?ng.", "L?i K?t N?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // ------------------------

            roundedButton1.Enabled = false;
            roundedButton1.Text = "Sending...";

            var packet = new ForgotPasswordPacket { Email = email };

            // G?i và ki?m tra k?t qu? g?i ngay l?p t?c
            bool sent = NetworkManager.Instance.SendPacket(packet);

            if (!sent)
            {
                MessageBox.Show("G?i yêu c?u th?t b?i (L?i Socket).", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                roundedButton1.Enabled = true;
                roundedButton1.Text = "Send OTP";
            }
            // N?u sent == true, c? ?? nút m? và ch? Server ph?n h?i qua s? ki?n HandleResult
        }

        private void btnResetPassword_Click(object sender, EventArgs e)
        {
            string otp = txtOTP.Text.Trim();
            string email = txtEmail.Text.Trim();

            if (string.IsNullOrEmpty(otp))
            {
                MessageBox.Show("Vui lòng nh?p mã OTP.", "C?nh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // G?i OTP lên server ?? ki?m tra tr??c khi m? form reset password
            // S? d?ng ResetPasswordPacket v?i m?t kh?u tr?ng ?? verify OTP
            var verifyPacket = new ResetPasswordPacket
            {
                Email = email,
                OtpCode = otp,
                NewPassword = "" // M?t kh?u tr?ng = ch? verify OTP
            };

            btnSend.Enabled = false;
            btnSend.Text = "?ang ki?m tra...";

            // L?u OTP và email ?? dùng sau khi verify thành công
            _pendingOtp = otp;
            _pendingEmail = email;

            // G?i packet và ch? k?t qu?
            NetworkManager.Instance.SendPacket(verifyPacket);
        }

        // X? lý k?t qu? t? Server tr? v?
        private void HandleResult(ForgotPasswordResultPacket result)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => HandleResult(result)));
                return;
            }

            Logger.Info($"[ForgotPass] Nh?n k?t qu?: Success={result.Success}, IsStep1Success={result.IsStep1Success}, Message={result.Message}");

            if (result.Success)
            {
                if (result.IsStep1Success)
                {
                    // ? B??c 1: G?i OTP thành công -> Hi?n form nh?p OTP
                    MessageBox.Show(result.Message, "OTP ?ã G?i", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Hi?n ô nh?p OTP và nút Reset
                    txtOTP.Visible = true;
                    btnSend.Visible = true;
                    btnSend.Enabled = true;
                    btnSend.Text = "Xác nh?n OTP";

                    // ?n nút g?i OTP và khóa email
                    roundedButton1.Visible = false;
                    txtEmail.Enabled = false;
                }
                else if (!string.IsNullOrEmpty(_pendingOtp) && result.Message == "OTP verified successfully")
                {
                    // ? B??c 2: OTP ?ã ???c xác minh thành công -> M? form reset password
                    string verifiedOtp = _pendingOtp;
                    string verifiedEmail = _pendingEmail;
                    _pendingOtp = null;
                    _pendingEmail = null;

                    btnSend.Enabled = true;
                    btnSend.Text = "Xác nh?n OTP";

                    Logger.Info($"[ForgotPass] OTP xác minh thành công, m? form reset password cho email: {verifiedEmail}");
                    
                    frmResetPassword resetForm = new frmResetPassword(verifiedEmail, verifiedOtp);
                    resetForm.Show();
                    this.Hide();
                }
                else if (result.Message == "Password Changed")
                {
                    // ? B??c 3: ??i m?t kh?u thành công (t? form reset password)
                    MessageBox.Show("??i m?t kh?u thành công!", "Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    frmLogin loginForm = new frmLogin();
                    loginForm.Show();
                    this.Hide();
                }
                else
                {
                    // Tr??ng h?p khác - không mong ??i
                    Logger.Warning($"[ForgotPass] Nh?n Success nh?ng không xác ??nh ???c b??c: {result.Message}");
                    MessageBox.Show(result.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                // ? L?i x?y ra
                MessageBox.Show(result.Message, "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // N?u l?i ? b??c g?i OTP (ch?a hi?n ô nh?p OTP), b?t l?i nút Send OTP
                if (!txtOTP.Visible)
                {
                    roundedButton1.Enabled = true;
                    roundedButton1.Text = "Send OTP";
                }
                else if (!string.IsNullOrEmpty(_pendingOtp))
                {
                    // L?i khi verify OTP - b?t l?i nút xác nh?n
                    btnSend.Enabled = true;
                    btnSend.Text = "Xác nh?n OTP";
                    _pendingOtp = null;
                    _pendingEmail = null;
                }
            }
        }

        private void lnkBack_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Hi?n th? l?i form login thay vì ch? ?óng form
            frmLogin loginForm = new frmLogin();
            loginForm.Show();
            this.Hide();
        }

        private void frmForgotPass_FormClosed(object sender, FormClosedEventArgs e)
        {
            // H?y ??ng ký s? ki?n ?? tránh l?i b? nh?
            NetworkManager.Instance.OnForgotPasswordResult -= HandleResult;
        }

        // Hàm thay th? InputBox
        private string? ShowPasswordInputDialog(string prompt, string title)
        {
            Form inputForm = new Form()
            {
                Width = 400,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label promptLabel = new Label() { Left = 20, Top = 20, Text = prompt, Width = 350 };
            TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 350, UseSystemPasswordChar = true };
            Button okButton = new Button() { Text = "OK", Left = 200, Width = 80, Top = 80, DialogResult = DialogResult.OK };
            Button cancelButton = new Button() { Text = "Cancel", Left = 290, Width = 80, Top = 80, DialogResult = DialogResult.Cancel };

            okButton.Click += (sender, e) => { inputForm.Close(); };
            cancelButton.Click += (sender, e) => { inputForm.Close(); };

            inputForm.Controls.Add(promptLabel);
            inputForm.Controls.Add(textBox);
            inputForm.Controls.Add(okButton);
            inputForm.Controls.Add(cancelButton);
            inputForm.AcceptButton = okButton;
            inputForm.CancelButton = cancelButton;

            if (inputForm.ShowDialog() == DialogResult.OK)
            {
                return textBox.Text;
            }
            return null;
        }

        // Hàm ki?m tra email h?p l?
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
