# Script PowerShell để mở Firewall port 9000
# Chạy với: Right-click → Run with PowerShell (as Administrator)

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "   MO FIREWALL CHO CHAT APP SERVER" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Kiểm tra quyền Admin
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "CAN QUYEN ADMINISTRATOR!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Vui long chay script nay voi quyen Administrator:" -ForegroundColor Yellow
    Write-Host "Right-click vao file -> 'Run with PowerShell' -> Chon 'Run as Administrator'" -ForegroundColor Yellow
    Write-Host ""
    pause
    exit 1
}

Write-Host "Dang mo port 9000 tren Windows Firewall..." -ForegroundColor Yellow
Write-Host ""

# Xóa rule cũ nếu có
Write-Host "[1/3] Xoa rule cu..." -ForegroundColor Cyan
netsh advfirewall firewall delete rule name="ChatAppServer" 2>$null
netsh advfirewall firewall delete rule name="ChatAppServer (Out)" 2>$null
Write-Host "    Done" -ForegroundColor Green

# Thêm rule mới - Inbound
Write-Host "[2/3] Them rule Inbound (cho phep ket noi den)..." -ForegroundColor Cyan
$result1 = netsh advfirewall firewall add rule name="ChatAppServer" dir=in action=allow protocol=TCP localport=9000 profile=private,domain enable=yes
if ($LASTEXITCODE -eq 0) {
    Write-Host "    Da them rule Inbound" -ForegroundColor Green
} else {
    Write-Host "    Loi khi them rule Inbound" -ForegroundColor Red
    Write-Host "    Output: $result1" -ForegroundColor Yellow
}

# Thêm rule mới - Outbound
Write-Host "[3/3] Them rule Outbound (cho phep ket noi di)..." -ForegroundColor Cyan
$result2 = netsh advfirewall firewall add rule name="ChatAppServer (Out)" dir=out action=allow protocol=TCP localport=9000 profile=private,domain enable=yes
if ($LASTEXITCODE -eq 0) {
    Write-Host "    Da them rule Outbound" -ForegroundColor Green
} else {
    Write-Host "    Loi khi them rule Outbound" -ForegroundColor Red
    Write-Host "    Output: $result2" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "   KET QUA" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

# Kiểm tra rule đã tạo chưa
$ruleExists = netsh advfirewall firewall show rule name="ChatAppServer" | Select-String "Rule Name"
if ($ruleExists) {
    Write-Host "DA MO PORT 9000 THANH CONG!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Bay gio cac may khac trong CUNG MANG WiFi co the ket noi den Server." -ForegroundColor Green
} else {
    Write-Host "CO THE CO LOI! Kiem tra lai rule trong Windows Firewall." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Mo thu cong:" -ForegroundColor Yellow
    Write-Host "1. Windows Security -> Firewall & network protection" -ForegroundColor Yellow
    Write-Host "2. Advanced settings -> Inbound Rules" -ForegroundColor Yellow
    Write-Host "3. Tim rule 'ChatAppServer' -> Kiem tra Status = Enabled" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "LUA Y:" -ForegroundColor Yellow
Write-Host "- Rule chi ap dung cho Private network (WiFi)" -ForegroundColor Yellow
Write-Host "- Neu dung Public network, can mo rule rieng" -ForegroundColor Yellow
Write-Host ""
pause

