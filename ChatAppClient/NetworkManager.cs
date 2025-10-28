#pragma warning disable SYSLIB0011 // Tắt cảnh báo BinaryFormatter (giống Server)

using ChatApp.Shared;
using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatAppClient.Forms
{
    // Lớp này sẽ quản lý MỘT kết nối duy nhất đến Server
    // Chúng ta dùng Singleton Pattern để truy cập nó từ mọi Form
    public class NetworkManager
    {
        #region Singleton
        private static NetworkManager _instance;
        public static NetworkManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new NetworkManager();
                return _instance;
            }
        }
        #endregion

        private TcpClient _client;
        private NetworkStream _stream;
        private BinaryFormatter _formatter;

        // Lưu lại Form Home để điều phối tin nhắn
        private frmHome _homeForm;

        // Thông tin User (lưu lại sau khi login thành công)
        public string UserID { get; private set; }
        public string UserName { get; private set; }

        // 1. THÊM: Biến này dùng để "đợi" kết quả Login
        private TaskCompletionSource<LoginResultPacket> _loginCompletionSource;

        private NetworkManager()
        {
            _client = new TcpClient();
            _formatter = new BinaryFormatter();
        }

        // Đăng ký Form Home để nhận tin
        public void RegisterHomeForm(frmHome homeForm)
        {
            _homeForm = homeForm;
        }

        // Bước 1: Kết nối và Bắt đầu Lắng nghe
        public async Task<bool> ConnectAsync(string ipAddress, int port)
        {
            if (_client.Connected) return true;

            try
            {
                Console.WriteLine("Client: Đang kết nối đến Server...");
                await _client.ConnectAsync(ipAddress, port);
                _stream = _client.GetStream();

                // Bắt đầu 1 luồng riêng để lắng nghe Server
                _ = StartListeningAsync();

                Console.WriteLine("Client: Đã kết nối!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client: Kết nối thất bại: {ex.Message}");
                return false;
            }
        }

        // Bước 2: Luồng Lắng nghe
        private async Task StartListeningAsync()
        {
            try
            {
                while (_client.Connected)
                {
                    // Đợi và nhận gói tin từ Server
                    object receivedPacket = await Task.Run(() => _formatter.Deserialize(_stream));

                    // Xử lý gói tin
                    HandlePacket(receivedPacket);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client: Mất kết nối Server: {ex.Message}");
                MessageBox.Show("Mất kết nối đến máy chủ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit(); // Thoát app nếu mất kết nối
            }
        }

        // 3. THÊM HÀM MỚI: (Để frmLogin gọi)
        public async Task<LoginResultPacket> LoginAsync(LoginPacket packet)
        {
            // Tạo 1 "cầu nối" để đợi kết quả
            _loginCompletionSource = new TaskCompletionSource<LoginResultPacket>();

            // Gửi gói tin đi
            SendPacket(packet);

            // Đợi kết quả hoặc 10 giây (timeout)
            var timeoutTask = Task.Delay(10000);
            var resultTask = _loginCompletionSource.Task;

            if (await Task.WhenAny(resultTask, timeoutTask) == resultTask)
            {
                // Nhận được kết quả
                return await resultTask;
            }
            else
            {
                // Hết giờ
                throw new TimeoutException("Không nhận được phản hồi từ máy chủ.");
            }
        }

        // 4. THÊM HÀM MỚI: (Để lưu thông tin user)
        public void SetUserCredentials(string userId, string userName)
        {
            this.UserID = userId;
            this.UserName = userName;
        }


        // 5. SỬA LẠI HÀM NÀY: (Bộ định tuyến - Router)
        private void HandlePacket(object packet)
        {
            // Ưu tiên 1: Xử lý Login Result
            if (packet is LoginResultPacket pLogin)
            {
                // Gửi kết quả về cho hàm LoginAsync đang "đợi"
                _loginCompletionSource?.TrySetResult(pLogin);
                return; // Xong, không làm gì nữa
            }

            // Nếu chưa login (chưa có _homeForm), thì bỏ qua các gói tin khác
            if (_homeForm == null)
            {
                return;
            }

            // Ưu tiên 2: Điều phối các gói tin khác về frmHome
            // (Phải dùng Invoke để đảm bảo chạy trên luồng UI chính)
            switch (packet)
            {
                case UserStatusPacket p:
                    _homeForm.Invoke(new Action(() => _homeForm.HandleUserStatusUpdate(p)));
                    break;
                case TextPacket p:
                    _homeForm.Invoke(new Action(() => _homeForm.HandleIncomingTextMessage(p)));
                    break;
                case FilePacket p:
                    _homeForm.Invoke(new Action(() => _homeForm.HandleIncomingFileMessage(p)));
                    break;
                case GameInvitePacket p:
                    _homeForm.Invoke(new Action(() => _homeForm.HandleIncomingGameInvite(p)));
                    break;
                case GameResponsePacket p:
                    _homeForm.Invoke(new Action(() => _homeForm.HandleGameResponse(p)));
                    break;
                case GameStartPacket p:
                    _homeForm.Invoke(new Action(() => _homeForm.HandleGameStart(p)));
                    break;
                case GameMovePacket p:
                    _homeForm.Invoke(new Action(() => _homeForm.HandleGameMove(p)));
                    break;
            }
        }

        // Bước 6: Gửi gói tin đi (Giữ nguyên)
        public void SendPacket(object packet)
        {
            if (!_client.Connected)
            {
                MessageBox.Show("Không có kết nối đến máy chủ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                lock (_stream) // Khóa stream khi gửi
                {
                    _formatter.Serialize(_stream, packet);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client: Lỗi gửi gói tin: {ex.Message}");
            }
        }
    }
}
#pragma warning restore SYSLIB0011 // Bật lại cảnh báo