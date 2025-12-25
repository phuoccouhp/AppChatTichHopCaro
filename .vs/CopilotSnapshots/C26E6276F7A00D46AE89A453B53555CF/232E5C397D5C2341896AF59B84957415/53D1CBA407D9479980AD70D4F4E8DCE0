using ChatApp.Shared;
using System;
using System.Collections.Generic;

namespace ChatAppServer
{
    public class TankGameManager
    {
        private Dictionary<string, TankGameState> _games = new Dictionary<string, TankGameState>();

        public class TankGameState
        {
            public string? GameID { get; set; }
            public string? Player1ID { get; set; }
            public string? Player2ID { get; set; }
            public int Player1Health { get; set; } = 100;
            public int Player2Health { get; set; } = 100;
            public List<BulletInfo> Bullets { get; set; } = new List<BulletInfo>();
        }

        public class BulletInfo
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Angle { get; set; }
            public string? OwnerID { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public void StartGame(string gameID, string player1ID, string player2ID)
        {
            _games[gameID] = new TankGameState
            {
                GameID = gameID,
                Player1ID = player1ID,
                Player2ID = player2ID
            };
        }

        public void AddBullet(string gameID, string ownerID, float x, float y, float angle)
        {
            if (_games.TryGetValue(gameID, out var game))
            {
                game.Bullets.Add(new BulletInfo
                {
                    X = x,
                    Y = y,
                    Angle = angle,
                    OwnerID = ownerID,
                    CreatedAt = DateTime.Now
                });
            }
        }

        public void UpdateBullets(string gameID, Server server)
        {
            if (!_games.TryGetValue(gameID, out var game)) return;

            const float BULLET_SPEED = 8f;
            const float TANK_SIZE = 40f;
            const int BULLET_LIFETIME_MS = 5000; // 5 giây

            for (int i = game.Bullets.Count - 1; i >= 0; i--)
            {
                var bullet = game.Bullets[i];

                // Xóa đạn quá cũ
                if ((DateTime.Now - bullet.CreatedAt).TotalMilliseconds > BULLET_LIFETIME_MS)
                {
                    game.Bullets.RemoveAt(i);
                    continue;
                }

                // Cập nhật vị trí đạn
                float rad = bullet.Angle * (float)Math.PI / 180f;
                bullet.X += (float)Math.Cos(rad) * BULLET_SPEED;
                bullet.Y += (float)Math.Sin(rad) * BULLET_SPEED;

                // Kiểm tra va chạm với tank (cần vị trí tank từ client)
                // Tạm thời bỏ qua, client sẽ gửi hit packet
            }
        }

        public void ProcessHit(string gameID, string hitPlayerID, int damage, Server server)
        {
            if (!_games.TryGetValue(gameID, out var game)) return;

            bool isPlayer1 = (hitPlayerID == game.Player1ID);
            if (isPlayer1)
            {
                game.Player1Health -= damage;
                if (game.Player1Health < 0) game.Player1Health = 0;
            }
            else
            {
                game.Player2Health -= damage;
                if (game.Player2Health < 0) game.Player2Health = 0;
            }

            bool isGameOver = (game.Player1Health <= 0 || game.Player2Health <= 0);
            string? winnerID = null;
            if (isGameOver)
            {
                winnerID = game.Player1Health > 0 ? game.Player1ID : game.Player2ID;
            }

            var hitPacket = new TankHitPacket
            {
                GameID = gameID,
                HitPlayerID = hitPlayerID,
                Damage = damage,
                RemainingHealth = isPlayer1 ? game.Player1Health : game.Player2Health,
                IsGameOver = isGameOver,
                WinnerID = winnerID
            };

            // Gửi cho cả 2 người chơi
            server.RelayPrivatePacket(game.Player1ID, hitPacket);
            server.RelayPrivatePacket(game.Player2ID, hitPacket);

            if (isGameOver)
            {
                _games.Remove(gameID);
            }
        }

        public void EndGame(string gameID)
        {
            _games.Remove(gameID);
        }
        
        public bool HasGame(string gameID)
        {
            return _games.ContainsKey(gameID);
        }
    }
}
