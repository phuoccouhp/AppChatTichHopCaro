using System;
namespace ChatApp.Shared
{
    [Serializable]
    public class GameInvitePacket
    {
        public string SenderID { get; set; }
        public string SenderName { get; set; }
        public string ReceiverID { get; set; }
    }
}