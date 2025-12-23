#pragma warning disable SYSLIB0011 
using ChatApp.Shared;
using ChatAppClient.Helpers;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatAppClient.Forms
{
    public class NetworkManager
    {
        #region Singleton
        private static NetworkManager? _instance;
        public static NetworkManager Instance => _instance ??= new NetworkManager();
        #endregion

        private TcpClient? _client;
        private NetworkStream? _stream;
        private BinaryFormatter _formatter;
        private frmHome? _homeForm;

        public string? UserID { get; private set; }
        public string? UserName { get; private set; }

        public string? CurrentServerIP { get; private set; }
        public int CurrentServerPort { get; private set; } = 9000;

        private TaskCompletionSource<LoginResultPacket>? _loginCompletionSource;
        private TaskCompletionSource<RegisterResultPacket>? _registerCompletionSource;
        private TaskCompletionSource<ChatHistoryResponsePacket>? _chatHistoryCompletionSource;

        public event Action<ForgotPasswordResultPacket>? OnForgotPasswordResult;
        public event Action<ChatHistoryResponsePacket>? OnChatHistoryReceived;

        private CancellationTokenSource? _listeningCts;
        private readonly object _loginLock = new object();

        private NetworkManager()
        {
            _formatter = new BinaryFormatter();
        }

        public void RegisterHomeForm(frmHome homeForm) => _homeForm = homeForm;

        // --- HÀM KẾT NỐI ĐƯỢC SỬA ĐỔI ---
        public async Task<bool> ConnectAsync(string ipAddress, int port)
        {
            // Validate IP address
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                Logger.Error("IP address không được để trống");
                return false;
            }

            // Ngắt kết nối cũ nếu có để tránh conflict
            DisconnectInternal(false);

            TcpClient? tempClient = null;
            try
            {
                tempClient = new TcpClient();
                // Tối ưu hóa cho truyền tải gói tin nhỏ (chat/game)
                tempClient.NoDelay = true;
                tempClient.ReceiveTimeout = 30000;
                tempClient.SendTimeout = 30000;
                // Set a reasonable buffer size for small packets
                tempClient.SendBufferSize = 8192;
                tempClient.ReceiveBufferSize = 8192;

                Logger.Info($"Đang thử kết nối đến {ipAddress}:{port}...");

                // Sử dụng timeout 15s cho mạng Wifi (tăng từ 10s để tránh timeout quá nhanh)
                const int timeoutMs = 15000;
                try
                {
                    // Sử dụng Socket.ConnectAsync với CancellationToken để có timeout chính xác hơn
                    var socket = tempClient.Client;
                    if (socket == null)
                    {
                        Logger.Error("Không thể lấy Socket từ TcpClient");
                        try { tempClient?.Close(); } catch { }
                        return false;
                    }

                    var endPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(ipAddress), port);
                    using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs));
                    
                    try
                    {
                        await socket.ConnectAsync(endPoint, timeoutCts.Token);
                        
                        // Kiểm tra kết nối thành công
                        if (!socket.Connected)
                        {
                            Logger.Error("Socket không connected sau khi ConnectAsync");
                            try { tempClient?.Close(); } catch { }
                            return false;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Timeout xảy ra
                        Logger.Error($"Timeout: Không thể kết nối đến Server {ipAddress}:{port} sau {timeoutMs / 1000} giây.");
                        Logger.Error("Nguyên nhân có thể:");
                        Logger.Error("  - Server chưa khởi động");
                        Logger.Error("  - IP address sai");
                        Logger.Error("  - Firewall chặn kết nối");
                        Logger.Error("  - Hai máy không cùng mạng");
                        try { tempClient?.Close(); } catch { }
                        return false;
                    }
                    catch (SocketException sockEx)
                    {
                        // Xử lý SocketException ngay tại đây để có thông báo chi tiết
                        string errorMsg = $"Lỗi Socket ({sockEx.SocketErrorCode}): ";
                        
                        switch (sockEx.SocketErrorCode)
                        {
                            case SocketError.ConnectionRefused:
                                errorMsg += $"Server tại {ipAddress}:{port} từ chối kết nối.\n";
                                errorMsg += "Nguyên nhân:\n";
                                errorMsg += "  - Server chưa khởi động hoặc chưa lắng nghe trên port này\n";
                                errorMsg += "  - Firewall trên Server chưa mở port 9000\n";
                                errorMsg += "  - Port 9000 đang bị ứng dụng khác sử dụng";
                                break;
                            case SocketError.TimedOut:
                                errorMsg += $"Không thể kết nối đến {ipAddress}:{port} (Timeout).\n";
                                errorMsg += "Nguyên nhân:\n";
                                errorMsg += "  - IP address sai hoặc không tồn tại\n";
                                errorMsg += "  - Firewall chặn kết nối\n";
                                errorMsg += "  - Hai máy không cùng mạng WiFi\n";
                                errorMsg += "  - Router có AP Isolation";
                                break;
                            case SocketError.HostUnreachable:
                                errorMsg += $"Không thể đến được host {ipAddress}.\n";
                                errorMsg += "Nguyên nhân:\n";
                                errorMsg += "  - IP address sai\n";
                                errorMsg += "  - Máy Server không có kết nối mạng\n";
                                errorMsg += "  - Hai máy không cùng mạng";
                                break;
                            case SocketError.NetworkUnreachable:
                                errorMsg += $"Không thể đến được mạng chứa {ipAddress}.\n";
                                errorMsg += "Nguyên nhân:\n";
                                errorMsg += "  - Máy Client không có kết nối mạng\n";
                                errorMsg += "  - Routing table không đúng";
                                break;
                            default:
                                errorMsg += $"Không thể kết nối đến {ipAddress}:{port}.\n";
                                errorMsg += $"Chi tiết: {sockEx.Message}";
                                break;
                        }
                        
                        Logger.Error(errorMsg);
                        try { tempClient?.Close(); } catch { }
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    // Lỗi xảy ra trong quá trình ConnectAsync
                    Logger.Error($"Lỗi trong quá trình kết nối: {ex.GetType().Name} - {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Logger.Error($"  Inner Exception: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
                    }
                    try { tempClient?.Close(); } catch { }
                    return false;
                }

                // Kiểm tra client đã kết nối thành công
                if (tempClient == null || !tempClient.Connected)
                {
                    Logger.Error("Kết nối không thành công - Client không connected sau khi ConnectAsync");
                    try { tempClient?.Close(); } catch { }
                    return false;
                }

                // Khởi tạo stream
                try
                {
                    NetworkStream? stream = tempClient.GetStream();
                    if (stream == null)
                    {
                        Logger.Error("Không thể lấy NetworkStream từ TcpClient");
                        try { tempClient?.Close(); } catch { }
                        return false;
                    }

                    // Ensure socket is properly configured
                    stream.ReadTimeout = 30000;
                    stream.WriteTimeout = 30000;

                    // Gán vào biến instance sau khi đã xác nhận kết nối thành công
                    _client = tempClient;
                    _stream = stream;

                    _listeningCts = new CancellationTokenSource();
                    // Bắt đầu lắng nghe gói tin từ Server - đợi một chút để đảm bảo task đã khởi động
                    var listeningTask = StartListeningAsync(_listeningCts.Token);
                    
                    // Đợi một chút để đảm bảo listening task đã bắt đầu (không block quá lâu)
                    await Task.Delay(100); // 100ms delay để listening task có thời gian khởi động

                    CurrentServerIP = ipAddress;
                    CurrentServerPort = port;

                    Logger.Success($"Kết nối THÀNH CÔNG đến {ipAddress}:{port}!");
                    return true;
                }
                catch (Exception streamEx)
                {
                    Logger.Error($"Lỗi khi khởi tạo stream: {streamEx.GetType().Name} - {streamEx.Message}");
                    if (streamEx.InnerException != null)
                    {
                        Logger.Error($"  Inner Exception: {streamEx.InnerException.GetType().Name} - {streamEx.InnerException.Message}");
                    }
                    try { tempClient?.Close(); } catch { }
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Catch-all cho các exception không mong đợi
                Logger.Error($"Lỗi kết nối không mong đợi: {ex.GetType().Name} - {ex.Message}");
                if (ex.InnerException != null)
                {
                    Logger.Error($"  Inner Exception: {ex.InnerException.GetType().Name} - {ex.InnerException.Message}");
                }
                try { tempClient?.Close(); } catch { }
                return false;
            }
        }

        private async Task StartListeningAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Yield để đảm bảo method thực sự chạy async
                await Task.Yield();
                
                Logger.Info("Listening task đã bắt đầu, sẵn sàng nhận packets từ server");

                while (_client != null && _client.Connected && _stream != null && !cancellationToken.IsCancellationRequested)
                {
                    object? receivedPacket = null;
                    try
                    {
                        // Kiểm tra stream trước khi deserialize
                        if (_stream == null || !_stream.CanRead)
                        {
                            Logger.Info("Stream không khả dụng, ngắt kết nối");
                            break;
                        }

                        // KHÔNG dùng Task.Run vì NetworkStream không thread-safe
                        // QUAN TRỌNG: Phải lock stream để tránh race condition với Serialize
                        bool shouldBreak = false;
                        // Lock stream để đảm bảo thread-safety với Serialize
                        lock (_stream)
                        {
                            // Kiểm tra lại sau khi lock
                            if (_stream == null || !_stream.CanRead)
                            {
                                Logger.Info("Stream không khả dụng sau khi lock, ngắt kết nối");
                                shouldBreak = true;
                            }
                            else
                            {
                                // Gọi Deserialize trực tiếp - method này đã chạy trên background thread
                                // Deserialize là blocking call, nhưng chạy trên background thread nên OK
                                receivedPacket = _formatter.Deserialize(_stream);
                            }
                        }
                        
                        if (shouldBreak) break;
                    }
                    catch (System.Runtime.Serialization.SerializationException serEx)
                    {
                        // SerializationException xảy ra khi stream bị đóng hoặc dữ liệu không đúng format
                        // Đây là tình huống bình thường khi connection đóng hoặc dữ liệu corrupt
                        if (serEx.Message.Contains("End of Stream") ||
                            serEx.Message.Contains("parsing was completed") ||
                            serEx.Message.Contains("does not contain a valid BinaryHeader"))
                        {
                            Logger.Info($"Server đã đóng kết nối hoặc dữ liệu không hợp lệ: {serEx.Message}");
                        }
                        else
                        {
                            Logger.Warning($"Serialization error: {serEx.Message}");
                        }
                        break; // Break để đóng connection gracefully
                    }
                    catch (OperationCanceledException)
                    {
                        // Cancellation được yêu cầu, break normally
                        break;
                    }
                    catch (IOException ioEx)
                    {
                        // IOException xảy ra khi stream bị đóng
                        Logger.Info($"Stream đã bị đóng: {ioEx.Message}");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning($"Lỗi khi deserialize: {ex.GetType().Name} - {ex.Message}");
                        break;
                    }

                    if (receivedPacket != null)
                    {
                        Logger.Info($"Đã nhận packet: {receivedPacket.GetType().Name}");
                        HandlePacket(receivedPacket);
                    }
                    else
                    {
                        // Nếu Deserialize trả về null -> Ngắt kết nối
                        Logger.Warning("Deserialize trả về null, ngắt kết nối");
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Cancellation requested - normal exit
                Logger.Info("Listening cancelled");
            }
            catch (Exception ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                    Logger.Warning($"Ngắt kết nối khi đang lắng nghe: {ex.GetType().Name} - {ex.Message}");
            }
            finally
            {
                // Thông báo rớt mạng nếu không phải do user chủ động ngắt
                if (!cancellationToken.IsCancellationRequested && _homeForm != null && !_homeForm.IsDisposed)
                {
                    _homeForm.BeginInvoke(new Action(() =>
                        MessageBox.Show("Mất kết nối đến máy chủ!", "Lỗi mạng", MessageBoxButtons.OK, MessageBoxIcon.Error)));
                    Disconnect();
                }
            }
        }

        private void HandlePacket(object packet)
        {
            try
            {
                if (packet is LoginResultPacket pLogin)
                {
                    Logger.Info($"Đã nhận LoginResultPacket: Success={pLogin.Success}, UserID={pLogin.UserID}");
                    
                    TaskCompletionSource<LoginResultPacket>? completionSource;
                    lock (_loginLock) completionSource = _loginCompletionSource;

                    if (completionSource != null && !completionSource.Task.IsCompleted)
                    {
                        Logger.Info("Đang set result cho LoginAsync...");
                        completionSource.TrySetResult(pLogin);
                        Logger.Info("Đã set result cho LoginAsync thành công");
                    }
                    else
                    {
                        Logger.Warning("Không có completion source hoặc task đã completed");
                    }
                    return;
                }
                if (packet is RegisterResultPacket pRegister)
                {
                    if (_registerCompletionSource != null && !_registerCompletionSource.Task.IsCompleted)
                        _registerCompletionSource.TrySetResult(pRegister);
                    return;
                }
                if (packet is ForgotPasswordResultPacket pForgot)
                {
                    OnForgotPasswordResult?.Invoke(pForgot);
                    return;
                }
                if (packet is ChatHistoryResponsePacket pChatHistory)
                {
                    if (_chatHistoryCompletionSource != null && !_chatHistoryCompletionSource.Task.IsCompleted)
                        _chatHistoryCompletionSource.TrySetResult(pChatHistory);
                    OnChatHistoryReceived?.Invoke(pChatHistory);
                    return;
                }

                if (string.IsNullOrEmpty(UserID) || _homeForm == null || _homeForm.IsDisposed) return;

                // Xử lý các gói tin UI - QUAN TRỌNG: Phải marshal về UI thread để tránh đơ giao diện
                Action? action = packet switch
                {
                    UserStatusPacket p => () => _homeForm.HandleUserStatusUpdate(p),
                    TextPacket p => () => _homeForm.HandleIncomingTextMessage(p),
                    FilePacket p => () => _homeForm.HandleIncomingFileMessage(p),
                    GameInvitePacket p => () => _homeForm.HandleIncomingGameInvite(p),
                    GameResponsePacket p => () => _homeForm.HandleGameResponse(p),
                    GameStartPacket p => () => _homeForm.HandleGameStart(p),
                    GameMovePacket p => () => _homeForm.HandleGameMove(p),
                    RematchRequestPacket p => () => _homeForm.HandleRematchRequest(p),
                    RematchResponsePacket p => () => _homeForm.HandleRematchResponse(p),
                    GameResetPacket p => () => _homeForm.HandleGameReset(p),
                    UpdateProfilePacket p => () => _homeForm.HandleUpdateProfile(p),
                    TankInvitePacket p => () => _homeForm.HandleTankInvite(p),
                    TankResponsePacket p => () => _homeForm.HandleTankResponse(p),
                    TankStartPacket p => () => _homeForm.HandleTankStart(p),
                    TankActionPacket p => () => _homeForm.HandleTankAction(p),
                    TankHitPacket p => () => _homeForm.HandleTankHit(p),
                    _ => null
                };
                
                // Sử dụng BeginInvoke để đảm bảo UI updates chạy trên UI thread và không block
                if (action != null && _homeForm != null && !_homeForm.IsDisposed)
                {
                    _homeForm.BeginInvoke(action);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Lỗi xử lý packet {packet?.GetType().Name}: {ex.Message}");
            }
        }

        public async Task<LoginResultPacket> LoginAsync(LoginPacket packet)
        {
            if (_client == null || !_client.Connected) throw new InvalidOperationException("Chưa kết nối Server.");

            TaskCompletionSource<LoginResultPacket> completionSource;
            lock (_loginLock)
            {
                _loginCompletionSource = new TaskCompletionSource<LoginResultPacket>();
                completionSource = _loginCompletionSource;
            }

            if (!SendPacket(packet))
            {
                lock (_loginLock) _loginCompletionSource = null;
                throw new InvalidOperationException("Gửi gói tin đăng nhập thất bại.");
            }

            // Tăng timeout lên 30s để tránh timeout quá nhanh trên mạng chậm
            var timeoutTask = Task.Delay(30000); // 30s timeout
            var resultTask = completionSource.Task;

            Logger.Info("Đang chờ phản hồi từ server...");
            
            var completedTask = await Task.WhenAny(resultTask, timeoutTask);
            
            if (completedTask == resultTask)
            {
                lock (_loginLock) _loginCompletionSource = null;
                var result = await resultTask;
                Logger.Info($"Đã nhận được phản hồi từ server: Success={result.Success}");
                return result;
            }

            // Timeout xảy ra
            lock (_loginLock) _loginCompletionSource = null;
            Logger.Warning("Timeout khi chờ phản hồi từ server");
            throw new TimeoutException("Server phản hồi quá lâu (Timeout 30s). Vui lòng kiểm tra kết nối mạng.");
        }

        public async Task<RegisterResultPacket> RegisterAsync(RegisterPacket packet)
        {
            if (_client == null || !_client.Connected) throw new InvalidOperationException("Chưa kết nối.");
            _registerCompletionSource = new TaskCompletionSource<RegisterResultPacket>();
            SendPacket(packet);

            if (await Task.WhenAny(_registerCompletionSource.Task, Task.Delay(10000)) == _registerCompletionSource.Task)
                return await _registerCompletionSource.Task;
            else
                throw new TimeoutException("Đăng ký Timeout.");
        }

        public async Task<ChatHistoryResponsePacket> RequestChatHistoryAsync(string friendID, int limit = 100)
        {
            if (_client == null || !_client.Connected || string.IsNullOrEmpty(UserID))
                throw new InvalidOperationException("Chưa sẵn sàng.");

            _chatHistoryCompletionSource = new TaskCompletionSource<ChatHistoryResponsePacket>();

            var request = new ChatHistoryRequestPacket { UserID = UserID, FriendID = friendID, Limit = limit };
            if (!SendPacket(request)) return new ChatHistoryResponsePacket { Success = false };

            if (await Task.WhenAny(_chatHistoryCompletionSource.Task, Task.Delay(10000)) == _chatHistoryCompletionSource.Task)
                return await _chatHistoryCompletionSource.Task;

            return new ChatHistoryResponsePacket { Success = false, Message = "Timeout" };
        }

        public bool SendPacket(object packet)
        {
            if (packet == null) return false;

            NetworkStream? currentStream = _stream;
            if (_client == null || !_client.Connected || currentStream == null || !currentStream.CanWrite)
            {
                Logger.Warning("Không thể gửi packet: Client chưa kết nối hoặc stream không khả dụng");
                return false;
            }

            try
            {
                lock (currentStream)
                {
                    // Kiểm tra lại sau khi lock
                    if (!_client.Connected || currentStream == null || !currentStream.CanWrite)
                    {
                        return false;
                    }

                    _formatter.Serialize(currentStream, packet);
                    currentStream.Flush();
                }
                return true;
            }
            catch (IOException ioEx)
            {
                Logger.Warning($"Lỗi IO khi gửi packet: {ioEx.Message}");
                DisconnectInternal(false);
                return false;
            }
            catch (System.Runtime.Serialization.SerializationException serEx)
            {
                Logger.Error($"Lỗi serialization khi gửi packet: {serEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Warning($"Lỗi không mong đợi khi gửi packet: {ex.GetType().Name} - {ex.Message}");
                return false;
            }
        }

        public void SetUserCredentials(string userId, string userName)
        {
            this.UserID = userId;
            this.UserName = userName;
        }

        public void Disconnect()
        {
            DisconnectInternal(true);
        }

        private void DisconnectInternal(bool showMessage)
        {
            try
            {
                // Hủy listening task trước
                _listeningCts?.Cancel();

                // Đợi một chút để listening task có thời gian dừng
                try
                {
                    Task.Delay(100).Wait();
                }
                catch { }

                // Đóng stream trước
                try
                {
                    if (_stream != null)
                    {
                        if (_stream.CanWrite)
                        {
                            _stream.Flush();
                        }
                        _stream.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Lỗi khi đóng stream: {ex.Message}");
                }

                // Đóng client
                try
                {
                    if (_client != null)
                    {
                        if (_client.Connected)
                        {
                            _client.Close();
                        }
                        _client.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Lỗi khi đóng client: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"Lỗi trong DisconnectInternal: {ex.Message}");
            }
            finally
            {
                _client = null;
                _stream = null;
                _listeningCts?.Dispose();
                _listeningCts = null;
                // KHÔNG set _homeForm = null ở đây vì có thể cần dùng lại
                // Chỉ clear credentials
                if (showMessage)
                {
                    UserID = null;
                    UserName = null;
                }
                Logger.Info("Đã ngắt kết nối.");
            }
        }
    }
}
#pragma warning restore SYSLIB0011