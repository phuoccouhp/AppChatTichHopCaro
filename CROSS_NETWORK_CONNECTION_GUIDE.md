H??NG D?N K?T N?I CROSS-NETWORK (CLIENT T? M?NG KHÁC)
====================================================

## V?n ??
Client t? **m?ng khác** không k?t n?i ???c ??n Server, m?c dù client cùng m?ng WiFi/LAN ho?t ??ng bình th??ng.

## Nguyên nhân chính
- **Windows Firewall rule không bao g?m t?t c? profiles** (Domain, Private, Public)
- **Profile hi?n t?i không phù h?p** khi client t? m?ng khác k?t n?i
- **IPv4 Forward không ???c b?t** (hi?m g?p)

## Gi?i pháp (Theo th? t?)

### 1?? CÁCH ?? XU?T: S? d?ng nút "M? Firewall" trong App
**B??c:**
1. M? form Server
2. Nh?n nút **"?? M? Firewall"** ? Ch?n **Yes**
3. **??ng ý c?p quy?n Admin** khi h?p tho?i UAC hi?n lên
4. Ch? xong, nút s? chuy?n sang màu xanh `? Firewall OK`
5. Nh?n **"Start Server"** ?? kh?i ??ng

**L?i ích:**
- T? ??ng t?o firewall rules cho **t?t c? profiles** (Domain, Private, Public)
- Không c?n thao tác th? công

---

### 2?? CÁCH TH? CÔNG: Windows Defender Firewall

**A. M? Firewall Settings**
- M? `Windows Defender Firewall` ? `Advanced Settings`
- Ho?c ch?y: `wf.msc` (Windows+R ? wf.msc)

**B. T?o Inbound Rule**
1. Click `Inbound Rules` (trái) ? `New Rule...` (ph?i)
2. Ch?n: **Port** ? Next
3. Ch?n: **TCP** ? Local port ? nh?p `9000` ? Next
4. Ch?n: **Allow the connection** ? Next
5. **Tick t?t c? 3 profiles**: Domain, Private, Public ? Next
6. Name: `ChatAppServer` ? Finish

**C. T?o Outbound Rule** (Gi?ng nh? trên)
1. Click `Outbound Rules` (trái) ? `New Rule...` (ph?i)
2. Port ? TCP ? `9000`
3. Allow ? **Tick Domain, Private, Public** ? Name: `ChatAppServer (Out)` ? Finish

**D. Verify**
- Trong `Inbound Rules` ? Tìm `ChatAppServer`
- ??m b?o: Enabled = Yes, Action = Allow, All Profiles ???c tick

---

### 3?? CÁCH DÒNG L?NH: Netsh (Nhanh nh?t)

**Ch?y Command Prompt v?i quy?n Admin:**
```cmd
netsh advfirewall firewall delete rule name="ChatAppServer"
netsh advfirewall firewall delete rule name="ChatAppServer (Out)"

netsh advfirewall firewall add rule name="ChatAppServer" dir=in action=allow protocol=TCP localport=9000 profile=domain,private,public enable=yes

netsh advfirewall firewall add rule name="ChatAppServer (Out)" dir=out action=allow protocol=TCP localport=9000 profile=domain,private,public enable=yes

netsh advfirewall firewall show rule name="ChatAppServer" dir=in
```

---

## Ki?m tra xem Firewall ?ã m? ch?a?

**Method 1: Trong App Server**
- Nút s? hi?n th?: `? Firewall OK` (màu xanh) ?
- N?u v?n là `?? M? Firewall` (màu cam) ?

**Method 2: Windows Defender Firewall**
- `Inbound Rules` ? Tìm `ChatAppServer` ? Enabled = Yes ?
- `Outbound Rules` ? Tìm `ChatAppServer (Out)` ? Enabled = Yes ?

**Method 3: Netsh**
```cmd
netsh advfirewall firewall show rule name="ChatAppServer"
```
K?t qu? ph?i ch?a `Enabled: Yes` và các profiles `domain`, `private`, `public`

---

## Ki?m tra k?t n?i t? máy khác

### T? máy Client:
1. M? form Login
2. Nh?p IP Server (WiFi IP, NOT 127.0.0.1)
3. Nh?p username/password ? **Login**

N?u v?n l?i, ki?m tra ti?p:

### Test k?t n?i t? máy Client:
```cmd
ping <server_ip>           # Ki?m tra 2 máy cùng m?ng
telnet <server_ip> 9000    # Ki?m tra port 9000 có m? không
```

**K?t qu? mong ??i:**
- `ping`: Reply nh?n ???c ? 2 máy cùng m?ng ?
- `telnet`: K?t n?i thành công ? Port 9000 m? ?

---

## N?u v?n không k?t n?i ???c

### Checklist:
- ? Server ?ã Start? (Status: Running ?)
- ? Firewall ?ã m?? (? Firewall OK)
- ? Client nh?p ?úng IP Server?
- ? 2 máy cùng WiFi/LAN?
- ? Ping ???c nhau?

### N?u ping không ???c:
**Nguyên nhân:** 2 máy không cùng subnet ho?c m?ng WiFi ch?n (AP Isolation)

**Gi?i pháp:**
1. Ki?m tra cùng WiFi (SSID gi?ng)
2. Ch?y `ipconfig` trên c? 2 máy ? IP ph?i cùng subnet:
   - ? 192.168.1.10 + 192.168.1.20 = Cùng subnet
   - ? 192.168.1.10 + 192.168.2.20 = Khác subnet
3. N?u WiFi c? ch?n ? Dùng Mobile Hotspot (4G) ?? test

### N?u ping ???c nh?ng telnet không:
**Nguyên nhân:** Port 9000 không m? ho?c Firewall rule ch?a áp d?ng

**Gi?i pháp:**
1. Nh?n l?i nút `?? M? Firewall` (Admin)
2. Ch? 2-3 giây
3. Th? test l?i

### Antivirus ch?n:
T?m t?t Antivirus (Kaspersky, McAfee, Norton...) ?? test

---

## Tóm t?t các thay ??i

? **FirewallHelper.cs** - ?ã s?a ?? t?o rules cho t?t c? profiles:
- Thay `profile=any` ? `profile=domain,private,public`
- C?i thi?n error messages ?? d? debug

? **Log messages** - Hi?n th? rõ ràng khi firewall rule ???c apply

---

## Liên h? h? tr?

N?u v?n g?p v?n ??:
1. Ghi l?i log t? form Server (các dòng ch?a `[Firewall]`)
2. Ch?y `netsh advfirewall firewall show rule name="ChatAppServer"`
3. Cung c?p thông tin: IP Server, IP Client, Windows Version, Antivirus
