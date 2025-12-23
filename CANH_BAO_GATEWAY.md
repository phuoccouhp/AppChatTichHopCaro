# ⚠️ CẢNH BÁO: KẾT NỐI ĐẾN DEFAULT GATEWAY

## ❌ VẤN ĐỀ QUAN TRỌNG

Code đã được thay đổi để **kết nối đến Default Gateway (Router IP)** thay vì IP WiFi của máy Server.

### ⚠️ ĐIỀU NÀY SẼ KHÔNG HOẠT ĐỘNG TRỪ KHI:

1. **Router được cấu hình Port Forwarding** để chuyển tiếp port 9000 đến máy Server
2. **Hoặc Server được chạy TRÊN Router** (không phải trên máy tính)

---

## 🔴 TẠI SAO SẼ KHÔNG HOẠT ĐỘNG?

### Khi Client kết nối đến Gateway (Router):
```
Client → Router (10.45.0.1:9000)
         ↑
         Router: "Tôi không có service nào chạy trên port 9000!"
         Router: "Connection refused!"
```

→ **Router sẽ TỪ CHỐI kết nối** vì không có ChatAppServer chạy trên router!

---

## ✅ ĐỂ HOẠT ĐỘNG, CẦN CẤU HÌNH PORT FORWARDING:

### Bước 1: Set IP tĩnh cho máy Server
- Truy cập router admin (thường: `10.45.0.1`)
- DHCP Reservation hoặc Static IP
- Set IP: `10.45.100.45` (IP của máy Server)

### Bước 2: Cấu hình Port Forwarding
- Virtual Server / Port Forwarding
- External Port: `9000`
- Internal IP: `10.45.100.45` (IP máy Server)
- Internal Port: `9000`
- Protocol: `TCP`

### Bước 3: Client kết nối đến Gateway
- Client nhập IP: `10.45.0.1` (Gateway IP)
- Router sẽ chuyển tiếp đến `10.45.100.45:9000`

---

## ⚠️ HẬU QUẢ CỦA THAY ĐỔI NÀY:

### ❌ Nếu KHÔNG có Port Forwarding:
- Client sẽ **KHÔNG THỂ** kết nối được
- Router sẽ từ chối kết nối
- Lỗi: "Connection refused"

### ✅ Nếu CÓ Port Forwarding:
- Client kết nối đến Gateway
- Router chuyển tiếp đến máy Server
- Hoạt động bình thường

---

## 💡 KHUYẾN NGHỊ:

**Nếu không có quyền cấu hình router, nên ĐỔI LẠI về IP WiFi:**

Code cũ (Đúng):
```csharp
// Lấy IP WiFi của máy Server
networkIP = socket.LocalEndPoint.Address.ToString();
// Ví dụ: 10.45.100.45
```

Code mới (Cần Port Forwarding):
```csharp
// Lấy Default Gateway
gatewayIP = gateway.Address.ToString();
// Ví dụ: 10.45.0.1 (Router IP)
```

---

## 🔄 CÁCH ĐỔI LẠI:

Nếu muốn đổi lại về IP WiFi, sửa hàm `GetLocalIPAddresses()` trong `frmServer.cs`:

```csharp
// Lấy IP WiFi thay vì Gateway
using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
{
    socket.Connect("8.8.8.8", 65530);
    var endPoint = socket.LocalEndPoint as IPEndPoint;
    networkIP = endPoint.Address.ToString(); // IP WiFi
}
```

