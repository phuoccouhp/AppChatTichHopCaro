# GIẢI THÍCH: IP MÀ FORM SERVER LẤY LÀ IP GÌ?

> **Xem thêm:** `TAI_SAO_IP_WIFI_KHONG_PHAI_GATEWAY.md` - Giải thích tại sao không dùng Default Gateway

# GIẢI THÍCH: IP MÀ FORM SERVER LẤY LÀ IP GÌ?

## 📍 IP được hiển thị là gì?

Khi bạn mở form Server, code lấy IP bằng **2 cách** (theo thứ tự ưu tiên):

---

## 🔍 CÁCH 1: Lấy IP từ Interface đang Active (Ưu tiên)

```csharp
using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
{
    socket.Connect("8.8.8.8", 65530);  // Connect đến Google DNS
    var endPoint = socket.LocalEndPoint as IPEndPoint;
    networkIP = endPoint.Address.ToString();  // Lấy IP của interface này
}
```

### Cách này làm gì?
1. **Tạo một socket UDP**
2. **Connect đến `8.8.8.8`** (Google DNS server)
3. **Lấy IP của interface mà socket sử dụng** để connect

### Kết quả:
- → Lấy được **IP của WiFi adapter đang được dùng để kết nối internet**
- → Đây là IP **THỰC TẾ** mà máy khác trong mạng WiFi có thể dùng để kết nối đến

### Ví dụ:
- Nếu máy bạn đang dùng WiFi → Lấy IP WiFi (vd: `10.215.204.194`)
- Nếu máy bạn đang dùng Ethernet → Lấy IP Ethernet
- Nếu có cả WiFi và Ethernet → Lấy IP của interface nào được dùng để đi internet

---

## 🔍 CÁCH 2: Fallback - Lấy từ Host Entry

Nếu cách 1 thất bại (không có internet), code sẽ:

```csharp
var host = Dns.GetHostEntry(Dns.GetHostName());
foreach (var ip in host.AddressList)
{
    if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
    {
        networkIP = ip.ToString();  // Lấy IP đầu tiên không phải 127.0.0.1
        break;
    }
}
```

### Cách này làm gì?
1. Lấy tên máy (hostname)
2. Resolve tên máy → Danh sách **TẤT CẢ IP** của máy
3. Chọn **IP đầu tiên** là IPv4 và không phải `127.0.0.1`

### Vấn đề:
- ❌ Có thể lấy **SAI IP** nếu máy có nhiều adapter (VirtualBox, VMware, VPN, ...)
- ❌ Không biết IP nào đang được dùng để kết nối mạng thực tế

---

## 📊 SO SÁNH 2 CÁCH:

| Tiêu chí | Cách 1 (Connect 8.8.8.8) | Cách 2 (Host Entry) |
|----------|---------------------------|---------------------|
| **Độ chính xác** | ✅ Rất cao - Lấy đúng IP đang dùng | ⚠️ Thấp - Có thể lấy sai IP |
| **Khi không có internet** | ❌ Fail | ✅ Vẫn hoạt động |
| **Nhiều adapter** | ✅ Lấy đúng adapter active | ❌ Có thể lấy sai |

---

## 🎯 IP NÀO ĐƯỢC HIỂN THỊ?

Form Server hiển thị: `127.0.0.1, 10.215.204.194`

Trong đó:
- `127.0.0.1` = **Loopback** (localhost) - chỉ dùng khi Client và Server **CÙNG MÁY**
- `10.215.204.194` = **IP WiFi thực tế** - dùng khi Client ở **MÁY KHÁC**

---

## ⚠️ LƯU Ý QUAN TRỌNG:

### Trường hợp máy có nhiều adapter:

Máy bạn có thể có:
- `192.168.3.3` → VirtualBox adapter
- `192.168.56.1` → VMware adapter  
- `192.168.206.1` → VPN adapter
- `10.0.3.1` → WSL adapter
- `10.215.204.194` → **WiFi adapter thực** ← Cái này đúng!

**Cách 1 (connect 8.8.8.8) sẽ tự động chọn đúng IP WiFi `10.215.204.194`**

---

## ✅ KẾT LUẬN:

**IP mà form Server lấy là: IP của WiFi adapter đang được dùng để kết nối internet**

- ✅ Đây là IP **ĐÚNG** để máy khác kết nối đến
- ✅ Không phải Default Gateway
- ✅ Không phải DNS Server IP
- ✅ Là IP **CỦA MÁY BẠN** trong mạng WiFi

