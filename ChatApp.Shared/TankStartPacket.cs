using System;

namespace ChatApp.Shared
{
    [Serializable]
    public class TankStartPacket
    {
        public string GameID { get; set; }
        public string OpponentName { get; set; }
        public string OpponentID { get; set; }
        public bool StartsFirst { get; set; } // Player 1 hoặc Player 2
    }
}
