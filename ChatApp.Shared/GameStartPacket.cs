using System;
namespace ChatApp.Shared
{
    [Serializable]
    public class GameStartPacket
    {
        public string GameID { get; set; } = string.Empty;
        public string OpponentName { get; set; } = string.Empty;
        public string OpponentID { get; set; } = string.Empty;
        public bool StartsFirst { get; set; }

        // Parameterless constructor required for BinaryFormatter
        public GameStartPacket() { }
    }
}