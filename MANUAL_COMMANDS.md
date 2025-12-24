# ?? MANUAL COMMANDS - If Scripts Don't Work

## Open PowerShell as Administrator

```
1. Press Windows Key
2. Type: powershell
3. Right-click "Windows PowerShell"
4. Click "Run as Administrator"
5. Click "Yes" to confirm
```

---

## Command 1: Delete Old Rules

```powershell
Remove-NetFirewallRule -DisplayName "ChatAppServer" -Direction Inbound -ErrorAction SilentlyContinue
Remove-NetFirewallRule -DisplayName "ChatAppServer (Out)" -Direction Outbound -ErrorAction SilentlyContinue
```

Expected: No output (that's normal)

---

## Command 2: Create INBOUND Rule

```powershell
New-NetFirewallRule -DisplayName "ChatAppServer" -Direction Inbound -Action Allow -Protocol TCP -LocalPort 9000 -Profile Domain,Private,Public -Enabled $true
```

Expected output:
```
Name                  : {UUID}
DisplayName           : ChatAppServer
Description           :
DisplayGroup          :
Group                 :
Enabled               : True
Profile               : Domain, Private, Public
Platform              : {}
Direction             : Inbound
Action                : Allow
...
```

---

## Command 3: Create OUTBOUND Rule (THE CRITICAL ONE)

```powershell
New-NetFirewallRule -DisplayName "ChatAppServer (Out)" -Direction Outbound -Action Allow -Protocol TCP -LocalPort 9000 -RemoteAddress Any -Profile Domain,Private,Public -Enabled $true
```

Expected: Same kind of output as Command 2

---

## Command 4: Verify Both Rules Exist

```powershell
Get-NetFirewallRule -DisplayName "ChatAppServer*" | Select DisplayName, Direction, Action, Enabled | Format-Table
```

Expected output:
```
DisplayName              Direction  Action  Enabled
???????????????????????? ?????????? ?????? ???????
ChatAppServer            Inbound    Allow  True
ChatAppServer (Out)      Outbound   Allow  True
```

If you see BOTH rules above with:
- ? DisplayName correct
- ? Direction correct (Inbound and Outbound)
- ? Action = Allow
- ? Enabled = True

**? YOU'RE DONE! Rules are created!**

---

## Command 5: Verify Outbound Remote Address

```powershell
Get-NetFirewallRule -DisplayName "ChatAppServer (Out)" -Direction Outbound | Get-NetFirewallAddressFilter | Select RemoteAddress
```

Expected output:
```
RemoteAddress
?????????????
Any
```

If you see "Any" ? ? Correct!

---

## If Any Command Fails

### If "Access Denied" error

Make sure PowerShell is running as Administrator:
- Right-click PowerShell
- "Run as Administrator"
- Click "Yes"

### If rule deletion says "No matching rules found"

That's fine - just means there were no old rules to delete.

### If rule creation succeeds but says "already exists"

That's fine too - means the rule was already there. Delete it first:

```powershell
Remove-NetFirewallRule -DisplayName "ChatAppServer (Out)" -Force
```

Then try creating again.

---

## After Creating Rules

### Start Server

```bash
cd ChatAppServer
dotnet run
```

Look for:
```
? Firewall port 9000 successfully opened and verified!
```

### In New Terminal: Start Client

```bash
cd ChatAppClient
dotnet run
```

### Login

- Server: 127.0.0.1
- Port: 9000
- User: admin
- Pass: 123456

Should work without timeout!

---

## Troubleshooting

### Client still times out

Run verification command again:
```powershell
Get-NetFirewallRule -DisplayName "ChatAppServer*" | Select DisplayName, Direction, Action, Enabled | Format-Table
```

If "ChatAppServer (Out)" is missing:
- Run the outbound rule creation command again
- Make sure it says it created successfully

### Rules show Enabled = False

Enable them:
```powershell
Enable-NetFirewallRule -DisplayName "ChatAppServer"
Enable-NetFirewallRule -DisplayName "ChatAppServer (Out)"
```

### RemoteAddress is not "Any"

Delete and recreate:
```powershell
Remove-NetFirewallRule -DisplayName "ChatAppServer (Out)" -Force
New-NetFirewallRule -DisplayName "ChatAppServer (Out)" -Direction Outbound -Action Allow -Protocol TCP -LocalPort 9000 -RemoteAddress Any -Profile Domain,Private,Public -Enabled $true
```

---

## Copy-Paste All Commands at Once

```powershell
# Paste this entire block into PowerShell (as Admin)

Remove-NetFirewallRule -DisplayName "ChatAppServer" -Direction Inbound -ErrorAction SilentlyContinue
Remove-NetFirewallRule -DisplayName "ChatAppServer (Out)" -Direction Outbound -ErrorAction SilentlyContinue

New-NetFirewallRule -DisplayName "ChatAppServer" -Direction Inbound -Action Allow -Protocol TCP -LocalPort 9000 -Profile Domain,Private,Public -Enabled $true

New-NetFirewallRule -DisplayName "ChatAppServer (Out)" -Direction Outbound -Action Allow -Protocol TCP -LocalPort 9000 -RemoteAddress Any -Profile Domain,Private,Public -Enabled $true

Get-NetFirewallRule -DisplayName "ChatAppServer*" | Select DisplayName, Direction, Action, Enabled | Format-Table
```

This will:
1. Delete old rules
2. Create inbound
3. Create outbound
4. Show both rules in a table

---

## Success Indicators

After running all commands, you should see:
- ? Both rules listed
- ? Both with Enabled = True
- ? DisplayName, Direction, Action all correct
- ? Server shows firewall verified message
- ? Client can login without timeout

**If you see all of these ? DONE!** ??
