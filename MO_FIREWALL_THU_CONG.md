# 🔧 HƯỚNG DẪN: MỞ FIREWALL THỦ CÔNG

Nếu script không hoạt động, bạn có thể mở Firewall thủ công như sau:

---

## 📋 CÁCH 1: DÙNG WINDOWS SETTINGS (Đơn giản nhất)

### Bước 1: Mở Windows Security
1. Click vào **Windows Start Menu**
2. Tìm và mở **Windows Security** (hoặc **Windows Defender Firewall**)

### Bước 2: Mở Advanced Settings
1. Click **Firewall & network protection**
2. Click **Advanced settings** (ở cuối trang)
   - Có thể cần quyền Admin → Click Yes

### Bước 3: Thêm Inbound Rule
1. Click **Inbound Rules** (bên trái)
2. Click **New Rule...** (bên phải)
3. Chọn **Port** → Next
4. Chọn **TCP** và **Specific local ports**: nhập `9000` → Next
5. Chọn **Allow the connection** → Next
6. Chọn **Private** và **Domain** (bỏ Public) → Next
7. Nhập tên: `ChatAppServer` → Finish

### Bước 4: Thêm Outbound Rule
1. Click **Outbound Rules** (bên trái)
2. Lặp lại các bước từ Bước 3, nhưng chọn **Outbound Rules**
3. Tên: `ChatAppServer (Out)`

---

## 📋 CÁCH 2: DÙNG COMMAND LINE (Nhanh hơn)

### Mở CMD với quyền Administrator:
1. Click **Windows Start Menu**
2. Tìm **Command Prompt** hoặc **cmd**
3. **Right-click** → **Run as administrator**

### Chạy các lệnh sau:

```cmd
netsh advfirewall firewall add rule name="ChatAppServer" dir=in action=allow protocol=TCP localport=9000 profile=private,domain enable=yes

netsh advfirewall firewall add rule name="ChatAppServer (Out)" dir=out action=allow protocol=TCP localport=9000 profile=private,domain enable=yes
```

### Kiểm tra đã tạo chưa:

```cmd
netsh advfirewall firewall show rule name="ChatAppServer"
```

Phải thấy dòng: `Rule Name: ChatAppServer`

---

## 📋 CÁCH 3: DÙNG POWERSHELL (Khuyến nghị)

### Mở PowerShell với quyền Administrator:
1. Click **Windows Start Menu**
2. Tìm **PowerShell**
3. **Right-click** → **Run as administrator**

### Chạy lệnh:

```powershell
New-NetFirewallRule -DisplayName "ChatAppServer" -Direction Inbound -Protocol TCP -LocalPort 9000 -Action Allow -Profile Private,Domain

New-NetFirewallRule -DisplayName "ChatAppServer (Out)" -Direction Outbound -Protocol TCP -LocalPort 9000 -Action Allow -Profile Private,Domain
```

---

## ✅ KIỂM TRA ĐÃ MỞ THÀNH CÔNG

### Cách 1: Dùng Command
```cmd
netsh advfirewall firewall show rule name="ChatAppServer"
```

Phải thấy:
- `Rule Name: ChatAppServer`
- `Enabled:                              Yes`
- `Direction:                            In`
- `Profiles:                             Domain,Private`

### Cách 2: Kiểm tra trong Windows Firewall
1. Windows Security → Firewall & network protection → Advanced settings
2. Inbound Rules → Tìm "ChatAppServer"
3. Kiểm tra:
   - ✅ Status = **Enabled** (màu xanh)
   - ✅ Profile = **Domain, Private**
   - ✅ Action = **Allow**

---

## ❌ NẾU VẪN KHÔNG ĐƯỢC

### Thử tạm tắt Windows Firewall (CHỈ để test):

1. Windows Security → Firewall & network protection
2. Click **Private network** (hoặc Domain network)
3. Tắt firewall tạm thời
4. Test kết nối
5. **NHỚ BẬT LẠI** sau khi test xong!

---

## ⚠️ LƯU Ý

- **Private network** = WiFi ở nhà, công ty
- **Public network** = WiFi công cộng (café, sân bay)
- Rule chỉ áp dụng cho **Private và Domain**, không áp dụng cho **Public**

Nếu bạn đang dùng Public network, cần thêm rule cho Public profile.

