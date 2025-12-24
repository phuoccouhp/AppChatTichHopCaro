@echo off
REM ============================================================================
REM ?????? PowerShell ????????? netsh ????
REM ============================================================================

setlocal enabledelayedexpansion

REM ?? Admin ??
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo.
    echo =========================================
    echo    ????????
    echo =========================================
    echo.
    echo ?????????????
    echo ???? -^> "????????"
    echo.
    pause
    exit /b 1
)

echo.
echo =========================================
echo    ?? ChatApp ?????
echo =========================================
echo.
echo ?? PowerShell ????...
echo.

REM ?? PowerShell ??????????? netsh ??????
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
"^
try { ^
  Remove-NetFirewallRule -DisplayName 'ChatAppServer' -Direction Inbound -ErrorAction SilentlyContinue | Out-Null; ^
  Remove-NetFirewallRule -DisplayName 'ChatAppServer (Out)' -Direction Outbound -ErrorAction SilentlyContinue | Out-Null; ^
  New-NetFirewallRule -DisplayName 'ChatAppServer' -Direction Inbound -Action Allow -Protocol TCP -LocalPort 9000 -Profile Domain,Private,Public -Enabled $true | Out-Null; ^
  New-NetFirewallRule -DisplayName 'ChatAppServer (Out)' -Direction Outbound -Action Allow -Protocol TCP -LocalPort 9000 -RemoteAddress Any -Profile Domain,Private,Public -Enabled $true | Out-Null; ^
  Write-Host 'Firewall rules created successfully!' -ForegroundColor Green; ^
  Write-Host ''; ^
  Write-Host 'Rules Status:' -ForegroundColor Cyan; ^
  Get-NetFirewallRule -DisplayName 'ChatAppServer*' | Format-Table DisplayName, Direction, Action, Enabled -AutoSize ^
} catch { ^
  Write-Host "Error: $_.Exception.Message" -ForegroundColor Red ^
}^
"

if %errorLevel% equ 0 (
    echo.
    echo =========================================
    echo    ? ??
    echo =========================================
    echo.
    echo ?? 9000 ?????
    echo ????????????????????
    echo.
) else (
    echo.
    echo =========================================
    echo    ? ??
    echo =========================================
    echo.
    echo ????
    echo 1. ???????????
    echo 2. Windows PowerShell ???
    echo 3. ?? Windows ?????
    echo.
)

pause


