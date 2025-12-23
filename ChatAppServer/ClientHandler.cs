#pragma warning disable SYSLIB0011 // Tắt cảnh báo BinaryFormatter
using ChatApp.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace ChatAppServer
{
    public class ClientHandler
    {
        private TcpClient? _client;
        private Server _server;
        private NetworkStream? _stream;
        private BinaryFormatter _formatter;

        public string? UserID { get; private set; }
        public string? UserName { get; private set; }

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
        private string? _currentOtp = null;
        private string? _currentResetEmail = null;

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
                while (_client != null && _client.Connected && _stream != null && _stream.CanRead)
                {
                    try
                    {
                        // Kiểm tra connection trước khi deserialize
                        if (!_client.Connected || _stream == null || !_stream.CanRead)
                        {
                            Logger.Info($"[Client {UserID ?? "???"}] Connection đã bị đóng trước khi deserialize");
                            break;
                        }

                        // Deserialize packet với error handling toàn diện
                        object? receivedPacket = null;
                        try
                        {
                            receivedPacket = await Task.Run(() =>
                            {
                                try
                                {
                                    return _formatter.Deserialize(_stream);
                                }
                                catch (System.Runtime.Serialization.SerializationException ex)
                                {
                                    // Wrap lại exception để có thể nhận biết ở ngoài
                                    throw new AggregateException("Deserialize failed", ex);
                                }
                                catch (Exception ex)
                                {
                                    // Wrap tất cả exception khác
                                    throw new AggregateException("Unexpected error in Deserialize", ex);
                                }
                            });
                        }
                        catch (AggregateException aggEx)
                        {
                            // Unwrap AggregateException
                            var innerEx = aggEx.GetBaseException();
                            if (innerEx is System.Runtime.Serialization.SerializationException serEx)
                            {
                                // Xử lý SerializationException - KHÔNG throw lại
                                if (serEx.Message.Contains("End of Stream") || 
                                    serEx.Message.Contains("parsing was completed"))
                                {
                                    Logger.Info($"[Client {UserID ?? "???"}] Client đã đóng kết nối (End of Stream - from Task.Run)");
                                }
                                else
                                {
                                    Logger.Warning($"[Client {UserID ?? "???"}] Serialization error (from Task.Run): {serEx.Message}");
                                }
                                break; // Break để đóng connection
                            }
                            // Nếu không phải SerializationException, re-throw để được catch ở catch block bên ngoài
                            throw innerEx ?? aggEx;
                        }
                        catch (System.Runtime.Serialization.SerializationException serEx)
                        {
                            // Catch trực tiếp nếu không qua AggregateException
                            if (serEx.Message.Contains("End of Stream") || 
                                serEx.Message.Contains("parsing was completed"))
                            {
                                Logger.Info($"[Client {UserID ?? "???"}] Client đã đóng kết nối (End of Stream - direct catch)");
                            }
                            else
                            {
                                Logger.Warning($"[Client {UserID ?? "???"}] Serialization error (direct catch): {serEx.Message}");
                            }
                            break; // Break để đóng connection
                        }
                        
                        if (receivedPacket != null)
                        {
                            HandlePacket(receivedPacket);
                        }
                    }
                    catch (System.Runtime.Serialization.SerializationException serEx)
                    {
                        // Lỗi serialization thường xảy ra khi stream bị đóng hoặc dữ liệu không đầy đủ
                        // Luôn xử lý gracefully, không throw lại
                        if (serEx.Message.Contains("End of Stream") || 
                            serEx.Message.Contains("parsing was completed"))
                        {
                            Logger.Info($"[Client {UserID ?? "???"}] Client đã đóng kết nối (End of Stream)");
                        }
                        else
                        {
                            Logger.Warning($"[Client {UserID ?? "???"}] Serialization error: {serEx.Message}");
                        }
                        break; // Luôn break để đóng connection gracefully
                    }
                    catch (IOException ioEx)
                    {
                        // IOException xảy ra khi connection bị đóng
                        Logger.Info($"[Client {UserID ?? "???"}] Connection đã bị đóng: {ioEx.Message}");
                        break;
                    }
                    catch (System.Net.Sockets.SocketException sockEx)
                    {
                        // SocketException xảy ra khi network có vấn đề
                        Logger.Warning($"[Client {UserID ?? "???"}] Socket error: {sockEx.Message}");
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        // Stream đã bị dispose, connection đã đóng
                        Logger.Info($"[Client {UserID ?? "???"}] Stream đã bị dispose");
                        break;
                    }
                }
            }
            catch (System.Runtime.Serialization.SerializationException serEx)
            {
                // Đảm bảo SerializationException LUÔN được handle ở đây - KHÔNG BAO GIỜ throw ra ngoài
                if (serEx.Message.Contains("End of Stream") || 
                    serEx.Message.Contains("parsing was completed"))
                {
                    Logger.Info($"[Client {UserID ?? "???"}] Client đã đóng kết nối (End of Stream - outer catch)");
                }
                else
                {
                    Logger.Warning($"[Client {UserID ?? "???"}] SerializationException (outer catch): {serEx.Message}");
                }
                // KHÔNG throw lại - chỉ log và tiếp tục đến finally block
            }
            catch (AggregateException aggEx)
            {
                // Kiểm tra xem có SerializationException bên trong không
                var baseEx = aggEx.GetBaseException();
                if (baseEx is System.Runtime.Serialization.SerializationException serEx)
                {
                    // Đã được handle, chỉ log
                    if (serEx.Message.Contains("End of Stream") || 
                        serEx.Message.Contains("parsing was completed"))
                    {
                        Logger.Info($"[Client {UserID ?? "???"}] Client đã đóng kết nối (End of Stream - AggregateException)");
                    }
                    else
                    {
                        Logger.Warning($"[Client {UserID ?? "???"}] SerializationException (AggregateException): {serEx.Message}");
                    }
                }
                else
                {
                    // Nếu không phải SerializationException, log warning
                    Logger.Warning($"[Client {UserID ?? "???"}] AggregateException: {aggEx.GetType().Name} - {aggEx.Message}");
                    foreach (var innerEx in aggEx.InnerExceptions)
                    {
                        Logger.Warning($"  Inner: {innerEx.GetType().Name} - {innerEx.Message}");
                    }
                }
                // KHÔNG throw lại
            }
            catch (Exception ex)
            {
                // Chỉ log warning cho các exception không mong đợi
                // KHÔNG BAO GIỜ throw lại để tránh unhandled exception
                if (!(ex is IOException) &&
                    !(ex is System.Net.Sockets.SocketException) &&
                    !(ex is ObjectDisposedException) &&
                    !(ex is System.Runtime.Serialization.SerializationException))
                {
                    Logger.Warning($"[Client {UserID ?? "???"}] Lỗi không mong đợi: {ex.GetType().Name} - {ex.Message}");
                }
                // KHÔNG throw lại - chỉ log
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

                case ChatHistoryRequestPacket p: HandleChatHistoryRequest(p); break;

                case TextPacket p:
                    Logger.Info($"[Chat] {p.SenderID} -> {p.ReceiverID}: {p.MessageContent}");
                    // Lưu tin nhắn vào database
                    DatabaseManager.Instance.SaveMessage(p.SenderID, p.ReceiverID, p.MessageContent, "Text", null);
                    _server.RelayPrivatePacket(p.ReceiverID, p);
                    break;
                case FilePacket p:
                    Logger.Info($"[File] {p.SenderID} -> {p.ReceiverID}: {p.FileName}");
                    // Lưu file vào database (lưu tên file, không lưu dữ liệu file)
                    string fileMessage = p.IsImage ? $"Đã gửi ảnh: {p.FileName}" : $"Đã gửi file: {p.FileName}";
                    DatabaseManager.Instance.SaveMessage(p.SenderID, p.ReceiverID, fileMessage, p.IsImage ? "Image" : "File", p.FileName);
                    _server.RelayPrivatePacket(p.ReceiverID, p);
                    break;
                case GameInvitePacket p:
                    Logger.Info($"[Game] {p.SenderID} mời {p.ReceiverID}");
                    // Lưu game invite vào database
                    string inviteMessage = $"{p.SenderName} mời bạn chơi Caro";
                    int messageId = DatabaseManager.Instance.SaveMessage(p.SenderID, p.ReceiverID, inviteMessage, "GameInvite", "Caro");
                    // Lưu MessageID vào một dictionary để có thể cập nhật sau
                    _server.StoreGameInviteMessageId(p.SenderID, p.ReceiverID, messageId);
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
                    // Lưu tank invite vào database
                    string tankInviteMessage = $"{p.SenderName} mời bạn chơi Tank Game";
                    int tankMessageId = DatabaseManager.Instance.SaveMessage(p.SenderID, p.ReceiverID, tankInviteMessage, "GameInvite", "Tank");
                    // Lưu MessageID vào một dictionary để có thể cập nhật sau
                    _server.StoreGameInviteMessageId(p.SenderID, p.ReceiverID, tankMessageId);
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

        private void HandleChatHistoryRequest(ChatHistoryRequestPacket p)
        {
            try
            {
                // Lấy lịch sử chat từ database
                var messages = DatabaseManager.Instance.GetChatHistory(p.UserID, p.FriendID, p.Limit);
                
                // Convert sang ChatHistoryMessage
                var response = new ChatHistoryResponsePacket
                {
                    Success = true,
                    Message = "Lấy lịch sử chat thành công",
                    Messages = messages.Select(m => new ChatHistoryMessage
                    {
                        MessageID = m.MessageID,
                        SenderID = m.SenderID ?? "",
                        ReceiverID = m.ReceiverID ?? "",
                        MessageContent = m.MessageContent ?? "",
                        MessageType = m.MessageType,
                        FileName = m.FileName,
                        CreatedAt = m.CreatedAt
                    }).ToList()
                };
                
                SendPacket(response);
                Logger.Info($"[Chat History] {p.UserID} đã lấy lịch sử chat với {p.FriendID} ({messages.Count} tin nhắn)");
            }
            catch (Exception ex)
            {
                Logger.Error("Lỗi lấy lịch sử chat", ex);
                SendPacket(new ChatHistoryResponsePacket
                {
                    Success = false,
                    Message = "Lỗi khi lấy lịch sử chat: " + ex.Message,
                    Messages = new List<ChatHistoryMessage>()
                });
            }
        }

        private void HandleLogin(LoginPacket p)
        {
            // Hỗ trợ đăng nhập bằng username hoặc email
            string loginValue = p.UseEmailLogin ? (p.Email ?? "") : (p.Username ?? "");
            var user = DatabaseManager.Instance.Login(loginValue, p.Password, p.UseEmailLogin);
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
            if (_client != null && _client.Connected && _stream != null)
            {
                try 
                { 
                    lock (_stream) 
                    { 
                        Logger.Info($"[ClientHandler] Sending packet {packet.GetType().Name} to {(UserID ?? "unknown")} ");
                        _formatter.Serialize(_stream, packet);
                        _stream.Flush(); // Đảm bảo dữ liệu được gửi ngay lập tức
                        Logger.Info($"[ClientHandler] Sent packet {packet.GetType().Name} to {(UserID ?? "unknown")} ");
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