@echo off
REM ============================================================================
REM CRITICAL FIX: Direct Outbound Rule Creation
REM ============================================================================

setlocal enabledelayedexpansion

echo.
echo ============================================================================
echo    OUTBOUND RULE CREATION - DIRECT METHOD
echo ============================================================================
echo.

REM Check Admin
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: Must run as Administrator!
    echo Right-click CMD/PowerShell ^> Run as administrator
    pause
    exit /b 1
)

echo Running as Administrator... OK
echo.

REM Step 1: Delete old rules
echo [Step 1] Deleting old rules...
powershell -NoProfile -Command "Remove-NetFirewallRule -DisplayName 'ChatAppServer' -ErrorAction SilentlyContinue | Out-Null"
powershell -NoProfile -Command "Remove-NetFirewallRule -DisplayName 'ChatAppServer (Out)' -ErrorAction SilentlyContinue | Out-Null"
echo Done.
echo.

REM Step 2: Create INBOUND
echo [Step 2] Creating INBOUND rule...
powershell -NoProfile -Command "New-NetFirewallRule -DisplayName 'ChatAppServer' -Direction Inbound -Action Allow -Protocol TCP -LocalPort 9000 -Profile Domain,Private,Public -Enabled $true | Out-Null"
if %errorLevel% equ 0 (
    echo ? INBOUND rule created
) else (
    echo ? INBOUND rule FAILED
)
echo.

REM Step 3: Create OUTBOUND - THIS IS THE CRITICAL ONE
echo [Step 3] Creating OUTBOUND rule...
powershell -NoProfile -Command "New-NetFirewallRule -DisplayName 'ChatAppServer (Out)' -Direction Outbound -Action Allow -Protocol TCP -LocalPort 9000 -RemoteAddress Any -Profile Domain,Private,Public -Enabled $true | Out-Null"
if %errorLevel% equ 0 (
    echo ? OUTBOUND rule created
) else (
    echo ? OUTBOUND rule FAILED
)
echo.

REM Step 4: Verify
echo [Step 4] Verifying rules exist...
echo.
powershell -NoProfile -Command "Get-NetFirewallRule -DisplayName 'ChatAppServer*' | Select DisplayName, Direction, Action, Enabled | Format-Table -AutoSize"
echo.

echo ============================================================================
echo DONE - Rules should now be created!
echo ============================================================================
echo.
echo Next steps:
echo 1. Run Server: dotnet run --project ChatAppServer
echo 2. Run Client: dotnet run --project ChatAppClient
echo 3. Login and test
echo.
pause
