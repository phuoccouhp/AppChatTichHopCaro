using System;
namespace ChatApp.Shared
{
    [Serializable]
    public class RematchRequestPacket
    {
        public string GameID { get; set; }
        public string SenderID { get; set; } 
    }
}