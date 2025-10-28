namespace ChatAppServer
{
    public class GameSession
    {
        public string GameID { get; set; }
        public string Player1_ID { get; set; }
        public string Player2_ID { get; set; }

        public GameSession(string gameID, string player1, string player2)
        {
            GameID = gameID;
            Player1_ID = player1;
            Player2_ID = player2;
        }

        // Lấy ID của đối thủ
        public string GetOpponent(string playerID)
        {
            if (playerID == Player1_ID)
                return Player2_ID;
            if (playerID == Player2_ID)
                return Player1_ID;
            return null;
        }
    }
}