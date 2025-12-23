using System;
namespace ChatApp.Shared
{
    [Serializable]
    public class GameMovePacket
    {
        public string GameID { get; set; } = string.Empty;
        public string SenderID { get; set; } = string.Empty;
        public int Row { get; set; }
        public int Col { get; set; }

        // Parameterless constructor required for BinaryFormatter
        public GameMovePacket() { }
    }
}