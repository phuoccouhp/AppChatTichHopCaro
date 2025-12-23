using System;
using System.Collections.Generic;

namespace ChatApp.Shared
{
    [Serializable]
    public class ChatHistoryResponsePacket
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<ChatHistoryMessage> Messages { get; set; } = new List<ChatHistoryMessage>();

        // Parameterless constructor required for BinaryFormatter
        public ChatHistoryResponsePacket() { }
    }

    [Serializable]
    public class ChatHistoryMessage
    {
        public int MessageID { get; set; }
        public string SenderID { get; set; } = string.Empty;
        public string ReceiverID { get; set; } = string.Empty;
        public string MessageContent { get; set; } = string.Empty;
        public string MessageType { get; set; } = "Text";
        public string? FileName { get; set; }
        public DateTime CreatedAt { get; set; }

        // Parameterless constructor required for BinaryFormatter
        public ChatHistoryMessage() { }
    }
}

