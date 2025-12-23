using System;

namespace ChatApp.Shared
{
    [Serializable]
    public class LoginPacket
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public bool UseEmailLogin { get; set; }

        // Parameterless constructor required for BinaryFormatter
        public LoginPacket() { }

        // Constructor for convenience
        public LoginPacket(string? username, string? password, string? email = null)
        {
            Username = username;
            Password = password;
            Email = email;
            UseEmailLogin = email != null && !string.IsNullOrEmpty(email);
        }
    }
}