using System;

namespace ChatApp.Shared
{
    [Serializable]
    public class User
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
    }
}
