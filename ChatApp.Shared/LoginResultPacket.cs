using System;
using System.Collections.Generic;
namespace ChatApp.Shared
{
    [Serializable]
    public class LoginResultPacket
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string UserID { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public List<UserStatus> OnlineUsers { get; set; } = new List<UserStatus>();

        // Parameterless constructor required for BinaryFormatter
        public LoginResultPacket() { }
    }

    [Serializable]
    public class UserStatus
    {
        public string UserID { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public bool IsOnline { get; set; }

        // Parameterless constructor required for BinaryFormatter
        public UserStatus() { }
    }
}