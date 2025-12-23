using System;

namespace ChatApp.Shared
{
    [Serializable]
    public class UpdateProfilePacket
    {
        public string UserID { get; set; } = string.Empty;        // ID người sửa
        public string NewDisplayName { get; set; } = string.Empty; // Tên hiển thị mới
        public byte[] NewAvatarData { get; set; } = Array.Empty<byte>();  // Dữ liệu ảnh mới (nếu có)
        public bool HasNewAvatar { get; set; }     // Cờ báo có đổi ảnh không

        // Parameterless constructor required for BinaryFormatter
        public UpdateProfilePacket() { }
    }
}