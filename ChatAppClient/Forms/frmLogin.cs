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
        // ? [FIX] Flag ch?n click nút Login 2 l?n
        private volatile bool _isProcessingLogin = false;

        public frmLogin()
        {
            InitializeComponent();
            // ??m b?o IsPassword ???c set sau khi InitializeComponent
            txtPass.IsPassword = true;
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            // ? [FIX] Ch? gán s? ki?n 1 l?n - xóa event c? tr??c khi gán m?i
            this.btnLogin.Click -= BtnLogin_Click;
            this.btnLogin.Click += BtnLogin_Click;
            
            // ??m b?o txtServerIP có th? nh?p ???c
            txtServerIP.InnerTextBox.ReadOnly = false;
            txtServerIP.InnerTextBox.Enabled = true;
            txtServerIP.Enabled = true;
            
            // Thi?t l?p m?c ??nh
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

        // S? ki?n click nút ??ng nh?p (Async)
        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            // ? [FIX] Ch?n click 2 l?n nhanh - ki?m tra flag tr??c
            if (_isProcessingLogin)
            {
                Logger.Warning("[Login] ?ang x? lý login, b? qua click trùng l?p");
                return;
            }
            
            // L?y d? li?u t? các RoundedTextBox
            // L?u ý: RoundedTextBox th??ng dùng thu?c tính .Text gi?ng TextBox th??ng
            string serverIp = txtServerIP.Text.Trim();
            string usernameOrEmail = txtUser.Text.Trim();
            string password = txtPass.Text.Trim();
            bool useEmail = rdoEmail.Checked;

            // 1. Ki?m tra ??u vào
            if (string.IsNullOrEmpty(serverIp))
            {
                MessageBox.Show("Vui lòng nh?p ??a ch? IP c?a máy ch?.\n\n" +
                    "L?y IP t? máy ch?:\n" +
                    "- M? form Server\n" +
                    "- Nh?n Start Server\n" +
                    "- Xem IP hi?n th? trên form\n\n" +
                    "L?U Ý:\n" +
                    "- KHÔNG nh?p 127.0.0.1 (ch? dùng khi cùng máy)\n" +
                    "- KHÔNG nh?p IP Gateway (router IP)\n" +
                    "- Ph?i là IP WiFi c?a máy Server", 
                    "Thi?u thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(usernameOrEmail) || string.IsNullOrEmpty(password))
            {
                string fieldName = useEmail ? "Email" : "Username";
                MessageBox.Show($"Vui lòng nh?p {fieldName} và Password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validate email format n?u ??ng nh?p b?ng email
            if (useEmail && !IsValidEmail(usernameOrEmail))
            {
                MessageBox.Show("Email không h?p l?. Vui lòng nh?p ?úng ??nh d?ng email.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ? [FIX] Set flag TR??C khi disable button
            _isProcessingLogin = true;

            // 2. Vô hi?u hóa nút
            btnLogin.Enabled = false;
            btnLogin.Text = "Connecting...";

            try
            {
                // 3. K?t n?i ??n Server
                bool connected = await NetworkManager.Instance.ConnectAsync(serverIp, 9000);

                if (!connected)
                {
                    // Ki?m tra xem có ping ???c không
                    bool canPing = false;
                    try
                    {
                        using (var ping = new System.Net.NetworkInformation.Ping())
                        {
                            var reply = ping.Send(serverIp, 3000);
                            canPing = (reply.Status == System.Net.NetworkInformation.IPStatus.Success);
                        }
                    }
                    catch { }

                    string helpText = $"Không th? k?t n?i ??n server t?i {serverIp}:9000\n\n";
                    
                    if (!canPing)
                    {
                        helpText += "?? KHÔNG PING ???C - HAI MÁY KHÔNG CÙNG M?NG!\n\n" +
                            "????????????????????????????????????????\n" +
                            "NGUYÊN NHÂN:\n" +
                            "????????????????????????????????????????\n" +
                            "• Hai máy KHÔNG cùng m?ng WiFi\n" +
                            "• Khác subnet (IP khác l?p)\n" +
                            "• Router có AP Isolation\n\n" +
                            "????????????????????????????????????????\n" +
                            "GI?I PHÁP (Th? theo th? t?):\n" +
                            "????????????????????????????????????????\n" +
                            "1?? DÙNG MOBILE HOTSPOT (??n gi?n nh?t)\n" +
                            "   ? B?t Hotspot trên ?i?n tho?i\n" +
                            "   ? C? hai máy k?t n?i WiFi t? ?i?n tho?i\n" +
                            "   ? Xem l?i IP m?i và th? l?i\n\n" +
                            "2?? KI?M TRA CÙNG WIFI\n" +
                            "   ? ??m b?o c? hai máy cùng tên WiFi\n" +
                            "   ? Ng?t/k?t n?i l?i WiFi trên c? hai máy\n" +
                            "   ? Ch?y ipconfig ?? xem IP m?i\n\n" +
                            "3?? KI?M TRA SUBNET\n" +
                            "   ? IP ph?i cùng subnet (3 s? ??u gi?ng)\n" +
                            "   ? Ví d?: 192.168.1.10 và 192.168.1.20 = OK ?\n" +
                            "   ? Ví d?: 192.168.1.10 và 192.168.2.20 = SAI ?\n\n" +
                            "????????????????????????????????????????\n" +
                            "Xem file HUONG_DAN_KHAC_MANG.md ?? bi?t chi ti?t!";
                    }
                    else
                    {
                        helpText += "? Ping ???c nh?ng không k?t n?i ???c port 9000\n\n" +
                            "????????????????????????????????????????\n" +
                            "KI?M TRA:\n" +
                            "????????????????????????????????????????\n" +
                            "? Server ?ã Start ch?a? (Ph?i th?y 'Server: Running...')\n" +
                            "? Firewall Server ?ã m? ch?a? (Ch?y OpenFirewall.bat)\n" +
                            "? Firewall Client ?ã m? ch?a? (Ch?y OpenFirewall.bat)\n" +
                            "? IP nh?p có ?úng không? (L?y t? form Server)\n\n" +
                            "Xem file CHECKLIST_KET_NOI.md ?? ki?m tra chi ti?t!";
                    }
                    
                    throw new Exception(helpText);
                }

                // 4. G?i gói tin Login
                btnLogin.Text = "Logging in...";
                var loginPacket = new LoginPacket 
                { 
                    Username = useEmail ? null : usernameOrEmail,
                    Email = useEmail ? usernameOrEmail : null,
                    Password = password,
                    UseEmailLogin = useEmail
                };

                // G?i hàm async trong NetworkManager
                LoginResultPacket result = await NetworkManager.Instance.LoginAsync(loginPacket);

                // 5. X? lý k?t qu? (ch? x? lý n?u form v?n còn visible và ch?a ?óng)
                if (!this.IsDisposed && this.Visible)
                {
                    ProcessLoginResult(result);
                }
            }
            catch (Exception ex)
            {
                // Ch? hi?n th? message box n?u form v?n còn visible và ch?a ?óng
                // (tránh hi?n th? sau khi ?ã login thành công và chuy?n sang form khác)
                if (!this.IsDisposed && this.Visible)
                {
                    // Ki?m tra xem ?ã login thành công ch?a (UserID ?ã ???c set)
                    if (string.IsNullOrEmpty(NetworkManager.Instance.UserID))
                    {
                        MessageBox.Show($"Login Failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    // N?u ?ã có UserID thì không hi?n th? error (có th? ?ã login thành công)
                }

                // Reset tr?ng thái nút (ch? n?u form v?n còn visible)
                if (!this.IsDisposed && this.Visible)
                {
                    btnLogin.Enabled = true;
                    btnLogin.Text = "Log in";
                }
            }
            finally
            {
                // ? [FIX] Luôn reset flag trong finally
                _isProcessingLogin = false;
            }
        }

        // X? lý k?t qu? tr? v? t? Server
        private void ProcessLoginResult(LoginResultPacket result)
        {
            // Ki?m tra form v?n còn t?n t?i và visible
            if (this.IsDisposed || !this.Visible) return;
            
            if (result.Success)
            {
                Logger.Info($"[Client] Login successful for {result.UserID}, online users: {result.OnlineUsers?.Count ?? 0}");
                // ??ng nh?p thành công!
                NetworkManager.Instance.SetUserCredentials(result.UserID, result.UserName);

                // M? Form Home
                frmHome homeForm = new frmHome(result.OnlineUsers ?? new System.Collections.Generic.List<UserStatus>());
                homeForm.Show();

                // ?n form login
                this.Hide();
            }
            else
            {
                Logger.Warning($"[Client] Login failed: {result.Message}");
                // ??ng nh?p th?t b?i - ch? hi?n th? n?u form v?n còn visible
                if (!this.IsDisposed && this.Visible)
                {
                    MessageBox.Show($"Login Failed: {result.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    btnLogin.Enabled = true;
                    btnLogin.Text = "Log in";
                }
            }
        }

        // S? ki?n click vào Link ??ng ký
        private void lnkSignup_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmSignup signup = new frmSignup();
            signup.Show();
            this.Hide();
        }

        // S? ki?n click vào Link Quên m?t kh?u
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
