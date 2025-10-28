using System;
namespace ChatApp.Shared
{
    [Serializable]
    public class GameResponsePacket
    {
        public string SenderID { get; set; } // Người gửi phản hồi
        public string ReceiverID { get; set; } // Người nhận phản hồi (người mời)
        public bool Accepted { get; set; }
    }
}