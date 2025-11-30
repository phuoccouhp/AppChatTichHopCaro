using System;
namespace ChatApp.Shared
{
    [Serializable]
    public class GameResetPacket
    {
        public string GameID { get; set; }
        public bool StartsFirst { get; set; }
    }
}