#pragma warning disable SYSLIB0011 // Tắt cảnh báo BinaryFormatter
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
            _formatter = new BinaryFormatter();
        }

        public async Task StartHandlingAsync()
        {
            try
            {
                // Vòng lặp liên tục đọc dữ liệu từ client
                while (_client.Connected)
                {
                    object receivedPacket = await Task.Run(() => _formatter.Deserialize(_stream));
                    HandlePacket(receivedPacket);
                }
            }
            catch (Exception ex)
            {
                // *** ĐÂY LÀ PHẦN SỬA LỖI ***
                // Khối catch chỉ nên log lỗi ngắt kết nối
                Logger.Warning($"[Client {UserID ?? "???"}] Đã ngắt kết nối: {ex.Message}");
            }
            finally
            {
                Close();
            }
        }

        // "Bộ định tuyến" - Xử lý các loại gói tin
        private void HandlePacket(object packet)
        {
            switch (packet)
            {
                case LoginPacket p:
                    HandleLogin(p);
                    break;
                case TextPacket p:
                    Logger.Info($"[Chat] {p.SenderID} -> {p.ReceiverID}: {p.MessageContent}");
                    _server.RelayPrivatePacket(p.ReceiverID, p);
                    break;
                case FilePacket p:
                    Logger.Info($"[File] {p.SenderID} -> {p.ReceiverID}: {p.FileName}");
                    _server.RelayPrivatePacket(p.ReceiverID, p);
                    break;
                case GameInvitePacket p:
                    Logger.Info($"[Game] {p.SenderID} mời {p.ReceiverID}");
                    _server.RelayPrivatePacket(p.ReceiverID, p);
                    break;
                case GameResponsePacket p:
                    // Server sẽ xử lý, không cần log ở đây
                    _server.ProcessGameResponse(p);
                    break;
                case GameMovePacket p:
                    Logger.Info($"[GameMove] {p.SenderID} đi ({p.Row},{p.Col})");
                    _server.ProcessGameMove(p);
                    break;
                default:
                    Logger.Warning($"[Warning] Nhận được gói tin không xác định.");
                    break;
            }
        }

        // Xử lý Đăng nhập
        private void HandleLogin(LoginPacket p)
        {
            // TODO: Bạn phải thay thế bằng logic check DB thật
            if ((p.Username == "user1" && p.Password == "123") ||
                (p.Username == "user2" && p.Password == "123"))
            {
                this.UserID = p.Username;
                this.UserName = (p.Username == "user1") ? "Bạn Bè A" : "Bạn Bè B";

                _server.RegisterClient(this.UserID, this);
                var onlineUsers = _server.GetOnlineUsers(this.UserID);

                var result = new LoginResultPacket
                {
                    Success = true,
                    UserID = this.UserID,
                    UserName = this.UserName,
                    OnlineUsers = onlineUsers
                };
                SendPacket(result);

                var statusPacket = new UserStatusPacket
                {
                    UserID = this.UserID,
                    UserName = this.UserName,
                    IsOnline = true
                };
                _server.BroadcastPacket(statusPacket, this.UserID);

                Logger.Success($"[Login] User '{this.UserID}' đã đăng nhập.");
            }
            else
            {
                SendPacket(new LoginResultPacket { Success = false, Message = "Sai tên đăng nhập hoặc mật khẩu." });
                Logger.Warning($"[Login] Thất bại: {p.Username}");
                Close();
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
                    // Sửa lỗi: Dùng Logger
                    Logger.Error($"Không thể gửi gói tin cho {UserID}", ex);
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
#pragma warning restore SYSLIB0011 // Bật lại cảnh báo