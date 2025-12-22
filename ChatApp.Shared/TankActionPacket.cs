using System;

namespace ChatApp.Shared
{
    [Serializable]
    public class TankActionPacket
    {
        public string GameID { get; set; }
        public string SenderID { get; set; }
        public TankActionType ActionType { get; set; } // Move, Rotate, Shoot
        public float X { get; set; }
        public float Y { get; set; }
        public float Angle { get; set; } // Góc quay của tank
    }

    [Serializable]
    public enum TankActionType
    {
        Move,
        Rotate,
        Shoot
    }
}
