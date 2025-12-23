using System;

namespace ChatApp.Shared
{
    [Serializable]
    public class TextPacket
    {
        public string SenderID { get; set; } = string.Empty;
        public string ReceiverID { get; set; } = string.Empty;
        public string MessageContent { get; set; } = string.Empty;

        // Parameterless constructor required for BinaryFormatter
        public TextPacket() { }
    }
}