#pragma warning disable SYSLIB0011 // Tắt cảnh báo BinaryFormatter
using ChatApp.Shared;
using System;
using System.IO;
using System.Net; // Cần thiết cho IPEndPoint
using System.Net.Sockets;
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

        // MỚI: Lấy IP để Admin xem
        public string ClientIP
        {
            get
            {
                try { return (_client.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString(); }
                catch { return "Unknown"; }
            }
        }

        // MỚI: Thời gian đăng nhập
        public DateTime LoginTime { get; private set; } = DateTime.Now;

        public ClientHandler(TcpClient client, Server server)
        {
            _client = client;
            _server = server;
            try
            {
                _stream = _client.GetStream();
                _formatter = new BinaryFormatter();
            }
            catch (Exception ex)
            {
                Logger.Error("Không thể khởi tạo ClientHandler", ex);
                Close();
            }
        }

        public async Task StartHandlingAsync()
        {
            if (_stream == null) return;
            try
            {
                while (_client.Connected && _stream != null)
                {
                    object receivedPacket = await Task.Run(() => _formatter.Deserialize(_stream));
                    HandlePacket(receivedPacket);
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"[Client {UserID ?? "???"}] Đã ngắt kết nối: {ex.Message}");
            }
            finally
            {
                Close();
            }
        }

        private void HandlePacket(object packet)
        {
            switch (packet)
            {
                case LoginPacket p: HandleLogin(p); break;
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
                case GameResponsePacket p: _server.ProcessGameResponse(p); break;
                case GameMovePacket p:
                    Logger.Info($"[Move] {p.SenderID} đi {p.Row},{p.Col}");
                    _server.ProcessGameMove(p);
                    break;
                case RematchRequestPacket p:
                    Logger.Info($"[Rematch] {p.SenderID} yêu cầu chơi lại.");
                    _server.ProcessRematchRequest(p);
                    break;
                case RematchResponsePacket p:
                    Logger.Info($"[Rematch] {p.SenderID} phản hồi chơi lại.");
                    _server.ProcessRematchResponse(p);
                    break;
                default: Logger.Warning($"Packet lạ: {packet.GetType().Name}"); break;
            }
        }

        private void HandleLogin(LoginPacket p)
        {
            // Kiểm tra DB
            var user = DatabaseManager.Instance.Login(p.Username, p.Password);

            if (user != null)
            {
                this.UserID = user.Username;
                this.UserName = user.DisplayName;
                this.LoginTime = DateTime.Now; // Lưu thời gian

                _server.RegisterClient(this.UserID, this);
                var onlineUsers = _server.GetOnlineUsers(this.UserID);

                var result = new LoginResultPacket { Success = true, UserID = this.UserID, UserName = this.UserName, OnlineUsers = onlineUsers };
                SendPacket(result);

                var statusPacket = new UserStatusPacket { UserID = this.UserID, UserName = this.UserName, IsOnline = true };
                _server.BroadcastPacket(statusPacket, this.UserID);

                Logger.Success($"[Login] {this.UserID} ({this.ClientIP}) đã đăng nhập.");
            }
            else
            {
                SendPacket(new LoginResultPacket { Success = false, Message = "Sai thông tin đăng nhập." });
                Logger.Warning($"[Login Fail] {p.Username} từ IP {this.ClientIP}");
                Close();
            }
        }

        public void SendPacket(object packet)
        {
            if (_client.Connected && _stream != null)
            {
                try { lock (_stream) { _formatter.Serialize(_stream, packet); } }
                catch (Exception ex) { Logger.Error($"Gửi thất bại cho {UserID}", ex); Close(); }
            }
        }

        public void Close()
        {
            if (UserID != null) _server.RemoveClient(this.UserID);
            _stream?.Close();
            _client?.Close();
        }
    }
}
#pragma warning restore SYSLIB0011