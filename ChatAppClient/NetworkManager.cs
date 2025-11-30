#pragma warning disable SYSLIB0011 

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
        public string UserID { get; private set; }
        public string UserName { get; private set; }
        private TaskCompletionSource<LoginResultPacket> _loginCompletionSource;
        private CancellationTokenSource _listeningCts; 

        private NetworkManager()
        {
            _formatter = new BinaryFormatter();
        }

        public void RegisterHomeForm(frmHome homeForm) => _homeForm = homeForm;

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

                var completedTask = await Task.WhenAny(connectTask, Task.Delay(-1, timeoutCts.Token));

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
            catch (SocketException sockEx) { Logger.Error($"Lỗi Socket (kiểm tra IP/Port/Firewall)", sockEx); DisconnectInternal(false); return false; }
            catch (Exception ex) { Logger.Error($"Kết nối thất bại (Lỗi khác)", ex); DisconnectInternal(false); return false; }
        }

        private async Task StartListeningAsync(CancellationToken cancellationToken)
        {
            Logger.Info("Bắt đầu lắng nghe Server...");
            try
            {
                while (_client.Connected && _stream != null && !cancellationToken.IsCancellationRequested)
                {
                    if (!_stream.DataAvailable) 
                    {
                        await Task.Delay(100, cancellationToken);
                        continue;
                    }
                    object receivedPacket = await Task.Run(() => _formatter.Deserialize(_stream), cancellationToken);
                    HandlePacket(receivedPacket);
                }
            }
            catch (OperationCanceledException) { Logger.Warning("Luồng lắng nghe đã bị hủy."); } 
            catch (IOException ioEx) { Logger.Warning($"Lỗi I/O khi lắng nghe (mất kết nối?): {ioEx.Message}"); }
            catch (SerializationException serEx) { Logger.Error("Lỗi Deserialize gói tin", serEx); }
            catch (Exception ex) { Logger.Error("Lỗi không xác định khi lắng nghe", ex); }
            finally
            {
                Logger.Warning("Dừng lắng nghe.");
                if (!cancellationToken.IsCancellationRequested && _homeForm != null && !_homeForm.IsDisposed)
                {
                    Disconnect(); 
                }
            }
        }

        public async Task<LoginResultPacket> LoginAsync(LoginPacket packet)
        {
            if (!_client.Connected || _stream == null) throw new InvalidOperationException("Chưa kết nối đến Server.");

            _loginCompletionSource = new TaskCompletionSource<LoginResultPacket>();
            SendPacket(packet);

            using var timeoutCts = new CancellationTokenSource(10000); 
            var completedTask = await Task.WhenAny(_loginCompletionSource.Task, Task.Delay(-1, timeoutCts.Token));

            if (completedTask == _loginCompletionSource.Task) return await _loginCompletionSource.Task;
            else throw new TimeoutException("Không nhận được phản hồi đăng nhập từ máy chủ.");
        }

        public void SetUserCredentials(string userId, string userName)
        {
            this.UserID = userId;
            this.UserName = userName;
        }

        // (Chỉ cần thay hàm HandlePacket trong NetworkManager.cs của bạn)

        private void HandlePacket(object packet)
        {
            if (packet is LoginResultPacket pLogin)
            {
                _loginCompletionSource?.TrySetResult(pLogin);
                return;
            }
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

                // --- CASE MỚI ---
                UpdateProfilePacket p => () => _homeForm.HandleUpdateProfile(p),

                _ => null
            };
            action?.Invoke();
        }

        public void SendPacket(object packet)
        {
            NetworkStream currentStream = _stream;
            if (!_client.Connected || currentStream == null)
            {
                Logger.Warning("Lỗi gửi gói tin: Chưa kết nối hoặc stream null.");
                return;
            }

            try
            {
                lock (currentStream) { _formatter.Serialize(currentStream, packet); }
            }
            catch (IOException ioEx)
            {
                Logger.Error($"Lỗi I/O khi gửi gói tin", ioEx);
                Disconnect(); 
            }
            catch (Exception ex)
            {
                Logger.Error($"Lỗi không xác định khi gửi gói tin", ex);
            }
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
            catch {}
            finally
            {
                _client = null;
                _stream = null;
                _listeningCts = null;
                _homeForm = null; 
                UserID = null;    
                UserName = null;
                Logger.Info("Đã ngắt kết nối.");

                if (showMessage && _loginCompletionSource == null) 
                {
                    MessageBox.Show("Đã mất kết nối đến máy chủ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Application.Exit();
                }
            }
        }
    }
}
#pragma warning restore SYSLIB0011 