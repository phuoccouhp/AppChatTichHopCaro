using ChatApp.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatAppServer
{
    public class TankMultiplayerManager
    {
        private readonly object _lock = new object();
        private Dictionary<string, TankMultiplayerRoom> _rooms = new Dictionary<string, TankMultiplayerRoom>();
        private Dictionary<string, TankMultiplayerGame> _games = new Dictionary<string, TankMultiplayerGame>();

        // Spawn points for 4 players (corners)
        private static readonly (float X, float Y, float Angle)[] SPAWN_POINTS = new[]
        {
            (100f, 100f, 45f),      // Top-left
            (700f, 100f, 135f),     // Top-right
            (100f, 500f, 315f),     // Bottom-left
            (700f, 500f, 225f)      // Bottom-right
        };

        public class TankMultiplayerRoom
        {
            public string RoomID { get; set; } = "";
            public string RoomName { get; set; } = "";
            public string HostID { get; set; } = "";
            public string HostName { get; set; } = "";
            public int MaxPlayers { get; set; } = 4;
            public int GameMode { get; set; } = 0; // 0 = FFA, 1 = Team
            public bool IsStarted { get; set; }
            public DateTime CreatedAt { get; set; } = DateTime.Now;
            public List<TankPlayerInfo> Players { get; set; } = new List<TankPlayerInfo>();
        }

        public class TankMultiplayerGame
        {
            public string GameID { get; set; } = "";
            public string RoomID { get; set; } = "";
            public int GameMode { get; set; }
            public List<TankMultiplayerPlayerState> Players { get; set; } = new List<TankMultiplayerPlayerState>();
            public DateTime StartedAt { get; set; } = DateTime.Now;
            public bool IsEnded { get; set; }
        }

        public class TankMultiplayerPlayerState
        {
            public string PlayerID { get; set; } = "";
            public string PlayerName { get; set; } = "";
            public int SlotIndex { get; set; }
            public int TeamID { get; set; }
            public float X { get; set; }
            public float Y { get; set; }
            public float Angle { get; set; }
            public int Health { get; set; } = 100;
            public bool IsEliminated { get; set; }
            public int Kills { get; set; }
            public int Deaths { get; set; }
            public int DamageDealt { get; set; }
        }

        #region Room Management

        public TankCreateRoomResultPacket CreateRoom(TankCreateRoomPacket packet)
        {
            lock (_lock)
            {
                // Validate
                if (string.IsNullOrEmpty(packet.HostID) || string.IsNullOrEmpty(packet.RoomName))
                {
                    return new TankCreateRoomResultPacket { Success = false, Message = "Invalid room data" };
                }

                // Check if host already in a room
                foreach (var r in _rooms.Values)
                {
                    if (r.Players.Any(p => p.PlayerID == packet.HostID))
                    {
                        return new TankCreateRoomResultPacket { Success = false, Message = "You are already in a room" };
                    }
                }

                string roomId = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                var room = new TankMultiplayerRoom
                {
                    RoomID = roomId,
                    RoomName = packet.RoomName,
                    HostID = packet.HostID,
                    HostName = packet.HostName,
                    MaxPlayers = Math.Clamp(packet.MaxPlayers, 2, 4),
                    GameMode = packet.GameMode,
                    Players = new List<TankPlayerInfo>
                    {
                        new TankPlayerInfo
                        {
                            PlayerID = packet.HostID,
                            PlayerName = packet.HostName,
                            SlotIndex = 0,
                            IsReady = false,
                            IsHost = true,
                            TeamID = packet.GameMode == 1 ? 1 : 0
                        }
                    }
                };

                _rooms[roomId] = room;
                Logger.Info($"[TankMP] Room created: {roomId} by {packet.HostName}");

                return new TankCreateRoomResultPacket
                {
                    Success = true,
                    Message = "Room created",
                    Room = ToRoomInfo(room)
                };
            }
        }

        public TankJoinRoomResultPacket JoinRoom(TankJoinRoomPacket packet)
        {
            lock (_lock)
            {
                if (!_rooms.TryGetValue(packet.RoomID, out var room))
                {
                    return new TankJoinRoomResultPacket { Success = false, Message = "Room not found" };
                }

                if (room.IsStarted)
                {
                    return new TankJoinRoomResultPacket { Success = false, Message = "Game already started" };
                }

                if (room.Players.Count >= room.MaxPlayers)
                {
                    return new TankJoinRoomResultPacket { Success = false, Message = "Room is full" };
                }

                if (room.Players.Any(p => p.PlayerID == packet.PlayerID))
                {
                    return new TankJoinRoomResultPacket { Success = false, Message = "Already in room" };
                }

                // Find next available slot
                int nextSlot = 0;
                var usedSlots = room.Players.Select(p => p.SlotIndex).ToHashSet();
                while (usedSlots.Contains(nextSlot)) nextSlot++;

                var player = new TankPlayerInfo
                {
                    PlayerID = packet.PlayerID,
                    PlayerName = packet.PlayerName,
                    SlotIndex = nextSlot,
                    IsReady = false,
                    IsHost = false,
                    TeamID = room.GameMode == 1 ? (nextSlot % 2 == 0 ? 1 : 2) : 0
                };

                room.Players.Add(player);
                Logger.Info($"[TankMP] {packet.PlayerName} joined room {room.RoomID}");

                return new TankJoinRoomResultPacket
                {
                    Success = true,
                    Message = "Joined room",
                    Room = ToRoomInfo(room)
                };
            }
        }

        public (bool success, TankRoomInfo room, List<string> otherPlayerIDs) LeaveRoom(TankLeaveRoomPacket packet)
        {
            lock (_lock)
            {
                if (!_rooms.TryGetValue(packet.RoomID, out var room))
                {
                    return (false, null, null);
                }

                var player = room.Players.FirstOrDefault(p => p.PlayerID == packet.PlayerID);
                if (player == null)
                {
                    return (false, null, null);
                }

                var otherPlayerIDs = room.Players.Where(p => p.PlayerID != packet.PlayerID).Select(p => p.PlayerID).ToList();
                room.Players.Remove(player);

                Logger.Info($"[TankMP] {player.PlayerName} left room {room.RoomID}");

                // If host left, assign new host or close room
                if (player.IsHost)
                {
                    if (room.Players.Count > 0)
                    {
                        room.Players[0].IsHost = true;
                        room.HostID = room.Players[0].PlayerID;
                        room.HostName = room.Players[0].PlayerName;
                        Logger.Info($"[TankMP] New host: {room.HostName}");
                    }
                    else
                    {
                        _rooms.Remove(packet.RoomID);
                        Logger.Info($"[TankMP] Room {room.RoomID} closed (empty)");
                        return (true, null, otherPlayerIDs);
                    }
                }

                return (true, ToRoomInfo(room), otherPlayerIDs);
            }
        }

        public List<TankRoomInfo> GetRoomList()
        {
            lock (_lock)
            {
                return _rooms.Values
                    .Where(r => !r.IsStarted)
                    .Select(r => ToRoomInfo(r))
                    .ToList();
            }
        }

        public (bool success, TankRoomInfo room, List<string> otherPlayerIDs) SetPlayerReady(TankPlayerReadyPacket packet)
        {
            lock (_lock)
            {
                if (!_rooms.TryGetValue(packet.RoomID, out var room))
                {
                    return (false, null, null);
                }

                var player = room.Players.FirstOrDefault(p => p.PlayerID == packet.PlayerID);
                if (player == null)
                {
                    return (false, null, null);
                }

                player.IsReady = packet.IsReady;
                var otherPlayerIDs = room.Players.Where(p => p.PlayerID != packet.PlayerID).Select(p => p.PlayerID).ToList();

                return (true, ToRoomInfo(room), otherPlayerIDs);
            }
        }

        #endregion

        #region Game Management

        public (bool success, string message, TankMultiplayerStartedPacket startPacket, List<string> playerIDs) StartGame(TankStartMultiplayerPacket packet)
        {
            lock (_lock)
            {
                if (!_rooms.TryGetValue(packet.RoomID, out var room))
                {
                    return (false, "Room not found", null, null);
                }

                if (room.HostID != packet.HostID)
                {
                    return (false, "Only host can start", null, null);
                }

                if (room.Players.Count < 2)
                {
                    return (false, "Need at least 2 players", null, null);
                }

                // Check all players ready (except host)
                var notReady = room.Players.Where(p => !p.IsHost && !p.IsReady).ToList();
                if (notReady.Count > 0)
                {
                    return (false, $"{notReady[0].PlayerName} is not ready", null, null);
                }

                // Create game
                string gameId = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                var game = new TankMultiplayerGame
                {
                    GameID = gameId,
                    RoomID = room.RoomID,
                    GameMode = room.GameMode,
                    Players = new List<TankMultiplayerPlayerState>()
                };

                var spawnPoints = new List<TankSpawnPoint>();

                for (int i = 0; i < room.Players.Count; i++)
                {
                    var p = room.Players[i];
                    var spawn = SPAWN_POINTS[p.SlotIndex % SPAWN_POINTS.Length];

                    game.Players.Add(new TankMultiplayerPlayerState
                    {
                        PlayerID = p.PlayerID,
                        PlayerName = p.PlayerName,
                        SlotIndex = p.SlotIndex,
                        TeamID = p.TeamID,
                        X = spawn.X,
                        Y = spawn.Y,
                        Angle = spawn.Angle,
                        Health = 100
                    });

                    spawnPoints.Add(new TankSpawnPoint
                    {
                        PlayerID = p.PlayerID,
                        X = spawn.X,
                        Y = spawn.Y,
                        Angle = spawn.Angle
                    });
                }

                _games[gameId] = game;
                room.IsStarted = true;

                var startedPacket = new TankMultiplayerStartedPacket
                {
                    RoomID = room.RoomID,
                    GameID = gameId,
                    GameMode = room.GameMode,
                    Players = room.Players.ToList(),
                    SpawnPoints = spawnPoints
                };

                var playerIDs = room.Players.Select(p => p.PlayerID).ToList();

                Logger.Info($"[TankMP] Game started: {gameId} with {playerIDs.Count} players");

                return (true, "Game started", startedPacket, playerIDs);
            }
        }

        public List<string> GetOtherPlayersInGame(string gameId, string excludePlayerId)
        {
            lock (_lock)
            {
                if (!_games.TryGetValue(gameId, out var game))
                {
                    return new List<string>();
                }

                return game.Players
                    .Where(p => p.PlayerID != excludePlayerId && !p.IsEliminated)
                    .Select(p => p.PlayerID)
                    .ToList();
            }
        }

        public (TankMultiplayerHitPacket hitPacket, TankPlayerEliminatedPacket eliminatedPacket, TankMultiplayerGameOverPacket gameOverPacket, List<string> playerIDs) 
            ProcessHit(string gameId, string shooterId, string hitPlayerId, int damage)
        {
            lock (_lock)
            {
                if (!_games.TryGetValue(gameId, out var game))
                {
                    return (null, null, null, null);
                }

                var hitPlayer = game.Players.FirstOrDefault(p => p.PlayerID == hitPlayerId);
                var shooter = game.Players.FirstOrDefault(p => p.PlayerID == shooterId);

                if (hitPlayer == null || hitPlayer.IsEliminated)
                {
                    return (null, null, null, null);
                }

                hitPlayer.Health -= damage;
                if (hitPlayer.Health < 0) hitPlayer.Health = 0;

                if (shooter != null)
                {
                    shooter.DamageDealt += damage;
                }

                var playerIDs = game.Players.Select(p => p.PlayerID).ToList();

                var hitPacket = new TankMultiplayerHitPacket
                {
                    GameID = gameId,
                    ShooterID = shooterId,
                    HitPlayerID = hitPlayerId,
                    Damage = damage,
                    RemainingHealth = hitPlayer.Health,
                    IsEliminated = hitPlayer.Health <= 0
                };

                TankPlayerEliminatedPacket eliminatedPacket = null;
                TankMultiplayerGameOverPacket gameOverPacket = null;

                if (hitPlayer.Health <= 0)
                {
                    hitPlayer.IsEliminated = true;
                    hitPlayer.Deaths++;
                    if (shooter != null) shooter.Kills++;

                    var alivePlayers = game.Players.Where(p => !p.IsEliminated).ToList();

                    eliminatedPacket = new TankPlayerEliminatedPacket
                    {
                        GameID = gameId,
                        EliminatedPlayerID = hitPlayer.PlayerID,
                        EliminatedPlayerName = hitPlayer.PlayerName,
                        KillerID = shooterId,
                        KillerName = shooter?.PlayerName ?? "Unknown",
                        RemainingPlayers = alivePlayers.Count
                    };

                    Logger.Info($"[TankMP] {hitPlayer.PlayerName} eliminated by {shooter?.PlayerName}");

                    // Check game over
                    if (game.GameMode == 0) // FFA
                    {
                        if (alivePlayers.Count <= 1)
                        {
                            var winner = alivePlayers.FirstOrDefault();
                            gameOverPacket = CreateGameOverPacket(game, winner?.PlayerID, winner?.PlayerName, 0);
                        }
                    }
                    else // Team mode
                    {
                        var aliveTeams = alivePlayers.Select(p => p.TeamID).Distinct().ToList();
                        if (aliveTeams.Count <= 1)
                        {
                            int winnerTeam = aliveTeams.FirstOrDefault();
                            var teamWinner = alivePlayers.FirstOrDefault();
                            gameOverPacket = CreateGameOverPacket(game, teamWinner?.PlayerID, $"Team {winnerTeam}", winnerTeam);
                        }
                    }
                }

                return (hitPacket, eliminatedPacket, gameOverPacket, playerIDs);
            }
        }

        private TankMultiplayerGameOverPacket CreateGameOverPacket(TankMultiplayerGame game, string winnerId, string winnerName, int winnerTeam)
        {
            game.IsEnded = true;

            var scores = game.Players
                .OrderByDescending(p => p.Kills)
                .ThenBy(p => p.Deaths)
                .ThenByDescending(p => p.DamageDealt)
                .Select((p, index) => new TankGameScore
                {
                    PlayerID = p.PlayerID,
                    PlayerName = p.PlayerName,
                    Kills = p.Kills,
                    Deaths = p.Deaths,
                    DamageDealt = p.DamageDealt,
                    Rank = index + 1
                })
                .ToList();

            Logger.Info($"[TankMP] Game over: {game.GameID} - Winner: {winnerName}");

            return new TankMultiplayerGameOverPacket
            {
                GameID = game.GameID,
                WinnerID = winnerId ?? "",
                WinnerName = winnerName ?? "",
                WinnerTeam = winnerTeam,
                Scores = scores
            };
        }

        public void EndGame(string gameId)
        {
            lock (_lock)
            {
                if (_games.TryGetValue(gameId, out var game))
                {
                    // Also remove the room
                    _rooms.Remove(game.RoomID);
                    _games.Remove(gameId);
                    Logger.Info($"[TankMP] Game {gameId} ended and cleaned up");
                }
            }
        }

        #endregion

        #region Helpers

        private TankRoomInfo ToRoomInfo(TankMultiplayerRoom room)
        {
            return new TankRoomInfo
            {
                RoomID = room.RoomID,
                RoomName = room.RoomName,
                HostID = room.HostID,
                HostName = room.HostName,
                CurrentPlayers = room.Players.Count,
                MaxPlayers = room.MaxPlayers,
                IsStarted = room.IsStarted,
                Players = room.Players.ToList()
            };
        }

        public TankRoomInfo GetRoom(string roomId)
        {
            lock (_lock)
            {
                if (_rooms.TryGetValue(roomId, out var room))
                {
                    return ToRoomInfo(room);
                }
                return null;
            }
        }

        #endregion
    }
}
