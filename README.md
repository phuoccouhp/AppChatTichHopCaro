# ChatApp + Caro Game

Ứng dụng chat thời gian thực tích hợp game Caro và Tank, xây dựng bằng .NET 8 và Windows Forms.

## 📁 Cấu trúc thư mục

```
📦 AppChatTichHopCaro
│
├── .gitignore                           # Cấu hình Git ignore
├── ChatAppCaro.sln                      # Solution file chính
├── README.md                            # Tài liệu hướng dẫn
│
├── # --- Database Scripts ---
├── SetupDatabase_Full.sql               # Script tạo database đầy đủ
├── UpdateDatabase_AddIndexes.sql        # Script thêm indexes tối ưu
│
├── # --- Batch/PowerShell Scripts ---
├── QuickStart.bat                       # Khởi động nhanh ứng dụng
├── RunServerAsAdmin.bat                 # Chạy Server với quyền Admin
├── RunMultipleClients.bat               # Chạy nhiều Client cùng lúc
├── StartMultipleClients.bat             # Khởi động nhiều Client
├── CreateOutboundRuleFix.ps1            # Script PowerShell mở firewall
│
├── 📂 ChatAppServer/                    # Server xử lý kết nối
│   ├── Program.cs                       # Entry point khởi động server
│   ├── Server.cs                        # Xử lý kết nối socket chính, quản lý clients
│   ├── ClientHandler.cs                 # Quản lý từng client kết nối
│   ├── DatabaseManager.cs               # Thao tác database (Users, Messages, Groups)
│   ├── GameSession.cs                   # Quản lý phiên chơi game Caro
│   ├── TankGameManager.cs               # Quản lý logic game Tank 1v1
│   ├── TankMultiplayerManager.cs        # Quản lý phòng chơi Tank multiplayer
│   ├── FirewallHelper.cs                # Hỗ trợ mở port firewall tự động
│   ├── EmailHelper.cs                   # Gửi email OTP reset password
│   ├── PasswordHelper.cs                # Mã hóa và xác thực password
│   ├── PasswordMigrationTool.cs         # Tool migrate password cũ sang hash mới
│   ├── OptimizationConfig.cs            # Cấu hình tối ưu hiệu năng server
│   ├── Logger.cs                        # Ghi log hoạt động server
│   ├── frmServer.cs                     # Form giao diện điều khiển server
│   └── frmServer.Designer.cs            # Designer cho form server
│
├── 📂 ChatAppClient/                    # Ứng dụng client cho người dùng
│   ├── Program.cs                       # Entry point khởi động client
│   ├── NetworkManager.cs                # Quản lý kết nối TCP đến server
│   ├── Logger.cs                        # Ghi log hoạt động client
│   │
│   ├── 📂 Forms/                        # Các form giao diện chính
│   │   ├── frmLogin.cs                  # Form đăng nhập
│   │   ├── frmLogin.Designer.cs         # Designer form đăng nhập
│   │   ├── frmSignup.cs                 # Form đăng ký tài khoản
│   │   ├── frmSignup.Designer.cs        # Designer form đăng ký
│   │   ├── frmHome.cs                   # Form màn hình chính (chat)
│   │   ├── frmHome.Designer.cs          # Designer form home
│   │   ├── frmSettings.cs               # Form cài đặt người dùng
│   │   ├── frmSettings.Designer.cs      # Designer form settings
│   │   ├── frmForgotPass.cs             # Form quên mật khẩu
│   │   ├── frmForgotPass.Designer.cs    # Designer form quên mật khẩu
│   │   ├── frmResetPassword.cs          # Form đặt lại mật khẩu
│   │   ├── frmResetPassword.Designer.cs # Designer form reset password
│   │   ├── frmCreateGroup.cs            # Form tạo nhóm chat
│   │   ├── frmInviteGroupMembers.cs     # Form mời thành viên vào nhóm
│   │   ├── frmForwardMessage.cs         # Form chuyển tiếp tin nhắn
│   │   ├── frmCaroGame.cs               # Form chơi game Caro
│   │   ├── frmCaroGame.Designer.cs      # Designer form Caro
│   │   ├── frmTankGame.cs               # Form chơi game Tank 1v1
│   │   ├── frmTankGame.Designer.cs      # Designer form Tank
│   │   ├── frmTankMultiplayer.cs        # Form chơi Tank multiplayer
│   │   └── frmTankMultiplayerLobby.cs   # Form lobby Tank multiplayer
│   │
│   ├── 📂 UserControls/                 # Custom controls tái sử dụng
│   │   ├── ChatViewControl.cs           # Control hiển thị khung chat
│   │   ├── ChatViewControl.Designer.cs  # Designer ChatViewControl
│   │   ├── GroupChatViewControl.cs      # Control hiển thị chat nhóm
│   │   ├── ChatMessageBubble.cs         # Bubble hiển thị tin nhắn text
│   │   ├── ChatMessageBubble.Designer.cs# Designer bubble tin nhắn
│   │   ├── ImageBubble.cs               # Bubble hiển thị hình ảnh
│   │   ├── ImageBubble.Designer.cs      # Designer bubble hình ảnh
│   │   ├── FileBubble.cs                # Bubble hiển thị file đính kèm
│   │   ├── FileBubble.Designer.cs       # Designer bubble file
│   │   ├── GameInviteBubble.cs          # Bubble hiển thị lời mời chơi game
│   │   ├── GameInviteBubble.Designer.cs # Designer bubble mời game
│   │   ├── FriendListItem.cs            # Item hiển thị bạn bè online
│   │   ├── FriendListItem.Designer.cs   # Designer item bạn bè
│   │   └── GroupListItem.cs             # Item hiển thị nhóm chat
│   │
│   ├── 📂 CustomControls/               # Controls tùy chỉnh giao diện
│   │   ├── CustomControl.cs             # Base custom control
│   │   └── RoundedButton.cs             # Button bo tròn góc
│   │
│   ├── 📂 Helpers/                      # Các class helper
│   │   ├── AppColors.cs                 # Định nghĩa màu sắc ứng dụng
│   │   └── DrawingHelper.cs             # Helper vẽ đồ họa
│   │
│   └── 📂 Properties/
│       └── Resources.Designer.cs        # Resources (icons, images)
│
└── 📂 ChatApp.Shared/                   # Thư viện dùng chung Client-Server
    ├── PacketMapper.cs                  # Map tên packet sang Type để deserialize
    ├── User.cs                          # Model thông tin người dùng
    │
    ├── # --- Authentication Packets ---
    ├── LoginPacket.cs                   # Gói yêu cầu đăng nhập
    ├── LoginResultPacket.cs             # Gói kết quả đăng nhập
    ├── RegisterPacket.cs                # Gói yêu cầu đăng ký
    ├── RegisterResultPacket.cs          # Gói kết quả đăng ký
    ├── ForgotPasswordPacket.cs          # Gói yêu cầu quên mật khẩu
    ├── ForgotPasswordResultPacket.cs    # Gói kết quả quên mật khẩu
    ├── ResetPasswordPacket.cs           # Gói đặt lại mật khẩu
    ├── UpdateProfilePacket.cs           # Gói cập nhật thông tin cá nhân
    │
    ├── # --- Chat Packets ---
    ├── TextPacket.cs                    # Gói tin nhắn text
    ├── FilePacket.cs                    # Gói gửi file/hình ảnh
    ├── ChatHistoryRequestPacket.cs      # Gói yêu cầu lịch sử chat
    ├── ChatHistoryResponsePacket.cs     # Gói trả về lịch sử chat
    │
    ├── # --- User Status Packets ---
    ├── RequestOnlineListPacket.cs       # Gói yêu cầu danh sách online
    ├── OnlineUsersPacket.cs             # Gói danh sách người dùng online
    ├── UserStatusPacket.cs              # Gói thông báo trạng thái user
    │
    ├── # --- Group Chat Packets ---
    ├── GroupPackets.cs                  # Các gói liên quan chat nhóm
    │
    ├── # --- Game Caro Packets ---
    ├── GameInvitePacket.cs              # Gói mời chơi game
    ├── GameResponsePacket.cs            # Gói phản hồi lời mời
    ├── GameStartPacket.cs               # Gói bắt đầu game
    ├── GameMovePacket.cs                # Gói nước đi trong game
    ├── GameResetPacket.cs               # Gói reset game
    ├── RematchRequestPacket.cs          # Gói yêu cầu chơi lại
    ├── RematchResponsePacket.cs         # Gói phản hồi chơi lại
    │
    ├── # --- Game Tank Packets ---
    ├── TankInvitePacket.cs              # Gói mời chơi Tank
    ├── TankResponseAndStartPackets.cs   # Gói phản hồi và bắt đầu Tank
    ├── TankStartPacket.cs               # Gói khởi động game Tank
    ├── TankActionPacket.cs              # Gói hành động (di chuyển, bắn)
    ├── TankHitPacket.cs                 # Gói thông báo trúng đạn
    └── TankMultiplayerPackets.cs        # Các gói Tank multiplayer (tạo phòng, join, ready)
```

## 🚀 Cách sử dụng

### 1. Chạy Server
1. Build và chạy `ChatAppServer`
2. Nhấn **"Mở Firewall ngay"** (yêu cầu quyền Admin)
3. Nhấn **"Start Server"**
4. Copy địa chỉ IP để gửi cho client

### 2. Chạy Client
1. Build và chạy `ChatAppClient`
2. Nhập **IP Server** 
3. Đăng ký hoặc đăng nhập
4. Bắt đầu chat và chơi game!

## ⚙️ Yêu cầu
- Windows 10/11
- .NET 8 Runtime
- SQL Server

## 🎮 Tính năng
- ✅ Chat realtime 1-1 và nhóm
- ✅ Gửi file và hình ảnh
- ✅ Game Caro online
- ✅ Game Tank (1v1 và multiplayer)
- ✅ Đăng ký / Đăng nhập
- ✅ Quên mật khẩu qua email

## 📝 Ghi chú
- Port mặc định: **9000**
- Đảm bảo firewall đã mở port
- Server và Client cần cùng mạng LAN hoặc có IP public

## 🔗 Link GitHub
https://github.com/phuoccouhp/AppChatTichHopCaro

