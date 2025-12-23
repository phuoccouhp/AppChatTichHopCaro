using System;

namespace ChatApp.Shared
{
    [Serializable]
    public class ForgotPasswordPacket
    {
        public string Email { get; set; } = string.Empty;

        // Parameterless constructor required for BinaryFormatter
        public ForgotPasswordPacket() { }
    }
}