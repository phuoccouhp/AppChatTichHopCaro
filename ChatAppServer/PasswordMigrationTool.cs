using System;
using System.Data.SqlClient;

namespace ChatAppServer
{
    /// <summary>
    /// Tool để migration password từ plain text sang hashed password
    /// Chạy tool này MỘT LẦN sau khi cập nhật code để hash lại tất cả password cũ
    /// </summary>
    public class PasswordMigrationTool
    {
        private readonly string _connectionString = @"Data Source=localhost;Initial Catalog=ChatAppDB;Integrated Security=True";

        /// <summary>
        /// Hash lại tất cả password trong database (từ plain text sang hashed)
        /// CHỈ CHẠY MỘT LẦN sau khi cập nhật code!
        /// </summary>
        public void MigratePasswords()
        {
            Console.WriteLine("Bắt đầu migration password...");
            
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    
                    // Lấy tất cả user có password chưa được hash (không chứa dấu ':')
                    // Password đã hash có format: "salt:hash" nên sẽ có dấu ':'
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
                                
                                // Hash password
                                string hashedPassword = PasswordHelper.HashPassword(plainPassword);
                                
                                // Cập nhật lại password đã hash
                                UpdatePassword(conn, userID, hashedPassword);
                                
                                Console.WriteLine($"Đã hash password cho user: {username} (ID: {userID})");
                                count++;
                            }
                            
                            Console.WriteLine($"\nHoàn thành! Đã hash {count} password.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi migration: {ex.Message}");
                }
            }
        }

        private void UpdatePassword(SqlConnection conn, int userID, string hashedPassword)
        {
            string updateQuery = "UPDATE Users SET Password = @p WHERE UserID = @id";
            using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
            {
                cmd.Parameters.AddWithValue("@p", hashedPassword);
                cmd.Parameters.AddWithValue("@id", userID);
                cmd.ExecuteNonQuery();
            }
        }

        // Hàm main để chạy tool này độc lập (nếu cần)
        // Uncomment để chạy:
        /*
        public static void Main(string[] args)
        {
            var tool = new PasswordMigrationTool();
            Console.WriteLine("CẢNH BÁO: Tool này sẽ hash lại TẤT CẢ password trong database!");
            Console.WriteLine("Nhấn Y để tiếp tục, phím khác để hủy...");
            var key = Console.ReadKey();
            if (key.KeyChar == 'Y' || key.KeyChar == 'y')
            {
                tool.MigratePasswords();
            }
            else
            {
                Console.WriteLine("Đã hủy.");
            }
            Console.WriteLine("Nhấn phím bất kỳ để thoát...");
            Console.ReadKey();
        }
        */
    }
}

