using System;
namespace ChatApp.Shared
{
    [Serializable]
    public class GameStartPacket
    {
        public string GameID { get; set; } 
        public string OpponentName { get; set; }
        public string OpponentID { get; set; }
        public bool StartsFirst { get; set; } 
    }
}