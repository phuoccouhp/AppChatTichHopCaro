using System;
using System.Collections.Generic;

namespace ChatApp.Shared
{
    // 1. PacketWrapper: Object dùng để đóng gói dữ liệu (Gửi/Nhận)
    // KHÔNG ĐƯỢC là static
    [Serializable]
    public class PacketWrapper
    {
        public string Type { get; set; }
        public string Payload { get; set; }
    }

    // 2. PacketMapper: Helper tĩnh dùng để tra cứu kiểu dữ liệu
    // Class này là STATIC
    public static class PacketMapper
    {
        private static readonly Dictionary<string, Type> _typeMap = new Dictionary<string, Type>
        {
            // Auth
            { nameof(LoginPacket), typeof(LoginPacket) },
            { nameof(LoginResultPacket), typeof(LoginResultPacket) },
            { nameof(RegisterPacket), typeof(RegisterPacket) },
            { nameof(RegisterResultPacket), typeof(RegisterResultPacket) },
            { nameof(ForgotPasswordPacket), typeof(ForgotPasswordPacket) },
            { nameof(ForgotPasswordResultPacket), typeof(ForgotPasswordResultPacket) },
            { nameof(ResetPasswordPacket), typeof(ResetPasswordPacket) },
            { nameof(UpdateProfilePacket), typeof(UpdateProfilePacket) },

            // Chat & System
            { nameof(UserStatusPacket), typeof(UserStatusPacket) },
            { nameof(TextPacket), typeof(TextPacket) },
            { nameof(FilePacket), typeof(FilePacket) },
            { nameof(RequestOnlineListPacket), typeof(RequestOnlineListPacket) },
            { nameof(OnlineListPacket), typeof(OnlineListPacket) },
            { nameof(ChatHistoryRequestPacket), typeof(ChatHistoryRequestPacket) },
            { nameof(ChatHistoryResponsePacket), typeof(ChatHistoryResponsePacket) },
            
            // Game Logic
            { nameof(GameInvitePacket), typeof(GameInvitePacket) },
            { nameof(GameResponsePacket), typeof(GameResponsePacket) },
            { nameof(GameStartPacket), typeof(GameStartPacket) },
            { nameof(GameMovePacket), typeof(GameMovePacket) },
            { nameof(RematchRequestPacket), typeof(RematchRequestPacket) },
            { nameof(RematchResponsePacket), typeof(RematchResponsePacket) },
            { nameof(GameResetPacket), typeof(GameResetPacket) },
            
            // Tank Game Logic
            { nameof(TankInvitePacket), typeof(TankInvitePacket) },
            { nameof(TankResponsePacket), typeof(TankResponsePacket) },
            { nameof(TankStartPacket), typeof(TankStartPacket) },
            { nameof(TankActionPacket), typeof(TankActionPacket) },
            { nameof(TankHitPacket), typeof(TankHitPacket) }
        };

        public static Type GetPacketType(string typeName)
        {
            if (_typeMap.TryGetValue(typeName, out Type type))
            {
                return type;
            }
            return null;
        }
    }
}