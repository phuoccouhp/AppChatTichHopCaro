# ?? TODO-LIST S?A CÁC L?I - PRIORITY

## ?? V?N ?? & NGUYÊN NHÂN

### 1. **Fallback Danh Sách Ng??i Dùng**
- **V?n ??**: Khi DB không s?n sàng, client 1 không th?y danh sách contacts
- **Nguyên nhân**: `GetContacts()` fallback v? cache nh?ng không include ?? users, ho?c fallback ch?a include t?t c?
- **S?a**: ??m b?o fallback tr? v? ALL users t? cache (không ch? contacts t? cache)

---

### 2. **Asymmetric Visibility (Client 2 th?y Client 1, Client 1 không th?y Client 2)**
- **V?n ??**: Danh sách contacts không ??i x?ng
- **Nguyên nhân**: 
  - `GetContacts()` ch? l?y t? b?ng Messages (ng??i t?ng nh?n tin)
  - N?u Client1?Client2 ch?a t?ng nh?n tin, Client2 không có contact Client1
  - Khi Client2 login, Server ch? notify contacts c?a Client2 (không include Client1 n?u ch?a chat)
- **S?a**: 
  - Tr? v? ALL users (t? cache/DB) thay vì ch? contacts
  - Ho?c thêm default "contacts" khi user ??u tiên login (t?t c? users online)

---

### 3. **Ch?a Có Thông Báo ?? + Status Không ??i T? Offline?Online**
- **V?n ??**: 
  - Tin nh?n g?i ??n không trigger alert/notification ??
  - Friend list không c?p nh?t tr?ng thái online t? offline
- **Nguyên nhân**:
  - Client không handle `UserStatusPacket` ? c?p nh?t UI ?? hi?n th? online
  - Client không trigger notification alert khi có message
  - Server broadcast `UserStatusPacket` nh?ng ch? g?i t?i contacts (n?u c? ch? contacts còn l?i)
- **S?a**:
  - ??m b?o `HandleUserStatusUpdate()` ???c g?i và c?p nh?t UI
  - Thêm red notification badge khi có message ??n
  - Server g?i `UserStatusPacket` cho T?T C? online users (không ch? contacts)

---

### 4. **L?i M?i Game ???c Ch?p Nh?n Nh?ng Không Hi?n Game**
- **V?n ??**: `GameStartPacket` không ???c g?i ho?c client không handle
- **Nguyên nhân**:
  - Server `ProcessGameResponse()` không tìm th?y player handler (có th? do c? ch? contacts)
  - Client `HandleGameStart()` không ???c g?i ho?c game form không kh?i ??ng
- **S?a**:
  - ??m b?o server tìm th?y c? 2 player trong `_clients` dictionary
  - Client handle `GameStartPacket` và kh?i ??ng game form ?úng cách
  - Log chi ti?t ?? debug

---

## ? DANH SÁCH ACTION

| # | Task | File | M?c ?? |
|---|------|------|--------|
| 1 | S?a `GetContacts()` tr? v? ALL users thay vì ch? contacts | `DatabaseManager.cs` | ?? CRITICAL |
| 2 | Server g?i `UserStatusPacket` cho T?T C? online users, không ch? contacts | `Server.cs` | ?? CRITICAL |
| 3 | Client handle `UserStatusPacket` ? update UI color online/offline | `frmHome.cs` | ?? CRITICAL |
| 4 | Thêm red notification badge khi có message | `FriendListItem.cs` | ?? HIGH |
| 5 | Server log chi ti?t `ProcessGameResponse()` ?? debug game không start | `Server.cs` | ?? HIGH |
| 6 | Client log chi ti?t `HandleGameStart()` ?? debug | `frmHome.cs` | ?? HIGH |
| 7 | Test l?i t? ??u: A login ? th?y danh sách; B login ? c? A&B th?y nhau | ALL | ?? VERIFY |

---

## ?? IMPLEMENTATION ORDER

1. **S?a DatabaseManager.GetContacts()** ? Tr? v? ALL users
2. **S?a Server.RegisterClient/RemoveClient()** ? Broadcast t?i ALL online users
3. **S?a Server.ProcessGameResponse()** ? Log chi ti?t, ensure both players found
4. **S?a Client HandleUserStatusUpdate()** ? Update UI color
5. **S?a Client HandleGameStart()** ? Log chi ti?t, verify game form opens
6. **Add notification badge** ? Red dot when message arrives
7. **Build + Test** ? T?ng b??c

---

**Estimated Time**: ~2 hours  
**Risk**: Medium (logic changes, need to verify all flows)
