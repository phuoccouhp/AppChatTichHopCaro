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
<<<<<<< HEAD
                    else
                    {
                        Logger.Error("Không thể tự động mở Firewall. Vui lòng chạy phần mềm bằng 'Run as Administrator' hoặc mở port thủ công.");
                    }
                }
                else
                {
                    Logger.Success($"Check Firewall: Port {_port} đã mở sẵn sàng.");
                }

                // --- BƯỚC 2: KIỂM TRA LISTENER ---
                if (_listener == null)
                {
                    Logger.Error("Listener is null! Không thể khởi động server.");
                    throw new InvalidOperationException("TcpListener is null");
                }

                // --- BƯỚC 3: CẤU HÌNH SOCKET ---
                try
                {
                    if (_listener.Server != null)
                    {
                        _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                        _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                        // KeepAliveTime: thời gian (ms) không có hoạt động trước khi gửi packet thăm dò (30s)
                        _listener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 30000);
                        _listener.Server.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1000);
                    }
                }
                catch (Exception optEx)
                {
                    Logger.Warning($"Không thể set socket options: {optEx.Message}");
                    // Không throw vì đây không phải lỗi nghiêm trọng
                }

                // --- BƯỚC 4: BẮT ĐẦU LẮNG NGHE ---
                try
                {
                    _listener.Start(100); // Backlog = 100 clients
                    Logger.Success($"Listener đã bắt đầu lắng nghe trên port {_port}");
                }
                catch (SocketException sockEx)
                {
                    Logger.Error($"Lỗi Socket khi khởi động listener trên port {_port}: {sockEx.SocketErrorCode} - {sockEx.Message}", sockEx);
                    if (sockEx.SocketErrorCode == SocketError.AddressAlreadyInUse)
                    {
                        Logger.Error($"Port {_port} đang được sử dụng bởi ứng dụng khác. Hãy đóng ứng dụng đó hoặc thay đổi port.");
                    }
                    throw;
                }
                catch (InvalidOperationException ioEx)
                {
                    Logger.Error($"Lỗi InvalidOperation khi khởi động listener: {ioEx.Message}", ioEx);
                    throw;
                }
                catch (Exception startEx)
                {
                    Logger.Error($"Không thể khởi động listener trên port {_port}. Port có thể đang bị dùng bởi ứng dụng khác.", startEx);
                    throw;
                }

                // --- BƯỚC 5: HIỂN THỊ THÔNG TIN KẾT NỐI CHO NGƯỜI DÙNG ---
                if (_listener == null)
                {
                    Logger.Error("Listener is null sau khi Start! Không thể lấy thông tin kết nối.");
                    throw new InvalidOperationException("TcpListener is null after Start");
                }

                var localEndpoint = _listener.LocalEndpoint as IPEndPoint;
                if (localEndpoint != null)
                {
                    Logger.Success($"Server đã khởi động tại Port: {localEndpoint.Port}");

                    // Lấy IP thực tế của WiFi/LAN adapter
                    string? wifiIP = GetWiFiIPAddress();

                    Logger.Info("=========================================================");
                    Logger.Info(" THÔNG TIN KẾT NỐI:");
                    Logger.Info(" 1. Nếu Client chạy cùng máy tính này: dùng IP [127.0.0.1]");

                    if (!string.IsNullOrEmpty(wifiIP))
                    {
                        Logger.Info($" 2. Nếu Client ở máy khác (cùng Wifi): dùng IP [{wifiIP}]");
                        Logger.Success($" -> HÃY GỬI IP [{wifiIP}] CHO MÁY CLIENT.");
                    }
                    else
                    {
                        Logger.Warning(" -> Không tìm thấy IP mạng Wifi/LAN. Hãy kiểm tra lại kết nối mạng.");
                    }
                    Logger.Info("=========================================================");
=======
                    catch (ObjectDisposedException) { break; }
>>>>>>> b682e7f49d0e9c32014cb9ee85c85a1bd8884fdf
                }
                else
                {
                    Logger.Warning("Không thể lấy LocalEndpoint từ listener");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Start Error: {ex.Message}");
                throw;
            }
<<<<<<< HEAD

            // Vòng lặp chấp nhận kết nối
            while (true)
            {
                try
                {
                    // Kiểm tra listener trước khi accept
                    if (_listener == null)
                    {
                        Logger.Error("Listener is null! Không thể chấp nhận kết nối mới.");
                        break;
                    }

                    TcpClient clientSocket = await _listener.AcceptTcpClientAsync();
                    
                    // Kiểm tra client socket
                    if (clientSocket == null)
                    {
                        Logger.Warning("AcceptTcpClientAsync trả về null client socket");
                        continue;
                    }

                    try
                    {
                        var remoteEP = clientSocket.Client.RemoteEndPoint as IPEndPoint;
                        Logger.Info($"[Connect] Client mới kết nối từ: {remoteEP?.Address}:{remoteEP?.Port}");
                    }
                    catch (Exception epEx)
                    {
                        Logger.Warning($"Không thể lấy RemoteEndPoint: {epEx.Message}");
                    }

                    ClientHandler? clientHandler = null;
                    try
                    {
                        clientHandler = new ClientHandler(clientSocket, this);
                    }
                    catch (Exception handlerEx)
                    {
                        Logger.Error($"Lỗi tạo ClientHandler: {handlerEx.Message}", handlerEx);
                        try 
                        { 
                            clientSocket.Close(); 
                            clientSocket.Dispose();
                        } 
                        catch { }
                        continue;
                    }

                    if (clientHandler != null)
                    {
                        // Chạy handler trên thread riêng, xử lý lỗi task
                        _ = clientHandler.StartHandlingAsync().ContinueWith(task =>
                        {
                            if (task.IsFaulted && task.Exception != null)
                            {
                                var baseEx = task.Exception.GetBaseException();
                                Logger.Warning($"[Server] Lỗi trong ClientHandler: {baseEx.GetType().Name} - {baseEx.Message}");
                            }
                        }, TaskContinuationOptions.OnlyOnFaulted);
                    }
                }
                catch (ObjectDisposedException)
                {
                    Logger.Warning("Listener đã dừng.");
                    break;
                }
                catch (InvalidOperationException ioEx)
                {
                    // Listener chưa start hoặc đã stop
                    Logger.Error($"Listener không sẵn sàng: {ioEx.Message}", ioEx);
                    await Task.Delay(2000);
                }
                catch (SocketException sockEx)
                {
                    Logger.Error($"Lỗi Socket khi chấp nhận client: {sockEx.SocketErrorCode} - {sockEx.Message}", sockEx);
                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    Logger.Error($"Lỗi khi chấp nhận client: {ex.GetType().Name} - {ex.Message}", ex);
                    await Task.Delay(1000);
                }
            }
=======
>>>>>>> b682e7f49d0e9c32014cb9ee85c85a1bd8884fdf
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