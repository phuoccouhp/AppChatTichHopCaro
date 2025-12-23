# Verification Guide for Network Connection Fixes

## Quick Test Steps

### 1. **Server-Side Test**

```bash
# Start the ChatApp Server
- Run ChatAppServer.exe or debug in Visual Studio
- Verify firewall prompt appears and port 9000 is opened
- Server should show:
  ? Server kh?i ??ng t?i Port: 9000
  ? THÔNG TIN K?T N?I displayed with IP addresses
```

### 2. **Single Client Login**

```
1. Start ChatAppClient on same machine
2. Enter credentials: username "user1", password "pass123"
3. Expected:
   ? "K?t n?i THÀNH CÔNG ??n 127.0.0.1:9000!"
   ? Login succeeds
   ? Home page loads
   ? Contact list is visible (if user1 has previous conversations)
4. Server log should show:
   ? "[Login] user1 (127.0.0.1) ?ã ??ng nh?p."
```

### 3. **Multiple Clients Test** (THE KEY FIX)

```
1. Start Server
2. Start Client #1 on Machine A (or same machine in different instance)
   - Login as "user1"
   - Verify successful connection
3. Start Client #2 on Machine B (or another instance)
   - Login as "user2"
   - Verify successful connection
4. Check Client #1's contact list:
   ? Should show "user2" in the contact list
   ? Should show "user2" as ONLINE (green status)
5. Check Client #2's contact list:
   ? Should show "user1" in the contact list
   ? Should show "user1" as ONLINE (green status)
6. Server log should show:
   ? "[Login] user1 (IP) ?ã ??ng nh?p."
   ? "[Login] user2 (IP) ?ã ??ng nh?p."
   ? Both users in online status broadcasts
```

### 4. **Connection Stability Test**

```
1. Two clients logged in and visible to each other
2. Send several messages between them (10+ messages)
3. Expected:
   ? All messages deliver successfully
   ? No "G?i th?t b?i" errors in logs
   ? No "Unable to write data to transport" errors
4. Send a file:
   ? File transfers without interruption
   ? Both sides receive notification
```

### 5. **Network Interruption Test**

```
1. Two clients logged in
2. Unplug network cable from one client (or disable WiFi)
3. Wait 30+ seconds
4. Expected:
   ? Server detects disconnection within 30 seconds
   ? Server logs: "[Disconnect] User 'user1' ?ã ng?t k?t n?i."
   ? Other client's contact list updates (user1 shows OFFLINE)
   ? No connection hangs or timeout issues
5. Plug network back in
6. Expected:
   ? Client automatically reconnects
   ? Status updates on other clients
```

### 6. **Check Logs for Error Messages**

**BAD SIGNS (These should NOT appear):**
```
? "G?i th?t b?i cho Unknown"
? "Unable to write data to the transport connection"
? "An established connection was aborted"
? "Không th? g?i LoginResult"
? Repeated connection timeouts
```

**GOOD SIGNS (These ARE expected):**
```
? "[Login] user1 (127.0.0.1) ?ã ??ng nh?p."
? "[Login] user2 (192.168.x.x) ?ã ??ng nh?p."
? Successful message sends
? "[Disconnect] User 'user1' ?ã ng?t k?t n?i." (when expected)
? "Stream không kh? d?ng, ng?t k?t n?i" (when client closes)
```

## Expected Behavior Changes

### Before Fix:
- Multiple clients couldn't see each other in contact list
- Random "connection aborted" errors when sending data
- Contact list remained empty even with active users
- Unclear if a user was actually online or not

### After Fix:
- All clients see each other in contact list immediately after login
- Online status correctly reflects who is available
- Stable connections with proper error handling
- Network issues are detected quickly (within 30 seconds)
- Graceful disconnection handling without hanging connections

## Troubleshooting

### Issue: "Contact list still empty"
```
Check:
1. Do users have previous conversation history in database?
   - GetContacts() only returns contacts from message history
   - First message between users creates the contact relationship
   
Solution: Send at least one message between users first
```

### Issue: "Still seeing 'G?i th?t b?i' errors"
```
Check:
1. Is the network stable?
2. Is the server still running?
3. Check Windows Firewall settings for port 9000
4. Try restarting both server and client

Log Analysis:
- Look for the specific error message type
- Check if it's during login, messaging, or file transfer
- Correlate with timestamp on other side
```

### Issue: "User shows online but shouldn't be"
```
Check:
1. Is the connection actually closed on both sides?
2. Is KeepAlive detecting the disconnection?
   - Default timeout is 30 seconds
   - Wait up to 30 seconds and check again

If still seeing ghost connections:
- Restart server to clear old connections
- Check database for stale sessions (not implemented yet)
```

## Performance Metrics to Monitor

After applying fixes, you should see:
- **Connection establishment time**: < 2 seconds
- **Message delivery time**: < 100ms local, < 500ms WiFi
- **Disconnection detection time**: < 35 seconds
- **CPU usage per client**: < 2% idle, < 5% active chatting
- **Memory per client**: < 50MB

## Configuration Values

If you need to adjust connection parameters:

**File: ChatAppClient/NetworkManager.cs**
```csharp
_client.ReceiveTimeout = 30000;  // milliseconds to wait for data
_client.SendTimeout = 30000;     // milliseconds to wait for send
```

**File: ChatAppServer/Server.cs**
```csharp
// Socket options (in StartAsync method)
_listener.Server.SetSocketOption(SocketOptionLevel.Tcp, 
    SocketOptionName.TcpKeepAliveTime, 30000); // 30 second keepalive probe
```

Change these if you have special network conditions (slow WiFi, high latency links, etc.)

## Common Questions

**Q: Will this fix work on WiFi?**
A: Yes. The fixes work better on WiFi because they detect disconnections faster.

**Q: Will this fix work across the internet (not just LAN)?**
A: Yes, but you'll need proper port forwarding and firewall rules set up.

**Q: Do I need to change database configuration?**
A: No. These fixes are purely network layer improvements.

**Q: Will existing connections be preserved after update?**
A: No. Users will need to reconnect after the server is restarted.

**Q: How do I monitor connection health?**
A: Check the Server console logs. Look for login/disconnect messages.
   You can add more detailed logging in Logger.cs if needed.
