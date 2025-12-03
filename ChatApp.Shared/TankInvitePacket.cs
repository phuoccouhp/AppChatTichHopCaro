using System;

namespace ChatApp.Shared
{
    // 1. GÓI TIN MỜI CHƠI (Client A -> Server -> Client B)
    [Serializable]
    public class TankInvitePacket
    {
        public string SenderID { get; set; }     // ID người mời
        public string SenderName { get; set; }   // Tên hiển thị người mời
        public string ReceiverID { get; set; }   // ID người được mời
    }
}