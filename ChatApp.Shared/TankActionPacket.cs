using System;

namespace ChatApp.Shared
{
 
    public enum TankDirection
    {
        Up,
        Down,
        Left,
        Right
    }


    [Serializable]
    public class TankActionPacket
    {
        public string GameID { get; set; }
        public string SenderID { get; set; }


        public int X { get; set; }
        public int Y { get; set; }


        public TankDirection Direction { get; set; }


        public bool IsShooting { get; set; }
    }
}