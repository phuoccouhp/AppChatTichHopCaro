using System;

namespace ChatApp.Shared
{
    [Serializable]
    public class RegisterResultPacket
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}