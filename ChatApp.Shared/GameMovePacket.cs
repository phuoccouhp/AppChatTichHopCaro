using System;
namespace ChatApp.Shared
{
    [Serializable]
    public class GameMovePacket
    {
        public string GameID { get; set; }
        public string SenderID { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
    }
}