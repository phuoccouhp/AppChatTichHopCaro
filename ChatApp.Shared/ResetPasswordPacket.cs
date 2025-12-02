using System;

namespace ChatApp.Shared
{
    [Serializable]
    public class ResetPasswordPacket
    {
        public string Email { get; set; }
        public string OtpCode { get; set; }
        public string NewPassword { get; set; }
    }
}