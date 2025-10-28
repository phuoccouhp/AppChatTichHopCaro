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

        public Server(int port)
        {
            _port = port;
            _listener = new TcpListener(IPAddress.Any, _port);
        }

        public async Task StartAsync()
        {
            _listener.Start();
            // Sử dụng Logger thay vì Console.WriteLine
            // Logger.Success($"Server đang lắng nghe tại port {_port}..."); // Dòng này nên ở Program.cs

            while (true)
            {
                try
                {
                    TcpClient clientSocket = await _listener.AcceptTcpClientAsync();
                    // Sử dụng Logger
                    Logger.Info("[Connect] Một client mới đã kết nối.");

                    ClientHandler clientHandler = new ClientHandler(clientSocket, this);
                    _ = clientHandler.StartHandlingAsync();
                }
                catch (Exception ex)
                {
                    Logger.Error($"Lỗi khi chấp nhận client", ex);
                }
                // *** ĐÃ XÓA CÁC DÒNG LOGGER BỊ LẠC KHỎI ĐÂY ***
            }
        }

        #region Quản lý Client

        public void RegisterClient(string userID, ClientHandler handler)
        {
            lock (_clients)
            {
                _clients[userID] = handler;
            }
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
                // UserName không cần thiết khi offline
            };
            BroadcastPacket(statusPacket, null); // Gửi cho TẤT CẢ

            // Log đúng chỗ
            Logger.Warning($"[Disconnect] User '{userID}' đã ngắt kết nối.");
        }

        public List<UserStatus> GetOnlineUsers(string excludeUserID)
        {
            lock (_clients)
            {
                return _clients.Values
                    .Where(c => c.UserID != null && c.UserID != excludeUserID) // Thêm kiểm tra null
                    .Select(c => new UserStatus { UserID = c.UserID, UserName = c.UserName, IsOnline = true })
                    .ToList();
            }
        }

        #endregion

        #region Gửi Gói Tin

        public void BroadcastPacket(object packet, string excludeUserID)
        {
            lock (_clients)
            {
                // Dùng ToList() để tránh lỗi thay đổi collection khi duyệt
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
                    // Log đúng chỗ
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
                // Log đúng chỗ
                Logger.Warning($"[Game] {response.SenderID} từ chối {response.ReceiverID}");
                RelayPrivatePacket(response.ReceiverID, response);
                return;
            }

            // Log đúng chỗ
            Logger.Success($"[Game] {response.SenderID} đồng ý {response.ReceiverID}. Bắt đầu game!");
            string player1_ID = response.ReceiverID;
            string player2_ID = response.SenderID;

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
                // TODO: Nên xóa GameSession vừa tạo
                return;
            }

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

        // TODO: Thêm hàm xử lý khi game kết thúc (dọn dẹp _gameSessions)

        #endregion
    }
}