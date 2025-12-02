using System;

namespace ChatApp.Shared
{
    [Serializable]
    public class ForgotPasswordResultPacket
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool IsStep1Success { get; set; } // True: Đã gửi OTP thành công, False: Đổi pass xong/Lỗi
    }
}