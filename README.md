# ChatApp + Caro Game + Tank Game

Ứng dụng chat thời gian thực tích hợp game Caro và Tank, xây dựng bằng .NET 8 và Windows Forms.

## 📁 Cấu trúc thư mục

```
📦 AppChatTichHopCaro
├── 📂 ChatAppServer/          # Server xử lý kết nối
│   ├── Server.cs              # Xử lý kết nối socket chính
│   ├── ClientHandler.cs       # Quản lý từng client
│   ├── DatabaseManager.cs     # Thao tác database
│   ├── GameSession.cs         # Quản lý phiên chơi Caro
│   ├── TankGameManager.cs     # Quản lý game Tank
│   ├── TankMultiplayerManager.cs # Tank multiplayer
│   ├── FirewallHelper.cs      # Hỗ trợ mở port firewall
│   ├── EmailHelper.cs         # Gửi email OTP
│   ├── PasswordHelper.cs      # Mã hóa password
│   ├── Logger.cs              # Ghi log
│   ├── frmServer.cs           # Giao diện server
│   └── Program.cs             # Entry point
│
├── 📂 ChatAppClient/          # Ứng dụng client
│   ├── 📂 Forms/              # Các form giao diện
│   │   ├── frmLogin.cs        # Đăng nhập
│   │   ├── frmSignup.cs       # Đăng ký
│   │   ├── frmHome.cs         # Màn hình chính
│   │   ├── frmCaroGame.cs     # Game Caro
│   │   ├── frmTankGame.cs     # Game Tank 1v1
│   │   ├── frmTankMultiplayer.cs # Tank multiplayer
│   │   ├── frmSettings.cs     # Cài đặt
│   │   └── frmForgotPass.cs   # Quên mật khẩu
│   ├── 📂 UserControls/       # Custom controls
│   │   ├── ChatViewControl.cs # Hiển thị chat
│   │   ├── ChatMessageBubble.cs # Bubble tin nhắn
│   │   └── FriendListItem.cs  # Item danh sách bạn
│   ├── 📂 Helpers/            # Helper classes
│   ├── NetworkManager.cs      # Quản lý kết nối server
│   ├── Logger.cs              # Ghi log
│   └── Program.cs             # Entry point
│
└── 📂 ChatApp.Shared/         # Thư viện dùng chung
    ├── LoginPacket.cs         # Gói đăng nhập
    ├── TextPacket.cs          # Gói tin nhắn
    ├── FilePacket.cs          # Gói gửi file
    ├── GameInvitePacket.cs    # Gói mời chơi game
    ├── GameMovePacket.cs      # Gói nước đi game
    ├── GroupPackets.cs        # Gói chat nhóm
    ├── User.cs                # Model User
    └── PacketMapper.cs        # Map packet types
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

