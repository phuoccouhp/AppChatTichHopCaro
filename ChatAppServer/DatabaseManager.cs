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

                        -- ✅ BẢNG NHÓM CHAT
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Groups')
                        CREATE TABLE Groups (
                            GroupID NVARCHAR(50) PRIMARY KEY,
                            GroupName NVARCHAR(100) NOT NULL,
                            CreatorID NVARCHAR(50) NOT NULL,
                            CreatedAt DATETIME DEFAULT GETDATE()
                        );

                        -- ✅ BẢNG THÀNH VIÊN NHÓM
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GroupMembers')
                        CREATE TABLE GroupMembers (
                            ID INT PRIMARY KEY IDENTITY(1,1),
                            GroupID NVARCHAR(50) NOT NULL,
                            UserID NVARCHAR(50) NOT NULL,
                            IsAdmin BIT DEFAULT 0,
                            JoinedAt DATETIME DEFAULT GETDATE(),
                            UNIQUE(GroupID, UserID)
                        );

                        -- ✅ BẢNG TIN NHẮN NHÓM
                        IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GroupMessages')
                        CREATE TABLE GroupMessages (
                            MessageID INT PRIMARY KEY IDENTITY(1,1),
                            GroupID NVARCHAR(50) NOT NULL,
                            SenderID NVARCHAR(50) NOT NULL,
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
                    Logger.Success("[Database] Đã khởi tạo các bảng Users, Messages, Groups, GroupMembers, GroupMessages!");
                    
                    // ✅ THÊM INDEXES ĐỂ TỐI ƯU HIỆU SUẤT
                    string indexScript = @"
                        -- Indexes cho Users
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Username' AND object_id = OBJECT_ID('Users'))
                            CREATE INDEX IX_Users_Username ON Users(Username);
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Email' AND object_id = OBJECT_ID('Users'))
                            CREATE INDEX IX_Users_Email ON Users(Email);
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_IsOnline' AND object_id = OBJECT_ID('Users'))
                            CREATE INDEX IX_Users_IsOnline ON Users(IsOnline);
                        
                        -- Indexes cho Messages
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Messages_SenderReceiver' AND object_id = OBJECT_ID('Messages'))
                            CREATE INDEX IX_Messages_SenderReceiver ON Messages(SenderID, ReceiverID);
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Messages_CreatedAt' AND object_id = OBJECT_ID('Messages'))
                            CREATE INDEX IX_Messages_CreatedAt ON Messages(CreatedAt DESC);
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Messages_SenderID' AND object_id = OBJECT_ID('Messages'))
                            CREATE INDEX IX_Messages_SenderID ON Messages(SenderID);
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Messages_ReceiverID' AND object_id = OBJECT_ID('Messages'))
                            CREATE INDEX IX_Messages_ReceiverID ON Messages(ReceiverID);
                        
                        -- Indexes cho Groups
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Groups_CreatorID' AND object_id = OBJECT_ID('Groups'))
                            CREATE INDEX IX_Groups_CreatorID ON Groups(CreatorID);
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Groups_CreatedAt' AND object_id = OBJECT_ID('Groups'))
                            CREATE INDEX IX_Groups_CreatedAt ON Groups(CreatedAt DESC);
                        
                        -- Indexes cho GroupMembers
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GroupMembers_GroupID' AND object_id = OBJECT_ID('GroupMembers'))
                            CREATE INDEX IX_GroupMembers_GroupID ON GroupMembers(GroupID);
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GroupMembers_UserID' AND object_id = OBJECT_ID('GroupMembers'))
                            CREATE INDEX IX_GroupMembers_UserID ON GroupMembers(UserID);
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GroupMembers_GroupUser' AND object_id = OBJECT_ID('GroupMembers'))
                            CREATE INDEX IX_GroupMembers_GroupUser ON GroupMembers(GroupID, UserID);
                        
                        -- Indexes cho GroupMessages
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GroupMessages_GroupID' AND object_id = OBJECT_ID('GroupMessages'))
                            CREATE INDEX IX_GroupMessages_GroupID ON GroupMessages(GroupID);
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GroupMessages_CreatedAt' AND object_id = OBJECT_ID('GroupMessages'))
                            CREATE INDEX IX_GroupMessages_CreatedAt ON GroupMessages(CreatedAt DESC);
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GroupMessages_SenderID' AND object_id = OBJECT_ID('GroupMessages'))
                            CREATE INDEX IX_GroupMessages_SenderID ON GroupMessages(SenderID);
                        IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GroupMessages_GroupCreated' AND object_id = OBJECT_ID('GroupMessages'))
                            CREATE INDEX IX_GroupMessages_GroupCreated ON GroupMessages(GroupID, CreatedAt DESC);
                    ";
                    using (var indexCmd = new SqlCommand(indexScript, conn))
                    {
                        indexCmd.ExecuteNonQuery();
                    }
                    Logger.Success("[Database] Đã thêm các Indexes để tối ưu hiệu suất!");
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
                    // ✅ [FIX] Chỉ lấy những người đã từng nhắn tin với user hiện tại
                    string query = @"
                        SELECT DISTINCT u.Username, u.DisplayName, u.IsOnline
                        FROM Users u
                        WHERE u.Username != @uid
                        AND (
                            EXISTS (SELECT 1 FROM Messages m WHERE m.SenderID = @uid AND m.ReceiverID = u.Username)
                            OR EXISTS (SELECT 1 FROM Messages m WHERE m.SenderID = u.Username AND m.ReceiverID = @uid)
                        )";
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

        // =====================================================
        // === QUẢN LÝ NHÓM CHAT ===
        // =====================================================

        public string CreateGroup(string groupName, string creatorId, List<string> memberIds)
        {
            if (!_dbAvailable) return null;
            try
            {
                string groupId = Guid.NewGuid().ToString("N").Substring(0, 12);
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    
                    // Tạo nhóm
                    using (var cmd = new SqlCommand("INSERT INTO Groups (GroupID, GroupName, CreatorID) VALUES (@gid, @name, @creator)", conn))
                    {
                        cmd.Parameters.AddWithValue("@gid", groupId);
                        cmd.Parameters.AddWithValue("@name", groupName);
                        cmd.Parameters.AddWithValue("@creator", creatorId);
                        cmd.ExecuteNonQuery();
                    }
                    
                    // Thêm creator là admin
                    using (var cmd = new SqlCommand("INSERT INTO GroupMembers (GroupID, UserID, IsAdmin) VALUES (@gid, @uid, 1)", conn))
                    {
                        cmd.Parameters.AddWithValue("@gid", groupId);
                        cmd.Parameters.AddWithValue("@uid", creatorId);
                        cmd.ExecuteNonQuery();
                    }
                    
                    // Thêm các thành viên khác
                    foreach (var memberId in memberIds)
                    {
                        if (memberId == creatorId) continue;
                        using (var cmd = new SqlCommand("INSERT INTO GroupMembers (GroupID, UserID, IsAdmin) VALUES (@gid, @uid, 0)", conn))
                        {
                            cmd.Parameters.AddWithValue("@gid", groupId);
                            cmd.Parameters.AddWithValue("@uid", memberId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                Logger.Success($"[Group] Đã tạo nhóm '{groupName}' (ID: {groupId}) với {memberIds.Count + 1} thành viên");
                return groupId;
            }
            catch (Exception ex)
            {
                Logger.Error($"[Group] Lỗi tạo nhóm: {ex.Message}");
                return null;
            }
        }

        public List<GroupMemberInfo> GetGroupMembers(string groupId)
        {
            var list = new List<GroupMemberInfo>();
            if (!_dbAvailable) return list;
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"SELECT gm.UserID, gm.IsAdmin, u.DisplayName, u.IsOnline 
                                     FROM GroupMembers gm 
                                     JOIN Users u ON gm.UserID = u.Username 
                                     WHERE gm.GroupID = @gid";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@gid", groupId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new GroupMemberInfo
                                {
                                    UserID = reader["UserID"].ToString(),
                                    UserName = reader["DisplayName"].ToString(),
                                    IsAdmin = reader["IsAdmin"] != DBNull.Value && (bool)reader["IsAdmin"],
                                    IsOnline = reader["IsOnline"] != DBNull.Value && (bool)reader["IsOnline"]
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Logger.Error($"[Group] Lỗi lấy thành viên: {ex.Message}"); }
            return list;
        }

        public List<GroupInfo> GetUserGroups(string userId)
        {
            var list = new List<GroupInfo>();
            if (!_dbAvailable) return list;
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT g.GroupID, g.GroupName, 
                               (SELECT COUNT(*) FROM GroupMembers WHERE GroupID = g.GroupID) AS MemberCount,
                               (SELECT TOP 1 MessageContent FROM GroupMessages WHERE GroupID = g.GroupID ORDER BY CreatedAt DESC) AS LastMessage,
                               (SELECT TOP 1 CreatedAt FROM GroupMessages WHERE GroupID = g.GroupID ORDER BY CreatedAt DESC) AS LastMessageTime
                        FROM Groups g
                        JOIN GroupMembers gm ON g.GroupID = gm.GroupID
                        WHERE gm.UserID = @uid
                        ORDER BY LastMessageTime DESC";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", userId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new GroupInfo
                                {
                                    GroupID = reader["GroupID"].ToString(),
                                    GroupName = reader["GroupName"].ToString(),
                                    MemberCount = (int)reader["MemberCount"],
                                    LastMessage = reader["LastMessage"] != DBNull.Value ? reader["LastMessage"].ToString() : null,
                                    LastMessageTime = reader["LastMessageTime"] != DBNull.Value ? (DateTime?)reader["LastMessageTime"] : null
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { Logger.Error($"[Group] Lỗi lấy danh sách nhóm: {ex.Message}"); }
            return list;
        }

        public int SaveGroupMessage(string groupId, string senderId, string content, string type = "Text", string fileName = null)
        {
            if (!_dbAvailable) return 0;
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"INSERT INTO GroupMessages (GroupID, SenderID, MessageContent, MessageType, FileName) 
                                     OUTPUT INSERTED.MessageID
                                     VALUES (@gid, @sid, @content, @type, @file)";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@gid", groupId);
                        cmd.Parameters.AddWithValue("@sid", senderId);
                        cmd.Parameters.AddWithValue("@content", content ?? "");
                        cmd.Parameters.AddWithValue("@type", type);
                        cmd.Parameters.AddWithValue("@file", (object)fileName ?? DBNull.Value);
                        return (int)cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"[Group] Lỗi lưu tin nhắn: {ex.Message}");
                return 0;
            }
        }

        public List<GroupMessageHistory> GetGroupHistory(string groupId, int limit)
        {
            var list = new List<GroupMessageHistory>();
            if (!_dbAvailable) return list;
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"SELECT TOP (@l) gm.MessageID, gm.SenderID, u.DisplayName AS SenderName, 
                                            gm.MessageContent, gm.MessageType, gm.FileName, gm.CreatedAt
                                     FROM GroupMessages gm
                                     JOIN Users u ON gm.SenderID = u.Username
                                     WHERE gm.GroupID = @gid
                                     ORDER BY gm.CreatedAt DESC";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@gid", groupId);
                        cmd.Parameters.AddWithValue("@l", limit);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                list.Add(new GroupMessageHistory
                                {
                                    MessageID = (int)reader["MessageID"],
                                    SenderID = reader["SenderID"].ToString(),
                                    SenderName = reader["SenderName"].ToString(),
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
            catch (Exception ex) { Logger.Error($"[Group] Lỗi lấy lịch sử: {ex.Message}"); }
            return list;
        }

        public bool AddGroupMember(string groupId, string userId)
        {
            if (!_dbAvailable) return false;
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("INSERT INTO GroupMembers (GroupID, UserID, IsAdmin) VALUES (@gid, @uid, 0)", conn))
                    {
                        cmd.Parameters.AddWithValue("@gid", groupId);
                        cmd.Parameters.AddWithValue("@uid", userId);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch { return false; }
        }

        public bool RemoveGroupMember(string groupId, string userId)
        {
            if (!_dbAvailable) return false;
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("DELETE FROM GroupMembers WHERE GroupID = @gid AND UserID = @uid", conn))
                    {
                        cmd.Parameters.AddWithValue("@gid", groupId);
                        cmd.Parameters.AddWithValue("@uid", userId);
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                }
            }
            catch { return false; }
        }

        public bool IsGroupMember(string groupId, string userId)
        {
            if (!_dbAvailable) return false;
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("SELECT COUNT(*) FROM GroupMembers WHERE GroupID = @gid AND UserID = @uid", conn))
                    {
                        cmd.Parameters.AddWithValue("@gid", groupId);
                        cmd.Parameters.AddWithValue("@uid", userId);
                        return (int)cmd.ExecuteScalar() > 0;
                    }
                }
            }
            catch { return false; }
        }

        public string GetGroupName(string groupId)
        {
            if (!_dbAvailable) return null;
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("SELECT GroupName FROM Groups WHERE GroupID = @gid", conn))
                    {
                        cmd.Parameters.AddWithValue("@gid", groupId);
                        return cmd.ExecuteScalar()?.ToString();
                    }
                }
            }
            catch { return null; }
        }

        public string GetDisplayName(string userId)
        {
            if (!_dbAvailable) return userId;
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("SELECT DisplayName FROM Users WHERE Username = @uid", conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", userId);
                        var result = cmd.ExecuteScalar();
                        return result?.ToString() ?? userId;
                    }
                }
            }
            catch { return userId; }
        }
    }
}