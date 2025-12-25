@echo off
REM === CACH SU DUNG DE CHAY NHIEU CLIENT ===
REM 
REM BUOC 1: Build project TRUOC (Ctrl+Shift+B trong Visual Studio)
REM BUOC 2: DONG TAT CA cac ChatAppClient dang chay
REM BUOC 3: Chay script nay
REM
REM LUU Y: Neu dang debug trong Visual Studio, hay STOP debug truoc!
REM ==========================================

echo ========================================
echo   CHAY NHIEU CHAT CLIENT INSTANCES
echo ========================================
echo.

set SOURCE_DIR=%~dp0ChatAppClient\bin\Debug\net8.0-windows

REM Ki?m tra th? m?c source t?n t?i
if not exist "%SOURCE_DIR%" (
    set SOURCE_DIR=%~dp0ChatAppClient\bin\Debug
)

if not exist "%SOURCE_DIR%\ChatAppClient.exe" (
    echo [LOI] Khong tim thay ChatAppClient.exe
    echo Vui long build project truoc!
    echo.
    echo Trong Visual Studio: Build ^> Build Solution (Ctrl+Shift+B)
    pause
    exit /b 1
)

echo Tim thay ChatAppClient tai: %SOURCE_DIR%
echo.

set /p NUM="Nhap so luong client can chay (1-10): "

echo.
echo Dang tao %NUM% ban copy client...
echo.

for /L %%i in (1,1,%NUM%) do (
    set "DEST=%TEMP%\ChatClient_Instance_%%i"
    
    echo [%%i] Tao thu muc: %TEMP%\ChatClient_Instance_%%i
    
    if exist "%TEMP%\ChatClient_Instance_%%i" rmdir /s /q "%TEMP%\ChatClient_Instance_%%i"
    mkdir "%TEMP%\ChatClient_Instance_%%i"
    
    xcopy "%SOURCE_DIR%\*" "%TEMP%\ChatClient_Instance_%%i\" /E /Y /Q >nul
    
    echo [%%i] Khoi dong client...
    start "ChatClient %%i" "%TEMP%\ChatClient_Instance_%%i\ChatAppClient.exe"
    
    timeout /t 2 /nobreak >nul
)

echo.
echo ========================================
echo   DA KHOI DONG %NUM% CLIENT THANH CONG!
echo ========================================
echo.
echo Moi client chay tu thu muc rieng trong TEMP
echo nen khong bi xung dot file.
echo.
pause
