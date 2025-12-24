using ChatApp.Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace ChatAppServer
{
    public class DatabaseManager
    {
        private static DatabaseManager? _instance;
        public static DatabaseManager Instance => _instance ??= new DatabaseManager();

        private string _connectionString = "";
        public string ConnectionString => _connectionString;

        private volatile bool _dbAvailable = false;

        private DatabaseManager()
        {
            // ==================================================================================
            // BƯỚC QUAN TRỌNG: SỬA TÊN SERVER TẠI ĐÂY
            // 1. Nếu dùng bản Full mặc định: để là "." hoặc "localhost"
            // 2. Nếu có tên cụ thể (VD: DESKTOP-ABC\SQL2019): điền y hệt vào
            // ==================================================================================

            string myServerName = "."; // <--- HÃY SỬA DÒNG NÀY (Ví dụ: @".\SQL2019" hoặc @"DESKTOP-ABC")
            string targetDB = "ChatAppDB"; // ✅ Sử dụng database đã có sẵn

            // ==================================================================================

            Logger.Info($"[Database] Đang kết nối tới SQL Server: {myServerName} ...");

            try
            {
                // ✅ BƯỚC 1: Kết nối vào "master" trước để kiểm tra/tạo Database
                string masterConnectionString = $@"Data Source={myServerName};Initial Catalog=master;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=5";
                
                using (var conn = new SqlConnection(masterConnectionString))
                {
                    conn.Open();
                    Logger.Success("[Database] Kết nối SQL Server THÀNH CÔNG!");
                    
                    // Kiểm tra database tồn tại không
                    string checkDb = $"SELECT database_id FROM sys.databases WHERE Name = '{targetDB}'";
                    using (var cmd = new SqlCommand(checkDb, conn))
                    {
                        if (cmd.ExecuteScalar() == null)
                        {
                            // ✅ TẠO DATABASE NẾU CHƯA CÓ
                            Logger.Warning($"[Database] Database '{targetDB}' chưa tồn tại. Đang tạo mới...");
                            using (var createCmd = new SqlCommand($"CREATE DATABASE [{targetDB}]", conn))
                            {
                                createCmd.ExecuteNonQuery();
                                Logger.Success($"[Database] Đã tạo Database '{targetDB}' thành công!");
                            }
                            
                            // Chờ một chút để database sẵn sàng
                            System.Threading.Thread.Sleep(1000);
                        }
                        else
                        {
                            Logger.Info($"[Database] Database '{targetDB}' đã tồn tại.");
                        }
                    }
                }

                // ✅ BƯỚC 2: Bây giờ mới tạo chuỗi kết nối đến ChatDB
                _connectionString = $@"Data Source={myServerName};Initial Catalog={targetDB};Integrated Security=True;TrustServerCertificate=True";
                
                // Thử kết nối để xác nhận
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                }

                _dbAvailable = true;
                Logger.Success($"[Database] Đã kết nối thành công đến '{targetDB}'!");
                InitializeDatabase();
            }
            catch (Exception ex)
            {
                Logger.Error($"[Database] LỖI KẾT NỐI: {ex.Message}");
                Logger.Warning($"LƯU Ý: Hãy kiểm tra xem:");
                Logger.Warning($"  1. Tên Server '{myServerName}' đã đúng chưa?");
                Logger.Warning($"  2. SQL Server đã chạy chưa? (Mở Services.msc -> SQL Server)");
                Logger.Warning($"  3. User Windows có quyền truy cập SQL Server không?");
            }
        }

        private void InitializeDatabase()
        {
            if (!_dbAvailable) return;
            try
            {
                // Tạo Bảng (Tables) nếu chưa có
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string script = @"
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
                        CREATE TABLE Users (
                            UserID INT PRIMARY KEY IDENTITY(1,1),
                            Username NVARCHAR(50) UNIQUE NOT NULL,
                            Password NVARCHAR(255) NOT NULL,
                            Email NVARCHAR(100),
                            DisplayName NVARCHAR(50),
                            IsOnline BIT DEFAULT 0,
                            LastLogin DATETIME DEFAULT GETDATE()
                        );

                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Messages')
                        CREATE TABLE Messages (
                            MessageID INT PRIMARY KEY IDENTITY(1,1),
                            SenderID NVARCHAR(50),
                            ReceiverID NVARCHAR(50),
                            MessageContent NVARCHAR(MAX),
                            MessageType NVARCHAR(20) DEFAULT 'Text',
                            FileName NVARCHAR(255),
                            CreatedAt DATETIME DEFAULT GETDATE()
                        );
                    ";
                    using (var cmd = new SqlCommand(script, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    Logger.Success("[Database] Đã khởi tạo các bảng Users và Messages!");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"[Database] Lỗi khởi tạo bảng: {ex.Message}");
            }
        }

        // --- CÁC HÀM XỬ LÝ DỮ LIỆU ---

        public User? Login(string usernameOrEmail, string password, bool useEmail)
        {
            if (!_dbAvailable) return null;
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = useEmail
                        ? "SELECT * FROM Users WHERE Email = @u"
                        : "SELECT * FROM Users WHERE Username = @u";

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@u", usernameOrEmail);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedPass = reader["Password"].ToString();
                                string realUser = reader["Username"].ToString();
                                string display = reader["DisplayName"] != DBNull.Value ? reader["DisplayName"].ToString() : realUser;
                                string email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : "";

                                // Kiểm tra mật khẩu (Hỗ trợ cả Plain text và Hash)
                                bool isValid = false;
                                if (storedPass.Contains(":"))
                                {
                                    isValid = VerifyPasswordInternal(password, storedPass);
                                }
                                else
                                {
                                    isValid = (storedPass == password);
                                    // Tự động nâng cấp lên Hash nếu đang dùng plain text
                                    if (isValid) UpdatePassword(email, password);
                                }

                                if (isValid)
                                {
                                    return new User { Username = realUser, DisplayName = display, Email = email };
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Logger.Error($"Login Error: {ex.Message}"); }
            return null;
        }

        public bool RegisterUser(string username, string password, string email)
        {
            if (!_dbAvailable) return false;
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    // Check tồn tại
                    using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Username = @u OR Email = @e", conn))
                    {
                        cmd.Parameters.AddWithValue("@u", username);
                        cmd.Parameters.AddWithValue("@e", email ?? "");
                        if ((int)cmd.ExecuteScalar() > 0) return false;
                    }

                    // Hash password
                    string hashedPassword = HashPasswordInternal(password);

                    // Insert
                    string insert = "INSERT INTO Users (Username, Password, Email, DisplayName, IsOnline) VALUES (@u, @p, @e, @u, 1)";
                    using (var cmd = new SqlCommand(insert, conn))
                    {
                        cmd.Parameters.AddWithValue("@u", username);
                        cmd.Parameters.AddWithValue("@p", hashedPassword);
                        cmd.Parameters.AddWithValue("@e", email ?? DBNull.Value.ToString());
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Register Error: {ex.Message}");
                return false;
            }
        }

        public List<UserStatus> GetContacts(string excludeUserID)
        {
            var list = new List<UserStatus>();
            if (!_dbAvailable) return list;
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT Username, DisplayName, IsOnline FROM Users WHERE Username != @uid";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", excludeUserID);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new UserStatus
                                {
                                    UserID = reader["Username"].ToString(),
                                    UserName = reader["DisplayName"].ToString(),
                                    IsOnline = reader["IsOnline"] != DBNull.Value && (bool)reader["IsOnline"]
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Logger.Error($"GetContacts Error: {ex.Message}"); }
            return list;
        }

        public int SaveMessage(string sender, string receiver, string content, string type = "Text", string fileName = null)
        {
            if (!_dbAvailable) return 0;
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO Messages (SenderID, ReceiverID, MessageContent, MessageType, FileName) 
                                     OUTPUT INSERTED.MessageID
                                     VALUES (@s, @r, @c, @t, @f)";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@s", sender);
                        cmd.Parameters.AddWithValue("@r", receiver);
                        cmd.Parameters.AddWithValue("@c", content ?? "");
                        cmd.Parameters.AddWithValue("@t", type);
                        cmd.Parameters.AddWithValue("@f", (object)fileName ?? DBNull.Value);
                        return (int)cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"SaveMessage Error: {ex.Message}");
                return 0;
            }
        }

        public List<ChatHistoryMessage> GetChatHistory(string u1, string u2, int limit)
        {
            var list = new List<ChatHistoryMessage>();
            if (!_dbAvailable) return list;
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"SELECT TOP (@l) * FROM Messages 
                                     WHERE (SenderID=@u1 AND ReceiverID=@u2) OR (SenderID=@u2 AND ReceiverID=@u1) 
                                     ORDER BY CreatedAt DESC";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@u1", u1);
                        cmd.Parameters.AddWithValue("@u2", u2);
                        cmd.Parameters.AddWithValue("@l", limit);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new ChatHistoryMessage
                                {
                                    MessageID = (int)reader["MessageID"],
                                    SenderID = reader["SenderID"].ToString(),
                                    ReceiverID = reader["ReceiverID"].ToString(),
                                    MessageContent = reader["MessageContent"].ToString(),
                                    MessageType = reader["MessageType"].ToString(),
                                    FileName = reader["FileName"] != DBNull.Value ? reader["FileName"].ToString() : null,
                                    CreatedAt = (DateTime)reader["CreatedAt"]
                                });
                            }
                        }
                    }
                }
                list.Reverse();
            }
            catch { }
            return list;
        }

        public void UpdateUserOnlineStatus(string username, bool isOnline)
        {
            if (!_dbAvailable) return;
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("UPDATE Users SET IsOnline = @o WHERE Username = @u", conn))
                    {
                        cmd.Parameters.AddWithValue("@o", isOnline);
                        cmd.Parameters.AddWithValue("@u", username);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }

        public void UpdateDisplayName(string userId, string newName)
        {
            if (!_dbAvailable) return;
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("UPDATE Users SET DisplayName = @n WHERE Username = @u", conn))
                    {
                        cmd.Parameters.AddWithValue("@n", newName);
                        cmd.Parameters.AddWithValue("@u", userId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }

        public void UpdateMessage(int msgId, string newContent)
        {
            if (!_dbAvailable) return;
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("UPDATE Messages SET MessageContent = @c WHERE MessageID = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@c", newContent);
                        cmd.Parameters.AddWithValue("@id", msgId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }

        public bool CheckEmailExists(string email)
        {
            if (!_dbAvailable) return false;
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Users WHERE Email = @e", conn))
                    {
                        cmd.Parameters.AddWithValue("@e", email);
                        return (int)cmd.ExecuteScalar() > 0;
                    }
                }
            }
            catch { return false; }
        }

        public void UpdatePassword(string email, string newPass)
        {
            if (!_dbAvailable) return;
            try
            {
                string hash = HashPasswordInternal(newPass);
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("UPDATE Users SET Password = @p WHERE Email = @e", conn))
                    {
                        cmd.Parameters.AddWithValue("@p", hash);
                        cmd.Parameters.AddWithValue("@e", email);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }

        // --- PASSWORD UTILS ---
        private string HashPasswordInternal(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes) + ":SHA256";
            }
        }

        private bool VerifyPasswordInternal(string inputPassword, string storedHash)
        {
            try
            {
                var parts = storedHash.Split(':');
                if (parts.Length != 2) return inputPassword == storedHash;

                using (var sha256 = SHA256.Create())
                {
                    var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(inputPassword));
                    string newHash = Convert.ToBase64String(bytes);
                    return newHash == parts[0];
                }
            }
            catch { return false; }
        }
    }
}