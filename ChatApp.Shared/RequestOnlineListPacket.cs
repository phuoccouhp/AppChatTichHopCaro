using System;
using System.Collections.Generic;

namespace ChatApp.Shared
{
    [Serializable]
    public class RequestOnlineListPacket
    {
    }

    [Serializable]
    public class OnlineListPacket
    {
        public List<UserStatus> OnlineUsers { get; set; } = new List<UserStatus>();
    }
}