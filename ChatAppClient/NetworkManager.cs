#pragma warning disable SYSLIB0011 // Tắt cảnh báo BinaryFormatter

using ChatApp.Shared;
using ChatAppClient.Helpers; // <-- THÊM USING LOGGER
using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading; // Thêm
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
        public string UserID { get; private set; }
        public string UserName { get; private set; }
        private TaskCompletionSource<LoginResultPacket> _loginCompletionSource;
        private CancellationTokenSource _listeningCts; // Để hủy luồng lắng nghe

        private NetworkManager()
        {
            _formatter = new BinaryFormatter();
            // Khởi tạo _client trong ConnectAsync để dễ retry
        }

        public void RegisterHomeForm(frmHome homeForm) => _homeForm = homeForm;

        public async Task<bool> ConnectAsync(string ipAddress, int port)
        {
            // Trả về true nếu đã kết nối và stream hợp lệ
            if (_client != null && _client.Connected && _stream != null) return true;

            // Đảm bảo client cũ đã được dọn dẹp
            DisconnectInternal(false); // Ngắt kết nối cũ nếu có (không thông báo)

            try
            {
                _client = new TcpClient(); // Tạo client mới
                Logger.Info($"Đang kết nối đến {ipAddress}:{port}...");

                var connectTask = _client.ConnectAsync(ipAddress, port);
                using var timeoutCts = new CancellationTokenSource(5000); // 5 giây timeout

                // Chờ kết nối hoặc timeout
                var completedTask = await Task.WhenAny(connectTask, Task.Delay(-1, timeoutCts.Token));

                if (completedTask == connectTask)
                {
                    // Kết nối thành công (không timeout)
                    await connectTask; // Đảm bảo không có lỗi ngầm khi kết nối

                    // *** SỬA LỖI NullRef: Thêm try-catch khi lấy stream ***
                    try
                    {
                        _stream = _client.GetStream();
                        if (_stream == null) throw new IOException("NetworkStream trả về null.");
                    }
                    catch (Exception streamEx)
                    {
                        Logger.Error("Lỗi khi lấy NetworkStream", streamEx);
                        DisconnectInternal(false); // Dọn dẹp
                        return false;
                    }
                    // *** KẾT THÚC SỬA ***

                    _listeningCts = new CancellationTokenSource(); // Tạo token mới để hủy
                    _ = StartListeningAsync(_listeningCts.Token); // Bắt đầu lắng nghe
                    Logger.Success("Đã kết nối!");
                    return true;
                }
                else
                {
                    // Kết nối timeout
                    Logger.Error("Kết nối thất bại (Timeout).");
                    DisconnectInternal(false); // Dọn dẹp
                    return false;
                }
            }
            catch (SocketException sockEx) { Logger.Error($"Lỗi Socket (kiểm tra IP/Port/Firewall)", sockEx); DisconnectInternal(false); return false; }
            catch (Exception ex) { Logger.Error($"Kết nối thất bại (Lỗi khác)", ex); DisconnectInternal(false); return false; }
        }

        private async Task StartListeningAsync(CancellationToken cancellationToken)
        {
            Logger.Info("Bắt đầu lắng nghe Server...");
            try
            {
                // Dùng vòng lặp an toàn hơn với CancellationToken
                while (_client.Connected && _stream != null && !cancellationToken.IsCancellationRequested)
                {
                    if (!_stream.DataAvailable) // Kiểm tra có dữ liệu không trước khi đọc
                    {
                        await Task.Delay(100, cancellationToken); // Đợi 100ms
                        continue;
                    }
                    object receivedPacket = await Task.Run(() => _formatter.Deserialize(_stream), cancellationToken);
                    HandlePacket(receivedPacket);
                }
            }
            catch (OperationCanceledException) { Logger.Warning("Luồng lắng nghe đã bị hủy."); } // Bị hủy bởi Disconnect
            catch (IOException ioEx) { Logger.Warning($"Lỗi I/O khi lắng nghe (mất kết nối?): {ioEx.Message}"); }
            catch (SerializationException serEx) { Logger.Error("Lỗi Deserialize gói tin", serEx); }
            catch (Exception ex) { Logger.Error("Lỗi không xác định khi lắng nghe", ex); }
            finally
            {
                Logger.Warning("Dừng lắng nghe.");
                // Chỉ thoát nếu lỗi xảy ra khi HomeForm còn hiển thị (không phải do người dùng chủ động disconnect)
                if (!cancellationToken.IsCancellationRequested && _homeForm != null && !_homeForm.IsDisposed)
                {
                    // Nên gọi hàm Disconnect để xử lý đồng bộ
                    Disconnect(); // Thông báo lỗi và thoát
                }
            }
        }

        public async Task<LoginResultPacket> LoginAsync(LoginPacket packet)
        {
            if (!_client.Connected || _stream == null) throw new InvalidOperationException("Chưa kết nối đến Server.");

            _loginCompletionSource = new TaskCompletionSource<LoginResultPacket>();
            SendPacket(packet);

            using var timeoutCts = new CancellationTokenSource(10000); // 10 giây timeout
            var completedTask = await Task.WhenAny(_loginCompletionSource.Task, Task.Delay(-1, timeoutCts.Token));

            if (completedTask == _loginCompletionSource.Task) return await _loginCompletionSource.Task;
            else throw new TimeoutException("Không nhận được phản hồi đăng nhập từ máy chủ.");
        }

        public void SetUserCredentials(string userId, string userName)
        {
            this.UserID = userId;
            this.UserName = userName;
        }

        private void HandlePacket(object packet)
        {
            if (packet is LoginResultPacket pLogin)
            {
                _loginCompletionSource?.TrySetResult(pLogin);
                return;
            }
            if (_homeForm == null || _homeForm.IsDisposed) return;

            // Dùng BeginInvoke để không chặn luồng lắng nghe
            Action action = packet switch
            {
                UserStatusPacket p => () => _homeForm.HandleUserStatusUpdate(p),
                TextPacket p => () => _homeForm.HandleIncomingTextMessage(p),
                FilePacket p => () => _homeForm.HandleIncomingFileMessage(p),
                GameInvitePacket p => () => _homeForm.HandleIncomingGameInvite(p),
                GameResponsePacket p => () => _homeForm.HandleGameResponse(p),
                GameStartPacket p => () => _homeForm.HandleGameStart(p),
                GameMovePacket p => () => _homeForm.HandleGameMove(p),
                _ => null
            };
            action?.Invoke(); // Gọi trực tiếp nếu cùng luồng, Invoke nếu khác luồng
        }

        public void SendPacket(object packet)
        {
            // *** SỬA LỖI NullRef: Kiểm tra _stream trước khi lock ***
            NetworkStream currentStream = _stream; // Tạo biến cục bộ để kiểm tra
            if (!_client.Connected || currentStream == null)
            {
                Logger.Warning("Lỗi gửi gói tin: Chưa kết nối hoặc stream null.");
                // Cân nhắc hiển thị lỗi cho người dùng ở đây nếu cần thiết
                return;
            }
            // *** KẾT THÚC SỬA ***

            try
            {
                // Lock stream đã kiểm tra
                lock (currentStream) { _formatter.Serialize(currentStream, packet); }
            }
            // Chia nhỏ lỗi để biết rõ hơn
            catch (IOException ioEx) // Lỗi I/O (stream bị đóng...)
            {
                Logger.Error($"Lỗi I/O khi gửi gói tin", ioEx);
                Disconnect(); // Ngắt kết nối nếu không gửi được
            }
            catch (Exception ex)
            {
                Logger.Error($"Lỗi không xác định khi gửi gói tin", ex);
                // Cân nhắc ngắt kết nối tùy theo lỗi
            }
        }

        // Hàm ngắt kết nối công khai (ví dụ: khi người dùng logout)
        public void Disconnect()
        {
            DisconnectInternal(true); // Ngắt và thông báo
        }

        // Hàm ngắt kết nối nội bộ
        private void DisconnectInternal(bool showMessage)
        {
            try
            {
                _listeningCts?.Cancel(); // Yêu cầu luồng lắng nghe dừng lại
                _stream?.Close();
                _client?.Close();
            }
            catch { /* Bỏ qua lỗi khi đóng */ }
            finally
            {
                _client = null;
                _stream = null;
                _listeningCts = null;
                _homeForm = null; // Hủy đăng ký home form
                UserID = null;    // Reset thông tin user
                UserName = null;
                Logger.Info("Đã ngắt kết nối.");

                // Chỉ hiện MessageBox nếu ngắt kết nối do lỗi (không phải do timeout lúc connect)
                if (showMessage && _loginCompletionSource == null) // Nếu loginCompletionSource null tức là chưa login hoặc đã login xong
                {
                    MessageBox.Show("Đã mất kết nối đến máy chủ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    // Cân nhắc việc thoát ứng dụng hoặc quay về màn hình Login
                    Application.Exit();
                }
            }
        }
    }
}
#pragma warning restore SYSLIB0011 // Bật lại cảnh báo