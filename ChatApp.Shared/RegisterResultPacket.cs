using System;

namespace ChatApp.Shared
{
    [Serializable]
    public class RegisterResultPacket
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;

        // Parameterless constructor required for BinaryFormatter
        public RegisterResultPacket() { }
    }
}