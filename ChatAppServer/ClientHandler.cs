#pragma warning disable SYSLIB0011
using ChatApp.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace ChatAppServer
{
    public class ClientHandler
    {
        private TcpClient _client;
        private Server _server;
        private NetworkStream _stream;
        private BinaryFormatter _formatter;

        public string UserID { get; private set; }
        public string UserName { get; private set; }

        public ClientHandler(TcpClient client, Server server)
        {
            _client = client;
            _server = server;
            _stream = _client.GetStream();

            // Cảnh báo: BinaryFormatter không an toàn
            _formatter = new BinaryFormatter();
        }

        public async Task StartHandlingAsync()
        {
            try
            {
                // Vòng lặp liên tục đọc dữ liệu từ client
                while (_client.Connected)
                {
                    // Đợi và nhận gói tin
                    object receivedPacket = await Task.Run(() => _formatter.Deserialize(_stream));

                    // Xử lý gói tin
                    HandlePacket(receivedPacket);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Client {UserID ?? "???"}] Đã ngắt kết nối: {ex.Message}");
            }
            finally
            {
                Close();
            }
        }

        // "Bộ định tuyến" - Xử lý các loại gói tin
        // "Bộ định tuyến" - Xử lý các loại gói tin
        private void HandlePacket(object packet)
        {
            switch (packet)
            {
                case LoginPacket p:
                    HandleLogin(p);
                    break;
                case TextPacket p:
                    Console.WriteLine($"[Chat] {p.SenderID} -> {p.ReceiverID}: {p.MessageContent}");
                    _server.RelayPrivatePacket(p.ReceiverID, p);
                    break;
                case FilePacket p:
                    Console.WriteLine($"[File] {p.SenderID} -> {p.ReceiverID}: {p.FileName}");
                    _server.RelayPrivatePacket(p.ReceiverID, p);
                    break;
                case GameInvitePacket p:
                    // THÊM DÒNG NÀY (hoặc kiểm tra xem đã có chưa)
                    Console.WriteLine($"[Game] {p.SenderID} mời {p.ReceiverID}");
                    _server.RelayPrivatePacket(p.ReceiverID, p);
                    break;
                case GameResponsePacket p:
                    // Server sẽ xử lý, không cần log ở đây
                    _server.ProcessGameResponse(p);
                    break;
                case GameMovePacket p:
                    // THÊM DÒNG NÀY
                    Console.WriteLine($"[GameMove] {p.SenderID} đi ({p.Row},{p.Col})");
                    _server.ProcessGameMove(p);
                    break;
                default:
                    Console.WriteLine($"[Warning] Nhận được gói tin không xác định.");
                    break;
            }
        }

        // Xử lý Đăng nhập
        private void HandleLogin(LoginPacket loginPacket)
        {
            // TODO: Bạn phải thay thế bằng logic check DB thật
            // (Tạm thời hard-code 2 user)
            if ((loginPacket.Username == "user1" && loginPacket.Password == "123") ||
                (loginPacket.Username == "user2" && loginPacket.Password == "123"))
            {
                this.UserID = loginPacket.Username;
                this.UserName = (loginPacket.Username == "user1") ? "Bạn Bè A" : "Bạn Bè B"; // Tên giả

                // 1. Đăng ký client này với Server
                _server.RegisterClient(this.UserID, this);

                // 2. Lấy danh sách bạn bè đang online
                var onlineUsers = _server.GetOnlineUsers(this.UserID);

                // 3. Gửi phản hồi thành công + danh sách bạn
                var result = new LoginResultPacket
                {
                    Success = true,
                    UserID = this.UserID,
                    UserName = this.UserName,
                    OnlineUsers = onlineUsers
                };
                SendPacket(result);

                // 4. Thông báo cho tất cả client khác là "tôi" đã online
                var statusPacket = new UserStatusPacket
                {
                    UserID = this.UserID,
                    UserName = this.UserName,
                    IsOnline = true
                };
                _server.BroadcastPacket(statusPacket, this.UserID); // Gửi cho mọi người, TRỪ TÔI

                Console.WriteLine($"[Login] User '{this.UserID}' đã đăng nhập.");
            }
            else
            {
                // Gửi phản hồi thất bại
                SendPacket(new LoginResultPacket { Success = false, Message = "Sai tên đăng nhập hoặc mật khẩu." });
                Console.WriteLine($"[Login] Thất bại: {loginPacket.Username}");
                Close(); // Ngắt kết nối
            }
        }

        // Gửi gói tin cho client này
        public void SendPacket(object packet)
        {
            if (_client.Connected)
            {
                try
                {
                    lock (_stream) // Đảm bảo an toàn luồng khi gửi
                    {
                        _formatter.Serialize(_stream, packet);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] Không thể gửi gói tin cho {UserID}: {ex.Message}");
                }
            }
        }

        // Dọn dẹp
        public void Close()
        {
            _server.RemoveClient(this.UserID);
            _client.Close();
        }
    }
}
#pragma warning restore SYSLIB0011