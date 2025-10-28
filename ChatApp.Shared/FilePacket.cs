using System;

namespace ChatApp.Shared // <-- Đảm bảo đúng namespace
{
    [Serializable]
    public class FilePacket
    {
        public string SenderID { get; set; }
        public string ReceiverID { get; set; }
        public string FileName { get; set; }
        public byte[] FileData { get; set; }

        // Cờ để phân biệt ảnh và file
        public bool IsImage { get; set; }
    }
}