# ✅ CHECKLIST: KIỂM TRA KẾT NỐI GIỮA HAI MÁY

## 🔍 CÁC BƯỚC KIỂM TRA CHI TIẾT

### 📱 **BƯỚC 1: TRÊN MÁY SERVER**

- [ ] **1.1. Kiểm tra WiFi đã kết nối**
  ```
  Mở CMD → gõ: ipconfig
  Xem "Wireless LAN adapter Wi-Fi" → Phải có "IPv4 Address"
  ```

- [ ] **1.2. Mở Firewall (QUAN TRỌNG!)**
  - Cách 1: Chạy file `OpenFirewall.bat` với quyền Admin
  - Cách 2: Click nút "🔓 Mở Firewall" trong form Server
  - Kiểm tra: Phải có thông báo "Đã mở port 9000"

- [ ] **1.3. Khởi động Server**
  - Click "Start Server"
  - Kiểm tra log: Phải thấy "Server đang lắng nghe tại 0.0.0.0:9000"
  - Ghi lại IP hiển thị (ví dụ: `10.215.204.194`)

- [ ] **1.4. Kiểm tra IP đúng không**
  - IP phải là IP WiFi (không phải VirtualBox, VMware, VPN)
  - IP phải cùng subnet với máy Client

---

### 📱 **BƯỚC 2: TRÊN MÁY CLIENT**

- [ ] **2.1. Kiểm tra WiFi đã kết nối**
  ```
  Mở CMD → gõ: ipconfig
  Xem "Wireless LAN adapter Wi-Fi" → Phải có "IPv4 Address"
  ```

- [ ] **2.2. Kiểm tra cùng mạng với Server**
  ```
  Mở CMD → gõ: ping <IP_SERVER>
  Ví dụ: ping 10.215.204.194
  
  ✅ Nếu "Reply from..." → Mạng OK
  ❌ Nếu "Request timed out" → KHÁC MẠNG → Cần kết nối cùng WiFi
  ```

- [ ] **2.3. Mở Firewall (QUAN TRỌNG!)**
  - Chạy file `OpenFirewall.bat` với quyền Admin
  - Client CŨNG CẦN mở firewall để tạo OUTBOUND connections

- [ ] **2.4. Mở ChatAppClient và nhập IP**
  - Nhập IP của máy Server (ví dụ: `10.215.204.194`)
  - KHÔNG nhập `127.0.0.1` (chỉ dùng khi cùng máy)
  - Click Login

---

## 🔧 CÁC LỖI THƯỜNG GẶP VÀ CÁCH SỬA

### ❌ Lỗi: "Connection refused" hoặc "Connection timeout"

**Nguyên nhân:**
1. Server chưa Start
2. Firewall trên Server chưa mở
3. IP nhập sai

**Cách sửa:**
```
1. Trên Server: Click "Start Server" → Kiểm tra log
2. Chạy OpenFirewall.bat trên Server với quyền Admin
3. Kiểm tra IP trên form Server → Copy chính xác IP đó
```

---

### ❌ Lỗi: "Request timed out" khi ping

**Nguyên nhân:**
- Hai máy KHÔNG cùng mạng WiFi
- Khác subnet

**Cách sửa:**
```
1. Kiểm tra cả hai máy đều kết nối cùng tên WiFi
2. Thử ngắt kết nối và kết nối lại WiFi trên cả hai máy
3. Nếu vẫn không được → Dùng Mobile Hotspot
```

---

### ❌ Lỗi: "Host unreachable"

**Nguyên nhân:**
- IP nhập sai
- Máy Client không có internet/WiFi

**Cách sửa:**
```
1. Kiểm tra WiFi trên máy Client đã kết nối chưa
2. Kiểm tra lại IP trên form Server → Copy lại
3. Không được nhập IP Gateway (router IP)
```

---

### ❌ Lỗi: "Access denied" hoặc "Socket error"

**Nguyên nhân:**
- Firewall trên Client chặn OUTBOUND connections
- Antivirus chặn

**Cách sửa:**
```
1. Chạy OpenFirewall.bat trên máy Client với quyền Admin
2. Tạm tắt Antivirus để test
3. Thử tắt Windows Firewall tạm thời (chỉ để test)
```

---

## 🧪 TEST KẾT NỐI TỪNG BƯỚC

### Test 1: Ping từ Client đến Server
```cmd
ping <IP_SERVER>
```
✅ Phải có "Reply from..." 

### Test 2: Test port 9000 từ Client
```powershell
Test-NetConnection -ComputerName <IP_SERVER> -Port 9000
```
✅ Phải thấy "TcpTestSucceeded : True"

### Test 3: Test từ Server
- Click nút "🔍 Test Kết Nối" trên form Server
- Nhập IP của máy Client
- Xem kết quả trong log

---

## ⚠️ LƯU Ý QUAN TRỌNG

1. **IP là gì?**
   - ✅ IP WiFi thực tế của máy Server (ví dụ: `10.215.204.194`)
   - ❌ KHÔNG phải Default Gateway (ví dụ: `10.215.204.1`)
   - ❌ KHÔNG phải DNS Server (ví dụ: `8.8.8.8`)
   - ❌ KHÔNG phải `127.0.0.1` (chỉ dùng khi cùng máy)

2. **Firewall cần mở ở đâu?**
   - ✅ Server: Để nhận INBOUND connections
   - ✅ Client: Để tạo OUTBOUND connections (nhiều firewall chặn cả outbound)

3. **Cùng mạng nghĩa là gì?**
   - Cả hai máy kết nối vào **CÙNG một WiFi access point**
   - IP phải cùng subnet (3 số đầu giống nhau, ví dụ: `10.215.204.x`)

4. **Subnet mask phải là gì?**
   - Thường là `255.255.255.0` (24 bits)
   - Nghĩa là 3 số đầu phải giống nhau

---

## 📞 NẾU VẪN KHÔNG ĐƯỢC

1. Gửi log từ Server (copy tất cả text trong RichTextBox log)
2. Gửi log từ Client (nếu có)
3. Gửi kết quả `ipconfig` từ cả hai máy
4. Gửi kết quả `ping` từ Client đến Server

