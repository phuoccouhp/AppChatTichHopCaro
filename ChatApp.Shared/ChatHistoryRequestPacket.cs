using System;

namespace ChatApp.Shared
{
    [Serializable]
    public class ChatHistoryRequestPacket
    {
        public string UserID { get; set; } = string.Empty;
        public string FriendID { get; set; } = string.Empty;
        public int Limit { get; set; } = 100; // Số lượng tin nhắn tối đa

        // Parameterless constructor required for BinaryFormatter
        public ChatHistoryRequestPacket() { }
    }
}

