using System;
using System.Collections.Generic;

namespace ChatApp.Shared
{
    [Serializable]
    public class ChatHistoryResponsePacket
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<ChatHistoryMessage> Messages { get; set; } = new List<ChatHistoryMessage>();
    }

    [Serializable]
    public class ChatHistoryMessage
    {
        public int MessageID { get; set; }
        public string SenderID { get; set; }
        public string ReceiverID { get; set; }
        public string MessageContent { get; set; }
        public string MessageType { get; set; } = "Text";
        public string? FileName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

