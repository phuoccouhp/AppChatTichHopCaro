using ChatApp.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ChatAppServer
{
    public class Server
    {
        private TcpListener? _listener;
        private readonly int _port;
        private readonly Dictionary<string, ClientHandler> _clients = new Dictionary<string, ClientHandler>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, GameSession> _gameSessions = new Dictionary<string, GameSession>();
        public TankGameManager TankGameManager { get; private set; }
        private readonly Dictionary<string, int> _gameInviteMessageIds = new Dictionary<string, int>();
        private CancellationTokenSource _serverCts = new CancellationTokenSource();

        public event Action<List<string>>? OnUserListChanged;

        public Server(int port)
        {
            _port = port;
            _listener = new TcpListener(IPAddress.Any, _port);
            TankGameManager = new TankGameManager();
        }

        public async Task StartAsync()
        {
            try
            {
                if (!FirewallHelper.IsPortOpen(_port)) FirewallHelper.OpenPortAsAdmin(_port);

                _listener.Start(100);
                Logger.Success($"Server đang chạy tại Port: {_port}");

                _ = Task.Run(() => RunGameLoop(_serverCts.Token));

                while (!_serverCts.IsCancellationRequested)
                {
                    try
                    {
                        TcpClient clientSocket = await _listener.AcceptTcpClientAsync();
                        _ = Task.Run(() =>
                        {
                            try
                            {
                                var handler = new ClientHandler(clientSocket, this);
                                _ = handler.StartHandlingAsync();
                            }
                            catch (Exception ex) { Logger.Error($"Lỗi client: {ex.Message}"); }
                        });
                    }
                    catch (ObjectDisposedException) { break; }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Start Error: {ex.Message}");
                throw;
            }
        }

        private async Task RunGameLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    List<string> tankGames;
                    lock (_gameSessions)
                        tankGames = _gameSessions.Values.Where(g => g.Type == GameType.Tank).Select(g => g.GameID).ToList();

                    foreach (var gid in tankGames) TankGameManager.UpdateBullets(gid, this);
                }
                catch { }
                await Task.Delay(16, token);
            }
        }

        public void Stop()
        {
            _serverCts.Cancel();
            _listener?.Stop();
        }

        // --- QUẢN LÝ CLIENT ---

        public void RegisterClient(string userID, ClientHandler handler)
        {
            lock (_clients) _clients[userID] = handler;
            DatabaseManager.Instance.UpdateUserOnlineStatus(userID, true);
            BroadcastPacket(new UserStatusPacket { UserID = userID, UserName = handler.UserName, IsOnline = true }, userID);
            NotifyUserListChanged();
        }

        public void RemoveClient(string userID)
        {
            if (string.IsNullOrEmpty(userID)) return;
            lock (_clients) _clients.Remove(userID);
            DatabaseManager.Instance.UpdateUserOnlineStatus(userID, false);
            BroadcastPacket(new UserStatusPacket { UserID = userID, IsOnline = false }, userID);
            NotifyUserListChanged();
        }

        // === [FIXED] Lấy danh sách kết hợp DB + RAM ===
        public List<UserStatus> GetOnlineUsers(string excludeUserID)
        {
            // 1. Lấy danh sách từ Database (History contacts) - Mặc định IsOnline = false
            var contacts = DatabaseManager.Instance.GetContacts(excludeUserID);

            // Dùng Dictionary để tra cứu nhanh
            var contactDict = new Dictionary<string, UserStatus>(StringComparer.OrdinalIgnoreCase);
            foreach (var c in contacts) contactDict[c.UserID] = c;

            lock (_clients)
            {
                // 2. Duyệt qua danh sách Client đang kết nối thực tế
                foreach (var client in _clients.Values)
                {
                    if (client.UserID == excludeUserID) continue;

                    if (contactDict.ContainsKey(client.UserID))
                    {
                        // Người này có trong DB -> Cập nhật trạng thái Online
                        contactDict[client.UserID].IsOnline = true;
                        contactDict[client.UserID].UserName = client.UserName; // Cập nhật tên mới nhất
                    }
                    else
                    {
                        // Người này là người lạ (chưa chat bao giờ) nhưng đang Online -> Thêm vào list
                        contactDict[client.UserID] = new UserStatus
                        {
                            UserID = client.UserID,
                            UserName = client.UserName,
                            IsOnline = true
                        };
                    }
                }
            }

            // 3. Trả về danh sách (Ưu tiên Online lên đầu)
            return contactDict.Values
                .OrderByDescending(u => u.IsOnline)
                .ThenBy(u => u.UserName)
                .ToList();
        }

        private void NotifyUserListChanged()
        {
            lock (_clients)
            {
                var list = _clients.Values.Select(c => $"{c.UserName} ({c.UserID})").ToList();
                OnUserListChanged?.Invoke(list);
            }
        }

        public void BroadcastPacket(object packet, string excludeID)
        {
            lock (_clients)
            {
                foreach (var c in _clients.Values) if (c.UserID != excludeID) c.SendPacket(packet);
            }
        }

        public void RelayPrivatePacket(string receiverID, object packet)
        {
            lock (_clients)
            {
                if (_clients.TryGetValue(receiverID, out var c)) c.SendPacket(packet);
            }
        }

        // --- LOGIC GAME ---
        public void StoreGameInviteMessageId(string s, string r, int id)
        {
            lock (_gameInviteMessageIds) _gameInviteMessageIds[$"{s}:{r}"] = id;
        }

        public void ProcessGameResponse(GameResponsePacket p)
        {
            string key = $"{p.ReceiverID}:{p.SenderID}";
            lock (_gameInviteMessageIds)
            {
                if (_gameInviteMessageIds.TryGetValue(key, out int msgId))
                {
                    DatabaseManager.Instance.UpdateMessage(msgId, "Game Invite: " + (p.Accepted ? "Accepted" : "Declined"));
                    _gameInviteMessageIds.Remove(key);
                }
            }
            if (!p.Accepted) { RelayPrivatePacket(p.ReceiverID, p); return; }

            string gameID = Guid.NewGuid().ToString("N");
            lock (_gameSessions) _gameSessions[gameID] = new GameSession(gameID, p.ReceiverID, p.SenderID, GameType.Caro);

            RelayPrivatePacket(p.ReceiverID, new GameStartPacket { GameID = gameID, OpponentID = p.SenderID, StartsFirst = true });
            RelayPrivatePacket(p.SenderID, new GameStartPacket { GameID = gameID, OpponentID = p.ReceiverID, StartsFirst = false });
        }

        public void ProcessGameMove(GameMovePacket p) => RelayIfGame(p.GameID, p.SenderID, p);
        public void ProcessRematchRequest(RematchRequestPacket p) => RelayIfGame(p.GameID, p.SenderID, p);
        public void ProcessTankAction(TankActionPacket p) => RelayIfGame(p.GameID, p.SenderID, p);

        public void ProcessRematchResponse(RematchResponsePacket p)
        {
            if (!p.Accepted) { RelayPrivatePacket(p.ReceiverID, p); return; }
            lock (_gameSessions)
            {
                if (_gameSessions.TryGetValue(p.GameID, out var s))
                {
                    if (s.Type == GameType.Tank)
                    {
                        TankGameManager.StartGame(p.GameID, s.Player1_ID, s.Player2_ID);
                        bool p1Start = (s.Player1_ID == p.SenderID);
                        RelayPrivatePacket(s.Player1_ID, new TankStartPacket { GameID = p.GameID, OpponentID = s.Player2_ID, StartsFirst = p1Start });
                        RelayPrivatePacket(s.Player2_ID, new TankStartPacket { GameID = p.GameID, OpponentID = s.Player1_ID, StartsFirst = !p1Start });
                    }
                    else
                    {
                        bool p1Start = (s.Player1_ID == p.SenderID);
                        RelayPrivatePacket(s.Player1_ID, new GameResetPacket { GameID = p.GameID, StartsFirst = p1Start });
                        RelayPrivatePacket(s.Player2_ID, new GameResetPacket { GameID = p.GameID, StartsFirst = !p1Start });
                    }
                }
            }
        }

        public void ProcessTankResponse(TankResponsePacket p)
        {
            string key = $"{p.ReceiverID}:{p.SenderID}";
            lock (_gameInviteMessageIds)
            {
                if (_gameInviteMessageIds.TryGetValue(key, out int msgId))
                {
                    DatabaseManager.Instance.UpdateMessage(msgId, "Tank Invite: " + (p.Accepted ? "Accepted" : "Declined"));
                    _gameInviteMessageIds.Remove(key);
                }
            }
            if (!p.Accepted) { RelayPrivatePacket(p.ReceiverID, p); return; }

            string gameID = Guid.NewGuid().ToString("N");
            lock (_gameSessions) _gameSessions[gameID] = new GameSession(gameID, p.ReceiverID, p.SenderID, GameType.Tank);
            TankGameManager.StartGame(gameID, p.ReceiverID, p.SenderID);

            RelayPrivatePacket(p.ReceiverID, new TankStartPacket { GameID = gameID, OpponentID = p.SenderID, StartsFirst = true });
            RelayPrivatePacket(p.SenderID, new TankStartPacket { GameID = gameID, OpponentID = p.ReceiverID, StartsFirst = false });
        }

        private void RelayIfGame(string gid, string sid, object p)
        {
            lock (_gameSessions)
            {
                if (_gameSessions.TryGetValue(gid, out var s))
                {
                    string? t = s.GetOpponent(sid);
                    if (t != null) RelayPrivatePacket(t, p);
                }
            }
        }

        public string GetUserInfo(string id) { lock (_clients) return _clients.TryGetValue(id, out var c) ? $"User: {c.UserName}\nIP: {c.ClientIP}" : "Not Found"; }
        public void KickUser(string id) => RemoveClient(id);
    }
}