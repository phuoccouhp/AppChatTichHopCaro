using System;

namespace ChatApp.Shared
{
    [Serializable]
    public class TankResponsePacket
    {
        public string SenderID { get; set; } = string.Empty;
        public string ReceiverID { get; set; } = string.Empty;
        public bool Accepted { get; set; }

        // Parameterless constructor required for BinaryFormatter
        public TankResponsePacket() { }
    }
}
