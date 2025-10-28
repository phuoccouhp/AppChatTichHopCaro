using System;
namespace ChatApp.Shared
{
    [Serializable]
    public class UserStatusPacket
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        public bool IsOnline { get; set; }
    }
}