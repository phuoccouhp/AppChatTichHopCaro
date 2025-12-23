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
                try 
                { 
                    if (_client == null)
                        return "Unknown";
                    
                    var socket = _client.Client;
                    if (socket == null)
                        return "Unknown";
                    
                    var remoteEndPoint = socket.RemoteEndPoint as IPEndPoint;
                    if (remoteEndPoint == null)
                        return "Unknown";
                    
                    return remoteEndPoint.Address?.ToString() ?? "Unknown";
                }
                catch (ObjectDisposedException)
                {
                    // Client đã bị dispose
                    return "Unknown";
                }
                catch (NullReferenceException)
                {
                    // Client hoặc Client.Client đã null
                    return "Unknown";
                }
                catch (InvalidOperationException)
                {
                    // Client đã bị đóng hoặc không connected
                    return "Unknown";
                }
                catch
                {
                    // Bất kỳ exception nào khác
                    return "Unknown";
                }
            }
        }
        public DateTime LoginTime { get; private set; } = DateTime.Now;

        // --- Biến cho chức năng Quên mật khẩu ---
        private string? _currentOtp = null;
        private string? _currentResetEmail = null;

        public ClientHandler(TcpClient client, Server server)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client), "TcpClient cannot be null");
            }
            if (server == null)
            {
                throw new ArgumentNullException(nameof(server), "Server cannot be null");
            }

            _client = client;
            _server = server;
            try
            {
                // Kiểm tra client đã connected chưa
                if (!_client.Connected)
                {
                    Logger.Warning($"[ClientHandler] Client chưa connected khi khởi tạo handler");
                    throw new InvalidOperationException("TcpClient is not connected");
                }

                // Cấu hình TcpClient để tối ưu kết nối
                _client.NoDelay = true; // Tắt Nagle algorithm để gửi packet ngay lập tức
                _client.ReceiveTimeout = 30000; // 30 giây timeout
                _client.SendTimeout = 30000;
                _client.ReceiveBufferSize = 8192;
                _client.SendBufferSize = 8192;

                _stream = _client.GetStream();
                if (_stream == null)
                {
                    Logger.Error("Không thể lấy NetworkStream từ TcpClient", null);
                    throw new InvalidOperationException("NetworkStream is null");
                }

                // Kiểm tra stream có thể đọc/ghi không
                if (!_stream.CanRead || !_stream.CanWrite)
                {
                    Logger.Error($"Stream không thể đọc/ghi: CanRead={_stream.CanRead}, CanWrite={_stream.CanWrite}", null);
                    throw new InvalidOperationException("NetworkStream is not readable/writable");
                }

                // Cấu hình stream timeout
                _stream.ReadTimeout = 30000;
                _stream.WriteTimeout = 30000;

                _formatter = new BinaryFormatter();
                Logger.Info($"[ClientHandler] Đã khởi tạo thành công cho client từ {ClientIP}");
            }
            catch (Exception ex)
            {
                Logger.Error($"Không thể khởi tạo ClientHandler: {ex.GetType().Name} - {ex.Message}", ex);
                // Đảm bảo cleanup đúng cách
                try
                {
                    _stream?.Close();
                    _stream?.Dispose();
                }
                catch { }
                try
                {
                    _client?.Close();
                    _client?.Dispose();
                }
                catch { }
                _stream = null;
                _client = null;
                throw; // Re-throw để Server biết và xử lý
            }
        }

        public async Task StartHandlingAsync()
        {
            if (_stream == null)
            {
                Logger.Warning($"[ClientHandler] Không thể bắt đầu xử lý: Stream is null cho client từ {ClientIP}");
                Close();
                return;
            }

            if (_client == null || !_client.Connected)
            {
                Logger.Warning($"[ClientHandler] Không thể bắt đầu xử lý: Client không connected từ {ClientIP}");
                Close();
                return;
            }

            Logger.Info($"[ClientHandler] Bắt đầu xử lý packets cho client từ {ClientIP}");
            
            try
            {
                while (_client != null && _client.Connected && _stream != null && _stream.CanRead)
                {
                    try
                    {
                        // Kiểm tra connection trước khi deserialize
                        if (_client == null || !_client.Connected)
                        {
                            Logger.Info($"[Client {UserID ?? ClientIP}] Client đã ngắt kết nối");
                            break;
                        }

                        if (_stream == null || !_stream.CanRead)
                        {
                            Logger.Info($"[Client {UserID ?? ClientIP}] Stream không khả dụng");
                            break;
                        }

                        // Deserialize packet với error handling toàn diện
                        // KHÔNG dùng Task.Run vì NetworkStream không thread-safe
                        // QUAN TRỌNG: Phải lock stream để tránh race condition với Serialize
                        object? receivedPacket = null;
                        bool shouldBreak = false;
                        try
                        {
                            // Lock stream để đảm bảo thread-safety với Serialize
                            lock (_stream)
                            {
                                // Kiểm tra lại sau khi lock
                                if (_stream == null || !_stream.CanRead)
                                {
                                    Logger.Info($"[Client {UserID ?? ClientIP}] Stream không khả dụng sau khi lock");
                                    shouldBreak = true;
                                }
                                else
                                {
                                    // Gọi Deserialize trực tiếp trên thread hiện tại
                                    // NetworkStream sẽ block cho đến khi có dữ liệu hoặc connection đóng
                                    receivedPacket = _formatter.Deserialize(_stream);
                                }
                            }
                            
                            if (shouldBreak) break;
                            
                            if (receivedPacket != null)
                            {
                                Logger.Info($"[Client {UserID ?? ClientIP}] Received packet: {receivedPacket.GetType().Name}");
                            }
                            else
                            {
                                Logger.Warning($"[Client {UserID ?? ClientIP}] Deserialize trả về null");
                                break;
                            }
                        }
                        catch (System.Runtime.Serialization.SerializationException serEx)
                        {
                            // SerializationException xảy ra khi stream bị đóng hoặc dữ liệu không đầy đủ
                            // Đây là tình huống bình thường khi client đóng kết nối
                            string errorMsg = serEx.Message ?? "";
                            if (errorMsg.Contains("End of Stream") ||
                                errorMsg.Contains("parsing was completed") ||
                                errorMsg.Contains("does not contain a valid BinaryHeader"))
                            {
                                Logger.Info($"[Client {UserID ?? ClientIP}] Client đã đóng kết nối (End of Stream)");
                            }
                            else
                            {
                                Logger.Warning($"[Client {UserID ?? ClientIP}] Serialization error: {errorMsg}");
                            }
                            break; // Break để đóng connection gracefully
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
                        Logger.Info($"[Client {UserID ?? ClientIP}] Connection đã bị đóng: {ioEx.Message}");
                        break;
                    }
                    catch (System.Net.Sockets.SocketException sockEx)
                    {
                        // SocketException xảy ra khi network có vấn đề
                        Logger.Warning($"[Client {UserID ?? ClientIP}] Socket error ({sockEx.SocketErrorCode}): {sockEx.Message}");
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        // Stream đã bị dispose, connection đã đóng
                        Logger.Info($"[Client {UserID ?? ClientIP}] Stream đã bị dispose");
                        break;
                    }
                }
            }
            catch (System.Runtime.Serialization.SerializationException serEx)
            {
                // Đảm bảo SerializationException LUÔN được handle ở đây - KHÔNG BAO GIỜ throw ra ngoài
                string errorMsg = serEx.Message ?? "";
                if (errorMsg.Contains("End of Stream") ||
                    errorMsg.Contains("parsing was completed") ||
                    errorMsg.Contains("does not contain a valid BinaryHeader"))
                {
                    Logger.Info($"[Client {UserID ?? ClientIP}] Client đã đóng kết nối (End of Stream - outer catch)");
                }
                else
                {
                    Logger.Warning($"[Client {UserID ?? ClientIP}] SerializationException (outer catch): {errorMsg}");
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
                    string errorMsg = serEx.Message ?? "";
                    if (errorMsg.Contains("End of Stream") ||
                        errorMsg.Contains("parsing was completed") ||
                        errorMsg.Contains("does not contain a valid BinaryHeader"))
                    {
                        Logger.Info($"[Client {UserID ?? ClientIP}] Client đã đóng kết nối (End of Stream - AggregateException)");
                    }
                    else
                    {
                        Logger.Warning($"[Client {UserID ?? ClientIP}] SerializationException (AggregateException): {errorMsg}");
                    }
                }
                else
                {
                    // Nếu không phải SerializationException, log warning
                    Logger.Warning($"[Client {UserID ?? ClientIP}] AggregateException: {aggEx.GetType().Name} - {aggEx.Message}");
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
                    Logger.Warning($"[Client {UserID ?? ClientIP}] Lỗi không mong đợi: {ex.GetType().Name} - {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Logger.Warning($"  Inner Exception: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
                    }
                }
                // KHÔNG throw lại - chỉ log
            }
            finally
            {
                Logger.Info($"[ClientHandler] Kết thúc xử lý cho client {UserID ?? ClientIP}");
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
            // Validate packet
            if (p == null || string.IsNullOrEmpty(p.Password))
            {
                var result = new LoginResultPacket { Success = false, Message = "Dữ liệu đăng nhập không hợp lệ." };
                SendPacket(result);
                Logger.Warning($"[Login Fail] Invalid LoginPacket from IP {this.ClientIP}");
                try
                {
                    _stream?.Flush();
                    System.Threading.Thread.Sleep(100);
                }
                catch { }
                Close();
                return;
            }

            // Hỗ trợ đăng nhập bằng username hoặc email
            string loginValue = p.UseEmailLogin ? (p.Email ?? "") : (p.Username ?? "");

            if (string.IsNullOrEmpty(loginValue))
            {
                var result = new LoginResultPacket { Success = false, Message = "Vui lòng nhập tên đăng nhập hoặc Email." };
                SendPacket(result);
                Logger.Warning($"[Login Fail] Empty login value from IP {this.ClientIP}");
                try
                {
                    _stream?.Flush();
                    System.Threading.Thread.Sleep(100);
                }
                catch { }
                Close();
                return;
            }

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
                Logger.Warning($"[Login Fail] Invalid credentials from IP {this.ClientIP}");
                Close();
            }
        }

        public void SendPacket(object packet)
        {
            if (packet == null)
            {
                Logger.Warning($"[ClientHandler] Không thể gửi packet null cho {UserID ?? "unknown"}");
                return;
            }

            if (_client == null || !_client.Connected)
            {
                Logger.Warning($"[ClientHandler] Không thể gửi packet: Client không connected cho {UserID ?? "unknown"}");
                return;
            }

            if (_stream == null || !_stream.CanWrite)
            {
                Logger.Warning($"[ClientHandler] Không thể gửi packet: Stream không khả dụng cho {UserID ?? "unknown"}");
                return;
            }

            try
            {
                lock (_stream)
                {
                    // Kiểm tra lại sau khi lock
                    if (_stream == null || !_stream.CanWrite)
                    {
                        Logger.Warning($"[ClientHandler] Stream không khả dụng sau khi lock cho {UserID ?? "unknown"}");
                        return;
                    }

                    _formatter.Serialize(_stream, packet);
                    _stream.Flush(); // Đảm bảo dữ liệu được gửi ngay lập tức
                }
            }
            catch (System.Runtime.Serialization.SerializationException serEx)
            {
                Logger.Error($"Lỗi serialization khi gửi packet {packet.GetType().Name} cho {UserID}: {serEx.Message}", serEx);
                Close();
            }
            catch (IOException ioEx)
            {
                Logger.Warning($"Lỗi IO khi gửi packet {packet.GetType().Name} cho {UserID}: {ioEx.Message}");
                Close();
            }
            catch (SocketException sockEx)
            {
                Logger.Warning($"Lỗi Socket khi gửi packet {packet.GetType().Name} cho {UserID}: {sockEx.SocketErrorCode} - {sockEx.Message}");
                Close();
            }
            catch (Exception ex)
            {
                Logger.Error($"Gửi thất bại cho {UserID}: {ex.GetType().Name} - {ex.Message}", ex);
                Close();
            }
        }

        public void Close()
        {
            try
            {
                if (UserID != null)
                {
                    Logger.Info($"[ClientHandler] Đóng kết nối cho user: {UserID}");
                    _server.RemoveClient(this.UserID);
                }
                else
                {
                    Logger.Info($"[ClientHandler] Đóng kết nối cho client từ {ClientIP} (chưa đăng nhập)");
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"Lỗi khi remove client: {ex.Message}");
            }

            try
            {
                _stream?.Close();
                _stream?.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Warning($"Lỗi khi đóng stream: {ex.Message}");
            }

            try
            {
                _client?.Close();
                _client?.Dispose();
            }
            catch (Exception ex)
            {
                Logger.Warning($"Lỗi khi đóng client: {ex.Message}");
            }

            _stream = null;
            _client = null;
        }
    }
}
#pragma warning restore SYSLIB0011