@echo off
REM Script ?? ch?y nhi?u ChatAppClient instances
REM Build project tr??c, sau ?ó ch?y script này

set SOURCE_DIR=%~dp0ChatAppClient\bin\Debug\net8.0-windows
set CLIENT_EXE=ChatAppClient.exe

REM Ki?m tra file exe t?n t?i
if not exist "%SOURCE_DIR%\%CLIENT_EXE%" (
    echo [ERROR] Khong tim thay %CLIENT_EXE%
    echo Vui long build project truoc khi chay script nay.
    pause
    exit /b 1
)

REM H?i s? l??ng client mu?n ch?y
set /p NUM_CLIENTS="Nhap so luong client muon chay (1-5): "

REM T?o th? m?c t?m và copy + ch?y t?ng client
for /L %%i in (1,1,%NUM_CLIENTS%) do (
    set "CLIENT_DIR=%TEMP%\ChatAppClient_%%i"
    
    REM T?o th? m?c n?u ch?a có
    if not exist "%TEMP%\ChatAppClient_%%i" mkdir "%TEMP%\ChatAppClient_%%i"
    
    REM Copy t?t c? file c?n thi?t
    xcopy "%SOURCE_DIR%\*.*" "%TEMP%\ChatAppClient_%%i\" /Y /Q >nul 2>&1
    
    REM Ch?y client
    echo Dang khoi dong Client %%i...
    start "" "%TEMP%\ChatAppClient_%%i\%CLIENT_EXE%"
    
    REM ??i 1 giây gi?a m?i client
    timeout /t 1 /nobreak >nul
)

echo.
echo Da khoi dong %NUM_CLIENTS% client(s).
echo Cac client duoc copy vao thu muc TEMP de tranh xung dot file.
pause
