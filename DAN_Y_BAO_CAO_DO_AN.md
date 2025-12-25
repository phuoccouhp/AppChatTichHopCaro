# DÀN Ý BÁO CÁO ĐỒ ÁN
## ỨNG DỤNG CHAT TÍCH HỢP GAME CARO VÀ TANK

---

## I. LÝ DO CHỌN ĐỀ TÀI

### 1.1. Bối cảnh thực tế
- Nhu cầu giao tiếp và giải trí trực tuyến ngày càng cao
- Xu hướng tích hợp nhiều tính năng trong một ứng dụng
- Nhu cầu ứng dụng real-time communication

### 1.2. Tính thực tiễn
- Ứng dụng thực tế có thể triển khai và sử dụng
- Kết hợp chat và game tạo trải nghiệm người dùng tốt
- Phù hợp với môi trường mạng LAN và Internet

### 1.3. Tính học thuật
- Áp dụng kiến thức về mạng máy tính (TCP/IP, Socket Programming)
- Thực hành lập trình đa luồng (Multi-threading, Async/Await)
- Quản lý cơ sở dữ liệu (SQL Server)
- Kiến trúc Client-Server

### 1.4. Thách thức kỹ thuật
- Xử lý đa kết nối đồng thời
- Đồng bộ hóa trạng thái game real-time
- Quản lý kết nối mạng không ổn định
- Tối ưu hiệu năng và băng thông

---

## II. MỤC TIÊU ĐỀ TÀI

### 2.1. Mục tiêu chính
- Xây dựng ứng dụng chat real-time với giao diện thân thiện
- Tích hợp game Caro (Cờ ca rô) và Tank để giải trí
- Hỗ trợ giao tiếp nhóm và cá nhân
- Lưu trữ lịch sử chat và quản lý người dùng

### 2.2. Mục tiêu kỹ thuật
- **Network**: Xây dựng hệ thống Client-Server sử dụng TCP/IP
- **Concurrency**: Xử lý đa kết nối đồng thời với async/await
- **Database**: Thiết kế và quản lý cơ sở dữ liệu SQL Server
- **Game Logic**: Đồng bộ trạng thái game real-time giữa các client

### 2.3. Mục tiêu chức năng
- Đăng nhập/Đăng ký/Quên mật khẩu
- Chat text và file real-time
- Tạo và quản lý nhóm chat
- Chơi game Caro và Tank online
- Xem lịch sử chat

---

## III. GIỚI THIỆU ĐỀ TÀI

### 3.1. Tổng quan ứng dụng
- **Tên ứng dụng**: ChatApp - Ứng dụng Chat Tích Hợp Game
- **Mô tả**: Ứng dụng desktop cho phép người dùng chat real-time và chơi game với nhau qua mạng
- **Công nghệ**: C# .NET 8.0, Windows Forms, SQL Server, TCP/IP Socket

### 3.2. Tính năng chính
1. **Chat**
   - Chat 1-1 (cá nhân)
   - Chat nhóm
   - Gửi file
   - Lịch sử chat

2. **Game**
   - Game Caro (Cờ ca rô)
   - Game Tank (Xe tăng)

3. **Quản lý người dùng**
   - Đăng ký/Đăng nhập
   - Quên mật khẩu (gửi OTP qua email)
   - Cập nhật profile
   - Quản lý danh sách bạn bè

---

## IV. LUẬT CHƠI VÀ HƯỚNG DẪN

### 4.1. GAME CARO (CỜ CA RÔ)

#### 4.1.1. Luật chơi
- **Bàn cờ**: 20x20 ô vuông
- **Người chơi**: 2 người (X và O)
- **Lượt chơi**: Luân phiên, người đi trước đánh X, người đi sau đánh O
- **Cách chơi**: 
  - Mỗi lượt, người chơi đánh một quân cờ vào một ô trống
  - Mục tiêu: Tạo được 5 quân cờ liên tiếp (ngang, dọc, hoặc chéo)
  - Quân cờ có thể chặn đối thủ bằng cách đánh vào vị trí đối thủ cần

#### 4.1.2. Điều kiện thắng
- **Thắng**: Người chơi tạo được 5 quân cờ liên tiếp (ngang, dọc, hoặc chéo)
- **Hòa**: Bàn cờ đầy (400 ô) nhưng không ai thắng
- **Kết thúc**: Khi có người thắng hoặc hòa

#### 4.1.3. Hướng dẫn chơi
1. Người chơi mời đối thủ qua menu chat
2. Đối thủ chấp nhận lời mời
3. Game bắt đầu, người đi trước đánh X
4. Click vào ô trống trên bàn cờ để đánh
5. Chờ đối thủ đánh (hiển thị "Đến lượt đối thủ")
6. Khi có người thắng, hiển thị thông báo
7. Có thể chơi lại bằng nút "Chơi lại"

### 4.2. GAME TANK

#### 4.2.1. Luật chơi
- **Màn chơi**: 800x600 pixels
- **Người chơi**: 2 người, mỗi người điều khiển 1 xe tăng
- **Điều khiển**: 
  - W/A/S/D hoặc mũi tên: Di chuyển và xoay xe tăng
  - Space: Bắn đạn
- **Máu**: Mỗi xe tăng có 100 HP
- **Đạn**: Bắn liên tục, mỗi viên đạn gây sát thương

#### 4.2.2. Điều kiện thắng
- **Thắng**: Giảm HP đối thủ xuống 0
- **Kết thúc**: Khi một trong hai người chơi hết máu
- **Hiển thị**: Thông báo người thắng và điểm số

#### 4.2.3. Hướng dẫn chơi
1. Người chơi mời đối thủ chơi Tank
2. Đối thủ chấp nhận
3. Game bắt đầu, mỗi người ở một góc màn hình
4. Sử dụng phím để di chuyển và bắn
5. Tránh đạn đối thủ, bắn trúng đối thủ để gây sát thương
6. Người giảm HP đối thủ xuống 0 trước sẽ thắng
7. Có thể chơi lại sau khi kết thúc

---

## V. NETWORK STACK VÀ KIẾN TRÚC HỆ THỐNG

### 5.1. Mô tả kiến trúc tổng quan

**Hình vẽ cần có:**
- Sơ đồ tổng quan hệ thống Client-Server
- Các thành phần: Client, Server, Database
- Luồng kết nối và giao tiếp giữa các thành phần

**Các thành phần chính:**
1. **Client Application** (ChatAppClient)
   - Giao diện người dùng (Windows Forms)
   - NetworkManager: Quản lý kết nối TCP
   - Xử lý giao diện game và chat

2. **Server Application** (ChatAppServer)
   - TcpListener: Lắng nghe kết nối từ client
   - ClientHandler: Xử lý từng client riêng biệt
   - GameSession: Quản lý phiên game
   - DatabaseManager: Tương tác với SQL Server

3. **Database** (SQL Server)
   - Lưu trữ thông tin người dùng
   - Lưu trữ tin nhắn và lịch sử chat
   - Lưu trữ lịch sử game

4. **Shared Library** (ChatApp.Shared)
   - Định nghĩa các Packet classes
   - PacketMapper: Serialization/Deserialization

### 5.2. Network Stack - Từng tầng

#### 5.2.1. Application Layer
- **Giao thức**: Custom Protocol dựa trên JSON
- **Message Structure**: 
  ```json
  {
    "Type": "PacketClassName",
    "Payload": "{...JSON data...}"
  }
  ```
- **Các loại Packet**:
  - Authentication: LoginPacket, RegisterPacket, LoginResultPacket
  - Chat: TextPacket, FilePacket, ChatHistoryRequestPacket
  - Game: GameInvitePacket, GameStartPacket, GameMovePacket, TankActionPacket
  - System: UserStatusPacket, OnlineListPacket

#### 5.2.2. Transport Layer
- **Giao thức**: TCP (Transmission Control Protocol)
- **Port**: 9000
- **Đặc điểm**:
  - Kết nối hướng kết nối (Connection-oriented)
  - Đảm bảo độ tin cậy (Reliable)
  - Đảm bảo thứ tự gói tin (Ordered delivery)
- **Cấu hình Socket**:
  - ReceiveBufferSize: 131072 bytes
  - SendBufferSize: 131072 bytes
  - NoDelay: true (Tắt Nagle algorithm)
  - KeepAlive: Enabled (30s interval)

#### 5.2.3. Network Layer
- **Giao thức**: IP (Internet Protocol)
- **Địa chỉ**: IPv4
- **Routing**: Tự động qua router/gateway

#### 5.2.4. Data Link & Physical Layer
- **Hỗ trợ**: Ethernet, WiFi, LAN
- **Cấu hình**: Tự động qua hệ điều hành

### 5.3. Message Structure chi tiết

**PacketWrapper Structure:**
```
[4 bytes: Length] + [JSON: PacketWrapper]
PacketWrapper {
  Type: string (tên class packet)
  Payload: string (JSON serialized packet data)
}
```

**Ví dụ TextPacket:**
```json
{
  "Type": "TextPacket",
  "Payload": "{\"SenderID\":\"user1\",\"ReceiverID\":\"user2\",\"MessageContent\":\"Hello\"}"
}
```

### 5.4. Server quản lý kết nối

**Hình vẽ cần có:**
- Sơ đồ luồng xử lý kết nối client
- Thread/Task model cho đa kết nối
- Cấu trúc dữ liệu quản lý clients

**Cơ chế:**
1. **TcpListener** lắng nghe trên port 9000
2. Khi có client kết nối:
   - `AcceptTcpClientAsync()` chấp nhận kết nối
   - Tạo `ClientHandler` mới cho mỗi client
   - Mỗi `ClientHandler` chạy trên Task riêng (async)
3. **Dictionary<string, ClientHandler>** lưu trữ clients theo UserID
4. **Lock mechanism** để thread-safe khi truy cập dictionary

**Code pattern:**
```csharp
// Server chấp nhận kết nối
TcpClient clientSocket = await _listener.AcceptTcpClientAsync();
Task.Run(() => {
    var handler = new ClientHandler(clientSocket, this);
    handler.StartHandlingAsync();
});
```

### 5.5. Xử lý đa kết nối (Concurrent Connections)

**Hình vẽ cần có:**
- Sơ đồ mô hình đa luồng
- Luồng xử lý khi nhiều client gửi request đồng thời

**Cơ chế:**
1. **Async/Await Pattern**: 
   - Mỗi client có một `Task` riêng chạy `StartHandlingAsync()`
   - Không block thread chính
   - Sử dụng `async/await` cho I/O operations

2. **Thread Pool**:
   - .NET Task Scheduler tự động quản lý thread pool
   - Tối ưu số lượng thread dựa trên CPU cores

3. **Lock Mechanism**:
   - `lock (_clients)` khi truy cập dictionary clients
   - `lock (_gameSessions)` khi truy cập game sessions
   - Đảm bảo thread-safety

4. **Xử lý request đồng thời**:
   - Mỗi ClientHandler đọc packet độc lập
   - Server xử lý packet theo thứ tự nhận được
   - Broadcast/Relay sử dụng lock để tránh race condition

**Ví dụ xử lý nhiều request:**
```
Client1 → [Packet1] → Server → [Process] → Client2
Client3 → [Packet2] → Server → [Process] → Client4
Client5 → [Packet3] → Server → [Process] → Client6
         (Xử lý song song, không block lẫn nhau)
```

### 5.6. Hoạt động trong môi trường Internet

**Hình vẽ cần có:**
- Sơ đồ kết nối qua Internet
- NAT traversal, firewall configuration
- Luồng dữ liệu qua router/gateway

**Cơ chế:**
1. **Local Network (LAN/WiFi)**:
   - Client kết nối trực tiếp đến Server IP trong mạng nội bộ
   - Ví dụ: Server IP: 192.168.1.100, Port: 9000

2. **Internet (Cross-network)**:
   - Server cần có Public IP hoặc Port Forwarding
   - Client kết nối qua Public IP:Port
   - Firewall phải mở port 9000 (Inbound + Outbound)

3. **NAT Traversal**:
   - Server mở port 9000 trên router
   - Forward port 9000 từ router đến Server IP

4. **Firewall Configuration**:
   - Windows Firewall: Tạo rule cho port 9000
   - Inbound rule: Cho phép client kết nối đến
   - Outbound rule: Cho phép server gửi phản hồi

5. **TCP KeepAlive**:
   - Giữ kết nối sống qua NAT/Firewall
   - Gửi probe mỗi 30 giây
   - Phát hiện kết nối bị ngắt

**Cấu hình kết nối:**
```
Client (192.168.1.50) 
  → Router (192.168.1.1)
    → Internet
      → Router Server (Public IP: 123.45.67.89)
        → Server (192.168.1.100:9000)
```

---

## VI. USER STORY

### 6.1. Authentication & User Management

**US-01: Đăng ký tài khoản**
- **As a** người dùng mới
- **I want to** đăng ký tài khoản với email và mật khẩu
- **So that** tôi có thể sử dụng ứng dụng
- **Hình vẽ**: Form đăng ký, luồng gửi RegisterPacket → Server → Database → RegisterResultPacket

**US-02: Đăng nhập**
- **As a** người dùng đã có tài khoản
- **I want to** đăng nhập bằng UserID và Password
- **So that** tôi có thể truy cập ứng dụng
- **Hình vẽ**: Form đăng nhập, luồng LoginPacket → Server (verify) → LoginResultPacket

**US-03: Quên mật khẩu**
- **As a** người dùng quên mật khẩu
- **I want to** nhận OTP qua email để reset mật khẩu
- **So that** tôi có thể lấy lại quyền truy cập
- **Hình vẽ**: Form quên mật khẩu, luồng ForgotPasswordPacket → Server (gửi email) → OTP → ResetPasswordPacket

### 6.2. Chat Features

**US-04: Chat 1-1**
- **As a** người dùng
- **I want to** gửi tin nhắn text cho người dùng khác
- **So that** tôi có thể giao tiếp real-time
- **Hình vẽ**: Giao diện chat, luồng TextPacket từ Client1 → Server → Client2

**US-05: Gửi file**
- **As a** người dùng
- **I want to** gửi file cho người dùng khác
- **So that** tôi có thể chia sẻ tài liệu, hình ảnh
- **Hình vẽ**: Dialog chọn file, luồng FilePacket (chunked) → Server → Client nhận

**US-06: Xem lịch sử chat**
- **As a** người dùng
- **I want to** xem lại tin nhắn đã gửi/nhận trước đó
- **So that** tôi có thể tra cứu thông tin cũ
- **Hình vẽ**: Giao diện hiển thị lịch sử, luồng ChatHistoryRequestPacket → Server (query DB) → ChatHistoryResponsePacket

**US-07: Chat nhóm**
- **As a** người dùng
- **I want to** tạo nhóm và chat với nhiều người cùng lúc
- **So that** tôi có thể thảo luận nhóm
- **Hình vẽ**: Form tạo nhóm, luồng GroupPackets → Server → Broadcast đến tất cả thành viên

### 6.3. Game Features

**US-08: Mời chơi Caro**
- **As a** người chơi
- **I want to** mời người dùng khác chơi Caro
- **So that** tôi có thể giải trí với bạn bè
- **Hình vẽ**: Menu mời game, luồng GameInvitePacket → Server → GameResponsePacket (Accept/Decline) → GameStartPacket

**US-09: Chơi Caro**
- **As a** người chơi
- **I want to** đánh quân cờ và xem nước đi của đối thủ real-time
- **So that** tôi có thể chơi game mượt mà
- **Hình vẽ**: Bàn cờ Caro, luồng GameMovePacket từ Client1 → Server → Client2, kiểm tra thắng/thua

**US-10: Chơi lại Caro**
- **As a** người chơi
- **I want to** chơi lại ván mới sau khi kết thúc
- **So that** tôi có thể tiếp tục giải trí
- **Hình vẽ**: Nút "Chơi lại", luồng RematchRequestPacket → RematchResponsePacket → GameResetPacket

**US-11: Chơi Tank**
- **As a** người chơi
- **I want to** điều khiển xe tăng và bắn đối thủ
- **So that** tôi có thể chơi game hành động
- **Hình vẽ**: Màn hình game Tank, luồng TankActionPacket (di chuyển, bắn) → Server → Broadcast, TankHitPacket

### 6.4. System Features

**US-12: Xem danh sách online**
- **As a** người dùng
- **I want to** xem danh sách người dùng đang online
- **So that** tôi biết ai đang sẵn sàng chat/chơi game
- **Hình vẽ**: Sidebar hiển thị danh sách, luồng RequestOnlineListPacket → Server → OnlineListPacket

**US-13: Cập nhật trạng thái online/offline**
- **As a** người dùng
- **I want to** thấy trạng thái online/offline của bạn bè
- **So that** tôi biết khi nào có thể liên lạc
- **Hình vẽ**: Icon online/offline, luồng UserStatusPacket broadcast khi login/logout

---

## VII. HOW? WHY? WHAT?

### 7.1. HOW - Cách thức hoạt động

**Hình vẽ cần có:**
- Sequence diagram cho luồng chat
- Sequence diagram cho luồng game
- State diagram cho game session

**7.1.1. Luồng Chat:**
1. Client1 gửi TextPacket → Server
2. Server nhận packet, lưu vào Database
3. Server tìm Client2 trong dictionary clients
4. Server gửi TextPacket đến Client2
5. Client2 nhận và hiển thị tin nhắn

**7.1.2. Luồng Game Caro:**
1. Client1 gửi GameInvitePacket → Server
2. Server lưu message ID, forward đến Client2
3. Client2 chấp nhận → GameResponsePacket → Server
4. Server tạo GameSession, gửi GameStartPacket đến cả 2
5. Client1 đánh cờ → GameMovePacket → Server → Client2
6. Server kiểm tra thắng/thua (client-side check)
7. Khi thắng, client gửi GameEndPacket → Server → Client2

**7.1.3. Luồng Game Tank:**
1. Tương tự mời game
2. Server tạo TankGameSession, khởi tạo vị trí xe tăng
3. Client gửi TankActionPacket (di chuyển, xoay, bắn) → Server
4. Server update game state, broadcast đến đối thủ
5. Server chạy game loop (16ms) để update đạn
6. Khi trúng đạn → TankHitPacket → Server → Client (giảm HP)

### 7.2. WHY - Lý do thiết kế

**7.2.1. Tại sao dùng TCP thay vì UDP?**
- **TCP**: Đảm bảo độ tin cậy, thứ tự gói tin (quan trọng cho chat và game)
- **UDP**: Nhanh nhưng mất gói tin (không phù hợp)

**7.2.2. Tại sao dùng JSON thay vì Binary?**
- **JSON**: Dễ debug, human-readable, dễ mở rộng
- **Binary**: Nhỏ hơn nhưng khó maintain

**7.2.3. Tại sao dùng Async/Await?**
- **Async**: Không block thread, xử lý nhiều client hiệu quả
- **Synchronous**: Block thread, không scale được

**7.2.4. Tại sao Server quản lý Game Session?**
- Đảm bảo tính nhất quán (consistency)
- Tránh cheat (client-side validation dễ bị hack)
- Dễ mở rộng (thêm AI, spectator mode)

**7.2.5. Tại sao cần Outbound Firewall Rule?**
- Server cần gửi phản hồi đến client
- Windows Firewall mặc định chỉ có Inbound rule
- Outbound rule cho phép server gửi packet ra ngoài

### 7.3. WHAT - Công nghệ và công cụ

**7.3.1. Programming Language & Framework:**
- **C# .NET 8.0**: Ngôn ngữ chính, cross-platform
- **Windows Forms**: Giao diện desktop
- **System.Net.Sockets**: TCP/IP socket programming

**7.3.2. Database:**
- **SQL Server**: Quản lý dữ liệu người dùng, tin nhắn, lịch sử
- **ADO.NET**: Kết nối và truy vấn database

**7.3.3. Serialization:**
- **System.Text.Json**: Serialize/Deserialize packets

**7.3.4. Email Service:**
- **SMTP**: Gửi OTP qua email (Gmail SMTP)

**7.3.5. Development Tools:**
- **Visual Studio**: IDE chính
- **SQL Server Management Studio**: Quản lý database
- **Git**: Version control

---

## VIII. BẢNG PHÂN CÔNG CÔNG VIỆC

| STT | Công việc | Người thực hiện | Thời gian | Ghi chú |
|-----|-----------|----------------|-----------|---------|
| 1 | Thiết kế database schema | [Tên] | [Thời gian] | Tạo các bảng Users, Messages, Groups, GameHistory |
| 2 | Implement Authentication (Login/Register) | [Tên] | [Thời gian] | LoginPacket, RegisterPacket, DatabaseManager |
| 3 | Implement Chat 1-1 | [Tên] | [Thời gian] | TextPacket, NetworkManager, frmHome |
| 4 | Implement Chat nhóm | [Tên] | [Thời gian] | GroupPackets, tạo nhóm, thêm thành viên |
| 5 | Implement Game Caro | [Tên] | [Thời gian] | GamePackets, frmCaroGame, game logic |
| 6 | Implement Game Tank | [Tên] | [Thời gian] | TankPackets, frmTankGame, game loop |
| 7 | Implement File Transfer | [Tên] | [Thời gian] | FilePacket, chunked transfer |
| 8 | Implement Chat History | [Tên] | [Thời gian] | ChatHistoryPackets, query database |
| 9 | Implement Quên mật khẩu | [Tên] | [Thời gian] | ForgotPasswordPacket, EmailHelper, OTP |
| 10 | Server: Xử lý đa kết nối | [Tên] | [Thời gian] | ClientHandler, async/await, thread-safe |
| 11 | Server: Game Session Management | [Tên] | [Thời gian] | GameSession, TankGameManager |
| 12 | UI/UX Design | [Tên] | [Thời gian] | Thiết kế giao diện, màu sắc, layout |
| 13 | Testing & Debug | [Tên] | [Thời gian] | Test các tính năng, fix bugs |
| 14 | Viết báo cáo | [Tên] | [Thời gian] | Soạn thảo báo cáo đồ án |
| 15 | Vẽ sơ đồ kiến trúc | [Tên] | [Thời gian] | Vẽ các diagram cần thiết |

**Ghi chú**: Điền tên và thời gian thực tế vào bảng trên.

---

## IX. LINK GITHUB

**Repository**: [Điền link GitHub của nhóm]

**Cấu trúc thư mục:**
```
AppChatTichHopCaro/
├── ChatAppClient/          # Client application
├── ChatAppServer/          # Server application
├── ChatApp.Shared/          # Shared library (Packets)
├── SetupDatabase_Full.sql  # Database setup script
└── README.md               # Hướng dẫn cài đặt
```

---

## X. CÁC HÌNH VẼ CẦN THIẾT

### 10.1. Kiến trúc hệ thống
- **Hình 1**: Sơ đồ tổng quan Client-Server-Database
- **Mô tả**: 
  - Client (Windows Forms) ↔ Server (TCP Socket) ↔ Database (SQL Server)
  - Hiển thị các thành phần: NetworkManager, ClientHandler, DatabaseManager
  - Luồng dữ liệu giữa các thành phần

### 10.2. Network Stack
- **Hình 2**: OSI Model / TCP/IP Stack cho ứng dụng
- **Mô tả**:
  - Application Layer: Custom JSON Protocol
  - Transport Layer: TCP, Port 9000
  - Network Layer: IP
  - Data Link & Physical: Ethernet/WiFi

### 10.3. Message Structure
- **Hình 3**: Cấu trúc PacketWrapper
- **Mô tả**:
  - [4 bytes Length] + [JSON PacketWrapper]
  - PacketWrapper chứa Type và Payload
  - Ví dụ TextPacket structure

### 10.4. Đa kết nối
- **Hình 4**: Mô hình xử lý đa kết nối
- **Mô tả**:
  - TcpListener chấp nhận nhiều client
  - Mỗi client có ClientHandler riêng trên Task riêng
  - Dictionary quản lý clients
  - Lock mechanism

### 10.5. Sequence Diagram - Chat
- **Hình 5**: Luồng gửi tin nhắn
- **Mô tả**:
  - Client1 → Server: TextPacket
  - Server → Database: Lưu tin nhắn
  - Server → Client2: TextPacket
  - Client2 hiển thị tin nhắn

### 10.6. Sequence Diagram - Game Caro
- **Hình 6**: Luồng chơi game Caro
- **Mô tả**:
  - GameInvitePacket → GameResponsePacket → GameStartPacket
  - GameMovePacket (luân phiên)
  - GameEndPacket (khi thắng)

### 10.7. Sequence Diagram - Game Tank
- **Hình 7**: Luồng chơi game Tank
- **Mô tả**:
  - TankStartPacket
  - TankActionPacket (di chuyển, bắn) liên tục
  - TankHitPacket (khi trúng)
  - Game loop 16ms update bullets

### 10.8. Database Schema
- **Hình 8**: ER Diagram
- **Mô tả**:
  - Bảng Users (UserID, UserName, Email, Password, ...)
  - Bảng Messages (MessageID, SenderID, ReceiverID, Content, ...)
  - Bảng Groups, GroupMembers
  - Bảng GameHistory
  - Foreign keys

### 10.9. Internet Connection
- **Hình 9**: Kết nối qua Internet
- **Mô tả**:
  - Client (LAN) → Router → Internet → Router Server → Server (LAN)
  - NAT traversal, Port forwarding
  - Firewall rules (Inbound + Outbound)

### 10.10. State Diagram - Game Session
- **Hình 10**: Trạng thái game session
- **Mô tả**:
  - States: Waiting, Playing, Ended
  - Transitions: Invite → Accept → Start → Playing → End → Rematch

---

## XI. KẾT LUẬN

### 11.1. Tổng kết
- Ứng dụng đã hoàn thành các tính năng cơ bản: Chat, Game Caro, Game Tank
- Hệ thống hoạt động ổn định với xử lý đa kết nối
- Giao diện thân thiện, dễ sử dụng

### 11.2. Hạn chế
- Chưa có voice/video call
- Chưa có AI cho game
- Chưa tối ưu cho mobile

### 11.3. Hướng phát triển
- Thêm voice/video call
- Port sang mobile (Xamarin/MAUI)
- Thêm nhiều game khác
- Tối ưu hiệu năng và bảo mật

---

**Lưu ý**: 
- Tất cả các hình vẽ cần được mô tả chi tiết để có thể vẽ bằng Gemini hoặc công cụ khác
- Điền thông tin thực tế vào bảng phân công công việc
- Cập nhật link GitHub khi có

