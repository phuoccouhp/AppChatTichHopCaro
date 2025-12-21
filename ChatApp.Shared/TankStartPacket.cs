using System;

namespace ChatApp.Shared
{
    [Serializable]
    public class TankStartPacket
    {
        public string GameID { get; set; }
        public string OpponentID { get; set; }
        public bool IsPlayer1 { get; set; }
    }
}