-- ============================================
-- SCRIPT HOÀN CHỈNH SETUP DATABASE CHAT APP
-- XÓA VÀ TẠO LẠI CẢ BẢNG USERS VÀ MESSAGES
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

-- Xóa bảng Messages trước (vì có thể có Foreign Key)
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
    Email NVARCHAR(100) NULL
);
GO

-- Tạo Index cho Username và Email
CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_Users_Email ON Users(Email);
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
CREATE INDEX IX_Messages_CreatedAt ON Messages(CreatedAt);
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
-- BƯỚC 4: TẠO CÁC TÀI KHOẢN TEST
-- ============================================

PRINT 'Đang tạo các tài khoản test...';
PRINT '';

-- User 1
INSERT INTO Users (Username, Password, DisplayName, Email)
VALUES ('user1', '123', N'Bạn Bè A', NULL);
PRINT '✓ Đã tạo: user1 / 123';

-- User 2
INSERT INTO Users (Username, Password, DisplayName, Email)
VALUES ('user2', '123', N'Bạn Bè B', NULL);
PRINT '✓ Đã tạo: user2 / 123';

-- User 3
INSERT INTO Users (Username, Password, DisplayName, Email)
VALUES ('user3', '123', N'Bạn Bè C', NULL);
PRINT '✓ Đã tạo: user3 / 123';

-- User 4 (user5 trong database cũ)
INSERT INTO Users (Username, Password, DisplayName, Email)
VALUES ('user5', '123', N'Bạn Bè B', NULL);
PRINT '✓ Đã tạo: user5 / 123';

-- Admin
INSERT INTO Users (Username, Password, DisplayName, Email)
VALUES ('admin', 'admin', N'Quản Trị Viên', 'admin@chatapp.com');
PRINT '✓ Đã tạo: admin / admin';

-- Test User 1
INSERT INTO Users (Username, Password, DisplayName, Email)
VALUES ('test1', 'test123', N'Người Dùng Test 1', 'test1@test.com');
PRINT '✓ Đã tạo: test1 / test123';

-- Test User 2
INSERT INTO Users (Username, Password, DisplayName, Email)
VALUES ('test2', 'test123', N'Người Dùng Test 2', 'test2@test.com');
PRINT '✓ Đã tạo: test2 / test123';

-- Huy Phước (nếu cần)
INSERT INTO Users (Username, Password, DisplayName, Email)
VALUES ('huyphuoc', '123', N'huyphuoc', 'huyphuoc09112005@gmail.com');
PRINT '✓ Đã tạo: huyphuoc / 123';

-- Huy Phước 1 (nếu cần)
INSERT INTO Users (Username, Password, DisplayName, Email)
VALUES ('huyphuoc1', '123123', N'huyphuoc1', 'gg');
PRINT '✓ Đã tạo: huyphuoc1 / 123123';

PRINT '';
GO

-- ============================================
-- BƯỚC 5: HIỂN THỊ KẾT QUẢ
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
    CASE 
        WHEN Password LIKE '%:%' THEN 'Đã hash'
        ELSE 'Chưa hash (sẽ tự động hash khi đăng nhập)'
    END AS PasswordStatus
FROM Users
ORDER BY UserID;

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
PRINT '- Bảng Users và Messages đã được tạo lại hoàn toàn';
PRINT '- Tất cả dữ liệu cũ đã bị xóa';
PRINT '========================================';
GO

