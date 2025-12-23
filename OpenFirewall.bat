@echo off
:: Kiem tra quyen Admin
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo.
    echo =========================================
    echo    CAN QUYEN ADMINISTRATOR!
    echo =========================================
    echo.
    echo Vui long chay file nay voi quyen Administrator:
    echo Right-click vao file -> "Run as administrator"
    echo.
    pause
    exit /b 1
)

echo =========================================
echo    MO FIREWALL CHO CHAT APP SERVER
echo =========================================
echo.
echo Dang mo port 9000 tren Windows Firewall...
echo.

:: Xoa rule cu neu co
echo [1/3] Xoa rule cu...
netsh advfirewall firewall delete rule name="ChatAppServer" >nul 2>&1
netsh advfirewall firewall delete rule name="ChatAppServer (Out)" >nul 2>&1
echo    ✓ Done

:: Them rule moi - Inbound (cho phep ket noi DEN tu BAT KY dau trong mang local)
echo [2/3] Them rule Inbound (cho phep ket noi den)...
netsh advfirewall firewall add rule name="ChatAppServer" dir=in action=allow protocol=TCP localport=9000 profile=private,domain enable=yes
if %errorLevel% equ 0 (
    echo    ✓ Da them rule Inbound
) else (
    echo    ✗ Loi khi them rule Inbound
)

:: Them rule moi - Outbound (cho phep ket noi di)  
echo [3/3] Them rule Outbound (cho phep ket noi di)...
netsh advfirewall firewall add rule name="ChatAppServer (Out)" dir=out action=allow protocol=TCP localport=9000 profile=private,domain enable=yes
if %errorLevel% equ 0 (
    echo    ✓ Da them rule Outbound
) else (
    echo    ✗ Loi khi them rule Outbound
)

echo.
echo =========================================
echo    DA MO PORT 9000 THANH CONG!
echo =========================================
echo.
echo Bay gio cac may khac trong CUNG MANG WiFi co the ket noi den Server.
echo.
echo LUA Y:
echo - Rule chi ap dung cho Private network (WiFi)
echo - Neu dung Public network, can mo rule rieng
echo.
pause

