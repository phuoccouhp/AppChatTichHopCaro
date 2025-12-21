using System;

namespace ChatApp.Shared
{
    [Serializable]
    public class TankResponsePacket
    {
        public string SenderID { get; set; }
        public string ReceiverID { get; set; }
        public bool Accepted { get; set; }


        public class TankStartPacket
        {
            public string GameID { get; set; }
            public string OpponentID { get; set; }
            public bool IsPlayer1 { get; set; }
        }
    }
}