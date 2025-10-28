using System;

namespace ChatApp.Shared 
{
    [Serializable]
    public class FilePacket
    {
        public string SenderID { get; set; }
        public string ReceiverID { get; set; }
        public string FileName { get; set; }
        public byte[] FileData { get; set; }
        public bool IsImage { get; set; }
    }
}