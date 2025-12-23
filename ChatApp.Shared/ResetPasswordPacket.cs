using System;

namespace ChatApp.Shared
{
    [Serializable]
    public class ResetPasswordPacket
    {
        public string Email { get; set; } = string.Empty;
        public string OtpCode { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;

        // Parameterless constructor required for BinaryFormatter
        public ResetPasswordPacket() { }
    }
}