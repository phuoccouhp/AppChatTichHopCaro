using System;
using System.Collections.Generic;

namespace ChatApp.Shared
{
    [Serializable]
    public class OnlineUsersPacket
    {
        public List<UserStatus> Users { get; set; } = new List<UserStatus>();
    }
}
