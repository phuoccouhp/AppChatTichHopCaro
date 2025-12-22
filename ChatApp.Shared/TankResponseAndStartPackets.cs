using System;

namespace ChatApp.Shared
{
    [Serializable]
    public class TankResponsePacket
    {
        public string SenderID { get; set; }
        public string ReceiverID { get; set; }
        public bool Accepted { get; set; }
    }
}
