# HƯỚNG DẪN DEBUG KẾT NỐI QUA WIFI

## BƯỚC 1: KIỂM TRA TRÊN SERVER

### 1.1. Kiểm tra Server có đang chạy không
- Mở ứng dụng Server
- Nhấn nút "Start Server"
- Kiểm tra log có hiển thị: "Server đã khởi động tại Port: 9000"

### 1.2. Kiểm tra IP WiFi của Server
- Xem trong log Server, tìm dòng: "Hãy gửi IP [XXX.XXX.XXX.XXX] cho máy Client"
- Ghi lại IP này (ví dụ: 10.45.100.45)

### 1.3. Kiểm tra Firewall
- Trên Server, nhấn nút "Mở Firewall"
- Chờ UAC hiện lên, chọn "Yes"
- Kiểm tra log có hiển thị: "Firewall rule được tạo thành công"
- Nếu không được, chạy file `OpenFirewall.bat` với quyền Administrator

### 1.4. Test Port trên Server
- Trên Server, nhấn nút "Test Connection"
- Chọn "YES" (Kiểm tra Port 9000 Local)
- Kiểm tra kết quả:
  - ✓ Port sẵn sàng
  - ✓ ĐANG LẮNG NGHE (Server đang chạy)
  - ✓ Firewall rule đã mở

## BƯỚC 2: KIỂM TRA TRÊN CLIENT

### 2.1. Kiểm tra cùng mạng WiFi
- Đảm bảo Client và Server cùng một mạng WiFi
- Không dùng mạng khách (Guest network) nếu có AP Isolation

### 2.2. Ping từ Client đến Server
- Mở CMD trên Client
- Chạy lệnh: `ping [IP_SERVER]`
  - Ví dụ: `ping 10.45.100.45`
- Nếu ping thành công → Mạng OK
- Nếu ping thất bại → Kiểm tra:
  - Có cùng WiFi không?
  - IP có đúng không?
  - Router có AP Isolation không?

### 2.3. Test Port từ Client
- Mở CMD trên Client
- Chạy lệnh: `telnet [IP_SERVER] 9000`
  - Nếu không có telnet, cài đặt: "Turn Windows features on/off" → Telnet Client
- Nếu kết nối được → Port đã mở
- Nếu không kết nối được → Firewall trên Server chưa mở

### 2.4. Kiểm tra Firewall trên Client
- Đảm bảo Windows Firewall trên Client không chặn ứng dụng
- Hoặc tạm thời tắt Firewall để test

## BƯỚC 3: DEBUG LOG

### 3.1. Xem log trên Server
Khi Client kết nối, Server log sẽ hiển thị:
```
[Connect] Client mới kết nối từ: XXX.XXX.XXX.XXX
```
Nếu không thấy dòng này → Client chưa kết nối được

### 3.2. Xem log trên Client
- Mở Console Output (nếu có) hoặc xem log trong ứng dụng
- Tìm các dòng:
  - "Đang thử kết nối đến..."
  - "Kết nối THÀNH CÔNG..."
  - "Server đã đóng kết nối..."

## BƯỚC 4: CÁC LỖI THƯỜNG GẶP

### Lỗi: "Không thể kết nối đến Server"
**Nguyên nhân:**
1. Server chưa chạy
2. IP sai
3. Firewall chặn
4. Không cùng mạng

**Giải pháp:**
- Kiểm tra Server có đang chạy không
- Kiểm tra IP có đúng không
- Mở Firewall trên Server
- Đảm bảo cùng mạng WiFi

### Lỗi: "Connection Refused"
**Nguyên nhân:**
- Server từ chối kết nối
- Port 9000 chưa được mở

**Giải pháp:**
- Kiểm tra Server có đang lắng nghe không
- Mở Firewall port 9000

### Lỗi: "SerializationException"
**Nguyên nhân:**
- Dữ liệu bị corrupt
- Version không khớp giữa Client và Server

**Giải pháp:**
- Build lại cả Client và Server với cùng version
- Đảm bảo cả hai đều dùng cùng file ChatApp.Shared.dll

### Lỗi: "Timeout"
**Nguyên nhân:**
- Mạng chậm
- Firewall đang chặn nhưng không reject ngay

**Giải pháp:**
- Kiểm tra tốc độ mạng
- Tắt Firewall tạm thời để test
- Kiểm tra router có chặn không

## BƯỚC 5: KIỂM TRA BẰNG NETSTAT

### Trên Server
Mở CMD, chạy:
```cmd
netstat -ano | findstr :9000
```

Kết quả mong đợi:
```
TCP    0.0.0.0:9000    0.0.0.0:0    LISTENING    [PID]
```

Nếu thấy `LISTENING` → Server đang lắng nghe đúng

### Trên Client (khi đã kết nối)
```cmd
netstat -ano | findstr :9000
```

Kết quả mong đợi:
```
TCP    [CLIENT_IP]:[PORT]    [SERVER_IP]:9000    ESTABLISHED    [PID]
```

## BƯỚC 6: TEST THỦ CÔNG VỚI TELNET

### Cài đặt Telnet (nếu chưa có)
1. Mở "Control Panel" → "Programs" → "Turn Windows features on/off"
2. Tích vào "Telnet Client"
3. Nhấn OK

### Test kết nối
Trên Client, mở CMD và chạy:
```cmd
telnet [IP_SERVER] 9000
```

- Nếu kết nối được (màn hình đen) → Port đã mở, vấn đề ở ứng dụng
- Nếu "Could not open connection" → Port chưa mở hoặc Firewall chặn

## BƯỚC 7: FIREWALL ADVANCED

Nếu vẫn không được, thử:

1. **Tắt Firewall tạm thời** (chỉ để test):
   - Control Panel → Windows Defender Firewall
   - Turn Windows Defender Firewall on or off
   - Tắt cả Private và Public network (tạm thời)

2. **Mở port thủ công**:
   - Windows Defender Firewall → Advanced settings
   - Inbound Rules → New Rule
   - Rule Type: Port
   - Protocol: TCP
   - Port: 9000
   - Action: Allow
   - Profile: All
   - Name: "ChatApp Server"

## BƯỚC 8: KIỂM TRA ROUTER

Một số router có tính năng **AP Isolation** hoặc **Client Isolation**:
- Tính năng này ngăn các thiết bị trong cùng WiFi giao tiếp với nhau
- Cần tắt tính năng này trong cài đặt router

Để kiểm tra:
- Đăng nhập vào router (thường là 192.168.1.1 hoặc 192.168.0.1)
- Tìm "Wireless" → "Advanced Settings"
- Tắt "AP Isolation" hoặc "Client Isolation"

## BƯỚC 9: SỬ DỤNG IP THAY VÌ HOSTNAME

Đảm bảo Client nhập đúng IP của Server, không dùng hostname:
- ✅ Đúng: `10.45.100.45`
- ❌ Sai: `SERVER-PC` hoặc `192.168.1.100` (nếu IP thay đổi)

## BƯỚC 10: LOG CHI TIẾT

Nếu vẫn không được, hãy:
1. Copy toàn bộ log từ Server khi Client kết nối
2. Copy toàn bộ log từ Client
3. Gửi kèm:
   - IP của Server
   - IP của Client
   - Kết quả ping
   - Kết quả netstat

