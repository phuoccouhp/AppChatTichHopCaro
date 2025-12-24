#!/usr/bin/env powershell
# Diagnostic script to check client-server communication

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Chat App Client-Server Diagnostics" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Check 1: Are services running?
Write-Host "[1/5] Checking running processes..." -ForegroundColor Yellow
$serverRunning = Get-Process | Where-Object { $_.ProcessName -like "*ChatAppServer*" -or $_.ProcessName -like "*dotnet*" } | Where-Object { $_.CommandLine -like "*ChatAppServer*" }
$clientRunning = Get-Process | Where-Object { $_.ProcessName -like "*ChatAppClient*" -or $_.ProcessName -like "*dotnet*" } | Where-Object { $_.CommandLine -like "*ChatAppClient*" }

if ($serverRunning) {
    Write-Host "? Server is running" -ForegroundColor Green
} else {
    Write-Host "? Server NOT running" -ForegroundColor Red
}

if ($clientRunning) {
    Write-Host "? Client is running" -ForegroundColor Green
} else {
    Write-Host "? Client NOT running" -ForegroundColor Red
}
Write-Host ""

# Check 2: Is port 9000 listening?
Write-Host "[2/5] Checking port 9000..." -ForegroundColor Yellow
$port9000 = netstat -ano | Select-String ":9000"
if ($port9000) {
    Write-Host "? Port 9000 is listening" -ForegroundColor Green
    Write-Host "  $port9000" -ForegroundColor Gray
} else {
    Write-Host "? Port 9000 is NOT listening" -ForegroundColor Red
}
Write-Host ""

# Check 3: Firewall rules
Write-Host "[3/5] Checking firewall rules..." -ForegroundColor Yellow
$inboundRule = Get-NetFirewallRule -DisplayName "ChatAppServer" -Direction Inbound -ErrorAction SilentlyContinue
$outboundRule = Get-NetFirewallRule -DisplayName "ChatAppServer (Out)" -Direction Outbound -ErrorAction SilentlyContinue

if ($inboundRule) {
    Write-Host "? Inbound rule exists: $($inboundRule.Enabled)" -ForegroundColor Green
} else {
    Write-Host "? Inbound rule missing" -ForegroundColor Red
}

if ($outboundRule) {
    Write-Host "? Outbound rule exists: $($outboundRule.Enabled)" -ForegroundColor Green
} else {
    Write-Host "? Outbound rule missing" -ForegroundColor Red
}
Write-Host ""

# Check 4: Test connection
Write-Host "[4/5] Testing TCP connection to localhost:9000..." -ForegroundColor Yellow
try {
    $tcpClient = New-Object System.Net.Sockets.TcpClient
    $tcpClient.Connect("127.0.0.1", 9000)
    if ($tcpClient.Connected) {
        Write-Host "? Can connect to localhost:9000" -ForegroundColor Green
        $tcpClient.Close()
    }
} catch {
    Write-Host "? Cannot connect to localhost:9000: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Check 5: Database check (if applicable)
Write-Host "[5/5] Database check..." -ForegroundColor Yellow
Write-Host "  (Assuming SQL Server on localhost)" -ForegroundColor Gray
try {
    $conn = New-Object System.Data.SqlClient.SqlConnection
    $conn.ConnectionString = "Server=localhost;Database=ChatAppDB;Integrated Security=true;Connection Timeout=5"
    $conn.Open()
    Write-Host "? Database connection OK" -ForegroundColor Green
    $conn.Close()
} catch {
    Write-Host "? Database not accessible (may be OK if not needed)" -ForegroundColor Yellow
}
Write-Host ""

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "Diagnostic Summary" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

if ($serverRunning) {
    Write-Host "? Server status: OK" -ForegroundColor Green
} else {
    Write-Host "? Server status: NOT RUNNING" -ForegroundColor Red
    Write-Host "  ? Start server: cd ChatAppServer; dotnet run" -ForegroundColor Yellow
}

if ($clientRunning) {
    Write-Host "? Client status: OK" -ForegroundColor Green
} else {
    Write-Host "? Client status: NOT RUNNING" -ForegroundColor Red
    Write-Host "  ? Start client: cd ChatAppClient; dotnet run" -ForegroundColor Yellow
}

if ($port9000) {
    Write-Host "? Port 9000: LISTENING" -ForegroundColor Green
} else {
    Write-Host "? Port 9000: NOT LISTENING" -ForegroundColor Red
    Write-Host "  ? Server may not be running" -ForegroundColor Yellow
}

if ($inboundRule -and $outboundRule) {
    Write-Host "? Firewall: OPEN" -ForegroundColor Green
} else {
    Write-Host "? Firewall: RULES MISSING" -ForegroundColor Red
    Write-Host "  ? Run: PowerShell -ExecutionPolicy Bypass -File CreateOutboundRuleFix.ps1" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. If server not running: start it" -ForegroundColor White
Write-Host "2. If client not running: start it" -ForegroundColor White
Write-Host "3. If firewall issues: create rules using CreateOutboundRuleFix.ps1" -ForegroundColor White
Write-Host "4. Try login again" -ForegroundColor White
Write-Host ""
