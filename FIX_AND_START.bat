@echo off
REM ============================================================================
REM MASTER FIX - Complete Outbound Rule Creation + Server Start
REM ============================================================================

setlocal enabledelayedexpansion

echo.
echo ============================================================================
echo         CHATAPP SERVER - FIREWALL OUTBOUND FIX + START
echo ============================================================================
echo.

REM Check if running as admin
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo Requesting Administrator privileges...
    echo.
    
    REM Create VBS to elevate
    set "vbs=%temp%\elevate_%random%.vbs"
    (
        echo CreateObject("Shell.Application").ShellExecute "%~s0", "", "", "runas", 1
    ) > "!vbs!"
    
    cscript.exe "!vbs!" //nologo
    del "!vbs!" 2>nul
    exit /b
)

echo ? Running with Administrator privileges
echo.

REM ============================================================================
REM STEP 1: CREATE FIREWALL RULES
REM ============================================================================
echo [STEP 1] Creating Firewall Rules...
echo ============================================================================
echo.

REM Delete old rules
echo [1a] Deleting old rules...
powershell -NoProfile -ExecutionPolicy Bypass -Command "Remove-NetFirewallRule -DisplayName 'ChatAppServer' -ErrorAction SilentlyContinue; Remove-NetFirewallRule -DisplayName 'ChatAppServer (Out)' -ErrorAction SilentlyContinue"
echo Done.
echo.

REM Create Inbound
echo [1b] Creating INBOUND rule...
powershell -NoProfile -ExecutionPolicy Bypass -Command "New-NetFirewallRule -DisplayName 'ChatAppServer' -Direction Inbound -Action Allow -Protocol TCP -LocalPort 9000 -Profile Domain,Private,Public -Enabled $true | Out-Null"
if %errorLevel% equ 0 (
    echo ? INBOUND rule created successfully
) else (
    echo ? INBOUND rule creation failed
)
echo.

REM Create Outbound - CRITICAL
echo [1c] Creating OUTBOUND rule...
powershell -NoProfile -ExecutionPolicy Bypass -Command "New-NetFirewallRule -DisplayName 'ChatAppServer (Out)' -Direction Outbound -Action Allow -Protocol TCP -LocalPort 9000 -RemoteAddress Any -Profile Domain,Private,Public -Enabled $true | Out-Null"
if %errorLevel% equ 0 (
    echo ? OUTBOUND rule created successfully
) else (
    echo ? OUTBOUND rule creation failed
)
echo.

REM Verify
echo [1d] Verifying rules...
powershell -NoProfile -ExecutionPolicy Bypass -Command "Get-NetFirewallRule -DisplayName 'ChatAppServer*' | Select DisplayName, Direction, Action, Enabled | Format-Table -AutoSize"
echo.

echo ============================================================================
REM STEP 2: START SERVER
REM ============================================================================
echo [STEP 2] Starting ChatApp Server...
echo ============================================================================
echo.
echo Server will start below. Press Ctrl+C to stop.
echo.

cd /d "%~dp0ChatAppServer"
dotnet run --configuration Release

echo.
echo Server stopped.
echo.
pause
