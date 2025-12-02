using System;

namespace ChatApp.Shared
{
    [Serializable]
    public class ForgotPasswordPacket
    {
        public string Email { get; set; }
    }
}