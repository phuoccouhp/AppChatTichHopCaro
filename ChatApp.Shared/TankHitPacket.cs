using System;

namespace ChatApp.Shared
{
    [Serializable]
    public class TankHitPacket
    {
        public string GameID { get; set; }
        public string HitPlayerID { get; set; } // Player bị bắn trúng
        public int Damage { get; set; }
        public int RemainingHealth { get; set; }
        public bool IsGameOver { get; set; }
        public string WinnerID { get; set; }
    }
}
