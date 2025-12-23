using System;
namespace ChatApp.Shared
{
    [Serializable]
    public class GameInvitePacket
    {
        public string SenderID { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string ReceiverID { get; set; } = string.Empty;

        // Parameterless constructor required for BinaryFormatter
        public GameInvitePacket() { }
    }
}