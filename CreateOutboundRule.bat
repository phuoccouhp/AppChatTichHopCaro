@echo off
REM ============================================================================
REM Batch Script: Create Outbound Firewall Rule (FORCEFULLY)
REM M?c ?ích: T?o Outbound rule v?i t?t c? tham s? c?n thi?t
REM ============================================================================

setlocal enabledelayedexpansion

REM Ki?m tra Admin privileges
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo.
    echo =========================================
    echo    REQUIRES ADMINISTRATOR PRIVILEGES!
    echo =========================================
    echo.
    echo Please run as administrator:
    echo Right-click on this file -^> "Run as administrator"
    echo.
    pause
    exit /b 1
)

set PORT=9000
set RULE_NAME=ChatAppServer
set RULE_NAME_OUT=ChatAppServer (Out)

echo.
echo =========================================
echo    CREATE OUTBOUND FIREWALL RULE
echo =========================================
echo.

REM Step 1: Delete old rules
echo [STEP 1] Deleting old outbound rule if exists...
netsh advfirewall firewall delete rule name="%RULE_NAME_OUT%" dir=out >nul 2>&1
if %errorLevel% equ 0 (
    echo    ? Old rule deleted
) else (
    echo    - No old rule found
)
echo.

REM Step 2: Create outbound rule
echo [STEP 2] Creating NEW outbound rule...
echo    Rule Name: %RULE_NAME_OUT%
echo    Port: %PORT%
echo    Direction: OUT (Outbound)
echo    Action: ALLOW
echo    Protocol: TCP
echo    Remote IP: ANY
echo    Profiles: Domain, Private, Public
echo.

netsh advfirewall firewall add rule ^
    name="%RULE_NAME_OUT%" ^
    dir=out ^
    action=allow ^
    protocol=TCP ^
    localport=%PORT% ^
    remoteip=any ^
    profile=domain,private,public ^
    enable=yes

if %errorLevel% equ 0 (
    echo.
    echo    ? Rule created successfully!
    echo.
) else (
    echo.
    echo    ? ERROR: Failed to create rule (Error code: %errorLevel%)
    echo.
    pause
    exit /b 1
)

REM Step 3: Verify
echo [STEP 3] Verifying rule creation...
echo.
netsh advfirewall firewall show rule name="%RULE_NAME_OUT%" dir=out
echo.

REM Step 4: Check both rules
echo [STEP 4] Checking BOTH Inbound and Outbound rules...
echo.
echo --- INBOUND RULE ---
netsh advfirewall firewall show rule name="%RULE_NAME%" dir=in | findstr /C:"Rule Name" /C:"Enabled" /C:"Action"
echo.
echo --- OUTBOUND RULE ---
netsh advfirewall firewall show rule name="%RULE_NAME_OUT%" dir=out | findstr /C:"Rule Name" /C:"Enabled" /C:"Action"
echo.

REM Step 5: Success message
echo =========================================
echo    ? COMPLETE
echo =========================================
echo.
echo Outbound rule for port %PORT% has been created!
echo.
echo Now you can:
echo 1. Run your ChatApp Server
echo 2. Test connection from another computer on the same network
echo.
pause
exit /b 0
