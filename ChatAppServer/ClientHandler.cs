#pragma warning disable SYSLIB0011 // Tắt cảnh báo BinaryFormatter
using ChatApp.Shared;
using System;
using System.IO;
using System.Net;
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

        public string ClientIP
        {
            get
            {
                try { return (_client.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString(); }
                catch { return "Unknown"; }
            }
        }
        public DateTime LoginTime { get; private set; } = DateTime.Now;

        // --- Biến cho chức năng Quên mật khẩu ---
        private string _currentOtp = null;
        private string _currentResetEmail = null;

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
                case RegisterPacket p: HandleRegister(p); break;

                // --- CÁC GÓI TIN QUÊN MẬT KHẨU ---
                case ForgotPasswordPacket p: HandleForgotPasswordRequest(p); break;
                case ResetPasswordPacket p: HandleResetPassword(p); break;

                case UpdateProfilePacket p: HandleUpdateProfile(p); break;

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

                case TankInvitePacket p:
                    Logger.Info($"[Tank Game] {p.SenderID} mời {p.ReceiverID}");
                    _server.RelayPrivatePacket(p.ReceiverID, p);
                    break;
                case TankResponsePacket p: _server.ProcessTankResponse(p); break;
                case TankActionPacket p:
                    Logger.Info($"[Tank Action] {p.SenderID} - {p.ActionType}");
                    _server.ProcessTankAction(p);
                    if (p.ActionType == TankActionType.Shoot)
                    {
                        _server.TankGameManager.AddBullet(p.GameID, p.SenderID, p.X, p.Y, p.Angle);
                    }
                    break;
                case TankHitPacket p:
                    // Client gửi hit packet khi phát hiện va chạm
                    _server.TankGameManager.ProcessHit(p.GameID, p.HitPlayerID, p.Damage, _server);
                    break;

                default: Logger.Warning($"Packet lạ: {packet.GetType().Name}"); break;
            }
        }

        // --- XỬ LÝ QUÊN MẬT KHẨU ---
        private void HandleForgotPasswordRequest(ForgotPasswordPacket p)
        {
            // 1. Kiểm tra email trong DB
            if (!DatabaseManager.Instance.CheckEmailExists(p.Email))
            {
                SendPacket(new ForgotPasswordResultPacket { Success = false, Message = "Email không tồn tại trong hệ thống." });
                return;
            }

            // 2. Tạo OTP ngẫu nhiên (6 số)
            Random r = new Random();
            _currentOtp = r.Next(100000, 999999).ToString();
            _currentResetEmail = p.Email;

            // 3. Gửi Mail
            // Lưu ý: Cần đảm bảo bạn đã tạo class EmailHelper và cấu hình App Password
            bool mailSent = EmailHelper.SendOTP(p.Email, _currentOtp);

            if (mailSent)
            {
                SendPacket(new ForgotPasswordResultPacket { Success = true, IsStep1Success = true, Message = "Đã gửi mã OTP. Vui lòng kiểm tra email." });
                Logger.Info($"[Forgot Pass] Đã gửi OTP đến {p.Email}");
            }
            else
            {
                SendPacket(new ForgotPasswordResultPacket { Success = false, Message = "Lỗi gửi email. Vui lòng thử lại sau." });
            }
        }

        private void HandleResetPassword(ResetPasswordPacket p)
        {
            // Kiểm tra OTP
            if (_currentOtp != null && _currentResetEmail == p.Email && _currentOtp == p.OtpCode)
            {
                // Đúng OTP -> Đổi pass trong DB
                DatabaseManager.Instance.UpdatePassword(p.Email, p.NewPassword);

                SendPacket(new ForgotPasswordResultPacket { Success = true, IsStep1Success = false, Message = "Đổi mật khẩu thành công! Hãy đăng nhập lại." });

                // Xóa OTP cũ
                _currentOtp = null;
                _currentResetEmail = null;
                Logger.Success($"[Reset Pass] Thành công cho {p.Email}");
            }
            else
            {
                SendPacket(new ForgotPasswordResultPacket { Success = false, Message = "Mã OTP không đúng hoặc đã hết hạn." });
            }
        }

        // --- CÁC HÀM XỬ LÝ KHÁC ---

        private void HandleRegister(RegisterPacket p)
        {
            bool success = DatabaseManager.Instance.RegisterUser(p.Username, p.Password, p.Email);
            var result = new RegisterResultPacket
            {
                Success = success,
                Message = success ? "Đăng ký thành công!" : "Tên đăng nhập đã tồn tại hoặc lỗi hệ thống."
            };
            SendPacket(result);
            Logger.Info($"[Register] User '{p.Username}' đăng ký: {(success ? "Thành công" : "Thất bại")}");
        }

        private void HandleUpdateProfile(UpdateProfilePacket p)
        {
            DatabaseManager.Instance.UpdateDisplayName(p.UserID, p.NewDisplayName);
            this.UserName = p.NewDisplayName;
            Logger.Info($"[Profile] {p.UserID} đổi tên thành '{p.NewDisplayName}' {(p.HasNewAvatar ? "& đổi Avatar" : "")}");
            _server.BroadcastPacket(p, null);
        }

        private void HandleLogin(LoginPacket p)
        {
            var user = DatabaseManager.Instance.Login(p.Username, p.Password);
            if (user != null)
            {
                this.UserID = user.Username;
                this.UserName = user.DisplayName;
                this.LoginTime = DateTime.Now;

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
                var result = new LoginResultPacket { Success = false, Message = "Sai thông tin đăng nhập." };
                SendPacket(result);
                // Đảm bảo flush stream trước khi đóng để client nhận được phản hồi
                try
                {
                    _stream?.Flush();
                    // Đợi một chút để đảm bảo packet được gửi đi
                    System.Threading.Thread.Sleep(100);
                }
                catch { }
                Logger.Warning($"[Login Fail] {p.Username} từ IP {this.ClientIP}");
                Close();
            }
        }

        public void SendPacket(object packet)
        {
            if (_client.Connected && _stream != null)
            {
                try 
                { 
                    lock (_stream) 
                    { 
                        _formatter.Serialize(_stream, packet);
                        _stream.Flush(); // Đảm bảo dữ liệu được gửi ngay lập tức
                    } 
                }
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