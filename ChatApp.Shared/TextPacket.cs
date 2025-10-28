using System;

namespace ChatApp.Shared // <-- Đảm bảo đúng namespace
{
    [Serializable]
    public class TextPacket
    {
        public string SenderID { get; set; }
        public string ReceiverID { get; set; }
        public string MessageContent { get; set; }
    }
}