using System;
namespace ChatApp.Shared
{
    [Serializable]
    public class GameResponsePacket
    {
        public string SenderID { get; set; } 
        public string ReceiverID { get; set; } 
        public bool Accepted { get; set; }
    }
}