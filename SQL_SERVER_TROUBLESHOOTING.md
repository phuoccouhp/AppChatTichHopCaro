SQL SERVER TIMEOUT FIX GUIDE
=============================

## Problem
[L?I: [Database] Không th? k?t n?i SQL Server. Ch? ?? Offline.]
Connection Timeout Expired during post-login phase (14025ms)

## Root Causes & Solutions

### 1. SQL Server NOT RUNNING (MOST COMMON)
Check if SQL Server service is started:
- Windows: Press Win+R ? services.msc ? Search for "SQL Server (SQLEXPRESS)"
- Should see status = "Started" with a green arrow
- If stopped, right-click ? Start

Alternative: Open SQL Server Configuration Manager:
- SQL Server Services ? SQL Server (SQLEXPRESS) ? Status should be "Running"

### 2. DATABASE DOESN'T EXIST
Check if database "ChatAppDB" exists:

Option A: Use SQL Server Management Studio (SSMS)
- Connect to: localhost\SQLEXPRESS
- View Object Explorer ? Databases
- Look for "ChatAppDB"
- If missing, you need to run the database creation script

Option B: Use Command Prompt (sqlcmd)
```
sqlcmd -S localhost\SQLEXPRESS -E
> SELECT name FROM sys.databases WHERE name='ChatAppDB'
> GO
> EXIT
```

### 3. WRONG INSTANCE NAME
Check which SQL Server instance you have:

```
sqlcmd -L
```

This lists all available instances. Common ones:
- localhost\SQLEXPRESS (Default Express)
- localhost (Default Standard)
- (localdb)\MSSQLLocalDB (LocalDB)

If your instance is different, update appsettings.json:
```json
"ChatAppDB": "Data Source=YOUR_INSTANCE_NAME;Initial Catalog=ChatAppDB;Integrated Security=True;Connection Timeout=30;"
```

### 4. AUTHENTICATION FAILURE
The app uses "Integrated Security=True" (Windows Auth). This requires:
- User account running the app must have permission to access SQL Server
- SQL Server must allow Windows Authentication

Check SQL Server allows Windows Auth:
1. Open SSMS
2. Connect to localhost\SQLEXPRESS
3. Right-click Server ? Properties ? Security
4. Check "Server Authentication" includes "Windows Authentication Mode"

### 5. NETWORK/FIREWALL ISSUE
SQL Server Express uses Named Pipes by default.

Test connectivity:
```
ping localhost
```

Enable Named Pipes in SQL Server Configuration Manager:
1. Open: SQL Server Configuration Manager
2. Navigate: SQL Server Network Configuration ? Protocols for SQLEXPRESS
3. Enable: "Named Pipes" (should show Status = Enabled)
4. Disable: "Shared Memory" (if causing issues)
5. Restart SQL Server

### 6. CONNECTION STRING ISSUES
The connection string should be:
```
Data Source=localhost\SQLEXPRESS;
Initial Catalog=ChatAppDB;
Integrated Security=True;
Connection Timeout=30;
Encrypt=false;
```

Key parameters:
- `Connection Timeout=30` - Wait 30 seconds (was defaulting to 15s, causing timeout)
- `Encrypt=false` - Disable encryption (SQL Express doesn't support by default)
- `Integrated Security=True` - Use Windows authentication

### 7. WORKING MODE - OFFLINE FALLBACK
App automatically detects SQL Server is unavailable and runs in **Offline Mode**:
- ? Users can still login and chat
- ? Messages won't be saved to database
- ? Data will be lost when server restarts
- ? Persistent storage not available

To re-enable database:
1. Fix SQL Server (see steps above)
2. Stop ChatAppServer
3. Start ChatAppServer (it will auto-detect DB is now available)

## Step-by-Step Troubleshooting

1. **First: Verify SQL Server is running**
   ```
   services.msc ? Find "SQL Server (SQLEXPRESS)" ? Status = Started
   ```

2. **Second: Test connection with SSMS**
   - Open SQL Server Management Studio
   - Connect to: localhost\SQLEXPRESS
   - If this works, your server is fine

3. **Third: Check database exists**
   ```
   sqlcmd -S localhost\SQLEXPRESS -E -Q "SELECT name FROM sys.databases WHERE name='ChatAppDB'"
   ```

4. **Fourth: If step 3 shows no results**
   - You need to create the database
   - Run your database creation script (likely in project root)
   - Or create manually:
   ```
   sqlcmd -S localhost\SQLEXPRESS -E -Q "CREATE DATABASE ChatAppDB"
   ```

5. **Fifth: Restart application**
   - Close ChatAppServer
   - Close ChatAppClient
   - Restart ChatAppServer
   - It should now show "[Database] K?t n?i SQL Server thành công!"

## Updated Connection Strings

### For SQL Express (Recommended for Local Dev)
```json
"ChatAppDB": "Data Source=localhost\\SQLEXPRESS;Initial Catalog=ChatAppDB;Integrated Security=True;Connection Timeout=30;Encrypt=false;"
```

### For Default SQL Server Instance
```json
"ChatAppDB": "Data Source=localhost;Initial Catalog=ChatAppDB;Integrated Security=True;Connection Timeout=30;Encrypt=false;"
```

### For SQL Authentication (with password)
```json
"ChatAppDB": "Data Source=localhost\\SQLEXPRESS;Initial Catalog=ChatAppDB;User Id=sa;Password=YourPassword;Connection Timeout=30;Encrypt=false;"
```

### For LocalDB
```json
"ChatAppDB": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ChatAppDB;Integrated Security=True;Connection Timeout=30;Encrypt=false;"
```

## Changes Made

? Added explicit `Connection Timeout=30` to appsettings.json
? Added `Encrypt=false` for SQL Express compatibility
? Improved error messages in DatabaseManager.cs with specific diagnostics
? Updated appsettings.example.json with timeout examples

These changes will:
1. Give SQL Server 30 seconds to respond (instead of default 15s)
2. Disable encryption (SQL Express doesn't support it by default)
3. Provide detailed error messages to identify the exact issue
4. Show helpful steps in the log output
