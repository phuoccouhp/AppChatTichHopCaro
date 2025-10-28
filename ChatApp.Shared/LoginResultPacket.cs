using System;
using System.Collections.Generic;
namespace ChatApp.Shared
{
    [Serializable]
    public class LoginResultPacket
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }
        public List<UserStatus> OnlineUsers { get; set; } 
    }

    [Serializable]
    public class UserStatus
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        public bool IsOnline { get; set; }
    }
}