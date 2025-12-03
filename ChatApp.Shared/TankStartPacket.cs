using System;

namespace ChatApp.Shared
{
    // 3. GÓI TIN BẮT ĐẦU GAME (Server -> Client A & Client B)
    [Serializable]
    public class TankStartPacket
    {
        public string GameID { get; set; }       // ID của ván game (do Server tạo)
        public string OpponentID { get; set; }   // ID đối thủ
        public bool IsPlayer1 { get; set; }      // True: Bạn là Player 1 (Xe xanh/Xuất phát điểm A)
                                                 // False: Bạn là Player 2 (Xe đỏ/Xuất phát điểm B)
    }
}