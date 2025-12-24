@echo off
REM ============================================================================
REM One-Click Fix: Auto-create Firewall Rules & Run Server
REM ============================================================================

setlocal enabledelayedexpansion

echo.
echo ============================================================================
echo    ChatApp Server - Auto-Setup & Launch
echo ============================================================================
echo.

REM Check if already admin
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo Requesting Administrator privileges...
    REM Create VBS to elevate
    set "tempvbs=%temp%\elevate.vbs"
    echo CreateObject("Shell.Application").ShellExecute "%~s0", "", "", "runas", 1 > "!tempvbs!"
    cscript.exe "!tempvbs!" //nologo
    del "!tempvbs!" 2>nul
    exit /b
)

echo ? Running with Administrator privileges
echo.

REM Step 1: Create Firewall Rules using PowerShell
echo [Step 1] Creating Firewall Rules...
echo.

powershell -NoProfile -ExecutionPolicy Bypass -Command ^
"^
Write-Host 'Creating firewall rules for port 9000...' -ForegroundColor Cyan; ^
Remove-NetFirewallRule -DisplayName 'ChatAppServer' -ErrorAction SilentlyContinue | Out-Null; ^
Remove-NetFirewallRule -DisplayName 'ChatAppServer (Out)' -ErrorAction SilentlyContinue | Out-Null; ^
New-NetFirewallRule -DisplayName 'ChatAppServer' -Direction Inbound -Action Allow -Protocol TCP -LocalPort 9000 -Profile Domain,Private,Public -Enabled `$true | Out-Null; ^
New-NetFirewallRule -DisplayName 'ChatAppServer (Out)' -Direction Outbound -Action Allow -Protocol TCP -LocalPort 9000 -RemoteAddress Any -Profile Domain,Private,Public -Enabled `$true | Out-Null; ^
`$inbound = Get-NetFirewallRule -DisplayName 'ChatAppServer' -ErrorAction SilentlyContinue; ^
`$outbound = Get-NetFirewallRule -DisplayName 'ChatAppServer (Out)' -ErrorAction SilentlyContinue; ^
if (`$inbound) {{ Write-Host '  ? Inbound rule created' -ForegroundColor Green }} else {{ Write-Host '  ? Inbound rule failed' -ForegroundColor Red }}; ^
if (`$outbound) {{ Write-Host '  ? Outbound rule created' -ForegroundColor Green }} else {{ Write-Host '  ? Outbound rule failed' -ForegroundColor Red }}; ^
Write-Host '' ^
"

echo.
echo [Step 2] Waiting for Firewall to sync... (2 seconds)
timeout /t 2 /nobreak

echo.
echo [Step 3] Starting ChatApp Server...
echo.
echo ============================================================================
echo    Server will start below. Press Ctrl+C to stop.
echo ============================================================================
echo.

cd /d "%~dp0ChatAppServer"
dotnet run --configuration Release

echo.
echo Server stopped. Press any key to exit...
pause >nul
