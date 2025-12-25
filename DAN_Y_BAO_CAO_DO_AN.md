# TỔNG HỢP THÔNG TIN ĐỒ ÁN
## ỨNG DỤNG CHAT TÍCH HỢP GAME CARO VÀ TANK

---

## I. LÝ DO CHỌN ĐỀ TÀI

### 1.1. Bối cảnh thực tế
- **Nhu cầu giao tiếp trực tuyến**: Trong thời đại số hóa, nhu cầu giao tiếp real-time qua mạng ngày càng cao
- **Xu hướng tích hợp đa chức năng**: Người dùng mong muốn một ứng dụng có thể vừa chat vừa giải trí
- **Ứng dụng thực tế**: Có thể triển khai và sử dụng trong môi trường LAN hoặc Internet

### 1.2. Tính học thuật
- **Mạng máy tính**: Áp dụng kiến thức về TCP/IP, Socket Programming
- **Lập trình đa luồng**: Thực hành Multi-threading, Async/Await pattern
- **Cơ sở dữ liệu**: Quản lý dữ liệu với SQL Server
- **Kiến trúc phần mềm**: Thiết kế Client-Server architecture

### 1.3. Thách thức kỹ thuật
- Xử lý đa kết nối đồng thời (concurrent connections)
- Đồng bộ hóa trạng thái game real-time
- Quản lý kết nối mạng không ổn định (WiFi, Internet)
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
- Đăng nhập/Đăng ký/Quên mật khẩu (OTP qua email)
- Chat text và file real-time
- Tạo và quản lý nhóm chat
- Chơi game Caro và Tank online (1v1 và multiplayer)
- Xem lịch sử chat

---

## III. GIỚI THIỆU ĐỀ TÀI

### 3.1. Tổng quan ứng dụng
- **Tên ứng dụng**: ChatApp - Ứng dụng Chat Tích Hợp Game
- **Mô tả**: Ứng dụng desktop cho phép người dùng chat real-time và chơi game với nhau qua mạng
- **Công nghệ**: 
  - C# .NET 8.0
  - Windows Forms (UI)
  - SQL Server (Database)
  - TCP/IP Socket (Network)

### 3.2. Tính năng chính

#### 3.2.1. Chat
- **Chat 1-1 (cá nhân)**: Gửi tin nhắn text và file giữa 2 người dùng
- **Chat nhóm**: Tạo nhóm, mời thành viên, chat với nhiều người cùng lúc
- **Gửi file**: Hỗ trợ gửi file và hình ảnh
- **Lịch sử chat**: Lưu trữ và xem lại tin nhắn đã gửi/nhận

#### 3.2.2. Game
- **Game Caro (Cờ ca rô)**: Game 2 người chơi, bàn cờ 20x20
- **Game Tank (Xe tăng)**: 
  - Chế độ 1v1: 2 người chơi đấu với nhau
  - Chế độ Multiplayer: Tối đa 4 người chơi trong một phòng

#### 3.2.3. Quản lý người dùng
- Đăng ký/Đăng nhập (username hoặc email)
- Quên mật khẩu (gửi OTP qua email)
- Cập nhật profile (tên hiển thị)
- Quản lý danh sách bạn bè/người dùng online

---

## IV. LUẬT CHƠI VÀ HƯỚNG DẪN

### 4.1. GAME CARO (CỜ CA RÔ)

#### 4.1.1. Luật chơi
- **Bàn cờ**: 20x20 ô vuông (tổng cộng 400 ô)
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
1. Người chơi mời đối thủ qua menu chat (click vào tên người dùng → "Mời chơi Caro")
2. Đối thủ nhận được thông báo và chấp nhận/từ chối lời mời
3. Nếu chấp nhận, game bắt đầu, người đi trước đánh X
4. Click vào ô trống trên bàn cờ để đánh quân cờ
5. Chờ đối thủ đánh (hiển thị "Đến lượt đối thủ")
6. Khi có người thắng, hiển thị thông báo "Bạn đã thắng!" hoặc "Bạn đã thua!"
7. Có thể chơi lại bằng nút "Chơi lại" (Rematch)

---

### 4.2. GAME TANK (XE TĂNG)

#### 4.2.1. Luật chơi (Chế độ 1v1)
- **Màn chơi**: 800x600 pixels
- **Người chơi**: 2 người, mỗi người điều khiển 1 xe tăng
- **Vị trí xuất phát**: 
  - Người chơi 1: Góc trái (100, 300)
  - Người chơi 2: Góc phải (700, 300)
- **Điều khiển**: 
  - W/A/S/D hoặc mũi tên: Di chuyển và xoay xe tăng
  - Space: Bắn đạn
- **Máu (HP)**: Mỗi xe tăng có 100 HP
- **Đạn**: Bắn liên tục, mỗi viên đạn gây sát thương khi trúng đối thủ
- **Tốc độ**: 
  - Di chuyển: 3 pixels/frame
  - Xoay: 5 độ/frame
  - Đạn: 8 pixels/frame

#### 4.2.2. Luật chơi (Chế độ Multiplayer)
- **Số người chơi**: Tối đa 4 người trong một phòng
- **Vị trí xuất phát**: 4 góc màn hình
  - Góc trên trái: (100, 100), góc 45 độ
  - Góc trên phải: (700, 100), góc 135 độ
  - Góc dưới trái: (100, 500), góc 315 độ
  - Góc dưới phải: (700, 500), góc 225 độ
- **Chế độ chơi**:
  - Free-for-All (FFA): Tất cả đánh với nhau
  - Team: Chia đội (chưa implement)
- **Quy trình**:
  1. Host tạo phòng (TankCreateRoomPacket)
  2. Người chơi khác tham gia phòng (TankJoinRoomPacket)
  3. Tất cả người chơi bấm "Ready"
  4. Host bấm "Start Game"
  5. Game bắt đầu, tất cả người chơi đấu với nhau

#### 4.2.3. Điều kiện thắng
- **Chế độ 1v1**: 
  - **Thắng**: Giảm HP đối thủ xuống 0
  - **Kết thúc**: Khi một trong hai người chơi hết máu
- **Chế độ Multiplayer**:
  - **Thắng**: Là người cuối cùng còn sống (HP > 0)
  - **Kết thúc**: Khi chỉ còn 1 người chơi còn sống
  - **Elimination**: Khi HP xuống 0, người chơi bị loại

#### 4.2.4. Hướng dẫn chơi
1. **Chế độ 1v1**:
   - Người chơi mời đối thủ chơi Tank
   - Đối thủ chấp nhận
   - Game bắt đầu, mỗi người ở một góc màn hình
   - Sử dụng phím W/A/S/D để di chuyển và xoay
   - Nhấn Space để bắn đạn
   - Tránh đạn đối thủ, bắn trúng đối thủ để gây sát thương
   - Người giảm HP đối thủ xuống 0 trước sẽ thắng
   - Có thể chơi lại sau khi kết thúc

2. **Chế độ Multiplayer**:
   - Vào menu "Tank Multiplayer"
   - Tạo phòng hoặc tham gia phòng có sẵn
   - Chờ đủ người chơi (tối đa 4 người)
   - Bấm "Ready" khi sẵn sàng
   - Host bấm "Start Game" để bắt đầu
   - Đấu với tất cả người chơi khác
   - Người cuối cùng còn sống sẽ thắng

---

## V. NETWORK STACK VÀ KIẾN TRÚC HỆ THỐNG

### 5.1. Kiến trúc tổng quan hệ thống

**Hình vẽ cần có:**
- Sơ đồ tổng quan hệ thống Client-Server
- Các thành phần: Client Application, Server Application, Database
- Luồng kết nối và giao tiếp giữa các thành phần
- Mô tả các lớp (layers) trong hệ thống

**Các thành phần chính:**

1. **Client Application (ChatAppClient)**
   - **Giao diện người dùng**: Windows Forms
   - **NetworkManager**: Quản lý kết nối TCP đến Server
   - **Forms**: 
     - frmLogin: Đăng nhập/Đăng ký
     - frmMain: Giao diện chat chính
     - frmCaroGame: Giao diện game Caro
     - frmTankGame: Giao diện game Tank (1v1)
     - frmTankMultiplayer: Giao diện game Tank Multiplayer
   - **Xử lý**: Nhận và gửi packet, cập nhật UI

2. **Server Application (ChatAppServer)**
   - **TcpListener**: Lắng nghe kết nối từ client trên port 9000
   - **ClientHandler**: Xử lý từng client riêng biệt (mỗi client một instance)
   - **Server**: Quản lý tất cả clients, game sessions
   - **GameSession**: Quản lý phiên game (Caro, Tank)
   - **TankGameManager**: Quản lý logic game Tank (update đạn, xử lý va chạm)
   - **TankMultiplayerManager**: Quản lý phòng chơi Tank Multiplayer
   - **DatabaseManager**: Tương tác với SQL Server

3. **Database (SQL Server)**
   - **Users**: Lưu trữ thông tin người dùng (Username, Password, Email, DisplayName, IsOnline)
   - **Messages**: Lưu trữ tin nhắn (SenderID, ReceiverID, MessageContent, MessageType, FileName, CreatedAt)
   - **Groups**: Lưu trữ thông tin nhóm (GroupID, GroupName, CreatorID)
   - **GroupMembers**: Lưu trữ thành viên nhóm (GroupID, UserID)
   - **GroupMessages**: Lưu trữ tin nhắn nhóm

4. **Shared Library (ChatApp.Shared)**
   - **Packets**: Định nghĩa tất cả các packet types (LoginPacket, TextPacket, GameMovePacket, ...)
   - **PacketWrapper**: Wrapper để đóng gói packet khi gửi/nhận
   - **PacketMapper**: Map tên packet type sang Type object để deserialize

**Luồng giao tiếp:**
```
Client ←→ TCP Socket ←→ Server ←→ SQL Server
```

### 5.2. Kiến trúc từng tầng (Network Stack)

**Hình vẽ cần có:**
- Sơ đồ OSI/TCP-IP stack
- Mô tả từng tầng và giao thức sử dụng
- Luồng dữ liệu từ Application Layer xuống Physical Layer

#### 5.2.1. Application Layer
- **Giao thức**: Custom Protocol (JSON-based)
- **Cấu trúc**: 
  - PacketWrapper: `{ Type: "LoginPacket", Payload: "{...}" }`
  - Payload là JSON string của packet object
- **Port**: 9000 (TCP)

#### 5.2.2. Transport Layer
- **Giao thức**: TCP (Transmission Control Protocol)
- **Lý do chọn TCP**:
  - Đảm bảo độ tin cậy (reliable)
  - Đảm bảo thứ tự gói tin (ordered)
  - Phù hợp cho chat và game (cần đảm bảo không mất dữ liệu)
- **Port**: 9000
- **Cấu hình**:
  - ReceiveBufferSize: 131072 bytes
  - SendBufferSize: 131072 bytes
  - NoDelay: true (TcpNoDelay - gửi ngay không đợi buffer đầy)
  - KeepAlive: true (duy trì kết nối qua NAT/Firewall)

#### 5.2.3. Network Layer
- **Giao thức**: IP (Internet Protocol)
- **Địa chỉ**: 
  - Server: IPAddress.Any (0.0.0.0) - lắng nghe trên tất cả interfaces
  - Client: Kết nối đến IP server (có thể là LAN IP hoặc Public IP)

#### 5.2.4. Data Link Layer
- **Giao thức**: Ethernet (LAN) hoặc WiFi (Wireless)
- **Không cần cấu hình**: Được xử lý bởi OS và network adapter

#### 5.2.5. Physical Layer
- **Medium**: Cáp mạng (LAN) hoặc sóng radio (WiFi)
- **Không cần cấu hình**: Được xử lý bởi hardware

### 5.3. Message Structure

**Hình vẽ cần có:**
- Sơ đồ cấu trúc message/packet
- Ví dụ cụ thể về một packet (LoginPacket)
- Luồng serialize/deserialize

**Cấu trúc Message:**

1. **PacketWrapper** (Lớp ngoài cùng):
```json
{
  "Type": "LoginPacket",
  "Payload": "{...JSON của LoginPacket...}"
}
```

2. **Format khi gửi qua TCP**:
```
[4 bytes: Length của JSON string] + [JSON string: PacketWrapper]
```

3. **Ví dụ cụ thể - LoginPacket**:
```json
{
  "Type": "LoginPacket",
  "Payload": "{\"Username\":\"user1\",\"Password\":\"pass123\",\"UseEmailLogin\":false}"
}
```

4. **Quy trình Serialize**:
   - Client tạo object: `new LoginPacket { Username = "user1", Password = "pass123" }`
   - Serialize thành JSON: `JsonSerializer.Serialize(loginPacket)`
   - Tạo PacketWrapper: `{ Type: "LoginPacket", Payload: "{...}" }`
   - Serialize PacketWrapper: `JsonSerializer.Serialize(wrapper)`
   - Convert sang bytes: `Encoding.UTF8.GetBytes(jsonString)`
   - Thêm length prefix: `BitConverter.GetBytes(data.Length)` (4 bytes)
   - Gửi: `[4 bytes length] + [JSON bytes]`

5. **Quy trình Deserialize**:
   - Đọc 4 bytes đầu để biết length
   - Đọc đủ length bytes
   - Convert bytes sang string: `Encoding.UTF8.GetString(bytes)`
   - Deserialize PacketWrapper: `JsonSerializer.Deserialize<PacketWrapper>(json)`
   - Lấy Type từ PacketMapper: `PacketMapper.GetPacketType(wrapper.Type)`
   - Deserialize Payload: `JsonSerializer.Deserialize(wrapper.Payload, type)`

### 5.4. Server quản lý kết nối

**Hình vẽ cần có:**
- Sơ đồ Server quản lý nhiều clients
- Dictionary _clients và _gameSessions
- Luồng AcceptTcpClientAsync và tạo ClientHandler

**Cơ chế quản lý:**

1. **TcpListener**:
   - Lắng nghe trên port 9000: `new TcpListener(IPAddress.Any, 9000)`
   - Backlog: 100 (số lượng kết nối chờ xử lý tối đa)
   - `AcceptTcpClientAsync()`: Chấp nhận kết nối mới (async, không block)

2. **ClientHandler**:
   - Mỗi client có một `ClientHandler` instance riêng
   - Chạy trong Task riêng: `Task.Run(() => handler.StartHandlingAsync())`
   - Quản lý NetworkStream riêng cho từng client

3. **Dictionary _clients**:
   - Key: UserID (string)
   - Value: ClientHandler
   - Thread-safe: Sử dụng `lock (_clients)` khi truy cập
   - Khi user đăng nhập lần 2: Đóng client cũ, thêm client mới

4. **Dictionary _gameSessions**:
   - Key: GameID (string, GUID)
   - Value: GameSession (chứa Player1_ID, Player2_ID, GameType)
   - Thread-safe: Sử dụng `lock (_gameSessions)` khi truy cập

### 5.5. Xử lý đa kết nối (Concurrent Connections)

**Hình vẽ cần có:**
- Sơ đồ mô hình đa luồng
- Luồng xử lý khi nhiều client gửi request đồng thời
- Mô tả Task.Run và async/await

**Cơ chế:**

1. **Async/Await Pattern**: 
   - Mỗi client có một `Task` riêng chạy `StartHandlingAsync()`
   - Không block thread chính
   - Sử dụng `async/await` cho I/O operations (ReadAsync, WriteAsync)

2. **Thread Pool**:
   - .NET Task Scheduler tự động quản lý thread pool
   - Tối ưu số lượng thread dựa trên CPU cores
   - Không cần tạo thread thủ công

3. **Lock Mechanism**:
   - `lock (_clients)` khi truy cập dictionary clients
   - `lock (_gameSessions)` khi truy cập game sessions
   - `lock (_stream)` khi gửi packet (tránh gửi đồng thời)
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
- NAT traversal
- Firewall configuration

**Cơ chế:**

1. **Kết nối qua Internet**:
   - Client kết nối đến Public IP của Server
   - Router/Firewall của Server cần forward port 9000
   - Server sử dụng `IPAddress.Any` để lắng nghe trên tất cả interfaces

2. **NAT Traversal**:
   - TCP KeepAlive: Duy trì kết nối qua NAT
   - Cấu hình KeepAlive:
     - KeepAliveTime: 30 giây (thời gian trước khi gửi probe đầu tiên)
     - KeepAliveInterval: 5 giây (thời gian giữa các probe)
   - Giúp kết nối không bị NAT timeout

3. **Firewall Configuration**:
   - Server tự động mở firewall rule (Inbound và Outbound) cho port 9000
   - Sử dụng `FirewallHelper.OpenPortAsAdmin()` (yêu cầu quyền Admin)
   - Windows Firewall mặc định chỉ có Inbound rule, cần thêm Outbound rule

4. **Timeout Handling**:
   - ReadTimeout: 120000ms (2 phút) - cho phép độ trễ cao của WiFi/Internet
   - WriteTimeout: 60000ms (1 phút)
   - Cho phép timeout tạm thời (không ngắt kết nối ngay)
   - Chỉ ngắt kết nối sau 5 lần timeout liên tiếp

5. **Xử lý kết nối không ổn định**:
   - Retry mechanism cho timeout
   - Graceful disconnection
   - Reconnection support (client có thể kết nối lại)

---

## VI. USER STORIES

**Hình vẽ cần có:**
- Sơ đồ User Story với format: As a... I want to... So that...
- Luồng hoạt động của từng User Story
- Sequence diagram cho các User Story quan trọng

### 6.1. Authentication Features

**US-01: Đăng ký tài khoản**
- **As a** người dùng mới
- **I want to** đăng ký tài khoản với username, password, email
- **So that** tôi có thể sử dụng ứng dụng
- **Luồng**: 
  1. User nhập thông tin → RegisterPacket
  2. Server kiểm tra username/email đã tồn tại chưa
  3. Nếu chưa: Tạo user mới trong DB → RegisterResultPacket (Success = true)
  4. Nếu đã tồn tại: RegisterResultPacket (Success = false, Message = "User exists")

**US-02: Đăng nhập**
- **As a** người dùng đã đăng ký
- **I want to** đăng nhập bằng username/email và password
- **So that** tôi có thể truy cập ứng dụng
- **Luồng**:
  1. User nhập username/email + password → LoginPacket
  2. Server kiểm tra trong DB
  3. Nếu đúng: 
     - Tạo ClientHandler, lưu vào _clients dictionary
     - Gửi LoginResultPacket (Success = true, UserID, UserName, OnlineUsers)
  4. Nếu sai: LoginResultPacket (Success = false) → Đóng kết nối sau 300ms

**US-03: Quên mật khẩu**
- **As a** người dùng quên mật khẩu
- **I want to** nhận OTP qua email để reset mật khẩu
- **So that** tôi có thể lấy lại quyền truy cập
- **Luồng**:
  1. User nhập email → ForgotPasswordPacket
  2. Server kiểm tra email có tồn tại không
  3. Nếu có: 
     - Tạo OTP (6 chữ số ngẫu nhiên)
     - Gửi email OTP (sử dụng EmailHelper.SendOTP)
     - Lưu OTP trong ClientHandler (_currentOtp, _currentResetEmail, _otpCreatedTime)
     - Gửi ForgotPasswordResultPacket (Success = true, IsStep1Success = true)
  4. User nhập OTP → ResetPasswordPacket (OtpCode, NewPassword = null) - Verify OTP
  5. Server kiểm tra OTP:
     - Kiểm tra lockout (nếu sai quá 5 lần → lock 5 phút)
     - Kiểm tra OTP có khớp không
     - Kiểm tra OTP có hết hạn không (5 phút)
  6. Nếu OTP đúng: ForgotPasswordResultPacket (Success = true, IsStep1Success = false)
  7. User nhập mật khẩu mới → ResetPasswordPacket (OtpCode, NewPassword)
  8. Server đổi mật khẩu → ForgotPasswordResultPacket (Success = true, Message = "Password Changed")

### 6.2. Chat Features

**US-04: Chat 1-1**
- **As a** người dùng
- **I want to** gửi tin nhắn text cho người dùng khác
- **So that** tôi có thể giao tiếp real-time
- **Luồng**:
  1. User chọn người dùng từ danh sách online
  2. User nhập tin nhắn và gửi → TextPacket (SenderID, ReceiverID, MessageContent)
  3. Server nhận TextPacket → RelayPrivatePacket (gửi đến ReceiverID)
  4. Server lưu vào DB (async, không block): SaveMessage()
  5. Client nhận được TextPacket → Hiển thị trong chat window

**US-05: Gửi file**
- **As a** người dùng
- **I want to** gửi file cho người dùng khác
- **So that** tôi có thể chia sẻ tài liệu, hình ảnh
- **Luồng**:
  1. User chọn file từ dialog
  2. Đọc file thành byte array
  3. Convert sang Base64 string
  4. Gửi FilePacket (SenderID, ReceiverID, FileName, FileData, IsImage)
  5. Server relay đến receiver
  6. Server lưu vào DB: SaveMessage(..., MessageType = "Image" hoặc "File", FileName)
  7. Client nhận FilePacket → Hiển thị file/hình ảnh

**US-06: Xem lịch sử chat**
- **As a** người dùng
- **I want to** xem lại tin nhắn đã gửi/nhận trước đó
- **So that** tôi có thể tra cứu thông tin cũ
- **Luồng**:
  1. User mở chat với một người → ChatHistoryRequestPacket (UserID, FriendID, Limit)
  2. Server query DB: GetChatHistory(UserID, FriendID, Limit)
  3. Server gửi ChatHistoryResponsePacket (Messages: List<ChatHistoryMessage>)
  4. Client hiển thị lịch sử trong chat window

**US-07: Chat nhóm**
- **As a** người dùng
- **I want to** tạo nhóm và chat với nhiều người cùng lúc
- **So that** tôi có thể thảo luận nhóm
- **Luồng**:
  1. User tạo nhóm → CreateGroupPacket (CreatorID, GroupName, MemberIDs)
  2. Server tạo nhóm trong DB: CreateGroup() → GroupID
  3. Server gửi CreateGroupResultPacket cho creator
  4. Server gửi GroupInviteNotificationPacket cho các thành viên
  5. User gửi tin nhắn nhóm → GroupTextPacket (GroupID, SenderID, MessageContent)
  6. Server lưu vào DB: SaveGroupMessage()
  7. Server lấy danh sách thành viên: GetGroupMembers(GroupID)
  8. Server relay GroupTextPacket đến tất cả thành viên (trừ người gửi)

### 6.3. Game Features

**US-08: Mời chơi Caro**
- **As a** người chơi
- **I want to** mời người dùng khác chơi Caro
- **So that** tôi có thể giải trí với bạn bè
- **Luồng**:
  1. User click "Mời chơi Caro" → GameInvitePacket (SenderID, ReceiverID, GameType = "Caro")
  2. Server relay đến receiver
  3. Server lưu vào DB: SaveMessage(..., MessageType = "GameInvite", FileName = "Caro")
  4. Receiver nhận GameInvitePacket → Hiển thị thông báo
  5. Receiver chấp nhận/từ chối → GameResponsePacket (Accepted = true/false)
  6. Nếu chấp nhận:
     - Server tạo GameSession (GameID, Player1_ID, Player2_ID, GameType = Caro)
     - Server gửi GameStartPacket đến cả 2 người chơi

**US-09: Chơi Caro**
- **As a** người chơi
- **I want to** đánh quân cờ và xem nước đi của đối thủ real-time
- **So that** tôi có thể chơi game mượt mà
- **Luồng**:
  1. User click vào ô trống trên bàn cờ → GameMovePacket (GameID, SenderID, Row, Col)
  2. Server kiểm tra GameSession có tồn tại không
  3. Server relay GameMovePacket đến đối thủ
  4. Client nhận GameMovePacket → Cập nhật bàn cờ
  5. Client kiểm tra thắng/thua (client-side validation)
  6. Nếu thắng: Hiển thị thông báo "Bạn đã thắng!"

**US-10: Mời chơi Tank (1v1)**
- **As a** người chơi
- **I want to** mời người dùng khác chơi Tank
- **So that** tôi có thể giải trí với bạn bè
- **Luồng**: Tương tự US-08, nhưng GameType = "Tank"

**US-11: Chơi Tank (1v1)**
- **As a** người chơi
- **I want to** điều khiển xe tăng và bắn đạn real-time
- **So that** tôi có thể chơi game mượt mà
- **Luồng**:
  1. User nhấn phím (W/A/S/D/Space) → TankActionPacket (GameID, SenderID, ActionType, X, Y, Angle)
  2. Server relay đến đối thủ
  3. Server update game state (nếu là bắn đạn → thêm vào TankGameManager)
  4. Server chạy game loop (16ms) để update đạn: UpdateBullets()
  5. Khi đạn trúng đối thủ → TankHitPacket (GameID, ShooterID, HitPlayerID, Damage)
  6. Server xử lý: Giảm HP, kiểm tra thắng/thua
  7. Server gửi TankHitPacket đến cả 2 người chơi

**US-12: Tank Multiplayer - Tạo phòng**
- **As a** người chơi
- **I want to** tạo phòng chơi Tank Multiplayer
- **So that** tôi có thể chơi với nhiều người cùng lúc
- **Luồng**:
  1. User vào menu "Tank Multiplayer"
  2. User nhập tên phòng, chọn chế độ (FFA/Team), số người tối đa → TankCreateRoomPacket
  3. Server tạo phòng: TankMultiplayerManager.CreateRoom() → RoomID
  4. Server gửi TankCreateRoomResultPacket (Success = true, RoomID, Room)

**US-13: Tank Multiplayer - Tham gia phòng**
- **As a** người chơi
- **I want to** tham gia phòng chơi Tank Multiplayer
- **So that** tôi có thể chơi với nhiều người
- **Luồng**:
  1. User xem danh sách phòng → TankRequestRoomListPacket
  2. Server gửi TankRoomListPacket (Rooms: List<TankRoom>)
  3. User chọn phòng → TankJoinRoomPacket (RoomID, PlayerID, PlayerName)
  4. Server thêm player vào phòng: TankMultiplayerManager.JoinRoom()
  5. Server gửi TankJoinRoomResultPacket cho player
  6. Server gửi TankRoomUpdatePacket (UpdateType = "PlayerJoined") cho các player khác trong phòng

**US-14: Tank Multiplayer - Bắt đầu game**
- **As a** host
- **I want to** bắt đầu game khi tất cả đã ready
- **So that** tôi có thể chơi game
- **Luồng**:
  1. Tất cả players bấm "Ready" → TankPlayerReadyPacket
  2. Server update trạng thái ready của player
  3. Server gửi TankRoomUpdatePacket (UpdateType = "PlayerReady") cho các player khác
  4. Host bấm "Start Game" → TankStartMultiplayerPacket
  5. Server kiểm tra tất cả đã ready chưa
  6. Nếu đủ: 
     - Tạo game: TankMultiplayerManager.StartGame() → GameID
     - Gửi TankMultiplayerStartedPacket đến tất cả players (GameID, Players, SpawnPoints)
  7. Game bắt đầu, tất cả players đấu với nhau

---

## VII. HOW? WHY? WHAT?

### 7.1. HOW - Cách thức hoạt động

**7.1.1. Luồng đăng nhập:**
1. Client kết nối đến Server (TCP Socket)
2. Client gửi LoginPacket → Server
3. Server kiểm tra trong DB
4. Nếu đúng: Server tạo ClientHandler, lưu vào _clients dictionary
5. Server gửi LoginResultPacket (Success = true, UserID, UserName, OnlineUsers)
6. Client nhận LoginResultPacket → Hiển thị giao diện chat chính

**7.1.2. Luồng Game Caro:**
1. Client1 mời Client2 → GameInvitePacket → Server
2. Server relay đến Client2
3. Client2 chấp nhận → GameResponsePacket → Server
4. Server tạo GameSession, gửi GameStartPacket đến cả 2
5. Client1 đánh cờ → GameMovePacket → Server → Client2
6. Server kiểm tra GameSession có tồn tại không (tránh cheat)
7. Server relay GameMovePacket đến Client2
8. Client2 nhận → Cập nhật bàn cờ
9. Client kiểm tra thắng/thua (client-side check)
10. Khi thắng, hiển thị thông báo

**7.1.3. Luồng Game Tank:**
1. Tương tự mời game
2. Server tạo TankGameSession, khởi tạo vị trí xe tăng
3. Client gửi TankActionPacket (di chuyển, xoay, bắn) → Server
4. Server update game state (nếu là bắn → thêm đạn vào TankGameManager)
5. Server chạy game loop (16ms) để update đạn: UpdateBullets()
6. Server broadcast trạng thái đạn đến cả 2 players
7. Khi trúng đạn → TankHitPacket → Server → Client (giảm HP)
8. Server kiểm tra thắng/thua (HP = 0)

### 7.2. WHY - Lý do thiết kế

**7.2.1. Tại sao dùng TCP thay vì UDP?**
- **TCP**: 
  - Đảm bảo độ tin cậy (reliable) - không mất gói tin
  - Đảm bảo thứ tự gói tin (ordered) - quan trọng cho chat và game
  - Connection-oriented - dễ quản lý kết nối
- **UDP**: 
  - Nhanh hơn nhưng mất gói tin (không phù hợp cho chat)
  - Không đảm bảo thứ tự (khó xử lý)

**7.2.2. Tại sao dùng JSON thay vì Binary?**
- **JSON**: 
  - Dễ debug (human-readable)
  - Dễ mở rộng (thêm field mới không cần versioning)
  - Dễ serialize/deserialize với System.Text.Json
- **Binary**: 
  - Nhỏ hơn nhưng khó maintain
  - Cần versioning khi thay đổi structure

**7.2.3. Tại sao dùng Async/Await?**
- **Async**: 
  - Không block thread, xử lý nhiều client hiệu quả
  - Sử dụng thread pool tối ưu
  - Scalable (có thể xử lý hàng trăm client)
- **Synchronous**: 
  - Block thread, mỗi client cần 1 thread riêng
  - Không scale được (giới hạn số thread)

**7.2.4. Tại sao Server quản lý Game Session?**
- Đảm bảo tính nhất quán (consistency) - tránh conflict
- Tránh cheat (client-side validation dễ bị hack)
- Dễ mở rộng (thêm AI, spectator mode, replay)
- Server là source of truth cho game state

**7.2.5. Tại sao cần Outbound Firewall Rule?**
- Server cần gửi phản hồi đến client
- Windows Firewall mặc định chỉ có Inbound rule
- Outbound rule cho phép server gửi packet ra ngoài
- Quan trọng khi client ở mạng khác (Internet)

**7.2.6. Tại sao dùng Dictionary để quản lý clients?**
- O(1) lookup time - nhanh khi tìm client theo UserID
- Dễ quản lý (add, remove, update)
- Thread-safe với lock mechanism

### 7.3. WHAT - Công nghệ và công cụ

**7.3.1. Programming Language & Framework:**
- **C# .NET 8.0**: 
  - Ngôn ngữ chính, cross-platform
  - Hỗ trợ async/await tốt
  - System.Text.Json cho serialization
- **Windows Forms**: 
  - Giao diện desktop
  - Dễ sử dụng, phù hợp cho ứng dụng desktop
- **System.Net.Sockets**: 
  - TCP/IP socket programming
  - TcpListener, TcpClient, NetworkStream

**7.3.2. Database:**
- **SQL Server**: 
  - Quản lý dữ liệu người dùng, tin nhắn, lịch sử
  - Relational database, ACID compliance
- **ADO.NET**: 
  - Kết nối và truy vấn database
  - SqlConnection, SqlCommand

**7.3.3. Email Service:**
- **SMTP**: 
  - Gửi email OTP
  - Sử dụng System.Net.Mail (SmtpClient)

**7.3.4. Development Tools:**
- **Visual Studio**: IDE chính
- **SQL Server Management Studio**: Quản lý database
- **Git**: Version control

---

## VIII. BẢNG PHÂN CÔNG CÔNG VIỆC

### 8.1. Phân công theo module

| STT | Module | Người thực hiện | Mô tả công việc | Thời gian |
|-----|--------|-----------------|-----------------|-----------|
| 1 | Database Design | [Tên] | Thiết kế schema, tạo bảng Users, Messages, Groups, GroupMembers, GroupMessages | 1 tuần |
| 2 | Authentication | [Tên] | Đăng ký, đăng nhập, quên mật khẩu (OTP), update profile | 1 tuần |
| 3 | Network Layer | [Tên] | TCP Socket, PacketWrapper, PacketMapper, serialize/deserialize | 1 tuần |
| 4 | Server Core | [Tên] | Server class, ClientHandler, quản lý kết nối, đa luồng | 1.5 tuần |
| 5 | Chat 1-1 | [Tên] | Gửi/nhận tin nhắn text, file, lịch sử chat | 1 tuần |
| 6 | Chat Nhóm | [Tên] | Tạo nhóm, mời thành viên, chat nhóm, lịch sử nhóm | 1 tuần |
| 7 | Game Caro | [Tên] | Logic game Caro, mời chơi, đánh cờ, kiểm tra thắng/thua | 1.5 tuần |
| 8 | Game Tank 1v1 | [Tên] | Logic game Tank, điều khiển xe tăng, bắn đạn, xử lý va chạm | 2 tuần |
| 9 | Game Tank Multiplayer | [Tên] | Tạo phòng, tham gia phòng, ready, start game, xử lý nhiều players | 2 tuần |
| 10 | UI/UX | [Tên] | Thiết kế giao diện, form login, chat, game | 1.5 tuần |
| 11 | Testing & Debug | [Tên] | Test các chức năng, fix bug, tối ưu hiệu năng | 1 tuần |
| 12 | Documentation | [Tên] | Viết tài liệu, báo cáo, hướng dẫn sử dụng | 0.5 tuần |

### 8.2. Phân công theo tính năng

| Tính năng | Người phụ trách | Trạng thái | Ghi chú |
|-----------|-----------------|------------|---------|
| Authentication | [Tên] | ✅ Hoàn thành | Đăng nhập, đăng ký, quên mật khẩu |
| Chat 1-1 | [Tên] | ✅ Hoàn thành | Text, file, lịch sử |
| Chat Nhóm | [Tên] | ✅ Hoàn thành | Tạo nhóm, mời thành viên |
| Game Caro | [Tên] | ✅ Hoàn thành | 2 người chơi |
| Game Tank 1v1 | [Tên] | ✅ Hoàn thành | 2 người chơi |
| Game Tank Multiplayer | [Tên] | ✅ Hoàn thành | Tối đa 4 người |
| UI/UX | [Tên] | ✅ Hoàn thành | Giao diện thân thiện |

---

## IX. LINK GITHUB

**Repository**: [Link GitHub của dự án]

**Ví dụ**: `https://github.com/[username]/AppChatTichHopCaro`

---

## X. TỔNG KẾT

### 10.1. Điểm mạnh
- Kiến trúc rõ ràng, dễ mở rộng
- Xử lý đa kết nối hiệu quả với async/await
- Hỗ trợ cả LAN và Internet
- Tích hợp nhiều tính năng (chat, game, nhóm)

### 10.2. Hạn chế
- Chưa có encryption cho tin nhắn
- Chưa có voice/video call
- Chưa có AI cho game
- Chưa có mobile app

### 10.3. Hướng phát triển
- Thêm encryption (TLS/SSL)
- Thêm voice/video call
- Thêm mobile app (Xamarin/MAUI)
- Thêm AI bot cho game
- Thêm replay system cho game

---

**Ngày hoàn thành**: [Ngày/Tháng/Năm]
**Phiên bản**: 1.0
**Tác giả**: [Tên nhóm/Tên thành viên]

