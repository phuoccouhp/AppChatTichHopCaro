-- ============================================
-- SCRIPT CẬP NHẬT DATABASE ChatAppDB
-- Thêm Indexes và tối ưu hóa hiệu suất
-- ============================================

USE ChatAppDB;
GO

PRINT '========================================';
PRINT 'BẮT ĐẦU CẬP NHẬT DATABASE...';
PRINT '========================================';
GO

-- ============================================
-- 1. THÊM INDEXES CHO BẢNG Users
-- ============================================
PRINT '';
PRINT 'Đang thêm Indexes cho bảng Users...';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Username' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE INDEX IX_Users_Username ON Users(Username);
    PRINT '✓ Đã tạo Index IX_Users_Username';
END
ELSE
    PRINT '  Index IX_Users_Username đã tồn tại';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Email' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE INDEX IX_Users_Email ON Users(Email);
    PRINT '✓ Đã tạo Index IX_Users_Email';
END
ELSE
    PRINT '  Index IX_Users_Email đã tồn tại';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_IsOnline' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE INDEX IX_Users_IsOnline ON Users(IsOnline);
    PRINT '✓ Đã tạo Index IX_Users_IsOnline';
END
ELSE
    PRINT '  Index IX_Users_IsOnline đã tồn tại';

GO

-- ============================================
-- 2. THÊM INDEXES CHO BẢNG Messages
-- ============================================
PRINT '';
PRINT 'Đang thêm Indexes cho bảng Messages...';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Messages_SenderReceiver' AND object_id = OBJECT_ID('Messages'))
BEGIN
    CREATE INDEX IX_Messages_SenderReceiver ON Messages(SenderID, ReceiverID);
    PRINT '✓ Đã tạo Index IX_Messages_SenderReceiver';
END
ELSE
    PRINT '  Index IX_Messages_SenderReceiver đã tồn tại';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Messages_CreatedAt' AND object_id = OBJECT_ID('Messages'))
BEGIN
    CREATE INDEX IX_Messages_CreatedAt ON Messages(CreatedAt DESC);
    PRINT '✓ Đã tạo Index IX_Messages_CreatedAt';
END
ELSE
    PRINT '  Index IX_Messages_CreatedAt đã tồn tại';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Messages_SenderID' AND object_id = OBJECT_ID('Messages'))
BEGIN
    CREATE INDEX IX_Messages_SenderID ON Messages(SenderID);
    PRINT '✓ Đã tạo Index IX_Messages_SenderID';
END
ELSE
    PRINT '  Index IX_Messages_SenderID đã tồn tại';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Messages_ReceiverID' AND object_id = OBJECT_ID('Messages'))
BEGIN
    CREATE INDEX IX_Messages_ReceiverID ON Messages(ReceiverID);
    PRINT '✓ Đã tạo Index IX_Messages_ReceiverID';
END
ELSE
    PRINT '  Index IX_Messages_ReceiverID đã tồn tại';

GO

-- ============================================
-- 3. THÊM INDEXES CHO BẢNG Groups
-- ============================================
PRINT '';
PRINT 'Đang thêm Indexes cho bảng Groups...';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Groups_CreatorID' AND object_id = OBJECT_ID('Groups'))
BEGIN
    CREATE INDEX IX_Groups_CreatorID ON Groups(CreatorID);
    PRINT '✓ Đã tạo Index IX_Groups_CreatorID';
END
ELSE
    PRINT '  Index IX_Groups_CreatorID đã tồn tại';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Groups_CreatedAt' AND object_id = OBJECT_ID('Groups'))
BEGIN
    CREATE INDEX IX_Groups_CreatedAt ON Groups(CreatedAt DESC);
    PRINT '✓ Đã tạo Index IX_Groups_CreatedAt';
END
ELSE
    PRINT '  Index IX_Groups_CreatedAt đã tồn tại';

GO

-- ============================================
-- 4. THÊM INDEXES CHO BẢNG GroupMembers
-- ============================================
PRINT '';
PRINT 'Đang thêm Indexes cho bảng GroupMembers...';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GroupMembers_GroupID' AND object_id = OBJECT_ID('GroupMembers'))
BEGIN
    CREATE INDEX IX_GroupMembers_GroupID ON GroupMembers(GroupID);
    PRINT '✓ Đã tạo Index IX_GroupMembers_GroupID';
END
ELSE
    PRINT '  Index IX_GroupMembers_GroupID đã tồn tại';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GroupMembers_UserID' AND object_id = OBJECT_ID('GroupMembers'))
BEGIN
    CREATE INDEX IX_GroupMembers_UserID ON GroupMembers(UserID);
    PRINT '✓ Đã tạo Index IX_GroupMembers_UserID';
END
ELSE
    PRINT '  Index IX_GroupMembers_UserID đã tồn tại';

-- Index composite cho truy vấn nhanh
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GroupMembers_GroupUser' AND object_id = OBJECT_ID('GroupMembers'))
BEGIN
    CREATE INDEX IX_GroupMembers_GroupUser ON GroupMembers(GroupID, UserID);
    PRINT '✓ Đã tạo Index IX_GroupMembers_GroupUser';
END
ELSE
    PRINT '  Index IX_GroupMembers_GroupUser đã tồn tại';

GO

-- ============================================
-- 5. THÊM INDEXES CHO BẢNG GroupMessages
-- ============================================
PRINT '';
PRINT 'Đang thêm Indexes cho bảng GroupMessages...';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GroupMessages_GroupID' AND object_id = OBJECT_ID('GroupMessages'))
BEGIN
    CREATE INDEX IX_GroupMessages_GroupID ON GroupMessages(GroupID);
    PRINT '✓ Đã tạo Index IX_GroupMessages_GroupID';
END
ELSE
    PRINT '  Index IX_GroupMessages_GroupID đã tồn tại';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GroupMessages_CreatedAt' AND object_id = OBJECT_ID('GroupMessages'))
BEGIN
    CREATE INDEX IX_GroupMessages_CreatedAt ON GroupMessages(CreatedAt DESC);
    PRINT '✓ Đã tạo Index IX_GroupMessages_CreatedAt';
END
ELSE
    PRINT '  Index IX_GroupMessages_CreatedAt đã tồn tại';

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GroupMessages_SenderID' AND object_id = OBJECT_ID('GroupMessages'))
BEGIN
    CREATE INDEX IX_GroupMessages_SenderID ON GroupMessages(SenderID);
    PRINT '✓ Đã tạo Index IX_GroupMessages_SenderID';
END
ELSE
    PRINT '  Index IX_GroupMessages_SenderID đã tồn tại';

-- Index composite cho truy vấn lịch sử nhóm
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GroupMessages_GroupCreated' AND object_id = OBJECT_ID('GroupMessages'))
BEGIN
    CREATE INDEX IX_GroupMessages_GroupCreated ON GroupMessages(GroupID, CreatedAt DESC);
    PRINT '✓ Đã tạo Index IX_GroupMessages_GroupCreated';
END
ELSE
    PRINT '  Index IX_GroupMessages_GroupCreated đã tồn tại';

GO

-- ============================================
-- 6. CẬP NHẬT DatabaseManager.cs để tự động chạy script này
-- (Script này sẽ được chạy tự động khi server khởi động)
-- ============================================

PRINT '';
PRINT '========================================';
PRINT 'HOÀN TẤT CẬP NHẬT DATABASE!';
PRINT '========================================';
PRINT '';
PRINT 'Đã thêm các Indexes để tối ưu hiệu suất:';
PRINT '  ✓ Users: Username, Email, IsOnline';
PRINT '  ✓ Messages: SenderID, ReceiverID, CreatedAt, Composite';
PRINT '  ✓ Groups: CreatorID, CreatedAt';
PRINT '  ✓ GroupMembers: GroupID, UserID, Composite';
PRINT '  ✓ GroupMessages: GroupID, SenderID, CreatedAt, Composite';
PRINT '';
PRINT 'Database đã được tối ưu hóa!';
GO

