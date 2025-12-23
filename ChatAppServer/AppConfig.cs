using Microsoft.Extensions.Configuration;
using System.IO;
using System.Windows.Forms;

namespace ChatAppServer
{
    /// <summary>
    /// Class quản lý cấu hình ứng dụng từ file appsettings.json
    /// Mỗi máy chỉ cần thay đổi file appsettings.json mà không cần sửa code
    /// </summary>
    public static class AppConfig
    {
        private static IConfiguration? _configuration;
        private static readonly string AppSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        private static readonly string ExamplePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.example.json");

        /// <summary>
        /// Khởi tạo configuration từ file appsettings.json
        /// Gọi hàm này một lần khi ứng dụng khởi động
        /// </summary>
        public static void Initialize()
        {
            // Kiểm tra nếu chưa có appsettings.json
            if (!File.Exists(AppSettingsPath))
            {
                // Nếu có file example thì copy
                if (File.Exists(ExamplePath))
                {
                    File.Copy(ExamplePath, AppSettingsPath);
                    MessageBox.Show(
                        "Đã tạo file appsettings.json từ template.\n\n" +
                        "Vui lòng mở file appsettings.json và sửa Data Source phù hợp với máy của bạn:\n\n" +
                        "• SQL Server mặc định: localhost\n" +
                        "• SQL Express: localhost\\SQLEXPRESS\n" +
                        "• LocalDB: (localdb)\\MSSQLLocalDB\n\n" +
                        "Sau đó khởi động lại ứng dụng.",
                        "Cấu hình Database",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    // Tạo file mặc định với SQLEXPRESS (phổ biến nhất)
                    CreateDefaultConfig();
                    MessageBox.Show(
                        "Đã tạo file appsettings.json với cấu hình mặc định (SQL Express).\n\n" +
                        "Nếu máy bạn dùng SQL Server khác, vui lòng sửa Data Source trong file appsettings.json:\n\n" +
                        "• SQL Server mặc định: localhost\n" +
                        "• SQL Express: localhost\\SQLEXPRESS\n" +
                        "• LocalDB: (localdb)\\MSSQLLocalDB\n\n" +
                        "Sau đó khởi động lại ứng dụng.",
                        "Cấu hình Database",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = builder.Build();
        }

        /// <summary>
        /// Tạo file cấu hình mặc định (SQL Express - phổ biến nhất)
        /// </summary>
        private static void CreateDefaultConfig()
        {
            string defaultConfig = @"{
  ""ConnectionStrings"": {
    ""ChatAppDB"": ""Data Source=localhost\\SQLEXPRESS;Initial Catalog=ChatAppDB;Integrated Security=True""
  },
  ""AppSettings"": {
    ""ServerPort"": 9000
  }
}";
            File.WriteAllText(AppSettingsPath, defaultConfig);
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
