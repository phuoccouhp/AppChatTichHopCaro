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
            // Ket noi su kien Click
            this.roundedButton1.Click += btnSendOTP_Click;
            this.btnSend.Click += btnResetPassword_Click;
            this.lnkBack.LinkClicked += lnkBack_LinkClicked;

            // Cai dat trang thai ban dau
            txtOTP.Visible = false;
            btnSend.Visible = false;

            // ? [FIX] ??ng ký event ngay t? ??u và KHÔNG h?y khi connect
            EnsureEventRegistered();
        }
        
        // ? [FIX] Hàm ??m b?o event luôn ???c ??ng ký
        private void EnsureEventRegistered()
        {
            // H?y tr??c ?? tránh ??ng ký trùng
            NetworkManager.Instance.OnForgotPasswordResult -= HandleResult;
            // ??ng ký l?i
            NetworkManager.Instance.OnForgotPasswordResult += HandleResult;
            System.Diagnostics.Debug.WriteLine("[ForgotPass] Event ?ã ???c ??ng ký");
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

            // Kiem tra ket noi
            string serverIp = NetworkManager.Instance.CurrentServerIP ?? "127.0.0.1";
            int port = NetworkManager.Instance.CurrentServerPort;

            bool isConnected = await NetworkManager.Instance.ConnectAsync(serverIp, port);

            if (!isConnected)
            {
                MessageBox.Show("Không th? k?t n?i ??n Server. Vui lòng ki?m tra m?ng.", "L?i K?t N?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            // ? [FIX] ??ng ký l?i event SAU khi k?t n?i thành công
            EnsureEventRegistered();

            roundedButton1.Enabled = false;
            roundedButton1.Text = "Sending...";

            var packet = new ForgotPasswordPacket { Email = email };

            bool sent = NetworkManager.Instance.SendPacket(packet);

            if (!sent)
            {
                MessageBox.Show("G?i yêu c?u th?t b?i (L?i Socket).", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                roundedButton1.Enabled = true;
                roundedButton1.Text = "Send OTP";
            }
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
            
            // ? [FIX] ??m b?o event v?n ???c ??ng ký tr??c khi g?i
            EnsureEventRegistered();

            // Gui OTP len server de kiem tra
            var verifyPacket = new ResetPasswordPacket
            {
                Email = email,
                OtpCode = otp,
                NewPassword = "" // Mat khau trong = chi verify OTP
            };

            btnSend.Enabled = false;
            btnSend.Text = "?ang ki?m tra...";

            // Luu OTP va email de dung sau khi verify thanh cong
            _pendingOtp = otp;
            _pendingEmail = email;

            // Gui packet va cho ket qua
            NetworkManager.Instance.SendPacket(verifyPacket);
        }

        // Xu ly ket qua tu Server tra ve
        private void HandleResult(ForgotPasswordResultPacket result)
        {
            // ? Log ngay khi nh?n ???c event
            System.Diagnostics.Debug.WriteLine($"[ForgotPass] HandleResult called - Success={result.Success}, IsStep1Success={result.IsStep1Success}, Message={result.Message}");
            
            // ? [FIX] Ki?m tra form còn t?n t?i không tr??c khi Invoke
            if (this.IsDisposed)
            {
                System.Diagnostics.Debug.WriteLine("[ForgotPass] Form ?ã b? disposed, b? qua HandleResult");
                return;
            }
            
            if (this.InvokeRequired)
            {
                System.Diagnostics.Debug.WriteLine("[ForgotPass] InvokeRequired = true, ?ang BeginInvoke...");
                try
                {
                    // ? [FIX] Dùng BeginInvoke thay vì Invoke ?? tránh deadlock
                    this.BeginInvoke(new Action(() => HandleResultOnUIThread(result)));
                }
                catch (ObjectDisposedException)
                {
                    System.Diagnostics.Debug.WriteLine("[ForgotPass] Form b? disposed trong khi BeginInvoke");
                }
                catch (InvalidOperationException ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ForgotPass] InvalidOperationException khi BeginInvoke: {ex.Message}");
                }
                return;
            }
            
            // N?u ?ã ? UI thread thì x? lý tr?c ti?p
            HandleResultOnUIThread(result);
        }
        
        private void HandleResultOnUIThread(ForgotPasswordResultPacket result)
        {
            System.Diagnostics.Debug.WriteLine($"[ForgotPass] HandleResultOnUIThread - Success={result.Success}, Message={result.Message}");
            
            // ? [DEBUG] MessageBox ?? xác nh?n ?ã vào hàm này
            // MessageBox.Show($"DEBUG: Success={result.Success}, IsStep1={result.IsStep1Success}, Msg={result.Message}", "DEBUG");

            if (result.Success)
            {
                if (result.IsStep1Success)
                {
                    // Buoc 1: Gui OTP thanh cong -> Hien form nhap OTP
                MessageBox.Show(result.Message, "OTP ?ã G?i", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Hi?n ô nh?p OTP và nút Reset
                    txtOTP.Visible = true;
                    txtOTP.Text = "";
                    txtOTP.Focus();
                    btnSend.Visible = true;
                    btnSend.Enabled = true;
                    btnSend.Text = "Xác nh?n OTP";

                    // ?n nút g?i OTP và khóa email
                    roundedButton1.Visible = false;
                    txtEmail.Enabled = false;
                }
                else if (result.Message == "OTP verified successfully")
                {
                    System.Diagnostics.Debug.WriteLine("[ForgotPass] OTP verified successfully - ?ang m? form reset password...");
                    
                    // Buoc 2: OTP da duoc xac minh thanh cong -> Mo form reset password
                    // Lay email tu textbox neu _pendingEmail bi null
                    string verifiedOtp = _pendingOtp ?? txtOTP.Text.Trim();
                    string verifiedEmail = _pendingEmail ?? txtEmail.Text.Trim();
                    
                    System.Diagnostics.Debug.WriteLine($"[ForgotPass] verifiedEmail={verifiedEmail}, verifiedOtp={verifiedOtp}");
                    
                    _pendingOtp = null;
                    _pendingEmail = null;

                    btnSend.Enabled = true;
                    btnSend.Text = "Xác nh?n OTP";
                    
                    // QUAN TR?NG: H?y ??ng ký event TR??C khi m? form m?i ?? tránh xung ??t
                    NetworkManager.Instance.OnForgotPasswordResult -= HandleResult;
                    
                    try
                    {
                        System.Diagnostics.Debug.WriteLine("[ForgotPass] ?ang t?o frmResetPassword...");
                        frmResetPassword resetForm = new frmResetPassword(verifiedEmail, verifiedOtp);
                        System.Diagnostics.Debug.WriteLine("[ForgotPass] ?ang g?i Show()...");
                        resetForm.Show();
                        System.Diagnostics.Debug.WriteLine("[ForgotPass] Form ?ã Show thành công!");
                        this.Close(); // Dong form thay vi Hide de giai phong tai nguyen
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ForgotPass] L?I khi m? frmResetPassword: {ex}");
                        MessageBox.Show($"L?i khi m? form ??i m?t kh?u: {ex.Message}", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        // Dang ky lai event neu loi
                        NetworkManager.Instance.OnForgotPasswordResult += HandleResult;
                    }
                }
                else if (result.Message == "Password Changed")
                {
                    // B??c 3: ??i m?t kh?u thành công
                    MessageBox.Show("??i m?t kh?u thành công!", "Thành Công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    frmLogin loginForm = new frmLogin();
                    loginForm.Show();
                    this.Close();
                }
                else
                {
                    // ? [FIX] Tr??ng h?p Success=true nh?ng message khác - v?n th? m? form reset
                    System.Diagnostics.Debug.WriteLine($"[ForgotPass] Nh?n Success v?i message khác: '{result.Message}'");
                    
                    // Ki?m tra xem có ph?i là verify OTP thành công không (message có th? khác m?t chút)
                    if (result.Message != null && result.Message.ToLower().Contains("otp") && result.Message.ToLower().Contains("verified"))
                    {
                        // V?n m? form reset password
                        string verifiedOtp = _pendingOtp ?? txtOTP.Text.Trim();
                        string verifiedEmail = _pendingEmail ?? txtEmail.Text.Trim();
                        
                        _pendingOtp = null;
                        _pendingEmail = null;
                        
                        NetworkManager.Instance.OnForgotPasswordResult -= HandleResult;
                        
                        frmResetPassword resetForm = new frmResetPassword(verifiedEmail, verifiedOtp);
                        resetForm.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            else
            {
                // L?i x?y ra - QUAN TR?NG: V?n cho phép nh?p l?i OTP
                MessageBox.Show(result.Message, "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // N?u l?i ? b??c g?i OTP (ch?a hi?n ô nh?p OTP), b?t l?i nút Send OTP
                if (!txtOTP.Visible)
                {
                    roundedButton1.Enabled = true;
                    roundedButton1.Text = "G?i OTP";
                }
                else
                {
                    // L?i khi verify OTP (sai OTP) - B?T L?I NÚT ?? CHO NH?P L?I
                    btnSend.Enabled = true;
                    btnSend.Text = "Xác nh?n OTP";
                    
                    // Xóa OTP c? ?? ng??i dùng nh?p l?i
                    txtOTP.Text = "";
                    txtOTP.Focus();
                }
            }
        }

        private void lnkBack_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmLogin loginForm = new frmLogin();
            loginForm.Show();
            this.Hide();
        }

        private void frmForgotPass_FormClosed(object sender, FormClosedEventArgs e)
        {
            NetworkManager.Instance.OnForgotPasswordResult -= HandleResult;
        }

        private string ShowPasswordInputDialog(string prompt, string title)
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
