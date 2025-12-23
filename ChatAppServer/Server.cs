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
        private TcpListener? _listener;
        private readonly int _port;
        // Dùng Dictionary để quản lý client (Key = UserID, Value = ClientHandler)
        private readonly Dictionary<string, ClientHandler> _clients = new Dictionary<string, ClientHandler>();
        // Quản lý các ván game đang diễn ra (Key = GameID)
        private readonly Dictionary<string, GameSession> _gameSessions = new Dictionary<string, GameSession>();
        // Quản lý Tank Game
        public TankGameManager TankGameManager { get; private set; }
        // Lưu MessageID của game invite (Key = "SenderID:ReceiverID", Value = MessageID)
        private readonly Dictionary<string, int> _gameInviteMessageIds = new Dictionary<string, int>();

        // Sự kiện để báo cho Giao diện (Form) biết khi danh sách user thay đổi
        public event Action<List<string>>? OnUserListChanged;

        public Server(int port)
        {
            _port = port;
            // QUAN TRỌNG: Sử dụng IPAddress.Any để lắng nghe trên tất cả interfaces (Wifi, LAN, Localhost)
            _listener = new TcpListener(IPAddress.Any, _port);
            TankGameManager = new TankGameManager();
        }

        public async Task StartAsync()
        {
            try
            {
                // --- BƯỚC 1: TỰ ĐỘNG CẤU HÌNH FIREWALL ---
                Logger.Info("Đang kiểm tra cấu hình Firewall...");
                bool isPortOpen = FirewallHelper.IsPortOpen(_port);

                if (!isPortOpen)
                {
                    Logger.Warning($"Port {_port} chưa được mở trong Windows Firewall.");
                    Logger.Info("Đang cố gắng mở port tự động với quyền Administrator...");

                    // Thử mở port tự động
                    bool opened = FirewallHelper.OpenPortAsAdmin(_port);

                    if (opened)
                    {
                        Logger.Success($"Đã mở port {_port} thành công! Client có thể kết nối.");
                    }
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
                }
                else
                {
                    Logger.Warning("Không thể lấy LocalEndpoint từ listener");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Lỗi nghiêm trọng khi khởi động Server: {ex.Message}", ex);
                throw;
            }

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
        }

        /// <summary>
        /// Lấy IP thực tế của WiFi adapter (interface đang active ra internet)
        /// </summary>
        private string? GetWiFiIPAddress()
        {
            try
            {
                // Mẹo: Tạo kết nối UDP giả đến Google DNS để hệ điều hành tự chọn Interface mạng chính
                using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    var endPoint = socket.LocalEndPoint as IPEndPoint;
                    return endPoint?.Address.ToString();
                }
            }
            catch
            {
                // Fallback: Lấy IP đầu tiên không phải Loopback
                try
                {
                    var host = Dns.GetHostEntry(Dns.GetHostName());
                    foreach (var ip in host.AddressList)
                    {
                        if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                        {
                            return ip.ToString();
                        }
                    }
                }
                catch { }
                return null;
            }
        }

        #region Quản lý Client (Kick, Info, Register...)

        public void RegisterClient(string userID, ClientHandler handler)
        {
            lock (_clients)
            {
                _clients[userID] = handler;
            }
            NotifyUserListChanged();
        }

        public void RemoveClient(string userID)
        {
            if (userID == null) return;

            lock (_clients)
            {
                _clients.Remove(userID);
            }

            var statusPacket = new UserStatusPacket
            {
                UserID = userID,
                IsOnline = false
            };
            BroadcastPacket(statusPacket, null);

            Logger.Warning($"[Disconnect] User '{userID}' đã ngắt kết nối.");
            NotifyUserListChanged();
        }

        public void KickUser(string userID)
        {
            lock (_clients)
            {
                if (_clients.TryGetValue(userID, out ClientHandler client))
                {
                    client.Close();
                    Logger.Warning($"[Admin] Đã KICK user '{userID}'.");
                }
            }
        }

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

        public List<UserStatus> GetOnlineUsers(string excludeUserID)
        {
            var result = new List<UserStatus>();
            try
            {
                var contacts = DatabaseManager.Instance.GetContacts(excludeUserID);
                var dict = new Dictionary<string, UserStatus>(StringComparer.OrdinalIgnoreCase);
                foreach (var c in contacts)
                {
                    if (c.UserID != null && c.UserID != excludeUserID)
                        dict[c.UserID] = new UserStatus { UserID = c.UserID, UserName = c.UserName, IsOnline = false };
                }

                lock (_clients)
                {
                    foreach (var client in _clients.Values)
                    {
                        if (string.IsNullOrEmpty(client.UserID) || client.UserID == excludeUserID) continue;
                        if (dict.ContainsKey(client.UserID))
                        {
                            dict[client.UserID].IsOnline = true;
                            dict[client.UserID].UserName = client.UserName;
                        }
                        else
                        {
                            dict[client.UserID] = new UserStatus { UserID = client.UserID, UserName = client.UserName, IsOnline = true };
                        }
                    }
                }
                result = new List<UserStatus>(dict.Values);
            }
            catch (Exception ex)
            {
                Logger.Error("Lỗi khi lấy danh sách online/contacts", ex);
            }
            return result;
        }

        private void NotifyUserListChanged()
        {
            lock (_clients)
            {
                var userList = _clients.Values.Select(c => $"{c.UserName} ({c.UserID})").ToList();
                OnUserListChanged?.Invoke(userList);
            }
        }

        #endregion

        #region Gửi Gói Tin & Logic Game (Giữ nguyên)

        public void BroadcastPacket(object packet, string excludeUserID)
        {
            lock (_clients)
            {
                foreach (var client in _clients.Values.ToList())
                {
                    if (client.UserID != excludeUserID)
                    {
                        client.SendPacket(packet);
                    }
                }
            }
        }

        public void RelayPrivatePacket(string receiverID, object packet)
        {
            lock (_clients)
            {
                if (_clients.TryGetValue(receiverID, out ClientHandler receiver))
                {
                    receiver.SendPacket(packet);
                }
                else
                {
                    Logger.Warning($"[Warning] Không tìm thấy client '{receiverID}' để chuyển tin.");
                }
            }
        }

        public void StoreGameInviteMessageId(string senderID, string receiverID, int messageId)
        {
            if (messageId > 0)
            {
                string key = $"{senderID}:{receiverID}";
                lock (_gameInviteMessageIds)
                {
                    _gameInviteMessageIds[key] = messageId;
                }
            }
        }

        private int GetAndRemoveGameInviteMessageId(string senderID, string receiverID)
        {
            string key = $"{senderID}:{receiverID}";
            lock (_gameInviteMessageIds)
            {
                if (_gameInviteMessageIds.TryGetValue(key, out int messageId))
                {
                    _gameInviteMessageIds.Remove(key);
                    return messageId;
                }
            }
            return 0;
        }

        public void ProcessGameResponse(GameResponsePacket response)
        {
            int messageId = GetAndRemoveGameInviteMessageId(response.SenderID, response.ReceiverID);
            if (messageId > 0)
            {
                string statusText = response.Accepted ? "✓ Đã chấp nhận" : "✗ Đã từ chối";
                var messages = DatabaseManager.Instance.GetChatHistory(response.ReceiverID, response.SenderID, 100);
                var message = messages.FirstOrDefault(m => m.MessageID == messageId);
                if (message != null)
                {
                    DatabaseManager.Instance.UpdateMessage(messageId, message.MessageContent + " - " + statusText);
                }
            }

            if (!response.Accepted)
            {
                RelayPrivatePacket(response.ReceiverID, response);
                return;
            }

            string player1_ID = response.ReceiverID;
            string player2_ID = response.SenderID;
            string gameID = Guid.NewGuid().ToString("N").Substring(0, 10);

            GameSession newGame = new GameSession(gameID, player1_ID, player2_ID, GameType.Caro);
            lock (_gameSessions) { _gameSessions.Add(gameID, newGame); }

            ClientHandler player1_Handler, player2_Handler;
            lock (_clients)
            {
                _clients.TryGetValue(player1_ID, out player1_Handler);
                _clients.TryGetValue(player2_ID, out player2_Handler);
            }

            if (player1_Handler == null || player2_Handler == null)
            {
                lock (_gameSessions) _gameSessions.Remove(gameID);
                return;
            }

            player1_Handler.SendPacket(new GameStartPacket { GameID = gameID, OpponentID = player2_Handler.UserID, OpponentName = player2_Handler.UserName, StartsFirst = true });
            player2_Handler.SendPacket(new GameStartPacket { GameID = gameID, OpponentID = player1_Handler.UserID, OpponentName = player1_Handler.UserName, StartsFirst = false });
        }

        public void ProcessGameMove(GameMovePacket move)
        {
            GameSession? session;
            lock (_gameSessions) { _gameSessions.TryGetValue(move.GameID, out session); }

            if (session != null)
            {
                string? opponentID = session.GetOpponent(move.SenderID);
                if (opponentID != null) RelayPrivatePacket(opponentID, move);
            }
        }

        public void ProcessRematchRequest(RematchRequestPacket request)
        {
            GameSession? session;
            lock (_gameSessions) _gameSessions.TryGetValue(request.GameID, out session);
            if (session != null)
            {
                string? opponentID = session.GetOpponent(request.SenderID);
                if (opponentID != null) RelayPrivatePacket(opponentID, request);
            }
        }

        public void ProcessRematchResponse(RematchResponsePacket response)
        {
            if (!response.Accepted)
            {
                RelayPrivatePacket(response.ReceiverID, response);
                return;
            }

            GameSession? session;
            lock (_gameSessions) _gameSessions.TryGetValue(response.GameID, out session);

            if (session != null)
            {
                ClientHandler? player1_Handler, player2_Handler;
                lock (_clients)
                {
                    _clients.TryGetValue(session.Player1_ID, out player1_Handler);
                    _clients.TryGetValue(session.Player2_ID, out player2_Handler);
                }

                if (player1_Handler != null && player2_Handler != null)
                {
                    bool isTankGame = (session.Type == GameType.Tank);
                    if (isTankGame)
                    {
                        this.TankGameManager.EndGame(response.GameID);
                        this.TankGameManager.StartGame(response.GameID, session.Player1_ID, session.Player2_ID);
                        bool player1Starts = (session.Player1_ID == response.SenderID);
                        player1_Handler.SendPacket(new TankStartPacket { GameID = response.GameID, OpponentID = player2_Handler.UserID, OpponentName = player2_Handler.UserName, StartsFirst = player1Starts });
                        player2_Handler.SendPacket(new TankStartPacket { GameID = response.GameID, OpponentID = player1_Handler.UserID, OpponentName = player1_Handler.UserName, StartsFirst = !player1Starts });
                    }
                    else
                    {
                        bool player1Starts = (session.Player1_ID == response.SenderID);
                        player1_Handler.SendPacket(new GameResetPacket { GameID = response.GameID, StartsFirst = player1Starts });
                        player2_Handler.SendPacket(new GameResetPacket { GameID = response.GameID, StartsFirst = !player1Starts });
                    }
                }
            }
        }

        public void ProcessTankResponse(TankResponsePacket response)
        {
            int messageId = GetAndRemoveGameInviteMessageId(response.ReceiverID, response.SenderID);
            if (messageId > 0)
            {
                string statusText = response.Accepted ? "✓ Đã chấp nhận" : "✗ Đã từ chối";
                var messages = DatabaseManager.Instance.GetChatHistory(response.ReceiverID, response.SenderID, 100);
                var message = messages.FirstOrDefault(m => m.MessageID == messageId);
                if (message != null) DatabaseManager.Instance.UpdateMessage(messageId, message.MessageContent + " - " + statusText);
            }

            if (!response.Accepted)
            {
                RelayPrivatePacket(response.ReceiverID, response);
                return;
            }

            string player1_ID = response.ReceiverID;
            string player2_ID = response.SenderID;
            string gameID = Guid.NewGuid().ToString("N").Substring(0, 10);
            GameSession newGame = new GameSession(gameID, player1_ID, player2_ID, GameType.Tank);
            lock (_gameSessions) { _gameSessions.Add(gameID, newGame); }

            ClientHandler player1_Handler, player2_Handler;
            lock (_clients)
            {
                _clients.TryGetValue(player1_ID, out player1_Handler);
                _clients.TryGetValue(player2_ID, out player2_Handler);
            }

            if (player1_Handler == null || player2_Handler == null)
            {
                lock (_gameSessions) _gameSessions.Remove(gameID);
                return;
            }

            this.TankGameManager.StartGame(gameID, player1_ID, player2_ID);
            player1_Handler.SendPacket(new TankStartPacket { GameID = gameID, OpponentID = player2_Handler.UserID, OpponentName = player2_Handler.UserName, StartsFirst = true });
            player2_Handler.SendPacket(new TankStartPacket { GameID = gameID, OpponentID = player1_Handler.UserID, OpponentName = player1_Handler.UserName, StartsFirst = false });
        }

        public void ProcessTankAction(TankActionPacket action)
        {
            GameSession? session;
            lock (_gameSessions) { _gameSessions.TryGetValue(action.GameID, out session); }
            if (session != null)
            {
                string? opponentID = (session.Player1_ID == action.SenderID) ? session.Player2_ID : session.Player1_ID;
                if (opponentID != null) RelayPrivatePacket(opponentID, action);
            }
        }

        #endregion
    }
}