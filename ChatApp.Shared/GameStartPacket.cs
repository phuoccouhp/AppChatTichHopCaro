using System;
namespace ChatApp.Shared
{
    [Serializable]
    public class GameStartPacket
    {
        public string GameID { get; set; } // ID duy nhất cho ván game
        public string OpponentName { get; set; }
        public string OpponentID { get; set; }
        public bool StartsFirst { get; set; } // Bạn có phải người đi trước không?
    }
}