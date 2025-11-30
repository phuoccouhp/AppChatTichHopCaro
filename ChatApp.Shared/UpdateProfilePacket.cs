using System;

namespace ChatApp.Shared
{
    [Serializable]
    public class UpdateProfilePacket
    {
        public string UserID { get; set; }        // ID người sửa
        public string NewDisplayName { get; set; } // Tên hiển thị mới
        public byte[] NewAvatarData { get; set; }  // Dữ liệu ảnh mới (nếu có)
        public bool HasNewAvatar { get; set; }     // Cờ báo có đổi ảnh không
    }
}