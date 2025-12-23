#pragma warning disable SYSLIB0011 // Tắt cảnh báo BinaryFormatter

using ChatApp.Shared;
using ChatAppClient.Helpers;
using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
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

        // Các biến thông tin User
        public string? UserID { get; private set; }
        public string? UserName { get; private set; }
        
        // Lưu IP server đã kết nối để các form khác sử dụng
        public string? CurrentServerIP { get; private set; }
        public int CurrentServerPort { get; private set; } = 9000;

        // Các biến Task/Event để xử lý phản hồi
        private TaskCompletionSource<LoginResultPacket>? _loginCompletionSource;
        private TaskCompletionSource<RegisterResultPacket>? _registerCompletionSource;
        private TaskCompletionSource<ChatHistoryResponsePacket>? _chatHistoryCompletionSource;
        public event Action<ForgotPasswordResultPacket>? OnForgotPasswordResult; // Sự kiện quên mật khẩu
        public event Action<ChatHistoryResponsePacket>? OnChatHistoryReceived; // Sự kiện nhận lịch sử chat

        private CancellationTokenSource? _listeningCts;
        private readonly object _loginLock = new object(); // Lock cho thread safety

        private NetworkManager()
        {
            _formatter = new BinaryFormatter();
        }

        public void RegisterHomeForm(frmHome homeForm) => _homeForm = homeForm;

        // --- KẾT NỐI ---
        public async Task<bool> ConnectAsync(string ipAddress, int port)
        {
            if (_client != null && _client.Connected && _stream != null) return true;

            DisconnectInternal(false);

            try
            {
                _client = new TcpClient();
                Logger.Info($"Đang kết nối đến {ipAddress}:{port}...");

                // Tăng timeout lên 10 giây để đảm bảo đủ thời gian kết nối
                var connectTask = _client.ConnectAsync(ipAddress, port);
                using var timeoutCts = new CancellationTokenSource(10000);

                var completedTask = await Task.WhenAny(connectTask, Task.Delay(10000, timeoutCts.Token));

                if (completedTask == connectTask)
                {
                    try
                    {
                        await connectTask;
                    }
                    catch (SocketException sockEx)
                    {
                        Logger.Error($"Lỗi kết nối Socket: {sockEx.Message} (ErrorCode: {sockEx.ErrorCode})");
                        DisconnectInternal(false);
                        return false;
                    }
                    catch (Exception connectEx)
                    {
                        Logger.Error($"Lỗi khi kết nối: {connectEx.Message}", connectEx);
                        DisconnectInternal(false);
                        return false;
                    }

                    // Kiểm tra kết nối đã thành công chưa
                    if (!_client.Connected)
                    {
                        Logger.Error("Kết nối không thành công (TcpClient.Connected = false)");
                        DisconnectInternal(false);
                        return false;
                    }

                    try
                    {
                        _stream = _client.GetStream();
                        if (_stream == null) throw new IOException("NetworkStream trả về null.");
                    }
                    catch (Exception streamEx)
                    {
                        Logger.Error("Lỗi khi lấy NetworkStream", streamEx);
                        DisconnectInternal(false);
                        return false;
                    }

                    _listeningCts = new CancellationTokenSource();
                    _ = StartListeningAsync(_listeningCts.Token);
                    
                    // Lưu IP và Port để các form khác sử dụng
                    CurrentServerIP = ipAddress;
                    CurrentServerPort = port;
                    
                    Logger.Success($"Đã kết nối thành công đến {ipAddress}:{port}!");
                    return true;
                }
                else
                {
                    Logger.Error($"Kết nối thất bại (Timeout sau 10 giây) đến {ipAddress}:{port}");
                    DisconnectInternal(false);
                    return false;
                }
            }
            catch (SocketException sockEx)
            {
                Logger.Error($"Lỗi Socket khi kết nối đến {ipAddress}:{port}: {sockEx.Message} (ErrorCode: {sockEx.ErrorCode})", sockEx);
                DisconnectInternal(false);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error($"Kết nối thất bại đến {ipAddress}:{port}: {ex.Message}", ex);
                DisconnectInternal(false);
                return false;
            }
        }

        // --- LẮNG NGHE ---
        private async Task StartListeningAsync(CancellationToken cancellationToken)
        {
            Logger.Info("Bắt đầu lắng nghe Server...");
            try
            {
                while (_client != null && _client.Connected && _stream != null && !cancellationToken.IsCancellationRequested)
                {
                    // BinaryFormatter.Deserialize sẽ block cho đến khi có dữ liệu
                    // Không cần check DataAvailable, chỉ cần deserialize trực tiếp
                    object? receivedPacket = null;
                    try
                    {
                        receivedPacket = await Task.Run(() => _formatter.Deserialize(_stream), cancellationToken);
                    }
                    catch (Exception deserializeEx)
                    {
                        // Nếu connection bị đóng trong khi deserialize, có thể vẫn còn packet chưa xử lý
                        // Kiểm tra xem có phải do server đóng connection sau khi gửi packet không
                        if (deserializeEx is IOException || deserializeEx.InnerException is IOException)
                        {
                            Logger.Warning($"Lỗi deserialize (có thể do server đóng connection): {deserializeEx.Message}");
                        }
                        throw;
                    }
                    
                    if (receivedPacket != null)
                    {
                        // Log receipt of certain packets for debugging
                        if (receivedPacket is GameResponsePacket grp)
                        {
                            Logger.Info($"[NetworkManager] Received GameResponsePacket: Sender={grp.SenderID}, Receiver={grp.ReceiverID}, Accepted={grp.Accepted}");
                        }
                        HandlePacket(receivedPacket);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"Ngắt kết nối khi đang lắng nghe: {ex.Message}");
                if (!cancellationToken.IsCancellationRequested && _homeForm != null && !_homeForm.IsDisposed)
                {
                    _homeForm.BeginInvoke(new Action(() =>
                        MessageBox.Show("Mất kết nối đến máy chủ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error)));
                    Disconnect();
                }
            }
        }

        // --- XỬ LÝ GÓI TIN ĐẾN ---
        private void HandlePacket(object packet)
        {
            try
            {
                // 1. Các gói tin Login/Register (Dùng TaskCompletionSource) - XỬ LÝ TRƯỚC TIÊN
                if (packet is LoginResultPacket pLogin)
                {
                    Logger.Info($"Nhận được LoginResultPacket: Success={pLogin.Success}, UserID={pLogin.UserID}");
                    
                    TaskCompletionSource<LoginResultPacket>? completionSource;
                    lock (_loginLock)
                    {
                        completionSource = _loginCompletionSource;
                    }
                    
                    // Chỉ set result nếu TaskCompletionSource vẫn còn tồn tại và chưa được set
                    if (completionSource != null && !completionSource.Task.IsCompleted)
                    {
                        bool setResult = completionSource.TrySetResult(pLogin);
                        if (setResult)
                        {
                            Logger.Success("Đã set result vào LoginCompletionSource thành công.");
                        }
                        else
                        {
                            Logger.Warning("Không thể set result vào LoginCompletionSource (có thể đã completed).");
                        }
                    }
                    else
                    {
                        Logger.Warning($"LoginCompletionSource is null hoặc đã completed. completionSource={completionSource != null}, IsCompleted={completionSource?.Task.IsCompleted}");
                    }
                    return; // QUAN TRỌNG: Return ngay để không xử lý các packet khác
                }
                if (packet is RegisterResultPacket pRegister)
                {
                    // Chỉ set result nếu TaskCompletionSource vẫn còn tồn tại và chưa được set
                    if (_registerCompletionSource != null && !_registerCompletionSource.Task.IsCompleted)
                    {
                        _registerCompletionSource.TrySetResult(pRegister);
                    }
                    return; // QUAN TRỌNG: Return ngay
                }

                // 2. Gói tin Quên Mật Khẩu (Dùng Event)
                if (packet is ForgotPasswordResultPacket pForgot)
                {
                    OnForgotPasswordResult?.Invoke(pForgot);
                    return; // QUAN TRỌNG: Return ngay
                }

                // 2.5. Gói tin Lịch sử Chat (Dùng TaskCompletionSource và Event)
                if (packet is ChatHistoryResponsePacket pChatHistory)
                {
                    if (_chatHistoryCompletionSource != null && !_chatHistoryCompletionSource.Task.IsCompleted)
                    {
                        _chatHistoryCompletionSource.TrySetResult(pChatHistory);
                    }
                    OnChatHistoryReceived?.Invoke(pChatHistory);
                    return; // QUAN TRỌNG: Return ngay
                }

                // 3. Các gói tin cần Update UI trên Home (Dùng Invoke)
                // CHỈ XỬ LÝ KHI ĐÃ LOGIN XONG (UserID đã được set)
                if (string.IsNullOrEmpty(UserID))
                {
                    Logger.Warning($"Bỏ qua packet {packet.GetType().Name} vì chưa login xong.");
                    return;
                }

                if (_homeForm == null || _homeForm.IsDisposed)
                {
                    Logger.Warning($"Bỏ qua packet {packet.GetType().Name} vì _homeForm is null hoặc disposed.");
                    return;
                }

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
                action?.Invoke();
            }
            catch (Exception ex)
            {
                // Xử lý exception để không làm gián đoạn quá trình login
                Logger.Error($"Lỗi khi xử lý packet {packet?.GetType().Name}: {ex.Message}", ex);
            }
        }

        // --- CÁC HÀM GỬI ---

        public async Task<LoginResultPacket> LoginAsync(LoginPacket packet)
        {
            if (_client == null || !_client.Connected || _stream == null) throw new InvalidOperationException("Chưa kết nối.");
            
            TaskCompletionSource<LoginResultPacket> completionSource;
            lock (_loginLock)
            {
                // Tạo TaskCompletionSource mới (không hủy task cũ để tránh lỗi cancel)
                _loginCompletionSource = new TaskCompletionSource<LoginResultPacket>();
                completionSource = _loginCompletionSource;
            }
            
            if (!SendPacket(packet)) // Gửi
            {
                lock (_loginLock)
                {
                    _loginCompletionSource = null;
                }
                throw new InvalidOperationException("Không thể gửi gói tin đăng nhập.");
            }
            
            Logger.Info("Đã gửi LoginPacket, đang chờ phản hồi...");
            
            try
            {
                var timeoutTask = Task.Delay(15000); // Tăng timeout lên 15 giây
                var resultTask = completionSource.Task;
                var completedTask = await Task.WhenAny(resultTask, timeoutTask);
                
                // Kiểm tra xem resultTask đã completed chưa
                // (có thể completed ngay cả khi timeoutTask hoàn thành trước do race condition)
                if (resultTask.IsCompleted)
                {
                    var result = await resultTask;
                    Logger.Success($"Đã nhận được phản hồi đăng nhập: Success={result.Success}");
                    lock (_loginLock)
                    {
                        _loginCompletionSource = null; // Clear sau khi nhận được kết quả
                    }
                    return result;
                }
                
                // Nếu đến đây nghĩa là timeout xảy ra và resultTask chưa completed
                // Kiểm tra lại một lần nữa để tránh race condition
                if (resultTask.IsCompleted)
                {
                    var result = await resultTask;
                    Logger.Success($"Đã nhận được phản hồi đăng nhập (sau timeout check): Success={result.Success}");
                    lock (_loginLock)
                    {
                        _loginCompletionSource = null;
                    }
                    return result;
                }
                
                Logger.Error("Timeout: Không nhận được phản hồi đăng nhập sau 15 giây.");
                lock (_loginLock)
                {
                    _loginCompletionSource = null;
                }
                throw new TimeoutException("Không nhận được phản hồi đăng nhập. Vui lòng kiểm tra kết nối và thử lại.");
            }
            catch (TaskCanceledException)
            {
                Logger.Warning("Đăng nhập bị hủy.");
                lock (_loginLock)
                {
                    _loginCompletionSource = null;
                }
                throw new InvalidOperationException("Đăng nhập bị hủy.");
            }
        }

        public async Task<RegisterResultPacket> RegisterAsync(RegisterPacket packet)
        {
            if (_client == null || !_client.Connected || _stream == null) throw new InvalidOperationException("Chưa kết nối.");
            _registerCompletionSource = new TaskCompletionSource<RegisterResultPacket>();
            SendPacket(packet);
            var timeoutTask = Task.Delay(10000);
            var resultTask = _registerCompletionSource.Task;
            if (await Task.WhenAny(resultTask, timeoutTask) == resultTask) return await resultTask;
            else throw new TimeoutException("Không nhận được phản hồi đăng ký.");
        }

        public async Task<ChatHistoryResponsePacket> RequestChatHistoryAsync(string friendID, int limit = 100)
        {
            if (_client == null || !_client.Connected || _stream == null) 
                throw new InvalidOperationException("Chưa kết nối.");
            
            if (string.IsNullOrEmpty(UserID))
                throw new InvalidOperationException("Chưa đăng nhập.");

            _chatHistoryCompletionSource = new TaskCompletionSource<ChatHistoryResponsePacket>();
            
            var request = new ChatHistoryRequestPacket
            {
                UserID = UserID,
                FriendID = friendID,
                Limit = limit
            };
            
            if (!SendPacket(request))
            {
                _chatHistoryCompletionSource = null;
                throw new InvalidOperationException("Không thể gửi yêu cầu lịch sử chat.");
            }
            
            try
            {
                var timeoutTask = Task.Delay(10000);
                var resultTask = _chatHistoryCompletionSource.Task;
                var completedTask = await Task.WhenAny(resultTask, timeoutTask);
                
                if (resultTask.IsCompleted)
                {
                    var result = await resultTask;
                    _chatHistoryCompletionSource = null;
                    return result;
                }
                
                _chatHistoryCompletionSource = null;
                throw new TimeoutException("Không nhận được lịch sử chat sau 10 giây.");
            }
            catch (TaskCanceledException)
            {
                _chatHistoryCompletionSource = null;
                throw new InvalidOperationException("Yêu cầu lịch sử chat bị hủy.");
            }
        }

        // *** HÀM QUAN TRỌNG ĐÃ SỬA: Trả về bool ***
        public bool SendPacket(object packet)
        {
            NetworkStream? currentStream = _stream;
            // Kiểm tra kỹ trước khi gửi
            if (_client == null || !_client.Connected || currentStream == null)
            {
                Logger.Warning("Lỗi gửi gói tin: Chưa kết nối hoặc stream null.");
                return false; // Báo thất bại
            }

            try
            {
                // Sử dụng lock để đảm bảo thread-safe và không bị gián đoạn khi đang login
                lock (currentStream) 
                { 
                    // Kiểm tra lại connection sau khi lock
                    if (_client == null || !_client.Connected || currentStream == null)
                    {
                        Logger.Warning("Kết nối bị mất trong khi gửi packet.");
                        return false;
                    }

                    _formatter.Serialize(currentStream, packet);
                    currentStream.Flush(); // Đảm bảo dữ liệu được gửi ngay lập tức
                }
                return true; // Gửi thành công
            }
            catch (Exception ex)
            {
                Logger.Error($"Lỗi I/O khi gửi gói tin: {ex.Message}", ex);
                // Không throw exception để tránh làm gián đoạn quá trình login
                return false; // Báo thất bại
            }
        }

        // --- TIỆN ÍCH ---
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
                _listeningCts?.Cancel();
                _stream?.Close();
                _client?.Close();
            }
            catch { }
            finally
            {
                _client = null;
                _stream = null;
                _listeningCts = null;
                _homeForm = null;
                UserID = null;
                UserName = null;
                Logger.Info("Đã ngắt kết nối.");
            }
        }
    }
}
#pragma warning restore SYSLIB0011