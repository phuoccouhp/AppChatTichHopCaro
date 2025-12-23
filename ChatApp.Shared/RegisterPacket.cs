using System;

namespace ChatApp.Shared
{
    [Serializable]
    public class RegisterPacket
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Parameterless constructor required for BinaryFormatter
        public RegisterPacket() { }
    }
}