@echo off
chcp 65001 >nul
echo ========================================
echo    CHẨN ĐOÁN MẠNG - DESTINATION HOST UNREACHABLE
echo ========================================
echo.

echo [1] THÔNG TIN CẤU HÌNH MẠNG:
echo ----------------------------------------
ipconfig /all
echo.
echo.

echo [2] ROUTING TABLE:
echo ----------------------------------------
route print | findstr /C:"10.215.204"
echo.
echo.

echo [3] KIỂM TRA GATEWAY THỰC TẾ:
echo ----------------------------------------
for /f "tokens=3" %%a in ('ipconfig ^| findstr /C:"Default Gateway"') do (
    set GATEWAY=%%a
    echo Default Gateway thực tế: %%a
    echo.
    echo Đang ping Gateway thực tế...
    ping -n 2 %%a
)
echo.
echo.

echo [4] KIỂM TRA PING ĐẾN 10.215.204.1:
echo ----------------------------------------
ping -n 4 10.215.204.1
echo.
echo.

echo [5] KIỂM TRA FIREWALL ICMP:
echo ----------------------------------------
netsh advfirewall firewall show rule name=all | findstr /i "ICMP"
echo.
echo.

echo [6] KIỂM TRA CÁC IP KHÁC TRONG MẠNG:
echo ----------------------------------------
echo Đang ping 10.215.204.254 (thường là Gateway)...
ping -n 2 10.215.204.254
echo.
echo Đang ping 10.215.204.2...
ping -n 2 10.215.204.2
echo.
echo Đang ping 10.215.204.100...
ping -n 2 10.215.204.100
echo.
echo.

echo [7] KIỂM TRA DNS:
echo ----------------------------------------
nslookup google.com
echo.
echo.

echo ========================================
echo    KẾT THÚC CHẨN ĐOÁN
echo ========================================
echo.
echo Xem file KHAC_PHUC_DESTINATION_HOST_UNREACHABLE.md để biết cách khắc phục!
echo.
pause

