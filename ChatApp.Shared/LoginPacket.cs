using System;
namespace ChatApp.Shared
{
    [Serializable]
    public class LoginPacket
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}