using System;

namespace ChatApp.Shared 
{
    [Serializable]
    public class FilePacket
    {
        public string SenderID { get; set; } = string.Empty;
        public string ReceiverID { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public byte[] FileData { get; set; } = Array.Empty<byte>();
        public bool IsImage { get; set; }

        // Parameterless constructor required for BinaryFormatter
        public FilePacket() { }
    }
}