using System;
using System.Collections.Generic;

namespace ChatApp.Shared
{
    // === PACKET TẠO NHÓM ===
    [Serializable]
    public class CreateGroupPacket
    {
        public string CreatorID { get; set; }
        public string GroupName { get; set; }
        public List<string> MemberIDs { get; set; } = new List<string>(); // Danh sách ID thành viên được mời
    }

    // === KẾT QUẢ TẠO NHÓM ===
    [Serializable]
    public class CreateGroupResultPacket
    {
        public bool Success { get; set; }
        public string GroupID { get; set; }
        public string GroupName { get; set; }
        public string Message { get; set; }
        public List<GroupMemberInfo> Members { get; set; } = new List<GroupMemberInfo>();
    }

    // === THÔNG TIN THÀNH VIÊN NHÓM ===
    [Serializable]
    public class GroupMemberInfo
    {
        public string UserID { get; set; }
        public string UserName { get; set; }
        public bool IsOnline { get; set; }
        public bool IsAdmin { get; set; }
    }

    // === TIN NHẮN NHÓM ===
    [Serializable]
    public class GroupTextPacket
    {
        public string GroupID { get; set; }
        public string SenderID { get; set; }
        public string SenderName { get; set; }
        public string MessageContent { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;
    }

    // === FILE TRONG NHÓM ===
    [Serializable]
    public class GroupFilePacket
    {
        public string GroupID { get; set; }
        public string SenderID { get; set; }
        public string SenderName { get; set; }
        public string FileName { get; set; }
        public byte[] FileData { get; set; }
        public bool IsImage { get; set; }
    }

    // === MỜI THÊM THÀNH VIÊN ===
    [Serializable]
    public class GroupInvitePacket
    {
        public string GroupID { get; set; }
        public string GroupName { get; set; }
        public string InviterID { get; set; }
        public string InviterName { get; set; }
        public List<string> InviteeIDs { get; set; } = new List<string>(); // Người được mời
    }

    // === THÔNG BÁO ĐƯỢC MỜI VÀO NHÓM ===
    [Serializable]
    public class GroupInviteNotificationPacket
    {
        public string GroupID { get; set; }
        public string GroupName { get; set; }
        public string InviterName { get; set; }
        public List<GroupMemberInfo> Members { get; set; } = new List<GroupMemberInfo>();
    }

    // === RỜI NHÓM ===
    [Serializable]
    public class LeaveGroupPacket
    {
        public string GroupID { get; set; }
        public string UserID { get; set; }
    }

    // === THÔNG BÁO THÀNH VIÊN RỜI/THAM GIA ===
    [Serializable]
    public class GroupMemberUpdatePacket
    {
        public string GroupID { get; set; }
        public string UserID { get; set; }
        public string UserName { get; set; }
        public bool Joined { get; set; } // true = vào, false = rời
    }

    // === YÊU CẦU DANH SÁCH NHÓM ===
    [Serializable]
    public class RequestGroupListPacket
    {
        public string UserID { get; set; }
    }

    // === DANH SÁCH NHÓM ===
    [Serializable]
    public class GroupListPacket
    {
        public List<GroupInfo> Groups { get; set; } = new List<GroupInfo>();
    }

    // === THÔNG TIN NHÓM ===
    [Serializable]
    public class GroupInfo
    {
        public string GroupID { get; set; }
        public string GroupName { get; set; }
        public int MemberCount { get; set; }
        public string LastMessage { get; set; }
        public DateTime? LastMessageTime { get; set; }
    }

    // === YÊU CẦU LỊCH SỬ CHAT NHÓM ===
    [Serializable]
    public class GroupHistoryRequestPacket
    {
        public string GroupID { get; set; }
        public string UserID { get; set; }
        public int Limit { get; set; } = 100;
    }

    // === LỊCH SỬ CHAT NHÓM ===
    [Serializable]
    public class GroupHistoryResponsePacket
    {
        public bool Success { get; set; }
        public string GroupID { get; set; }
        public List<GroupMessageHistory> Messages { get; set; } = new List<GroupMessageHistory>();
    }

    [Serializable]
    public class GroupMessageHistory
    {
        public int MessageID { get; set; }
        public string SenderID { get; set; }
        public string SenderName { get; set; }
        public string MessageContent { get; set; }
        public string MessageType { get; set; }
        public string FileName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

