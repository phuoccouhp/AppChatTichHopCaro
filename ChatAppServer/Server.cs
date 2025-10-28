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
            Console.WriteLine($"Server đang lắng nghe tại port {_port}...");

            while (true)
            {
                try
                {
                    TcpClient clientSocket = await _listener.AcceptTcpClientAsync();
                    Console.WriteLine("[Connect] Một client mới đã kết nối.");

                    // Tạo một handler mới cho client
                    ClientHandler clientHandler = new ClientHandler(clientSocket, this);
                    // Chạy handler này trên một luồng riêng
                    _ = clientHandler.StartHandlingAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] Lỗi khi chấp nhận client: {ex.Message}");
                }
            }
        }

        #region Quản lý Client

        // Đăng ký client (sau khi login thành công)
        public void RegisterClient(string userID, ClientHandler handler)
        {
            lock (_clients)
            {
                _clients[userID] = handler;
            }
        }

        // Hủy đăng ký (khi ngắt kết nối)
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

            Console.WriteLine($"[Disconnect] User '{userID}' đã ngắt kết nối.");
        }

        // Lấy danh sách bạn bè đang online
        public List<UserStatus> GetOnlineUsers(string excludeUserID)
        {
            lock (_clients)
            {
                return _clients.Values
                    .Where(c => c.UserID != excludeUserID)
                    .Select(c => new UserStatus { UserID = c.UserID, UserName = c.UserName, IsOnline = true })
                    .ToList();
            }
        }

        #endregion

        #region Gửi Gói Tin

        // Gửi gói tin cho TẤT CẢ client (TRỪ người gửi)
        public void BroadcastPacket(object packet, string excludeUserID)
        {
            lock (_clients)
            {
                foreach (var client in _clients.Values)
                {
                    if (client.UserID != excludeUserID)
                    {
                        client.SendPacket(packet);
                    }
                }
            }
        }

        // Chuyển tiếp gói tin riêng tư (Chat, File, Mời game)
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
                    Console.WriteLine($"[Warning] Không tìm thấy client '{receiverID}' để chuyển tin.");
                }
            }
        }

        #endregion

        #region Xử lý Logic Game

        // Xử lý khi nhận được phản hồi mời game
        public void ProcessGameResponse(GameResponsePacket response)
        {
            // Nếu bị từ chối
            if (!response.Accepted)
            {
                Console.WriteLine($"[Game] {response.SenderID} từ chối {response.ReceiverID}");
                // Chỉ cần chuyển tiếp gói tin từ chối này cho người mời
                RelayPrivatePacket(response.ReceiverID, response);
                return;
            }

            // Nếu đồng ý
            Console.WriteLine($"[Game] {response.SenderID} đồng ý {response.ReceiverID}. Bắt đầu game!");
            string player1_ID = response.ReceiverID; // Người mời (A)
            string player2_ID = response.SenderID; // Người nhận (B)

            // 1. Tạo Game ID
            string gameID = Guid.NewGuid().ToString("N").Substring(0, 10);

            // 2. Lưu lại ván game
            GameSession newGame = new GameSession(gameID, player1_ID, player2_ID);
            lock (_gameSessions)
            {
                _gameSessions.Add(gameID, newGame);
            }

            // 3. Lấy thông tin Client
            ClientHandler player1_Handler;
            ClientHandler player2_Handler;
            lock (_clients)
            {
                _clients.TryGetValue(player1_ID, out player1_Handler);
                _clients.TryGetValue(player2_ID, out player2_Handler);
            }

            if (player1_Handler == null || player2_Handler == null)
            {
                Console.WriteLine("[Error] Không thể bắt đầu game, 1 trong 2 người đã offline.");
                return;
            }

            // 4. Gửi gói tin GameStart cho cả 2

            // Gói cho Player 1 (người mời, đi trước)
            var startPacket1 = new GameStartPacket
            {
                GameID = gameID,
                OpponentID = player2_Handler.UserID,
                OpponentName = player2_Handler.UserName,
                StartsFirst = true // Người mời đi trước
            };
            player1_Handler.SendPacket(startPacket1);

            // Gói cho Player 2 (người nhận, đi sau)
            var startPacket2 = new GameStartPacket
            {
                GameID = gameID,
                OpponentID = player1_Handler.UserID,
                OpponentName = player1_Handler.UserName,
                StartsFirst = false // Người nhận đi sau
            };
            player2_Handler.SendPacket(startPacket2);
        }

        // Xử lý khi nhận được một nước đi
        public void ProcessGameMove(GameMovePacket move)
        {
            GameSession session;
            lock (_gameSessions)
            {
                _gameSessions.TryGetValue(move.GameID, out session);
            }

            if (session != null)
            {
                // Tìm đối thủ
                string opponentID = session.GetOpponent(move.SenderID);
                if (opponentID != null)
                {
                    // Chuyển tiếp nước đi cho đối thủ
                    RelayPrivatePacket(opponentID, move);
                }
            }
        }

        // TODO: Thêm hàm xử lý khi game kết thúc (dọn dẹp _gameSessions)

        #endregion
    }
}