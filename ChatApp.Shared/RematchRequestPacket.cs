using System;
namespace ChatApp.Shared
{
    [Serializable]
    public class RematchRequestPacket
    {
        public string GameID { get; set; } = string.Empty;
        public string SenderID { get; set; } = string.Empty;

        // Parameterless constructor required for BinaryFormatter
        public RematchRequestPacket() { }
    }
}