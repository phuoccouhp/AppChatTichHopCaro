using System;
namespace ChatApp.Shared
{
    [Serializable]
    public class RematchResponsePacket
    {
        public string GameID { get; set; } = string.Empty;
        public string SenderID { get; set; } = string.Empty;
        public string ReceiverID { get; set; } = string.Empty;
        public bool Accepted { get; set; }

        // Parameterless constructor required for BinaryFormatter
        public RematchResponsePacket() { }
    }
}