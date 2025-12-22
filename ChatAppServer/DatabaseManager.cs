using System;
using System.Data.SqlClient;

namespace ChatAppServer
{
    public class DatabaseManager
    {
        // CHUỖI KẾT NỐI (CONNECTION STRING)
        // Bạn cần sửa lại cho đúng với máy của bạn
        // Server=Tên_Máy_Của_Bạn; Database=ChatAppDB; Trusted_Connection=True; (Nếu dùng Windows Auth)
        // Hoặc: Server=...; User Id=sa; Password=...; (Nếu dùng SQL Auth)
        private readonly string _connectionString = @"Data Source=localhost;Initial Catalog=ChatAppDB;Integrated Security=True";
        private static DatabaseManager? _instance;
        public static DatabaseManager Instance => _instance ??= new DatabaseManager();

        private DatabaseManager() { }

        // Hàm kiểm tra đăng nhập
        public UserData? Login(string username, string password)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT Username, DisplayName FROM Users WHERE Username = @u AND Password = @p";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        // Dùng tham số (@u, @p) để chống hack SQL Injection
                        cmd.Parameters.AddWithValue("@u", username);
                        cmd.Parameters.AddWithValue("@p", password);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Tìm thấy user!
                                return new UserData
                                {
                                    Username = reader["Username"].ToString(),
                                    DisplayName = reader["DisplayName"].ToString()
                                };
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Lỗi kết nối CSDL", ex);
                }
            }
            return null; // Không tìm thấy hoặc lỗi
        }
        // (Trong class DatabaseManager)
        public void UpdateDisplayName(string username, string newDisplayName)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "UPDATE Users SET DisplayName = @name WHERE Username = @u";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", newDisplayName);
                        cmd.Parameters.AddWithValue("@u", username);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Lỗi cập nhật tên User", ex);
                }
            }
        }
        // (Trong class DatabaseManager)

        public bool RegisterUser(string username, string password, string email)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    // Kiểm tra xem user đã tồn tại chưa
                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @u";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@u", username);
                        int count = (int)checkCmd.ExecuteScalar();
                        if (count > 0) return false; // Username đã tồn tại
                    }

                    // Nếu chưa, thêm mới
                    // Mặc định Tên hiển thị (DisplayName) sẽ giống Username
                    string insertQuery = "INSERT INTO Users (Username, Password, DisplayName, Email) VALUES (@u, @p, @d, @e)";
                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@u", username);
                        cmd.Parameters.AddWithValue("@p", password);
                        cmd.Parameters.AddWithValue("@d", username); // DisplayName mặc định
                        cmd.Parameters.AddWithValue("@e", email);
                        cmd.ExecuteNonQuery();
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Error("Lỗi Đăng ký User", ex);
                    return false;
                }
            }
        }
        // (Trong DatabaseManager.cs)

        // Kiểm tra email có tồn tại không
        public bool CheckEmailExists(string email)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM Users WHERE Email = @e";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@e", email);
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }

        // Cập nhật mật khẩu mới
        public void UpdatePassword(string email, string newPassword)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "UPDATE Users SET Password = @p WHERE Email = @e";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@p", newPassword);
                    cmd.Parameters.AddWithValue("@e", email);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }


    // Class nhỏ để chứa dữ liệu trả về
    public class UserData
    {
        public string? Username { get; set; }
        public string? DisplayName { get; set; }
    }
}