using Microsoft.Extensions.Configuration;
using System.IO;

namespace ChatAppServer
{
    /// <summary>
    /// Class quản lý cấu hình ứng dụng từ file appsettings.json
    /// Mỗi máy chỉ cần thay đổi file appsettings.json mà không cần sửa code
    /// </summary>
    public static class AppConfig
    {
        private static IConfiguration? _configuration;

        /// <summary>
        /// Khởi tạo configuration từ file appsettings.json
        /// Gọi hàm này một lần khi ứng dụng khởi động
        /// </summary>
        public static void Initialize()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
        }

        /// <summary>
        /// Lấy connection string từ file cấu hình
        /// </summary>
        /// <param name="name">Tên connection string (mặc định: "ChatAppDB")</param>
        /// <returns>Connection string</returns>
        public static string GetConnectionString(string name = "ChatAppDB")
        {
            if (_configuration == null)
            {
                Initialize();
            }
            
            return _configuration!.GetConnectionString(name) 
                ?? throw new InvalidOperationException($"Connection string '{name}' không tìm thấy trong appsettings.json");
        }

        /// <summary>
        /// Lấy giá trị cấu hình theo key
        /// </summary>
        /// <param name="key">Key của cấu hình (ví dụ: "AppSettings:ServerPort")</param>
        /// <returns>Giá trị cấu hình</returns>
        public static string? GetValue(string key)
        {
            if (_configuration == null)
            {
                Initialize();
            }
            
            return _configuration![key];
        }

        /// <summary>
        /// Lấy giá trị cấu hình và convert sang kiểu T
        /// </summary>
        public static T GetValue<T>(string key, T defaultValue)
        {
            if (_configuration == null)
            {
                Initialize();
            }
            
            var value = _configuration![key];
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}

