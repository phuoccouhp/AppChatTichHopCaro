using System;

namespace ChatApp.Shared
{
    [Serializable]
    public class TankStartPacket
    {
        public string GameID { get; set; } = string.Empty;
        public string OpponentName { get; set; } = string.Empty;
        public string OpponentID { get; set; } = string.Empty;
        public bool StartsFirst { get; set; } // Player 1 hoặc Player 2

        // Parameterless constructor required for BinaryFormatter
        public TankStartPacket() { }
    }
}
