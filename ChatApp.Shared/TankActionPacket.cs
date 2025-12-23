using System;

namespace ChatApp.Shared
{
    [Serializable]
    public class TankActionPacket
    {
        public string GameID { get; set; } = string.Empty;
        public string SenderID { get; set; } = string.Empty;
        public TankActionType ActionType { get; set; } // Move, Rotate, Shoot
        public float X { get; set; }
        public float Y { get; set; }
        public float Angle { get; set; } // Góc quay của tank

        // Parameterless constructor required for BinaryFormatter
        public TankActionPacket() { }
    }

    [Serializable]
    public enum TankActionType
    {
        Move,
        Rotate,
        Shoot
    }
}
