#pragma warning disable SYSLIB0011 
using ChatApp.Shared;
using ChatAppClient.Helpers;
using System;
using System.IO;
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
            // Ngắt kết nối cũ nếu có để tránh conflict
            DisconnectInternal(false);

            try
            {
                _client = new TcpClient();
                // Tối ưu hóa cho truyền tải gói tin nhỏ (chat/game)
                _client.NoDelay = true;
                _client.ReceiveTimeout = 30000;
                _client.SendTimeout = 30000;

                Logger.Info($"Đang thử kết nối đến {ipAddress}:{port}...");

                // Sử dụng timeout 10s cho mạng Wifi (mạng LAN có thể cần thời gian tìm host)
                using var timeoutCts = new CancellationTokenSource(10000);
                try
                {
                    // Hỗ trợ cả .NET Framework và .NET Core bằng cách dùng ConnectAsync với Task.WhenAny
                    var connectTask = _client.ConnectAsync(ipAddress, port);
                    var completedTask = await Task.WhenAny(connectTask, Task.Delay(10000, timeoutCts.Token));

                    if (completedTask != connectTask)
                    {
                        Logger.Error($"Timeout: Không thể kết nối đến Server {ipAddress} sau 10 giây.");
                        DisconnectInternal(false);
                        return false;
                    }

                    // Nếu task hoàn thành, kiểm tra xem có exception không
                    await connectTask;
                }
                catch (Exception ex)
                {
                    // Lỗi xảy ra trong quá trình ConnectAsync
                    throw ex;
                }

                if (_client.Connected)
                {
                    _stream = _client.GetStream();
                    _listeningCts = new CancellationTokenSource();
                    // Bắt đầu lắng nghe gói tin từ Server
                    _ = StartListeningAsync(_listeningCts.Token);

                    CurrentServerIP = ipAddress;
                    CurrentServerPort = port;

                    Logger.Success($"Kết nối THÀNH CÔNG đến {ipAddress}:{port}!");
                    return true;
                }
            }
            catch (SocketException sockEx)
            {
                string msg = $"Lỗi Socket ({sockEx.SocketErrorCode}): Không thể kết nối đến {ipAddress}.";
                if (sockEx.SocketErrorCode == SocketError.ConnectionRefused)
                    msg += " (Server từ chối hoặc chưa mở Port 9000)";
                else if (sockEx.SocketErrorCode == SocketError.TimedOut)
                    msg += " (Sai IP hoặc Firewall chặn)";

                Logger.Error(msg);
            }
            catch (Exception ex)
            {
                Logger.Error($"Lỗi kết nối chung: {ex.Message}");
            }

            DisconnectInternal(false);
            return false;
        }

        private async Task StartListeningAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (_client != null && _client.Connected && _stream != null && !cancellationToken.IsCancellationRequested)
                {
                    object? receivedPacket = null;
                    try
                    {
                        // Deserialize trực tiếp, Task.Run để tránh block UI thread
                        receivedPacket = await Task.Run(() =>
                        {
                            try { return _formatter.Deserialize(_stream); }
                            catch { return null; }
                        }, cancellationToken);
                    }
                    catch { break; }

                    if (receivedPacket != null)
                    {
                        HandlePacket(receivedPacket);
                    }
                    else
                    {
                        // Nếu Deserialize trả về null hoặc throw exception -> Ngắt kết nối
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                    Logger.Warning($"Ngắt kết nối khi đang lắng nghe: {ex.Message}");
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
                    TaskCompletionSource<LoginResultPacket>? completionSource;
                    lock (_loginLock) completionSource = _loginCompletionSource;

                    if (completionSource != null && !completionSource.Task.IsCompleted)
                        completionSource.TrySetResult(pLogin);
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

                // Xử lý các gói tin UI
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

            var timeoutTask = Task.Delay(15000); // 15s timeout
            var resultTask = completionSource.Task;

            if (await Task.WhenAny(resultTask, timeoutTask) == resultTask)
            {
                lock (_loginLock) _loginCompletionSource = null;
                return await resultTask;
            }

            lock (_loginLock) _loginCompletionSource = null;
            throw new TimeoutException("Server phản hồi quá lâu (Timeout).");
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
            NetworkStream? currentStream = _stream;
            if (_client == null || !_client.Connected || currentStream == null) return false;

            try
            {
                lock (currentStream)
                {
                    _formatter.Serialize(currentStream, packet);
                    currentStream.Flush();
                }
                return true;
            }
            catch { return false; }
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