@echo off
echo =========================================
echo    KIEM TRA KET NOI MANG
echo =========================================
echo.

set /p SERVER_IP="Nhap IP cua may Server (vd: 10.45.100.45): "

echo.
echo [1] Dang ping den %SERVER_IP%...
ping -n 4 %SERVER_IP%

echo.
echo [2] Dang kiem tra port 9000...
powershell -Command "Test-NetConnection -ComputerName %SERVER_IP% -Port 9000 -WarningAction SilentlyContinue | Select-Object TcpTestSucceeded"

echo.
echo =========================================
echo KET QUA:
echo - Neu ping thanh cong (Reply from...) => Mang OK
echo - Neu TcpTestSucceeded = True => Port 9000 da mo
echo - Neu TcpTestSucceeded = False => Can mo Firewall
echo =========================================
echo.
pause

