using System;

namespace ChatApp.Shared
{
    // 2. GÓI TIN PHẢN HỒI LỜI MỜI (Client B -> Server -> Client A/Server)
    [Serializable]
    public class TankResponsePacket
    {
        public string SenderID { get; set; }     // Người trả lời (B)
        public string ReceiverID { get; set; }   // Người mời (A)
        public bool Accepted { get; set; }       // Đồng ý hay không?
    }
}