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

        // Dùng Dictionary để quản lý client (Key = UserID, Value = ClientHandler)
        private readonly Dictionary<string, ClientHandler> _clients = new Dictionary<string, ClientHandler>();
        // Quản lý các ván game đang diễn ra (Key = GameID)
        private readonly Dictionary<string, GameSession> _gameSessions = new Dictionary<string, GameSession>();

        // Sự kiện để báo cho Giao diện (Form) biết khi danh sách user thay đổi
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
                // Logger.Success được gọi ở Program.cs hoặc Form, không cần gọi ở đây để tránh lặp
            }
            catch (Exception ex)
            {
                Logger.Error($"Không thể khởi động listener trên port {_port}", ex);
                return;
            }

            while (true)
            {
                try
                {
                    TcpClient clientSocket = await _listener.AcceptTcpClientAsync();
                    Logger.Info($"[Connect] Client mới từ {(clientSocket.Client.RemoteEndPoint as IPEndPoint)?.Address} đã kết nối.");

                    ClientHandler clientHandler = new ClientHandler(clientSocket, this);
                    // Chạy handler này trên một luồng riêng để không chặn vòng lặp chính
                    _ = clientHandler.StartHandlingAsync();
                }
                catch (ObjectDisposedException)
                {
                    Logger.Warning("Listener đã dừng.");
                    break;
                }
                catch (Exception ex)
                {
                    Logger.Error($"Lỗi khi chấp nhận client", ex);
                    await Task.Delay(1000); // Đợi 1 chút trước khi thử lại
                }
            }
        }

        #region Quản lý Client

        public void RegisterClient(string userID, ClientHandler handler)
        {
            lock (_clients)
            {
                _clients[userID] = handler;
            }
            NotifyUserListChanged(); // Cập nhật giao diện Server
        }

        public void RemoveClient(string userID)
        {
            if (userID == null) return;

            lock (_clients)
            {
                _clients.Remove(userID);
            }

            // Thông báo cho mọi người là user này đã offline
            var statusPacket = new UserStatusPacket
            {
                UserID = userID,
                IsOnline = false
            };
            BroadcastPacket(statusPacket, null); // Gửi cho TẤT CẢ

            Logger.Warning($"[Disconnect] User '{userID}' đã ngắt kết nối.");
            NotifyUserListChanged(); // Cập nhật giao diện Server
        }

        public List<UserStatus> GetOnlineUsers(string excludeUserID)
        {
            lock (_clients)
            {
                return _clients.Values
                    .Where(c => c.UserID != null && c.UserID != excludeUserID)
                    .Select(c => new UserStatus { UserID = c.UserID, UserName = c.UserName, IsOnline = true })
                    .ToList();
            }
        }

        // Hàm cập nhật danh sách user lên giao diện Server
        private void NotifyUserListChanged()
        {
            lock (_clients)
            {
                var userList = _clients.Values.Select(c => $"{c.UserName} ({c.UserID})").ToList();
                OnUserListChanged?.Invoke(userList);
            }
        }

        #endregion

        #region Gửi Gói Tin

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

        #endregion

        #region Xử lý Logic Game

        public void ProcessGameResponse(GameResponsePacket response)
        {
            if (!response.Accepted)
            {
                Logger.Warning($"[Game] {response.SenderID} từ chối {response.ReceiverID}");
                RelayPrivatePacket(response.ReceiverID, response);
                return;
            }

            Logger.Success($"[Game] {response.SenderID} đồng ý {response.ReceiverID}. Bắt đầu game!");
            string player1_ID = response.ReceiverID; // Người mời
            string player2_ID = response.SenderID;   // Người nhận

            string gameID = Guid.NewGuid().ToString("N").Substring(0, 10);

            GameSession newGame = new GameSession(gameID, player1_ID, player2_ID);
            lock (_gameSessions)
            {
                _gameSessions.Add(gameID, newGame);
            }

            ClientHandler player1_Handler;
            ClientHandler player2_Handler;
            lock (_clients)
            {
                _clients.TryGetValue(player1_ID, out player1_Handler);
                _clients.TryGetValue(player2_ID, out player2_Handler);
            }

            if (player1_Handler == null || player2_Handler == null)
            {
                Logger.Error("[Error] Không thể bắt đầu game, 1 trong 2 người đã offline.");
                lock (_gameSessions) _gameSessions.Remove(gameID);
                return;
            }

            // Gửi gói tin bắt đầu
            var startPacket1 = new GameStartPacket
            {
                GameID = gameID,
                OpponentID = player2_Handler.UserID,
                OpponentName = player2_Handler.UserName,
                StartsFirst = true
            };
            player1_Handler.SendPacket(startPacket1);

            var startPacket2 = new GameStartPacket
            {
                GameID = gameID,
                OpponentID = player1_Handler.UserID,
                OpponentName = player1_Handler.UserName,
                StartsFirst = false
            };
            player2_Handler.SendPacket(startPacket2);
        }

        public void ProcessGameMove(GameMovePacket move)
        {
            GameSession session;
            lock (_gameSessions)
            {
                _gameSessions.TryGetValue(move.GameID, out session);
            }

            if (session != null)
            {
                string opponentID = session.GetOpponent(move.SenderID);
                if (opponentID != null)
                {
                    RelayPrivatePacket(opponentID, move);
                }
                else
                {
                    Logger.Warning($"[GameMove] Không tìm thấy đối thủ cho {move.SenderID} trong game {move.GameID}");
                }
            }
            else
            {
                Logger.Warning($"[GameMove] Nhận được nước đi cho GameID không tồn tại: {move.GameID}");
            }
        }

        // --- LOGIC CHƠI LẠI (REMATCH) ---

        public void ProcessRematchRequest(RematchRequestPacket request)
        {
            GameSession session;
            lock (_gameSessions) _gameSessions.TryGetValue(request.GameID, out session);

            if (session != null)
            {
                string opponentID = session.GetOpponent(request.SenderID);
                if (opponentID != null)
                {
                    RelayPrivatePacket(opponentID, request);
                }
                else Logger.Warning($"[Rematch] Không tìm thấy đối thủ cho {request.SenderID} trong game {request.GameID}");
            }
            else Logger.Warning($"[Rematch] Yêu cầu cho GameID không tồn tại: {request.GameID}");
        }

        public void ProcessRematchResponse(RematchResponsePacket response)
        {
            if (!response.Accepted)
            {
                Logger.Warning($"[Rematch] {response.SenderID} từ chối chơi lại game {response.GameID}");
                RelayPrivatePacket(response.ReceiverID, response);
                return;
            }

            Logger.Success($"[Rematch] {response.SenderID} đồng ý chơi lại game {response.GameID}. Reset game!");
            GameSession session;
            lock (_gameSessions) _gameSessions.TryGetValue(response.GameID, out session);

            if (session != null)
            {
                ClientHandler player1_Handler, player2_Handler;
                lock (_clients)
                {
                    _clients.TryGetValue(session.Player1_ID, out player1_Handler);
                    _clients.TryGetValue(session.Player2_ID, out player2_Handler);
                }

                if (player1_Handler != null && player2_Handler != null)
                {
                    // Người đồng ý (response.SenderID) sẽ đi trước trong ván mới
                    bool player1Starts = (session.Player1_ID == response.SenderID);

                    var resetPacket1 = new GameResetPacket { GameID = response.GameID, StartsFirst = player1Starts };
                    player1_Handler.SendPacket(resetPacket1);

                    var resetPacket2 = new GameResetPacket { GameID = response.GameID, StartsFirst = !player1Starts };
                    player2_Handler.SendPacket(resetPacket2);
                }
                else Logger.Error($"[Rematch] Không tìm thấy client handler khi reset game {response.GameID}");
            }
            else Logger.Warning($"[Rematch] Phản hồi cho GameID không tồn tại: {response.GameID}");
        }

        #endregion
    }
}