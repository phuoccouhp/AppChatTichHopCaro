-- ============================================
-- FULL SCRIPT SETUP DATABASE CHAT APP + GAME CARO
-- SQL SERVER (T-SQL)
-- XÓA + TẠO LẠI TOÀN BỘ DATABASE STRUCTURE
-- (ĐÃ LOẠI BỎ PHẦN GROUPCHAT)
-- ============================================
IF DB_ID('ChatAppDB') IS NULL
    CREATE DATABASE ChatAppDB;
GO

USE ChatAppDB;
GO

PRINT '========================================';
PRINT 'BẮT ĐẦU SETUP DATABASE CHAT APP + CARO';
PRINT '========================================';
PRINT '';

/* =====================================================
   BƯỚC 1 - XÓA CÁC BẢNG CŨ (THEO ĐÚNG THỨ TỰ FK)
===================================================== */

PRINT 'Đang xóa các bảng cũ...';

IF OBJECT_ID('GameHistory', 'U') IS NOT NULL DROP TABLE GameHistory;
IF OBJECT_ID('Messages', 'U') IS NOT NULL DROP TABLE Messages;
IF OBJECT_ID('Users', 'U') IS NOT NULL DROP TABLE Users;
GO

PRINT '✓ Đã xóa xong toàn bộ bảng cũ';
PRINT '';

/* =====================================================
   BƯỚC 2 - TẠO BẢNG USERS
===================================================== */

CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Password NVARCHAR(255) NOT NULL,
    DisplayName NVARCHAR(100),
    Email NVARCHAR(100),
    IsOnline BIT DEFAULT 0,
    LastSeen DATETIME NULL,
    TotalWins INT DEFAULT 0,
    TotalLosses INT DEFAULT 0,
    TotalDraws INT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

CREATE INDEX IX_Users_Username ON Users(Username);
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_IsOnline ON Users(IsOnline);
GO

PRINT '✓ Đã tạo bảng Users';

/* =====================================================
   BƯỚC 3 - TẠO BẢNG MESSAGES
   (Chú ý: phần groupchat đã bị loại bỏ; Messages giữ cho
    nhắn 1-1 giữa các Username)
===================================================== */

CREATE TABLE Messages (
    MessageID INT IDENTITY(1,1) PRIMARY KEY,
    SenderID NVARCHAR(50) NOT NULL,
    ReceiverID NVARCHAR(50) NOT NULL,
    MessageContent NVARCHAR(MAX) NOT NULL,
    MessageType VARCHAR(20) DEFAULT 'Text',
    FileName NVARCHAR(255),
    -- IsGroupMessage cũ đã bỏ vì bảng Groups/GroupMembers không còn
    CreatedAt DATETIME DEFAULT GETDATE()
);
GO

CREATE INDEX IX_Messages_SenderReceiver ON Messages(SenderID, ReceiverID);
CREATE INDEX IX_Messages_CreatedAt ON Messages(CreatedAt);
GO

ALTER TABLE Messages
ADD CONSTRAINT FK_Messages_Sender
FOREIGN KEY (SenderID) REFERENCES Users(Username);

ALTER TABLE Messages
ADD CONSTRAINT FK_Messages_Receiver
FOREIGN KEY (ReceiverID) REFERENCES Users(Username);
GO

PRINT '✓ Đã tạo bảng Messages';

/* =====================================================
   BƯỚC 4 - TẠO BẢNG GAME HISTORY (CARO)
===================================================== */

CREATE TABLE GameHistory (
    GameID INT IDENTITY(1,1) PRIMARY KEY,
    Player1 NVARCHAR(50) NOT NULL,
    Player2 NVARCHAR(50) NOT NULL,
    WinnerUsername NVARCHAR(50),
    GameResult VARCHAR(20) NOT NULL, -- Player1Win | Player2Win | Draw
    TotalMoves INT DEFAULT 0,
    GameDuration INT,
    StartedAt DATETIME DEFAULT GETDATE(),
    EndedAt DATETIME
);
GO

CREATE INDEX IX_GameHistory_Player1 ON GameHistory(Player1);
CREATE INDEX IX_GameHistory_Player2 ON GameHistory(Player2);
CREATE INDEX IX_GameHistory_Winner ON GameHistory(WinnerUsername);
GO

PRINT '✓ Đã tạo bảng GameHistory';

/* =====================================================
   BƯỚC 5 - INSERT USER TEST
===================================================== */

INSERT INTO Users (Username, Password, DisplayName, Email) VALUES
('user1','123',N'Bạn Bè A',NULL),
('user2','123',N'Bạn Bè B',NULL),
('user3','123',N'Bạn Bè C',NULL),
('user5','123',N'Bạn Bè D',NULL),
('admin','admin',N'Quản Trị Viên','admin@chatapp.com'),
('test1','test123',N'Người Dùng Test 1','test1@test.com'),
('test2','test123',N'Người Dùng Test 2','test2@test.com'),
('huyphuoc','123',N'Huy Phước','huyphuoc09112005@gmail.com'),
('huyphuoc1','123123',N'Huy Phước 1',NULL);
GO

PRINT '✓ Đã insert tài khoản test';

/* =====================================================
   BƯỚC 6 - HIỂN THỊ KẾT QUẢ
   (ĐÃ LOẠI BỎ HIỂN THỊ GROUPS / GROUP MEMBERS)
===================================================== */

PRINT '';
PRINT '========== USERS =========='; 
SELECT * FROM Users;

PRINT '========== MESSAGES =========='; 
SELECT * FROM Messages;

PRINT '========== GAME HISTORY =========='; 
SELECT * FROM GameHistory;

PRINT '';
PRINT '========================================';
PRINT 'SETUP DATABASE HOÀN TẤT 🚀';
PRINT '========================================';
GO
