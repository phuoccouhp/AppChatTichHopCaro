# 📱 HƯỚNG DẪN: DÙNG MOBILE HOTSPOT ĐỂ KẾT NỐI

## ✅ TẠI SAO DÙNG MOBILE HOTSPOT?

Mạng WiFi hiện tại của bạn có vẻ bị router phân chia VLAN hoặc có AP Isolation:
- Máy bạn: `10.45.100.45`
- Máy bạn kia: `10.45.210.103`
- Cùng subnet mask nhưng KHÔNG ping được → Router chặn giao tiếp

**Mobile Hotspot** tạo mạng WiFi riêng, đảm bảo cả hai máy cùng subnet và không bị chặn.

---

## 📱 BƯỚC 1: BẬT HOTSPOT TRÊN ĐIỆN THOẠI

### Android:
1. Mở **Settings** (Cài đặt)
2. Vào **Network & internet** (Mạng & Internet)
3. Vào **Hotspot & tethering** (Điểm truy cập di động & chia sẻ)
4. Bật **Wi-Fi hotspot** (Điểm truy cập Wi-Fi)
5. Ghi lại:
   - **Tên mạng (SSID)**: Ví dụ: "AndroidAP"
   - **Mật khẩu**: Ví dụ: "abc123456"

### iPhone:
1. Mở **Settings** (Cài đặt)
2. Vào **Personal Hotspot** (Điểm phát sóng cá nhân)
3. Bật **Allow Others to Join** (Cho phép người khác tham gia)
4. Ghi lại:
   - **Tên mạng (SSID)**: Tên iPhone của bạn
   - **Mật khẩu**: Mật khẩu hiển thị (hoặc đổi mật khẩu mới)

---

## 💻 BƯỚC 2: KẾT NỐI WIFI TỪ ĐIỆN THOẠI

### Trên cả hai máy (Server và Client):

1. **Ngắt WiFi hiện tại**
   - Click icon WiFi ở taskbar
   - Disconnect khỏi WiFi hiện tại

2. **Kết nối vào WiFi từ điện thoại**
   - Click icon WiFi
   - Tìm tên mạng (SSID) từ điện thoại
   - Nhập mật khẩu
   - Kết nối

3. **Đợi 30 giây** để nhận IP mới

---

## 🔍 BƯỚC 3: KIỂM TRA IP MỚI

### Trên cả hai máy, mở CMD và chạy:
```cmd
ipconfig
```

### Tìm dòng "Wireless LAN adapter Wi-Fi":
```
Wireless LAN adapter Wi-Fi:
   IPv4 Address. . . . . . . . . . . : 192.168.43.10  ← IP này
   Subnet Mask . . . . . . . . . . . : 255.255.255.0
   Default Gateway . . . . . . . . . : 192.168.43.1
```

### Kiểm tra:
- ✅ Cả hai máy phải có IP bắt đầu bằng **192.168.43.x** (Android) hoặc **192.168.x.x** (iPhone)
- ✅ **3 số đầu phải GIỐNG NHAU** (ví dụ: 192.168.43.10 và 192.168.43.20)
- ✅ Subnet Mask phải là **255.255.255.0**

---

## 🖥️ BƯỚC 4: CHẠY SERVER VÀ CLIENT

### Trên máy Server (máy bạn):

1. **Xem IP mới:**
   ```cmd
   ipconfig
   ```
   - Ghi lại IP WiFi (ví dụ: `192.168.43.10`)

2. **Mở ChatAppServer:**
   - Click **"🔓 Mở Firewall"** → Đồng ý UAC
   - Click **"Start Server"**
   - Xem IP hiển thị (phải là IP mới, ví dụ: `192.168.43.10`)
   - Ghi lại IP này

### Trên máy Client (máy bạn của bạn):

1. **Kiểm tra IP:**
   ```cmd
   ipconfig
   ```
   - Phải có IP WiFi cùng subnet (ví dụ: `192.168.43.20`)

2. **Test ping:**
   ```cmd
   ping 192.168.43.10
   ```
   - ✅ Phải thấy "Reply from 192.168.43.10"
   - ❌ Nếu "Request timed out" → Kiểm tra lại WiFi

3. **Mở ChatAppClient:**
   - Nhập IP của máy Server: `192.168.43.10` (IP bạn ghi lại ở trên)
   - Nhập Username/Password
   - Click Login

---

## ✅ KẾT QUẢ MONG ĐỢI

### Khi thành công:
- ✅ Ping thành công từ Client đến Server
- ✅ Client kết nối được đến Server
- ✅ Login thành công
- ✅ Có thể chat và chơi game

---

## ❌ NẾU VẪN KHÔNG ĐƯỢC

### Kiểm tra lại:

1. **Cả hai máy đã kết nối WiFi từ điện thoại chưa?**
   - Click icon WiFi → Phải thấy tên mạng từ điện thoại

2. **IP có cùng subnet không?**
   - Ví dụ: `192.168.43.10` và `192.168.43.20` = ✅ OK
   - Ví dụ: `192.168.43.10` và `192.168.1.20` = ❌ SAI

3. **Đã ping được chưa?**
   - Từ Client: `ping <IP_SERVER>`
   - Phải có "Reply from..."

4. **Firewall đã mở chưa?**
   - Server: Chạy `OpenFirewall.bat` hoặc click "Mở Firewall"
   - Client: Chạy `OpenFirewall.bat`

---

## 💡 MẸO

- **Để tiết kiệm pin điện thoại**: Kết nối điện thoại vào sạc khi bật Hotspot
- **Tốc độ**: Hotspot có thể chậm hơn WiFi thông thường, nhưng đủ dùng cho chat
- **Bảo mật**: Đổi mật khẩu Hotspot mạnh hơn nếu cần

---

## 🎯 TÓM TẮT

1. ✅ Bật Hotspot trên điện thoại
2. ✅ Cả hai máy kết nối WiFi từ điện thoại
3. ✅ Kiểm tra IP mới (phải cùng subnet)
4. ✅ Server: Start Server → Ghi lại IP mới
5. ✅ Client: Nhập IP mới → Login
6. ✅ Done! 🎉

