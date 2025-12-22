using System;
namespace ChatApp.Shared
{
    [Serializable]
    public class LoginPacket
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string? Email { get; set; } // Thêm email để hỗ trợ đăng nhập bằng email
        public bool UseEmailLogin { get; set; } // Flag để xác định đăng nhập bằng email hay username
    }
}