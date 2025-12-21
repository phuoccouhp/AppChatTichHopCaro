using System;

namespace ChatApp.Shared
{
   
    [Serializable]
    public class TankHitPacket
    {
        public string GameID { get; set; }
        public string SenderID { get; set; } // who fired the shot
        public string VictimID { get; set; }     
        public int Damage { get; set; }        
        // New fields set by server to indicate current score/hit count of shooter
        public int ShooterHits { get; set; }
        public int ShooterScore { get; set; }    }}