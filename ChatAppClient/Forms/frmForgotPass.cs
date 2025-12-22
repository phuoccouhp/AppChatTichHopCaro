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
        public frmForgotPass()
        {
            InitializeComponent();
        }

        private void frmForgotPass_Load(object sender, EventArgs e)
        {
            // Kết nối sự kiện Click
            this.roundedButton1.Click += btnSendOTP_Click; // Nút Send OTP
            this.btnSend.Click += btnResetPassword_Click; // Nút Reset Pass
            this.lnkBack.LinkClicked += lnkBack_LinkClicked;

            // Cài đặt trạng thái ban đầu
            txtOTP.Visible = false;
            btnSend.Visible = false; // Nút Reset ẩn

            // Đăng ký nhận kết quả từ Server
            NetworkManager.Instance.OnForgotPasswordResult += HandleResult;
        }

        private async void btnSendOTP_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            if (string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Vui lòng nhập Email.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // --- KIỂM TRA KẾT NỐI ---
            // Tạm dùng IP mặc định localhost nếu user chưa login
            string defaultIp = "127.0.0.1";

            // Hàm ConnectAsync sẽ tự trả về true nếu đã kết nối rồi
            bool isConnected = await NetworkManager.Instance.ConnectAsync(defaultIp, 9000);

            if (!isConnected)
            {
                MessageBox.Show("Không thể kết nối đến Server. Vui lòng kiểm tra mạng.", "Lỗi Kết Nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            // ------------------------

            roundedButton1.Enabled = false;
            roundedButton1.Text = "Sending...";

            var packet = new ForgotPasswordPacket { Email = email };

            // Gửi và kiểm tra kết quả gửi ngay lập tức
            bool sent = NetworkManager.Instance.SendPacket(packet);

            if (!sent)
            {
                MessageBox.Show("Gửi yêu cầu thất bại (Lỗi Socket).", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                roundedButton1.Enabled = true;
                roundedButton1.Text = "Send OTP";
            }
            // Nếu sent == true, cứ để nút mờ và chờ Server phản hồi qua sự kiện HandleResult
        }

        private void btnResetPassword_Click(object sender, EventArgs e)
        {
            string otp = txtOTP.Text.Trim();

            if (string.IsNullOrEmpty(otp))
            {
                MessageBox.Show("Vui lòng nhập mã OTP.", "Cảnh báo");
                return;
            }

            // Hỏi mật khẩu mới (Vì giao diện chưa có ô nhập Pass mới)
            string? newPass = ShowPasswordInputDialog("Nhập mật khẩu mới:", "Đặt lại mật khẩu");
            if (string.IsNullOrEmpty(newPass)) return;

            var packet = new ResetPasswordPacket
            {
                Email = txtEmail.Text.Trim(),
                OtpCode = otp,
                NewPassword = newPass
            };

            NetworkManager.Instance.SendPacket(packet);
        }

        // Xử lý kết quả từ Server trả về
        private void HandleResult(ForgotPasswordResultPacket result)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => HandleResult(result)));
                return;
            }

            if (result.Success)
            {
                if (result.IsStep1Success)
                {
                    // Gửi OTP thành công -> Chuyển sang bước 2
                    MessageBox.Show(result.Message, "OTP Đã Gửi");

                    // Hiện ô nhập OTP và nút Reset
                    txtOTP.Visible = true;
                    btnSend.Visible = true;

                    // Ẩn nút gửi OTP và khóa email
                    roundedButton1.Visible = false;
                    txtEmail.Enabled = false;
                }
                else
                {
                    // Đổi pass thành công
                    MessageBox.Show(result.Message, "Thành Công");
                    this.Close(); // Đóng form quay về Login
                }
            }
            else
            {
                // Lỗi (Sai email hoặc sai OTP)
                MessageBox.Show(result.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Nếu lỗi ở bước gửi OTP, bật lại nút
                if (!txtOTP.Visible)
                {
                    roundedButton1.Enabled = true;
                    roundedButton1.Text = "Send OTP";
                }
            }
        }

        private void lnkBack_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Close();
        }

        private void frmForgotPass_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Hủy đăng ký sự kiện để tránh lỗi bộ nhớ
            NetworkManager.Instance.OnForgotPasswordResult -= HandleResult;
        }

        // Hàm thay thế InputBox
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
    }
}