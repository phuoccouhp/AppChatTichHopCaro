@echo off
REM ============================================================================
REM Script: Check Firewall Rule Details
REM M?c ?ích: Capture netsh output ?? debug v?n ?? rule detection
REM ============================================================================

setlocal enabledelayedexpansion

set RULE_NAME=ChatAppServer
set PORT=9000

echo.
echo ============================================================================
echo    FIREWALL RULE DETAILS CHECKER
echo ============================================================================
echo.
echo Rule Name: %RULE_NAME%
echo Port: %PORT%
echo.

REM Check if running as admin
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo [ERROR] This script requires Administrator privileges!
    echo Please run: Right-click ^> Run as administrator
    echo.
    pause
    exit /b 1
)

REM ============================================================================
REM SECTION 1: Show Inbound Rule
REM ============================================================================
echo.
echo [SECTION 1] INBOUND RULE: %RULE_NAME%
echo ============================================================================
echo.
netsh advfirewall firewall show rule name="%RULE_NAME%" dir=in
echo.

REM ============================================================================
REM SECTION 2: Show Outbound Rule
REM ============================================================================
echo.
echo [SECTION 2] OUTBOUND RULE: %RULE_NAME% (Out)
echo ============================================================================
echo.
netsh advfirewall firewall show rule name="%RULE_NAME% (Out)" dir=out
echo.

REM ============================================================================
REM SECTION 3: List all rules for port 9000
REM ============================================================================
echo.
echo [SECTION 3] ALL RULES FOR PORT %PORT%
echo ============================================================================
echo.
netsh advfirewall firewall show rule dir=in | find "9000"
echo.
netsh advfirewall firewall show rule dir=out | find "9000"
echo.

REM ============================================================================
REM SECTION 4: Detailed firewall state
REM ============================================================================
echo.
echo [SECTION 4] FIREWALL STATE FOR ALL PROFILES
echo ============================================================================
echo.
netsh advfirewall show allprofiles
echo.

REM ============================================================================
REM SECTION 5: PowerShell verification
REM ============================================================================
echo.
echo [SECTION 5] POWERSHELL VERIFICATION
echo ============================================================================
echo.
powershell -NoProfile -Command "Get-NetFirewallRule -DisplayName 'ChatAppServer*' | Format-Table DisplayName, Direction, Action, Enabled -AutoSize"
echo.

echo.
echo ============================================================================
echo    COPY THE OUTPUT ABOVE AND SHARE FOR DEBUGGING
echo ============================================================================
echo.
pause
