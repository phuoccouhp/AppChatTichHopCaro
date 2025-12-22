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
        private static NetworkManager _instance;
        public static NetworkManager Instance => _instance ??= new NetworkManager();
        #endregion

        private TcpClient _client;
        private NetworkStream _stream;
        private BinaryFormatter _formatter;
        private frmHome _homeForm;

        // Các biến thông tin User
        public string UserID { get; private set; }
        public string UserName { get; private set; }

        // Các biến Task/Event để xử lý phản hồi
        private TaskCompletionSource<LoginResultPacket> _loginCompletionSource;
        private TaskCompletionSource<RegisterResultPacket> _registerCompletionSource;
        public event Action<ForgotPasswordResultPacket> OnForgotPasswordResult; // Sự kiện quên mật khẩu

        private CancellationTokenSource _listeningCts;
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

                var connectTask = _client.ConnectAsync(ipAddress, port);
                using var timeoutCts = new CancellationTokenSource(5000);

                var completedTask = await Task.WhenAny(connectTask, Task.Delay(5000, timeoutCts.Token));

                if (completedTask == connectTask)
                {
                    await connectTask;

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
                    Logger.Success("Đã kết nối!");
                    return true;
                }
                else
                {
                    Logger.Error("Kết nối thất bại (Timeout).");
                    DisconnectInternal(false);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Kết nối thất bại: {ex.Message}");
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
                while (_client.Connected && _stream != null && !cancellationToken.IsCancellationRequested)
                {
                    // BinaryFormatter.Deserialize sẽ block cho đến khi có dữ liệu
                    // Không cần check DataAvailable, chỉ cần deserialize trực tiếp
                    object receivedPacket = null;
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
            // 1. Các gói tin Login/Register (Dùng TaskCompletionSource)
            if (packet is LoginResultPacket pLogin)
            {
                Logger.Info($"Nhận được LoginResultPacket: Success={pLogin.Success}, UserID={pLogin.UserID}");
                
                TaskCompletionSource<LoginResultPacket> completionSource;
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
                return;
            }
            if (packet is RegisterResultPacket pRegister)
            {
                // Chỉ set result nếu TaskCompletionSource vẫn còn tồn tại và chưa được set
                if (_registerCompletionSource != null && !_registerCompletionSource.Task.IsCompleted)
                {
                    _registerCompletionSource.TrySetResult(pRegister);
                }
                return;
            }

            // 2. Gói tin Quên Mật Khẩu (Dùng Event)
            if (packet is ForgotPasswordResultPacket pForgot)
            {
                OnForgotPasswordResult?.Invoke(pForgot);
                return;
            }

            // 3. Các gói tin cần Update UI trên Home (Dùng Invoke)
            if (_homeForm == null || _homeForm.IsDisposed) return;

            Action action = packet switch
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

        // --- CÁC HÀM GỬI ---

        public async Task<LoginResultPacket> LoginAsync(LoginPacket packet)
        {
            if (!_client.Connected || _stream == null) throw new InvalidOperationException("Chưa kết nối.");
            
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
                var timeoutTask = Task.Delay(10000);
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
                Logger.Error("Timeout: Không nhận được phản hồi đăng nhập sau 10 giây.");
                lock (_loginLock)
                {
                    _loginCompletionSource = null;
                }
                throw new TimeoutException("Không nhận được phản hồi đăng nhập.");
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
            if (!_client.Connected || _stream == null) throw new InvalidOperationException("Chưa kết nối.");
            _registerCompletionSource = new TaskCompletionSource<RegisterResultPacket>();
            SendPacket(packet);
            var timeoutTask = Task.Delay(10000);
            var resultTask = _registerCompletionSource.Task;
            if (await Task.WhenAny(resultTask, timeoutTask) == resultTask) return await resultTask;
            else throw new TimeoutException("Không nhận được phản hồi đăng ký.");
        }

        // *** HÀM QUAN TRỌNG ĐÃ SỬA: Trả về bool ***
        public bool SendPacket(object packet)
        {
            NetworkStream currentStream = _stream;
            // Kiểm tra kỹ trước khi gửi
            if (_client == null || !_client.Connected || currentStream == null)
            {
                Logger.Warning("Lỗi gửi gói tin: Chưa kết nối hoặc stream null.");
                return false; // Báo thất bại
            }

            try
            {
                lock (currentStream) 
                { 
                    _formatter.Serialize(currentStream, packet);
                    currentStream.Flush(); // Đảm bảo dữ liệu được gửi ngay lập tức
                }
                return true; // Gửi thành công
            }
            catch (Exception ex)
            {
                Logger.Error($"Lỗi I/O khi gửi gói tin", ex);
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