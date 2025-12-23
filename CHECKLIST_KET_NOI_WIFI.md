# ✅ CHECKLIST KẾT NỐI WIFI - KIỂM TRA NHANH

## 🔴 BƯỚC 1: SERVER (Máy chủ)

- [ ] **Server đang chạy?**
  - Mở ứng dụng Server
  - Nhấn nút "Start Server"
  - Log hiển thị: "Server đã khởi động tại Port: 9000"

- [ ] **Ghi lại IP WiFi của Server**
  - Xem trong log: "Hãy gửi IP [XXX.XXX.XXX.XXX] cho máy Client"
  - IP này: ________________ (ví dụ: 10.45.100.45)

- [ ] **Firewall đã mở?**
  - Nhấn nút "Mở Firewall" trên Server
  - Chấp nhận UAC (chọn Yes)
  - Log hiển thị: "Firewall rule được tạo thành công"
  - **Nếu không được:** Chạy file `OpenFirewall.bat` với quyền Administrator

- [ ] **Test Port Local**
  - Nhấn "Test Connection" → Chọn "YES"
  - Kiểm tra tất cả đều ✓ (đặc biệt là "ĐANG LẮNG NGHE")

---

## 🔵 BƯỚC 2: CLIENT (Máy khách)

- [ ] **Cùng mạng WiFi?**
  - Đảm bảo Client và Server cùng một WiFi
  - Không dùng WiFi khách (Guest)

- [ ] **Nhập đúng IP Server**
  - Nhập IP từ bước 1 vào ô Server IP
  - Không dùng localhost hoặc 127.0.0.1
  - Đúng: `10.45.100.45` ❌ Sai: `192.168.1.100` (nếu IP thay đổi)

- [ ] **Ping test (Tùy chọn)**
  - Mở CMD: `ping [IP_SERVER]`
  - Nếu thành công → Mạng OK
  - Nếu thất bại → Kiểm tra cùng WiFi, router có AP Isolation không

---

## 🟢 BƯỚC 3: KẾT NỐI VÀ ĐĂNG NHẬP

- [ ] **Kết nối**
  - Nhấn nút "Log in" trên Client
  - Đợi "Connecting..."
  - Kiểm tra log Client: "Kết nối THÀNH CÔNG..."

- [ ] **Đăng nhập**
  - Nhập username/password
  - Nhấn "Log in"
  - Đợi "Logging in..."
  - Kiểm tra có vào được màn hình chính không

---

## 🟡 NẾU VẪN KHÔNG ĐƯỢC

### Kiểm tra nhanh:

1. **Server log có hiển thị Client kết nối?**
   - Tìm: `[Connect] Client mới kết nối từ: XXX.XXX.XXX.XXX`
   - Nếu KHÔNG có → Client chưa kết nối được (Firewall/Network)

2. **Client log có lỗi gì?**
   - "Timeout" → Mạng chậm hoặc Firewall chặn
   - "Connection Refused" → Server chưa mở port
   - "SerializationException" → Version không khớp (build lại)

3. **Test bằng Telnet (Nếu có)**
   ```cmd
   telnet [IP_SERVER] 9000
   ```
   - Kết nối được → Port OK, vấn đề ở ứng dụng
   - Không kết nối được → Firewall/Network

### Giải pháp nhanh:

1. **Tắt Firewall tạm thời để test**
   - Control Panel → Windows Defender Firewall → Turn off (tạm thời)

2. **Kiểm tra Router có AP Isolation không**
   - Đăng nhập router → Tắt "AP Isolation" hoặc "Client Isolation"

3. **Build lại cả Client và Server**
   - Đảm bảo cùng version ChatApp.Shared.dll

---

## 📋 THÔNG TIN CẦN CUNG CẤP KHI BÁO LỖI

Nếu vẫn không được, cung cấp:

1. IP Server: _______
2. IP Client: _______
3. Kết quả ping: ✓ / ✗
4. Log Server (khi Client kết nối): _______
5. Log Client: _______
6. Lỗi hiển thị: _______

