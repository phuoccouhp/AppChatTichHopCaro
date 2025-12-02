using System;
using System.Net;
using System.Net.Mail;

namespace ChatAppServer
{
    public static class EmailHelper
    {
        // CẤU HÌNH EMAIL CỦA SERVER (NGƯỜI GỬI)
        private static string _senderEmail = "taonekmay09112005@gmail.com";
        private static string _appPassword = "npwl hkcy zshj knbx"; 

        public static bool SendOTP(string toEmail, string otpCode)
        {
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
                Logger.Error($"Lỗi gửi mail đến {toEmail}", ex);
                return false;
            }
        }
    }
}