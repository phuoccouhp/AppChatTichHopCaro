#pragma warning disable SYSLIB0011
using ChatApp.Shared;
using System;
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
            try
            {
                _stream = _client.GetStream();
                _formatter = new BinaryFormatter();
            }
            catch (Exception ex) { Logger.Error("Không thể lấy NetworkStream", ex); Close(); }
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
            catch (IOException ioEx) { Logger.Warning($"[Client {UserID ?? "???"}] Lỗi I/O (mất kết nối?): {ioEx.Message}"); }
            catch (SerializationException serEx) { Logger.Error($"[Client {UserID ?? "???"}] Lỗi Deserialize", serEx); }
            catch (Exception ex) { Logger.Error($"[Client {UserID ?? "???"}] Lỗi không xác định", ex); }
            finally { Logger.Warning($"[Client {UserID ?? "???"}] Dừng lắng nghe."); Close(); }
        }

        private void HandlePacket(object packet)
        {
            switch (packet)
            {
                case LoginPacket p: HandleLogin(p); break;
                case TextPacket p: Logger.Info($"[Chat] {p.SenderID}->{p.ReceiverID}"); _server.RelayPrivatePacket(p.ReceiverID, p); break;
                case FilePacket p: Logger.Info($"[File] {p.SenderID}->{p.ReceiverID}"); _server.RelayPrivatePacket(p.ReceiverID, p); break;
                case GameInvitePacket p: Logger.Info($"[Game] {p.SenderID} mời {p.ReceiverID}"); _server.RelayPrivatePacket(p.ReceiverID, p); break;
                case GameResponsePacket p: _server.ProcessGameResponse(p); break;
                case GameMovePacket p: Logger.Info($"[Move] {p.SenderID} đi {p.Row},{p.Col}"); _server.ProcessGameMove(p); break; 
                default: Logger.Warning($"[Warning] Gói tin không xác định: {packet.GetType().Name}"); break;
            }
        }

        private void HandleLogin(LoginPacket p)
        {
            if ((p.Username == "user1" && p.Password == "123") || (p.Username == "user2" && p.Password == "123"))
            {
                this.UserID = p.Username;
                this.UserName = (p.Username == "user1") ? "Bạn Bè A" : "Bạn Bè B"; 
                _server.RegisterClient(this.UserID, this);
                var onlineUsers = _server.GetOnlineUsers(this.UserID);
                var result = new LoginResultPacket { Success = true, UserID = this.UserID, UserName = this.UserName, OnlineUsers = onlineUsers };
                SendPacket(result);
                var statusPacket = new UserStatusPacket { UserID = this.UserID, UserName = this.UserName, IsOnline = true };
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

        public void SendPacket(object packet)
        {
            if (_client.Connected && _stream != null)
            {
                try { lock (_stream) { _formatter.Serialize(_stream, packet); } }
                catch (Exception ex) { Logger.Error($"Không thể gửi gói tin cho {UserID}", ex); Close(); }
            }
        }

        public void Close()
        {
            if (this.UserID != null)
            {
                _server.RemoveClient(this.UserID);
            }
            _stream?.Close(); 
            _client?.Close(); 
        }
    }
}
#pragma warning restore SYSLIB0011