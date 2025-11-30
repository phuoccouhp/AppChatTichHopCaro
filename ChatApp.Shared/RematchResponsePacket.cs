using System;
namespace ChatApp.Shared
{
    [Serializable]
    public class RematchResponsePacket
    {
        public string GameID { get; set; }
        public string SenderID { get; set; }
        public string ReceiverID { get; set; } 
        public bool Accepted { get; set; }
    }
}