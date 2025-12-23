using System;
namespace ChatApp.Shared
{
    [Serializable]
    public class GameResetPacket
    {
        public string GameID { get; set; } = string.Empty;
        public bool StartsFirst { get; set; }

        // Parameterless constructor required for BinaryFormatter
        public GameResetPacket() { }
    }
}