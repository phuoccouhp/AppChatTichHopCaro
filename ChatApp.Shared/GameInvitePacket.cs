using System;
namespace ChatApp.Shared
{
    [Serializable]
    public class GameInvitePacket
    {
        public string SenderID { get; set; }
        public string SenderName { get; set; } // Tên người gửi
        public string ReceiverID { get; set; }
    }
}