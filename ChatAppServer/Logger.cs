using System;
using System.Drawing; // Thư viện để dùng màu sắc (Color)

namespace ChatAppServer
{
    public static class Logger
    {
        // KHAI BÁO SỰ KIỆN: Để Form đăng ký nhận tin nhắn
        // Action<Nội dung, Màu sắc>
        public static event Action<string, Color> OnLogReceived;

        public static void Info(string message)
        {
            // Tin thường: Màu trắng (hoặc đen tùy nền form của bạn)
            Log(message, Color.White);
        }

        public static void Success(string message)
        {
            // Tin thành công: Màu xanh lá sáng
            Log(message, Color.LimeGreen);
        }

        public static void Warning(string message)
        {
            // Cảnh báo: Màu vàng
            Log(message, Color.Yellow);
        }

        public static void Error(string message, Exception ex = null)
        {
            // Lỗi: Màu đỏ
            string errorMsg = $"LỖI: {message}" + (ex != null ? $"\n{ex.Message}" : "");
            Log(errorMsg, Color.Red);
        }

        // Hàm nội bộ để xử lý chung
        private static void Log(string message, Color color)
        {
            // Định dạng thời gian
            string finalMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";

            // BẮN SỰ KIỆN RA NGOÀI (Thay vì Console.WriteLine)
            // Dấu ? có nghĩa là: nếu không có Form nào đang nghe thì không làm gì cả
            OnLogReceived?.Invoke(finalMessage, color);
        }
    }
}