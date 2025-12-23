using System;
namespace ChatApp.Shared
{
    [Serializable]
    public class UserStatusPacket
    {
        public string UserID { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public bool IsOnline { get; set; }

        // Parameterless constructor required for BinaryFormatter
        public UserStatusPacket() { }
    }
}