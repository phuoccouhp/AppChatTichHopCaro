using System;

namespace ChatApp.Shared
{
    // 5. GÓI TIN KẾT THÚC/TRÚNG ĐẠN (Tùy chọn nâng cao)
    [Serializable]
    public class TankHitPacket
    {
        public string GameID { get; set; }
        public string VictimID { get; set; }     // Ai bị trúng đạn?
        public int Damage { get; set; }          // Mất bao nhiêu máu?
    }
}