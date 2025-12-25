using System;
using System.Net;
using System.Net.Mail;

namespace ChatAppServer
{
    public static class EmailHelper
    {
        // [BẢO MẬT] Hãy thay đổi bằng App Password mới của bạn, không để lộ code này
        private static string _senderEmail = "taonekmay09112005@gmail.com";
        private static string _appPassword = "ulsk rlnz uwgo tahu"; // <--- THAY BẰNG MẬT KHẨU MỚI CỦA BẠN

        public static bool SendOTP(string toEmail, string otpCode)
        {
            if (_appPassword == "YOUR_APP_PASSWORD_HERE")
            {
                Logger.Error("Chưa cấu hình mật khẩu Email trong EmailHelper.cs!");
                return false;
            }

            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(_senderEmail, _appPassword),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_senderEmail, "ChatApp Support"),
                    Subject = "Mã xác nhận quên mật khẩu",
                    Body = $"Mã OTP của bạn là: <b>{otpCode}</b>. Mã này có hiệu lực trong 5 phút.",
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(toEmail);

                smtpClient.Send(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Lỗi gửi mail: {ex.Message}");
                return false;
            }
        }
    }
}