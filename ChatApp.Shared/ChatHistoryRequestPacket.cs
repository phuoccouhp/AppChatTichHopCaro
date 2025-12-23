using System;

namespace ChatApp.Shared
{
    [Serializable]
    public class ChatHistoryRequestPacket
    {
        public string UserID { get; set; }
        public string FriendID { get; set; }
        public int Limit { get; set; } = 100; // Số lượng tin nhắn tối đa
    }
}

