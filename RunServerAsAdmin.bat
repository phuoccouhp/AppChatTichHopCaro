@echo off
REM ============================================================================
REM Auto-run ChatApp Server with Admin privileges
REM ============================================================================

setlocal enabledelayedexpansion

REM Check if already running as admin
net session >nul 2>&1
if %errorLevel% equ 0 (
    REM Already admin, run server directly
    echo.
    echo ? Running with Administrator privileges
    echo.
    echo Starting ChatApp Server...
    echo.
    cd /d "%~dp0ChatAppServer"
    dotnet run
) else (
    REM Not admin, re-run with admin privileges
    echo.
    echo Requesting Administrator privileges...
    echo.
    
    REM Create temp VBS script to run this batch file with admin rights
    set "tempvbs=%temp%\run_admin.vbs"
    echo CreateObject("Shell.Application").ShellExecute "%~s0", "", "", "runas", 1 > "%tempvbs%"
    cscript.exe "%tempvbs%"
    del "%tempvbs%" 2>nul
)

pause
