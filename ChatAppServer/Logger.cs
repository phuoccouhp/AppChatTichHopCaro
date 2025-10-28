using System;

namespace ChatAppServer
{
    // Lớp tĩnh để ghi log có màu
    public static class Logger
    {
        // Khóa để tránh các luồng ghi đè màu của nhau
        private static readonly object _lock = new object();

        public static void Info(string message)
        {
            Log(message, ConsoleColor.Gray);
        }

        public static void Success(string message)
        {
            Log(message, ConsoleColor.Green); // Màu xanh lá
        }

        public static void Warning(string message)
        {
            Log(message, ConsoleColor.Yellow); // Màu vàng
        }

        public static void Error(string message, Exception ex = null)
        {
            Log($"LỖI: {message}" + (ex != null ? $"\n{ex.Message}" : ""),
                ConsoleColor.Red); // Màu đỏ
        }

        private static void Log(string message, ConsoleColor color)
        {
            lock (_lock)
            {
                Console.ForegroundColor = color;
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
                Console.ResetColor(); // Trả lại màu mặc định
            }
        }
    }
}