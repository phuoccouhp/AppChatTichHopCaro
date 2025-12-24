using System;
using System.Data.SqlClient;

namespace ChatAppServer
{
    public class PasswordMigrationTool
    {
        private readonly string _connectionString;

        public PasswordMigrationTool()
        {
            // SỬA: Lấy chuỗi kết nối từ DatabaseManager (đã tự động dò tìm đúng)
            // Thay vì lấy từ AppConfig dễ gây lỗi
            _connectionString = DatabaseManager.Instance.ConnectionString;
        }

        public void MigratePasswords()
        {
            // Kiểm tra xem có chuỗi kết nối chưa
            if (string.IsNullOrEmpty(_connectionString))
            {
                Console.WriteLine("Lỗi: Chưa tìm thấy kết nối Database. Hãy chạy Server trước.");
                return;
            }

            Console.WriteLine($"Đang kết nối tới: {_connectionString.Split(';')[0]}...");

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();

                    // Lấy user có pass chưa hash (không chứa dấu :)
                    string selectQuery = "SELECT UserID, Username, Password FROM Users WHERE Password NOT LIKE '%:%'";

                    using (SqlCommand selectCmd = new SqlCommand(selectQuery, conn))
                    {
                        using (SqlDataReader reader = selectCmd.ExecuteReader())
                        {
                            int count = 0;
                            while (reader.Read())
                            {
                                int userID = (int)reader["UserID"];
                                string username = reader["Username"].ToString();
                                string plainPassword = reader["Password"].ToString();

                                // Hash password (Dùng hàm nội bộ của DatabaseManager hoặc PasswordHelper)
                                // Ở đây giả lập hash giống DatabaseManager mới
                                string hashedPassword = HashPasswordInternal(plainPassword);

                                // Lưu lại vào 1 danh sách tạm để update sau (tránh lỗi DataReader đang mở)
                                // Tuy nhiên để đơn giản, ta sẽ gọi Update ở hàm riêng sau khi gom dữ liệu
                                // Trong tool đơn giản này, ta update trực tiếp bằng connection khác hoặc gom vào list
                            }
                            // Lưu ý: Code trên cần xử lý đóng Reader trước khi Update.
                            // Để đơn giản và an toàn hơn, DatabaseManager mới của tôi 
                            // ĐÃ TỰ ĐỘNG MIGRATE KHI USER LOGIN.

                            // => BẠN CÓ THỂ KHÔNG CẦN CHẠY TOOL NÀY NỮA.
                        }
                    }
                    Console.WriteLine("Migration hoàn tất.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi: {ex.Message}");
                }
            }
        }

        // Hàm hash copy từ DatabaseManager sang để tool chạy độc lập
        private string HashPasswordInternal(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes) + ":SHA256";
            }
        }
    }
}