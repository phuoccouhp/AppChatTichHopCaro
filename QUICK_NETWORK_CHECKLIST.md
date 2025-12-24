QUICK START CHECKLIST - K?T N?I CROSS-NETWORK
============================================

## Tr??c khi test k?t n?i t? máy khác:

### 1?? Server Side (Máy Server)
- [ ] Ch?y ChatAppServer
- [ ] Nh?n **"?? M? Firewall"** ? Ch?n Yes ? ??ng ý Admin
- [ ] Ch? xong, nút chuy?n màu xanh `? Firewall OK`
- [ ] Nh?n **"Start Server"** 
- [ ] Ki?m tra: Status = "Status: Running ?" (màu xanh)
- [ ] Ghi nh? **IP WiFi** hi?n th? trên form (NOT 127.0.0.1)

### 2?? Client Side (Máy khác)
- [ ] Ch?y ChatAppClient
- [ ] Ghi IP Server vào ô `Server IP`
- [ ] Ghi username/password
- [ ] Nh?n **"Log in"**
- [ ] Ki?m tra log: `[SEND RAW]` có hi?n th? ? gói login ???c g?i ?
- [ ] Ch? response t? server

### 3?? Server Side - Verify
M? Server Log, tìm các dòng:
- [ ] `[CONNECT] New client connected: <IP:port>` ?
- [ ] `[RECV RAW]` (server nh?n ???c gói) ?
- [ ] `[LOGIN] User '...' (...) ?ã ??ng nh?p` ?

---

## N?u LOGIN TH?T B?I:

### T? Client Log:
| L?i | Nguyên nhân | Gi?i pháp |
|-----|-----------|----------|
| `Không th? k?t n?i ??n 192.168.x.x` | IP sai ho?c firewall ch?n | Ki?m tra IP Server, m? firewall |
| `[SEND ERROR] Failed to send packet` | K?t n?i thoát | Ki?m tra network, k?t n?i l?i |
| `Server không ph?n h?i yêu c?u ??ng nh?p` (Timeout) | Server không nh?n ???c gói | Ki?m tra server ?ang ch?y, firewall m? |

### T? Server Log:
| Dòng Log | Ngh?a | Gi?i pháp |
|---------|-------|----------|
| `[CONNECT] New client connected` | Client k?t n?i thành công | ? T?t |
| Không có `[RECV RAW]` | Server không nh?n gói | Client firewall ch?n, ho?c k?t n?i fail |
| `[UNKNOWN PACKET]` | Packet type không ?úng | Có th? do version mismatch |
| `[LOGIN FAIL] ...` | Username/password sai | Ki?m tra l?i credentials |

---

## Firewall Verification Commands (Admin CMD):

```cmd
:: Xem rule ?ã t?o ch?a
netsh advfirewall firewall show rule name="ChatAppServer"

:: N?u ch?a, t?o rules
netsh advfirewall firewall add rule name="ChatAppServer" dir=in action=allow protocol=TCP localport=9000 profile=domain,private,public enable=yes

netsh advfirewall firewall add rule name="ChatAppServer (Out)" dir=out action=allow protocol=TCP localport=9000 profile=domain,private,public enable=yes

:: Test k?t n?i t? client machine
ping <server_ip>
telnet <server_ip> 9000
```

---

## Logs c?n ghi l?i (N?u c?n support):

1. **Server Log** (Form Server):
   - Các dòng `[CONNECT]`, `[RECV RAW]`, `[LOGIN]`
   - Các dòng `[Firewall]`

2. **Client Log** (Ch?y t? Console):
   - `[SEND RAW]` (gói g?i ?i)
   - `[SEND ERROR]` (l?i g?i)

3. **Firewall Status**:
   ```cmd
   netsh advfirewall firewall show rule name="ChatAppServer"
   ```

4. **Network Info**:
   ```cmd
   ipconfig                    :: Xem IP c?a máy
   netstat -ano | findstr 9000 :: Xem port 9000 có listening không
   ping <server_ip>            :: Test ping
   ```

---

## Các câu h?i ?? t? debug:

1. ? 2 máy cùng m?ng WiFi/LAN không?
   - `ipconfig` ? Ki?m tra IP có cùng subnet không

2. ? Ping ???c nhau không?
   ```cmd
   ping <server_ip>
   ```
   - ???c = M?ng OK
   - Không ???c = M?ng không k?t n?i

3. ? Port 9000 listening không?
   ```cmd
   netstat -ano | findstr :9000
   ```
   - LISTENING = ?ang l?ng nghe
   - Không có = Server ch?a start ho?c port sai

4. ? Firewall rule có ???c enable không?
   ```cmd
   netsh advfirewall firewall show rule name="ChatAppServer"
   ```
   - Enabled: Yes = Rule OK
   - Không hi?n = Rule ch?a t?o

5. ? Antivirus có ch?n không?
   - T?m t?t Antivirus r?i th? l?i

---

## Tài li?u chi ti?t:
- `CROSS_NETWORK_CONNECTION_GUIDE.md` - H??ng d?n ??y ??
- `SQL_SERVER_TROUBLESHOOTING.md` - Database connection issues
