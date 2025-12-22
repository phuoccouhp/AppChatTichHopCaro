namespace ChatAppServer
{
    public enum GameType
    {
        Caro,
        Tank
    }

    public class GameSession
    {
        public string? GameID { get; set; }
        public string? Player1_ID { get; set; }
        public string? Player2_ID { get; set; }
        public GameType Type { get; set; } = GameType.Caro; // Mặc định là Caro

        public GameSession(string gameID, string player1, string player2, GameType type = GameType.Caro)
        {
            GameID = gameID;
            Player1_ID = player1;
            Player2_ID = player2;
            Type = type;
        }

        public string? GetOpponent(string playerID)
        {
            if (playerID == Player1_ID)
                return Player2_ID;
            if (playerID == Player2_ID)
                return Player1_ID;
            return null;
        }
    }
}