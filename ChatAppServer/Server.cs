using ChatApp.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ChatAppServer
{
    public class Server
    {
        private TcpListener _listener;
        private readonly int _port;
        private readonly Dictionary<string, ClientHandler> _clients = new Dictionary<string, ClientHandler>();
        private readonly Dictionary<string, GameSession> _gameSessions = new Dictionary<string, GameSession>();

        public event Action<List<string>> OnUserListChanged;

        public Server(int port)
        {
            _port = port;
            _listener = new TcpListener(IPAddress.Any, _port);
        }

        public async Task StartAsync()
        {
            try
            {
                _listener.Start();
            }
            catch (Exception ex)
            {
                Logger.Error($"Không thể start port {_port}", ex);
                return;
            }

            while (true)
            {
                try
                {
                    TcpClient clientSocket = await _listener.AcceptTcpClientAsync();
                    Logger.Info($"[Connect] IP: {(clientSocket.Client.RemoteEndPoint as IPEndPoint)?.Address}");
                    ClientHandler clientHandler = new ClientHandler(clientSocket, this);
                    _ = clientHandler.StartHandlingAsync();
                }
                catch (ObjectDisposedException) { break; }
                catch (Exception ex) { Logger.Error($"Lỗi Accept Client", ex); await Task.Delay(1000); }
            }
        }

        #region Quản lý Client (Kick, Info, Register...)

        public void RegisterClient(string userID, ClientHandler handler)
        {
            lock (_clients) { _clients[userID] = handler; }
            NotifyUserListChanged();
        }

        public void RemoveClient(string userID)
        {
            if (userID == null) return;
            lock (_clients) { _clients.Remove(userID); }

            var statusPacket = new UserStatusPacket { UserID = userID, IsOnline = false };
            BroadcastPacket(statusPacket, null);
            Logger.Warning($"[Disconnect] User '{userID}' thoát.");
            NotifyUserListChanged();
        }

        // MỚI: Hàm Kick User
        public void KickUser(string userID)
        {
            lock (_clients)
            {
                if (_clients.TryGetValue(userID, out ClientHandler client))
                {
                    client.Close(); // Đóng kết nối -> Tự động gọi RemoveClient
                    Logger.Warning($"[Admin] Đã KICK user '{userID}'.");
                }
            }
        }

        // MỚI: Hàm Lấy thông tin User
        public string GetUserInfo(string userID)
        {
            lock (_clients)
            {
                if (_clients.TryGetValue(userID, out ClientHandler client))
                {
                    TimeSpan duration = DateTime.Now - client.LoginTime;
                    return $"ID: {client.UserID}\n" +
                           $"Name: {client.UserName}\n" +
                           $"IP: {client.ClientIP}\n" +
                           $"Login At: {client.LoginTime:HH:mm:ss}\n" +
                           $"Online: {duration.Hours}h {duration.Minutes}m {duration.Seconds}s";
                }
            }
            return "Người dùng không tồn tại hoặc đã thoát.";
        }

        private void NotifyUserListChanged()
        {
            lock (_clients)
            {
                var list = _clients.Values.Select(c => $"{c.UserName} ({c.UserID})").ToList();
                OnUserListChanged?.Invoke(list);
            }
        }

        public List<UserStatus> GetOnlineUsers(string excludeUserID)
        {
            lock (_clients)
            {
                return _clients.Values.Where(c => c.UserID != excludeUserID)
                    .Select(c => new UserStatus { UserID = c.UserID, UserName = c.UserName, IsOnline = true }).ToList();
            }
        }

        #endregion

        #region Giao tiếp & Game (Giữ nguyên logic cũ)

        public void BroadcastPacket(object packet, string excludeUserID)
        {
            lock (_clients)
            {
                foreach (var c in _clients.Values.ToList())
                    if (c.UserID != excludeUserID) c.SendPacket(packet);
            }
        }

        public void RelayPrivatePacket(string receiverID, object packet)
        {
            lock (_clients)
            {
                if (_clients.TryGetValue(receiverID, out ClientHandler r)) r.SendPacket(packet);
                else Logger.Warning($"Không tìm thấy user '{receiverID}'");
            }
        }

        public void ProcessGameResponse(GameResponsePacket response)
        {
            if (!response.Accepted)
            {
                Logger.Warning($"[Game] {response.SenderID} từ chối {response.ReceiverID}");
                RelayPrivatePacket(response.ReceiverID, response);
                return;
            }
            Logger.Success($"[Game] Bắt đầu: {response.ReceiverID} vs {response.SenderID}");

            string gameID = Guid.NewGuid().ToString("N").Substring(0, 10);
            GameSession newGame = new GameSession(gameID, response.ReceiverID, response.SenderID);
            lock (_gameSessions) _gameSessions.Add(gameID, newGame);

            // Gửi start packet
            ClientHandler p1, p2;
            lock (_clients)
            {
                _clients.TryGetValue(newGame.Player1_ID, out p1);
                _clients.TryGetValue(newGame.Player2_ID, out p2);
            }

            if (p1 != null && p2 != null)
            {
                p1.SendPacket(new GameStartPacket { GameID = gameID, OpponentID = p2.UserID, OpponentName = p2.UserName, StartsFirst = true });
                p2.SendPacket(new GameStartPacket { GameID = gameID, OpponentID = p1.UserID, OpponentName = p1.UserName, StartsFirst = false });
            }
        }

        public void ProcessGameMove(GameMovePacket move)
        {
            lock (_gameSessions)
            {
                if (_gameSessions.TryGetValue(move.GameID, out GameSession s))
                {
                    string oppID = s.GetOpponent(move.SenderID);
                    if (oppID != null) RelayPrivatePacket(oppID, move);
                }
            }
        }

        public void ProcessRematchRequest(RematchRequestPacket request)
        {
            lock (_gameSessions)
            {
                if (_gameSessions.TryGetValue(request.GameID, out GameSession s))
                {
                    string oppID = s.GetOpponent(request.SenderID);
                    if (oppID != null) RelayPrivatePacket(oppID, request);
                }
            }
        }

        public void ProcessRematchResponse(RematchResponsePacket response)
        {
            if (!response.Accepted)
            {
                RelayPrivatePacket(response.ReceiverID, response);
                return;
            }
            // Reset game
            lock (_gameSessions)
            {
                if (_gameSessions.TryGetValue(response.GameID, out GameSession s))
                {
                    ClientHandler p1, p2;
                    lock (_clients) { _clients.TryGetValue(s.Player1_ID, out p1); _clients.TryGetValue(s.Player2_ID, out p2); }

                    if (p1 != null && p2 != null)
                    {
                        bool p1Starts = (s.Player1_ID == response.SenderID);
                        p1.SendPacket(new GameResetPacket { GameID = response.GameID, StartsFirst = p1Starts });
                        p2.SendPacket(new GameResetPacket { GameID = response.GameID, StartsFirst = !p1Starts });
                    }
                }
            }
        }
        #endregion
    }
}