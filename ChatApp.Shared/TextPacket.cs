using System;

namespace ChatApp.Shared
{
    [Serializable]
    public class TextPacket
    {
        public string SenderID { get; set; }
        public string ReceiverID { get; set; }
        public string MessageContent { get; set; }
    }
}