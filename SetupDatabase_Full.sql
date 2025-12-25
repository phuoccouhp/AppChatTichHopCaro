-- ============================================
-- SCRIPT HOÀN CHỈNH SETUP DATABASE CHAT APP + GAME CARO
-- XÓA VÀ TẠO LẠI TẤT CẢ CÁC BẢNG
-- ============================================
USE ChatAppDB;
GO

PRINT '========================================';
PRINT 'BẮT ĐẦU SETUP DATABASE...';
PRINT '========================================';
PRINT '';

-- ============================================
-- BƯỚC 1: XÓA CÁC BẢNG CŨ (NẾU CÓ)
-- ============================================

PRINT 'Đang xóa các bảng cũ...';

-- Xóa bảng GameHistory trước (vì có Foreign Key đến Users)
IF OBJECT_ID('GameHistory', 'U') IS NOT NULL
BEGIN
    DROP TABLE GameHistory;
    PRINT '✓ Đã xóa bảng GameHistory';
END
ELSE
    PRINT '  Bảng GameHistory chưa tồn tại';

-- Xóa bảng Messages (vì có thể có Foreign Key)
IF OBJECT_ID('Messages', 'U') IS NOT NULL
BEGIN
    -- Xóa Foreign Key nếu có
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Messages_Sender')
        ALTER TABLE Messages DROP CONSTRAINT FK_Messages_Sender;
    
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Messages_Receiver')
        ALTER TABLE Messages DROP CONSTRAINT FK_Messages_Receiver;
    
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name LIKE 'FK__Messages__Sender%')
    BEGIN
        DECLARE @FKName NVARCHAR(200);
        SELECT @FKName = name FROM sys.foreign_keys WHERE name LIKE 'FK__Messages__Sender%';
        EXEC('ALTER TABLE Messages DROP CONSTRAINT ' + @FKName);
    END
    
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name LIKE 'FK__Messages__Receiver%')
    BEGIN
        DECLARE @FKName2 NVARCHAR(200);
        SELECT @FKName2 = name FROM sys.foreign_keys WHERE name LIKE 'FK__Messages__Receiver%';
        EXEC('ALTER TABLE Messages DROP CONSTRAINT ' + @FKName2);
    END
    
    DROP TABLE Messages;
    PRINT '✓ Đã xóa bảng Messages';
END
ELSE
    PRINT '  Bảng Messages chưa tồn tại';

-- Xóa bảng GroupMessages trước (vì có thể có Foreign Key)
IF OBJECT_ID('GroupMessages', 'U') IS NOT NULL
BEGIN
    DROP TABLE GroupMessages;
    PRINT '✓ Đã xóa bảng GroupMessages';
END
ELSE
    PRINT '  Bảng GroupMessages chưa tồn tại';

-- Xóa bảng GroupMembers
IF OBJECT_ID('GroupMembers', 'U') IS NOT NULL
BEGIN
    DROP TABLE GroupMembers;
    PRINT '✓ Đã xóa bảng GroupMembers';
END
ELSE
    PRINT '  Bảng GroupMembers chưa tồn tại';

-- Xóa bảng Groups
IF OBJECT_ID('Groups', 'U') IS NOT NULL
BEGIN
    DROP TABLE Groups;
    PRINT '✓ Đã xóa bảng Groups';
END
ELSE
    PRINT '  Bảng Groups chưa tồn tại';

-- Xóa bảng Users
IF OBJECT_ID('Users', 'U') IS NOT NULL
BEGIN
    DROP TABLE Users;
    PRINT '✓ Đã xóa bảng Users';
END
ELSE
    PRINT '  Bảng Users chưa tồn tại';

PRINT '';
GO

-- ============================================
-- BƯỚC 2: TẠO LẠI BẢNG USERS
-- ============================================

PRINT 'Đang tạo bảng Users...';

CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Password NVARCHAR(255) NOT NULL,
    DisplayName NVARCHAR(100) NULL,
    Email NVARCHAR(100) NULL,
    IsOnline BIT DEFAULT 0,                    -- Trạng thái online (0: offline, 1: online)
    LastSeen DATETIME NULL,                    -- Lần cuối hoạt động
    TotalWins INT DEFAULT 0,                   -- Tổng số trận thắng (Caro)
    TotalLosses INT DEFAULT 0,                 -- Tổng số trận thua (Caro)
    TotalDraws INT DEFAULT 0,                  -- Tổng số trận hòa (Caro)
    CreatedAt DATETIME DEFAULT GETDATE()       -- Ngày tạo tài khoản
);
GO

-- Tạo Index cho Username và Email
CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_IsOnline ON Users(IsOnline);
GO

PRINT '✓ Đã tạo bảng Users thành công!';
PRINT '';
GO

-- ============================================
-- BƯỚC 3: TẠO BẢNG MESSAGES
-- ============================================

PRINT 'Đang tạo bảng Messages...';

CREATE TABLE Messages (
    MessageID INT IDENTITY(1,1) PRIMARY KEY,
    SenderID NVARCHAR(50) NOT NULL,
    ReceiverID NVARCHAR(50) NOT NULL,
    MessageContent NVARCHAR(MAX) NOT NULL,
    MessageType VARCHAR(20) DEFAULT 'Text',
    FileName NVARCHAR(255) NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

-- Tạo Index để tìm kiếm nhanh
CREATE INDEX IX_Messages_SenderReceiver ON Messages(SenderID, ReceiverID);
CREATE INDEX IX_Messages_CreatedAt ON Messages(CreatedAt DESC);
CREATE INDEX IX_Messages_SenderID ON Messages(SenderID);
CREATE INDEX IX_Messages_ReceiverID ON Messages(ReceiverID);
GO

-- Thêm Foreign Key (nếu có thể)
BEGIN TRY
    ALTER TABLE Messages
    ADD CONSTRAINT FK_Messages_Sender FOREIGN KEY (SenderID) REFERENCES Users(Username);
    PRINT '✓ Đã thêm Foreign Key FK_Messages_Sender';
END TRY
BEGIN CATCH
    PRINT '  Không thể thêm Foreign Key FK_Messages_Sender: ' + ERROR_MESSAGE();
    PRINT '  (Bảng Messages vẫn hoạt động bình thường)';
END CATCH
GO

BEGIN TRY
    ALTER TABLE Messages
    ADD CONSTRAINT FK_Messages_Receiver FOREIGN KEY (ReceiverID) REFERENCES Users(Username);
    PRINT '✓ Đã thêm Foreign Key FK_Messages_Receiver';
END TRY
BEGIN CATCH
    PRINT '  Không thể thêm Foreign Key FK_Messages_Receiver: ' + ERROR_MESSAGE();
    PRINT '  (Bảng Messages vẫn hoạt động bình thường)';
END CATCH
GO

PRINT '✓ Đã tạo bảng Messages thành công!';
PRINT '';
GO

-- ============================================
-- BƯỚC 4: TẠO BẢNG GAMEHISTORY (LỊCH SỬ GAME CARO)
-- ============================================

PRINT 'Đang tạo bảng GameHistory...';

CREATE TABLE GameHistory (
    GameID INT IDENTITY(1,1) PRIMARY KEY,
    Player1 NVARCHAR(50) NOT NULL,             -- Username người chơi 1 (X)
    Player2 NVARCHAR(50) NOT NULL,             -- Username người chơi 2 (O)
    WinnerUsername NVARCHAR(50) NULL,          -- Username người thắng (NULL nếu hòa)
    GameResult VARCHAR(20) NOT NULL,           -- 'Player1Win', 'Player2Win', 'Draw', 'Abandoned'
    TotalMoves INT DEFAULT 0,                  -- Tổng số nước đi
    GameDuration INT NULL,                     -- Thời gian chơi (giây)
    StartedAt DATETIME DEFAULT GETDATE(),      -- Thời gian bắt đầu
    EndedAt DATETIME NULL                      -- Thời gian kết thúc
);
GO

-- Tạo Index để tìm kiếm nhanh
CREATE INDEX IX_GameHistory_Player1 ON GameHistory(Player1);
CREATE INDEX IX_GameHistory_Player2 ON GameHistory(Player2);
CREATE INDEX IX_GameHistory_Winner ON GameHistory(WinnerUsername);
CREATE INDEX IX_GameHistory_StartedAt ON GameHistory(StartedAt);
GO

PRINT '✓ Đã tạo bảng GameHistory thành công!';
PRINT '';
GO

-- ============================================
-- BƯỚC 5: TẠO BẢNG NHÓM CHAT (GROUPS)
-- ============================================

PRINT 'Đang tạo bảng Groups...';

CREATE TABLE Groups (
    GroupID NVARCHAR(50) PRIMARY KEY,
    GroupName NVARCHAR(100) NOT NULL,
    CreatorID NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

-- Tạo Index cho Groups
CREATE INDEX IX_Groups_CreatorID ON Groups(CreatorID);
CREATE INDEX IX_Groups_CreatedAt ON Groups(CreatedAt DESC);
GO

PRINT '✓ Đã tạo bảng Groups thành công!';
PRINT '';
GO

-- ============================================
-- BƯỚC 6: TẠO BẢNG THÀNH VIÊN NHÓM (GROUPMEMBERS)
-- ============================================

PRINT 'Đang tạo bảng GroupMembers...';

CREATE TABLE GroupMembers (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    GroupID NVARCHAR(50) NOT NULL,
    UserID NVARCHAR(50) NOT NULL,
    IsAdmin BIT DEFAULT 0,
    JoinedAt DATETIME DEFAULT GETDATE(),
    UNIQUE(GroupID, UserID)
);
GO

-- Tạo Index cho GroupMembers
CREATE INDEX IX_GroupMembers_GroupID ON GroupMembers(GroupID);
CREATE INDEX IX_GroupMembers_UserID ON GroupMembers(UserID);
CREATE INDEX IX_GroupMembers_GroupUser ON GroupMembers(GroupID, UserID);
GO

PRINT '✓ Đã tạo bảng GroupMembers thành công!';
PRINT '';
GO

-- ============================================
-- BƯỚC 7: TẠO BẢNG TIN NHẮN NHÓM (GROUPMESSAGES)
-- ============================================

PRINT 'Đang tạo bảng GroupMessages...';

CREATE TABLE GroupMessages (
    MessageID INT IDENTITY(1,1) PRIMARY KEY,
    GroupID NVARCHAR(50) NOT NULL,
    SenderID NVARCHAR(50) NOT NULL,
    MessageContent NVARCHAR(MAX),
    MessageType NVARCHAR(20) DEFAULT 'Text',
    FileName NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

-- Tạo Index cho GroupMessages
CREATE INDEX IX_GroupMessages_GroupID ON GroupMessages(GroupID);
CREATE INDEX IX_GroupMessages_CreatedAt ON GroupMessages(CreatedAt DESC);
CREATE INDEX IX_GroupMessages_SenderID ON GroupMessages(SenderID);
CREATE INDEX IX_GroupMessages_GroupCreated ON GroupMessages(GroupID, CreatedAt DESC);
GO

PRINT '✓ Đã tạo bảng GroupMessages thành công!';
PRINT '';
GO

-- ============================================
-- BƯỚC 8: TẠO CÁC TÀI KHOẢN TEST
-- ============================================

PRINT 'Đang tạo các tài khoản test...';
PRINT '';

-- User 1
INSERT INTO Users (Username, Password, DisplayName, Email, IsOnline, TotalWins, TotalLosses, TotalDraws)
VALUES ('user1', '123', N'Bạn Bè A', NULL, 0, 0, 0, 0);
PRINT '✓ Đã tạo: user1 / 123';

-- User 2
INSERT INTO Users (Username, Password, DisplayName, Email, IsOnline, TotalWins, TotalLosses, TotalDraws)
VALUES ('user2', '123', N'Bạn Bè B', NULL, 0, 0, 0, 0);
PRINT '✓ Đã tạo: user2 / 123';

-- User 3
INSERT INTO Users (Username, Password, DisplayName, Email, IsOnline, TotalWins, TotalLosses, TotalDraws)
VALUES ('user3', '123', N'Bạn Bè C', NULL, 0, 0, 0, 0);
PRINT '✓ Đã tạo: user3 / 123';

-- User 4 (user5 trong database cũ)
INSERT INTO Users (Username, Password, DisplayName, Email, IsOnline, TotalWins, TotalLosses, TotalDraws)
VALUES ('user5', '123', N'Bạn Bè D', NULL, 0, 0, 0, 0);
PRINT '✓ Đã tạo: user5 / 123';

-- Admin
INSERT INTO Users (Username, Password, DisplayName, Email, IsOnline, TotalWins, TotalLosses, TotalDraws)
VALUES ('admin', 'admin', N'Quản Trị Viên', 'admin@chatapp.com', 0, 0, 0, 0);
PRINT '✓ Đã tạo: admin / admin';

-- Test User 1
INSERT INTO Users (Username, Password, DisplayName, Email, IsOnline, TotalWins, TotalLosses, TotalDraws)
VALUES ('test1', 'test123', N'Người Dùng Test 1', 'test1@test.com', 0, 0, 0, 0);
PRINT '✓ Đã tạo: test1 / test123';

-- Test User 2
INSERT INTO Users (Username, Password, DisplayName, Email, IsOnline, TotalWins, TotalLosses, TotalDraws)
VALUES ('test2', 'test123', N'Người Dùng Test 2', 'test2@test.com', 0, 0, 0, 0);
PRINT '✓ Đã tạo: test2 / test123';

-- Huy Phước (nếu cần)
INSERT INTO Users (Username, Password, DisplayName, Email, IsOnline, TotalWins, TotalLosses, TotalDraws)
VALUES ('huyphuoc', '123', N'Huy Phước', 'huyphuoc09112005@gmail.com', 0, 0, 0, 0);
PRINT '✓ Đã tạo: huyphuoc / 123';

-- Huy Phước 1 (nếu cần)
INSERT INTO Users (Username, Password, DisplayName, Email, IsOnline, TotalWins, TotalLosses, TotalDraws)
VALUES ('huyphuoc1', '123123', N'Huy Phước 1', NULL, 0, 0, 0, 0);
PRINT '✓ Đã tạo: huyphuoc1 / 123123';

PRINT '';
GO

-- ============================================
-- BƯỚC 9: HIỂN THỊ KẾT QUẢ
-- ============================================

PRINT '========================================';
PRINT 'DANH SÁCH TẤT CẢ TÀI KHOẢN:';
PRINT '========================================';

SELECT 
    UserID,
    Username,
    Password,
    DisplayName,
    Email,
    IsOnline,
    TotalWins,
    TotalLosses,
    TotalDraws,
    CreatedAt,
    CASE 
        WHEN Password LIKE '%:%' THEN N'Đã hash'
        ELSE N'Chưa hash (sẽ tự động hash khi đăng nhập)'
    END AS PasswordStatus
FROM Users
ORDER BY UserID;

PRINT '';
PRINT '========================================';
PRINT 'CẤU TRÚC CÁC BẢNG ĐÃ TẠO:';
PRINT '========================================';
PRINT '';
PRINT '1. BẢNG USERS:';
PRINT '   - UserID, Username, Password, DisplayName, Email';
PRINT '   - IsOnline (trạng thái online)';
PRINT '   - LastSeen (lần cuối hoạt động)';
PRINT '   - TotalWins, TotalLosses, TotalDraws (thống kê game)';
PRINT '   - CreatedAt (ngày tạo tài khoản)';
PRINT '';
PRINT '2. BẢNG MESSAGES:';
PRINT '   - MessageID, SenderID, ReceiverID';
PRINT '   - MessageContent, MessageType, FileName';
PRINT '   - CreatedAt';
PRINT '';
PRINT '3. BẢNG GAMEHISTORY:';
PRINT '   - GameID, Player1, Player2';
PRINT '   - WinnerUsername, GameResult';
PRINT '   - TotalMoves, GameDuration';
PRINT '   - StartedAt, EndedAt';
PRINT '';
PRINT '4. BẢNG GROUPS (NHÓM CHAT):';
PRINT '   - GroupID, GroupName, CreatorID';
PRINT '   - CreatedAt';
PRINT '';
PRINT '5. BẢNG GROUPMEMBERS (THÀNH VIÊN NHÓM):';
PRINT '   - ID, GroupID, UserID';
PRINT '   - IsAdmin, JoinedAt';
PRINT '';
PRINT '6. BẢNG GROUPMESSAGES (TIN NHẮN NHÓM):';
PRINT '   - MessageID, GroupID, SenderID';
PRINT '   - MessageContent, MessageType, FileName';
PRINT '   - CreatedAt';
PRINT '';
PRINT '========================================';
PRINT 'HOÀN THÀNH SETUP DATABASE!';
PRINT '========================================';
PRINT '';
PRINT 'CÁC TÀI KHOẢN TEST:';
PRINT '1. user1 / 123';
PRINT '2. user2 / 123';
PRINT '3. user3 / 123';
PRINT '4. user5 / 123';
PRINT '5. admin / admin';
PRINT '6. test1 / test123';
PRINT '7. test2 / test123';
PRINT '8. huyphuoc / 123';
PRINT '9. huyphuoc1 / 123123';
PRINT '';
PRINT 'Lưu ý:';
PRINT '- Password sẽ được tự động hash khi đăng nhập lần đầu';
PRINT '- Tất cả các bảng đã được tạo lại hoàn toàn:';
PRINT '  + Users, Messages, GameHistory';
PRINT '  + Groups, GroupMembers, GroupMessages';
PRINT '- Tất cả dữ liệu cũ đã bị xóa';
PRINT '- Tất cả các Indexes đã được tạo để tối ưu hiệu suất';
PRINT '- Sử dụng IsOnline để theo dõi trạng thái online của user';
PRINT '- Sử dụng GameHistory để lưu lịch sử các ván Caro';
PRINT '- Sử dụng Groups/GroupMembers/GroupMessages cho chức năng nhắn tin nhóm';
PRINT '========================================';
GO

