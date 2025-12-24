#pragma warning disable SYSLIB0011 
using ChatApp.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatAppClient.Forms
{
    public class NetworkManager
    {
        private static NetworkManager? _instance;
        public static NetworkManager Instance => _instance ??= new NetworkManager();

        private TcpClient? _client;
        private NetworkStream? _stream;
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
        private readonly object _streamLock = new object();
        private Queue<UserStatusPacket> _pendingStatusPackets = new Queue<UserStatusPacket>();

        private JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        private NetworkManager() { }

        public void RegisterHomeForm(frmHome homeForm)
        {
            _homeForm = homeForm;
            while (_pendingStatusPackets.Count > 0)
            {
                var packet = _pendingStatusPackets.Dequeue();
                SafeInvokeHomeForm(() => _homeForm.HandleUserStatusUpdate(packet));
            }
            SendPacket(new RequestOnlineListPacket());
        }

        private void SafeInvokeHomeForm(Action action)
        {
            if (_homeForm != null && !_homeForm.IsDisposed && _homeForm.IsHandleCreated)
            {
                try { _homeForm.BeginInvoke(action); } catch { }
            }
        }

        // === [FIX: CONNECTION LOGIC] ===
        public async Task<bool> ConnectAsync(string ipAddress, int port)
        {
            // Nếu đã kết nối đúng IP/Port thì không cần kết nối lại
            if (_client != null && _client.Connected && CurrentServerIP == ipAddress && CurrentServerPort == port)
                return true;

            DisconnectInternal(false); // Reset sạch sẽ trước khi kết nối mới

            var client = new TcpClient();
            _client = client;
            try
            {
                // Configure socket options BEFORE connecting
                client.ReceiveBufferSize = 131072;
                client.SendBufferSize = 131072;
                client.NoDelay = true;

                // Set keepalive to prevent firewall timeouts
                try
                {
                    client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                    client.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 60000);
                }
                catch { }

                // Connect with timeout
                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None))
                {
                    cts.CancelAfter(TimeSpan.FromSeconds(10));
                    await client.ConnectAsync(ipAddress, port, cts.Token);
                }

                // Check connection after connect attempt
                if (client.Connected)
                {
                    lock (_streamLock)
                    {
                        _stream = client.GetStream();
                    }
                    CurrentServerIP = ipAddress;
                    CurrentServerPort = port;

                    // ✅ Start listening FIRST before any communication
                    _listeningCts = new CancellationTokenSource();
                    _ = StartListeningAsync(_listeningCts.Token);

                    Logger.Success($"Đã kết nối đến {ipAddress}:{port}");
                    return true;
                }
            }
            catch (OperationCanceledException)
            {
                Logger.Error($"Timeout: Không thể kết nối đến {ipAddress}:{port} trong 10 giây");
            }
            catch (Exception ex)
            {
                Logger.Error($"Không thể kết nối đến {ipAddress}: {ex}");
            }

            DisconnectInternal(false);
            return false;
        }

        private async Task StartListeningAsync(CancellationToken token)
        {
            byte[] lenBuf = new byte[4];
            while (!token.IsCancellationRequested && _client != null && _client.Connected && _stream != null)
            {
                try
                {
                    // ✅ CHECK NULL trước khi dùng
                    if (_stream == null)
                    {
                        Logger.Warning("[Listening] Stream is null, stopping listen loop.");
                        break;
                    }

                    // Đọc độ dài (4 bytes) - với timeout để prevent indefinite hanging
                    int read = 0;
                    while (read < 4)
                    {
                        // ✅ CHECK NULL trong vòng lặp
                        if (_stream == null || !_client.Connected)
                        {
                            Logger.Warning("[Listening] Connection lost while reading length.");
                            throw new EndOfStreamException();
                        }

                        // Add read timeout to prevent indefinite waiting (especially over network)
                        using (var cts = CancellationTokenSource.CreateLinkedTokenSource(token))
                        {
                            cts.CancelAfter(TimeSpan.FromSeconds(30)); // 30s timeout per read
                            try
                            {
                                int r = await _stream.ReadAsync(lenBuf, read, 4 - read, cts.Token);
                                if (r == 0) throw new EndOfStreamException();
                                read += r;
                            }
                            catch (OperationCanceledException) when (cts.Token.IsCancellationRequested && !token.IsCancellationRequested)
                            {
                                // ReadAsync timeout, not main token cancelled
                                Logger.Warning("[Listening] ReadAsync timeout - server may be unresponsive");
                                throw new EndOfStreamException();
                            }
                        }
                    }
                    int len = BitConverter.ToInt32(lenBuf, 0);

                    // ✅ Validate packet size (chặn packet quá lớn)
                    if (len <= 0 || len > 10 * 1024 * 1024) // 10MB max
                    {
                        Logger.Error($"[Listening] Invalid packet size: {len}");
                        break;
                    }

                    // Đọc payload
                    byte[] payload = new byte[len];
                    int offset = 0;
                    while (offset < len)
                    {
                        // ✅ CHECK NULL trước khi ReadAsync
                        if (_stream == null || !_client.Connected)
                        {
                            Logger.Warning("[Listening] Connection lost while reading payload.");
                            throw new EndOfStreamException();
                        }

                        // Add read timeout to prevent indefinite waiting
                        using (var cts = CancellationTokenSource.CreateLinkedTokenSource(token))
                        {
                            cts.CancelAfter(TimeSpan.FromSeconds(30)); // 30s timeout per read
                            try
                            {
                                int r = await _stream.ReadAsync(payload, offset, len - offset, cts.Token);
                                if (r == 0) throw new EndOfStreamException();
                                offset += r;
                            }
                            catch (OperationCanceledException) when (cts.Token.IsCancellationRequested && !token.IsCancellationRequested)
                            {
                                // ReadAsync timeout, not main token cancelled
                                Logger.Warning("[Listening] ReadAsync timeout while reading payload");
                                throw new EndOfStreamException();
                            }
                        }
                    }

                    // Deserialize JSON
                    string jsonString = Encoding.UTF8.GetString(payload);
                    var wrapper = JsonSerializer.Deserialize<PacketWrapper>(jsonString, _jsonOptions);

                    if (wrapper != null && !string.IsNullOrEmpty(wrapper.Type))
                    {
                        Type type = PacketMapper.GetPacketType(wrapper.Type);
                        if (type != null)
                        {
                            object packet = JsonSerializer.Deserialize(wrapper.Payload, type, _jsonOptions);
                            if (packet != null)
                            {
                                HandlePacket(packet);
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Token bị cancel
                    Logger.Info("[Listening] Listening cancelled by token.");
                    break;
                }
                catch (EndOfStreamException)
                {
                    // Server đóng kết nối bình thường
                    Logger.Warning("[Listening] End of stream - server closed connection.");
                    break;
                }
                catch (IOException ioEx)
                {
                    // Network error
                    Logger.Warning($"[Listening] IO Error: {ioEx.Message}");
                    break;
                }
                catch (SocketException sockEx)
                {
                    // Network socket error (timeout, connection reset, etc.)
                    Logger.Error($"[Listening] Socket Error: {sockEx.SocketErrorCode} - {sockEx.Message}");
                    break;
                }
                catch (Exception ex)
                {
                    // Lỗi khác
                    Logger.Error($"[Listening] Error: {ex.Message}", ex);
                    break;
                }
            }
            
            Logger.Info("[Listening] Listen loop ended, disconnecting...");
            DisconnectInternal(true);
        }

        private void HandlePacket(object packet)
        {
            // Async Responses
            if (packet is LoginResultPacket pLogin) { lock (_loginLock) _loginCompletionSource?.TrySetResult(pLogin); return; }
            if (packet is RegisterResultPacket pReg) { _registerCompletionSource?.TrySetResult(pReg); return; }
            if (packet is ForgotPasswordResultPacket pForgot) { OnForgotPasswordResult?.Invoke(pForgot); return; }
            if (packet is ChatHistoryResponsePacket pHist) { _chatHistoryCompletionSource?.TrySetResult(pHist); OnChatHistoryReceived?.Invoke(pHist); return; }

            // UI Updates
            if (packet is UserStatusPacket pStatus && _homeForm == null) { _pendingStatusPackets.Enqueue(pStatus); return; }
            if (_homeForm == null || _homeForm.IsDisposed) return;

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
                OnlineListPacket p => () => _homeForm.HandleOnlineListUpdate(p),
                _ => null
            };
            if (action != null) SafeInvokeHomeForm(action);
        }

        public bool SendPacket(object packet)
        {
            if (_client == null || !_client.Connected || _stream == null) return false;
            try
            {
                string typeName = packet.GetType().Name; // Lấy tên class (VD: LoginPacket)
                string payloadJson = JsonSerializer.Serialize(packet, packet.GetType(), _jsonOptions);
                var wrapper = new PacketWrapper { Type = typeName, Payload = payloadJson };
                string wrapperJson = JsonSerializer.Serialize(wrapper, _jsonOptions);
                byte[] data = Encoding.UTF8.GetBytes(wrapperJson);
                byte[] lenBuf = BitConverter.GetBytes(data.Length);

                // Log outgoing raw payload (truncated)
                try
                {
                    string snippet = wrapperJson.Length > 1000 ? wrapperJson.Substring(0, 1000) + "..." : wrapperJson;
                    Logger.Info($"[SEND RAW] {CurrentServerIP}:{CurrentServerPort} -> {snippet}");
                }
                catch { }

                lock (_streamLock)
                {
                    if (_stream == null) return false;
                    _stream.Write(lenBuf, 0, 4);
                    _stream.Write(data, 0, data.Length);
                    _stream.Flush();
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"[SEND ERROR] Failed to send packet: {ex}");
                DisconnectInternal(true);
                return false;
            }
        }

        public async Task<LoginResultPacket> LoginAsync(LoginPacket packet)
        {
            _loginCompletionSource = new TaskCompletionSource<LoginResultPacket>();
            if (!SendPacket(packet))
            {
                // Trả về gói lỗi thay vì ném Exception để caller xử lý theo luồng bình thường
                return new LoginResultPacket { Success = false, Message = "Gửi yêu cầu đăng nhập thất bại." };
            }

            // Tăng timeout lên 10s hoặc hơn để đảm bảo không bị timeout ảo
            if (await Task.WhenAny(_loginCompletionSource.Task, Task.Delay(10000)) == _loginCompletionSource.Task)
                return await _loginCompletionSource.Task;
            return new LoginResultPacket { Success = false, Message = "Server không phản hồi yêu cầu đăng nhập (Timeout)." };
        }

        public async Task<RegisterResultPacket> RegisterAsync(RegisterPacket packet)
        {
            _registerCompletionSource = new TaskCompletionSource<RegisterResultPacket>();
            if (!SendPacket(packet))
            {
                return new RegisterResultPacket { Success = false, Message = "Gửi yêu cầu đăng ký thất bại." };
            }

            if (await Task.WhenAny(_registerCompletionSource.Task, Task.Delay(10000)) == _registerCompletionSource.Task)
                return await _registerCompletionSource.Task;
            return new RegisterResultPacket { Success = false, Message = "Server không phản hồi yêu cầu đăng ký (Timeout)." };
        }

        public async Task<ChatHistoryResponsePacket> RequestChatHistoryAsync(string friendID, int limit = 100)
        {
            if (string.IsNullOrEmpty(UserID)) return new ChatHistoryResponsePacket { Success = false };
            _chatHistoryCompletionSource = new TaskCompletionSource<ChatHistoryResponsePacket>();
            SendPacket(new ChatHistoryRequestPacket { UserID = UserID, FriendID = friendID, Limit = limit });

            if (await Task.WhenAny(_chatHistoryCompletionSource.Task, Task.Delay(5000)) == _chatHistoryCompletionSource.Task)
                return await _chatHistoryCompletionSource.Task;
            return new ChatHistoryResponsePacket { Success = false, Message = "Timeout" };
        }

        public void SetUserCredentials(string id, string name) { UserID = id; UserName = name; }
        public void Disconnect() { DisconnectInternal(true); }

        private void DisconnectInternal(bool notify)
        {
            lock (_streamLock)
            {
                try { _listeningCts?.Cancel(); _client?.Close(); } catch { }
                _client = null; _stream = null; _listeningCts = null;
            }

            if (notify && _homeForm != null && !_homeForm.IsDisposed && _homeForm.IsHandleCreated)
                SafeInvokeHomeForm(() => MessageBox.Show("Mất kết nối máy chủ.", "Lỗi Mạng", MessageBoxButtons.OK, MessageBoxIcon.Warning));

            UserID = null; UserName = null;
        }
    }
}