using System;

namespace ChatApp.Shared
{
    // Enum hỗ trợ hướng di chuyển cho dễ code
    public enum TankDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    // 4. GÓI TIN HÀNH ĐỘNG (Di chuyển, Bắn)
    [Serializable]
    public class TankActionPacket
    {
        public string GameID { get; set; }
        public string SenderID { get; set; }     // Ai đang thực hiện hành động này?

        // Tọa độ hiện tại của Tank
        public int X { get; set; }
        public int Y { get; set; }

        // Hướng quay
        public TankDirection Direction { get; set; }

        // Hành động bắn
        public bool IsShooting { get; set; }      // True nếu người chơi vừa nhấn phím bắn
    }
}