using System;
using System.Collections.Generic;

namespace ChatApp.Shared
{
    // === TANK MULTIPLAYER ROOM INFO ===
    [Serializable]
    public class TankRoomInfo
    {
        public string RoomID { get; set; } = "";
        public string RoomName { get; set; } = "";
        public string HostID { get; set; } = "";
        public string HostName { get; set; } = "";
        public int CurrentPlayers { get; set; }
        public int MaxPlayers { get; set; } = 4;
        public bool IsStarted { get; set; }
        public List<TankPlayerInfo> Players { get; set; } = new List<TankPlayerInfo>();
    }

    [Serializable]
    public class TankPlayerInfo
    {
        public string PlayerID { get; set; } = "";
        public string PlayerName { get; set; } = "";
        public int SlotIndex { get; set; } // 0-3 for 4 players
        public bool IsReady { get; set; }
        public bool IsHost { get; set; }
        public int TeamID { get; set; } // 0 = FFA, 1 = Team1, 2 = Team2
    }

    // === CREATE ROOM ===
    [Serializable]
    public class TankCreateRoomPacket
    {
        public string HostID { get; set; } = "";
        public string HostName { get; set; } = "";
        public string RoomName { get; set; } = "";
        public int MaxPlayers { get; set; } = 4; // 2-4
        public int GameMode { get; set; } = 0; // 0 = FFA, 1 = Team
    }

    [Serializable]
    public class TankCreateRoomResultPacket
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public TankRoomInfo Room { get; set; }
    }

    // === JOIN ROOM ===
    [Serializable]
    public class TankJoinRoomPacket
    {
        public string RoomID { get; set; } = "";
        public string PlayerID { get; set; } = "";
        public string PlayerName { get; set; } = "";
    }

    [Serializable]
    public class TankJoinRoomResultPacket
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public TankRoomInfo Room { get; set; }
    }

    // === LEAVE ROOM ===
    [Serializable]
    public class TankLeaveRoomPacket
    {
        public string RoomID { get; set; } = "";
        public string PlayerID { get; set; } = "";
    }

    // === REQUEST ROOM LIST ===
    [Serializable]
    public class TankRequestRoomListPacket
    {
        public string PlayerID { get; set; } = "";
    }

    [Serializable]
    public class TankRoomListPacket
    {
        public List<TankRoomInfo> Rooms { get; set; } = new List<TankRoomInfo>();
    }

    // === ROOM UPDATE (broadcast to all in room) ===
    [Serializable]
    public class TankRoomUpdatePacket
    {
        public string RoomID { get; set; } = "";
        public TankRoomInfo Room { get; set; }
        public string UpdateType { get; set; } = ""; // "PlayerJoined", "PlayerLeft", "PlayerReady", "GameStarting", "RoomClosed"
        public string AffectedPlayerID { get; set; } = "";
        public string AffectedPlayerName { get; set; } = "";
    }

    // === PLAYER READY ===
    [Serializable]
    public class TankPlayerReadyPacket
    {
        public string RoomID { get; set; } = "";
        public string PlayerID { get; set; } = "";
        public bool IsReady { get; set; }
    }

    // === START GAME (host only) ===
    [Serializable]
    public class TankStartMultiplayerPacket
    {
        public string RoomID { get; set; } = "";
        public string HostID { get; set; } = "";
    }

    // === GAME STARTED (sent to all players) ===
    [Serializable]
    public class TankMultiplayerStartedPacket
    {
        public string RoomID { get; set; } = "";
        public string GameID { get; set; } = "";
        public List<TankPlayerInfo> Players { get; set; } = new List<TankPlayerInfo>();
        public int GameMode { get; set; } // 0 = FFA, 1 = Team
        public List<TankSpawnPoint> SpawnPoints { get; set; } = new List<TankSpawnPoint>();
    }

    [Serializable]
    public class TankSpawnPoint
    {
        public string PlayerID { get; set; } = "";
        public float X { get; set; }
        public float Y { get; set; }
        public float Angle { get; set; }
    }

    // === MULTIPLAYER ACTION (extends TankActionPacket for multiple players) ===
    [Serializable]
    public class TankMultiplayerActionPacket
    {
        public string GameID { get; set; } = "";
        public string SenderID { get; set; } = "";
        public TankActionType ActionType { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Angle { get; set; }
    }

    // === MULTIPLAYER HIT ===
    [Serializable]
    public class TankMultiplayerHitPacket
    {
        public string GameID { get; set; } = "";
        public string ShooterID { get; set; } = "";
        public string HitPlayerID { get; set; } = "";
        public int Damage { get; set; }
        public int RemainingHealth { get; set; }
        public bool IsEliminated { get; set; }
    }

    // === PLAYER ELIMINATED ===
    [Serializable]
    public class TankPlayerEliminatedPacket
    {
        public string GameID { get; set; } = "";
        public string EliminatedPlayerID { get; set; } = "";
        public string EliminatedPlayerName { get; set; } = "";
        public string KillerID { get; set; } = "";
        public string KillerName { get; set; } = "";
        public int RemainingPlayers { get; set; }
    }

    // === GAME OVER ===
    [Serializable]
    public class TankMultiplayerGameOverPacket
    {
        public string GameID { get; set; } = "";
        public string WinnerID { get; set; } = "";
        public string WinnerName { get; set; } = "";
        public int WinnerTeam { get; set; } // For team mode
        public List<TankGameScore> Scores { get; set; } = new List<TankGameScore>();
    }

    [Serializable]
    public class TankGameScore
    {
        public string PlayerID { get; set; } = "";
        public string PlayerName { get; set; } = "";
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int DamageDealt { get; set; }
        public int Rank { get; set; }
    }
}
