@echo off
REM Quick PowerShell Firewall Rule Test
REM ?? PowerShell ????????????

setlocal enabledelayedexpansion

echo.
echo ============================================================================
echo    PowerShell Firewall Rule Creation Test
echo ============================================================================
echo.

REM Check admin
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: Requires admin privileges!
    echo Please run as administrator.
    pause
    exit /b 1
)

echo Testing PowerShell firewall rule creation...
echo.

REM Test 1: Simple Inbound Rule
echo [TEST 1] Creating simple Inbound rule without parentheses...
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
"^
try { ^
  New-NetFirewallRule -DisplayName 'TestInbound' -Direction Inbound -Action Allow -Protocol TCP -LocalPort 9999 -Enabled $true -ErrorAction Stop | Out-Null; ^
  Write-Host 'OK: Inbound rule created' -ForegroundColor Green; ^
  Remove-NetFirewallRule -DisplayName 'TestInbound' -ErrorAction SilentlyContinue | Out-Null ^
} catch { ^
  Write-Host \"ERROR: $_.Exception.Message\" -ForegroundColor Red ^
}^
"
echo.

REM Test 2: Outbound Rule WITHOUT parentheses
echo [TEST 2] Creating simple Outbound rule without parentheses...
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
"^
try { ^
  New-NetFirewallRule -DisplayName 'TestOutbound' -Direction Outbound -Action Allow -Protocol TCP -LocalPort 9999 -RemoteAddress Any -Enabled $true -ErrorAction Stop | Out-Null; ^
  Write-Host 'OK: Outbound rule created' -ForegroundColor Green; ^
  Remove-NetFirewallRule -DisplayName 'TestOutbound' -ErrorAction SilentlyContinue | Out-Null ^
} catch { ^
  Write-Host \"ERROR: $_.Exception.Message\" -ForegroundColor Red ^
}^
"
echo.

REM Test 3: Rule WITH parentheses (the critical test)
echo [TEST 3] Creating rule WITH parentheses in the name...
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
"^
try { ^
  New-NetFirewallRule -DisplayName 'Test (Out)' -Direction Outbound -Action Allow -Protocol TCP -LocalPort 9999 -RemoteAddress Any -Enabled $true -ErrorAction Stop | Out-Null; ^
  Write-Host 'OK: Rule with parentheses created' -ForegroundColor Green; ^
  Remove-NetFirewallRule -DisplayName 'Test (Out)' -ErrorAction SilentlyContinue | Out-Null ^
} catch { ^
  Write-Host \"ERROR: $_.Exception.Message\" -ForegroundColor Red ^
}^
"
echo.

REM Test 4: Verify existing ChatAppServer rules
echo [TEST 4] Checking existing ChatAppServer rules...
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
"^
Get-NetFirewallRule -DisplayName 'ChatAppServer*' -ErrorAction SilentlyContinue | Format-Table DisplayName, Direction, Action, Enabled -AutoSize ^
"
echo.

echo ============================================================================
echo Press any key to exit...
pause >nul
