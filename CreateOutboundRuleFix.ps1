# ============================================================================
# OUTBOUND RULE FIX - PowerShell Only
# Run with: powershell -ExecutionPolicy Bypass -File CreateOutboundRuleFix.ps1
# ============================================================================

Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host "    FIREWALL RULE CREATION - OUTBOUND FIX" -ForegroundColor Cyan
Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host ""

# Check Admin
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")
if (-not $isAdmin) {
    Write-Host "ERROR: This script must run as Administrator!" -ForegroundColor Red
    Write-Host "Solution: Right-click PowerShell > Run as administrator" -ForegroundColor Yellow
    exit 1
}

Write-Host "? Running as Administrator" -ForegroundColor Green
Write-Host ""

# Step 1: Delete old rules
Write-Host "[Step 1] Deleting old rules..." -ForegroundColor Yellow
Remove-NetFirewallRule -DisplayName "ChatAppServer" -Direction Inbound -ErrorAction SilentlyContinue | Out-Null
Remove-NetFirewallRule -DisplayName "ChatAppServer (Out)" -Direction Outbound -ErrorAction SilentlyContinue | Out-Null
Write-Host "    Done" -ForegroundColor Gray
Write-Host ""

# Step 2: Create Inbound
Write-Host "[Step 2] Creating INBOUND rule..." -ForegroundColor Yellow
try {
    New-NetFirewallRule `
        -DisplayName "ChatAppServer" `
        -Direction Inbound `
        -Action Allow `
        -Protocol TCP `
        -LocalPort 9000 `
        -Profile Domain,Private,Public `
        -Enabled $true `
        -ErrorAction Stop | Out-Null
    Write-Host "    ? INBOUND rule created" -ForegroundColor Green
}
catch {
    Write-Host "    ? INBOUND rule failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Step 3: Create Outbound - THE CRITICAL PART
Write-Host "[Step 3] Creating OUTBOUND rule..." -ForegroundColor Yellow
try {
    New-NetFirewallRule `
        -DisplayName "ChatAppServer (Out)" `
        -Direction Outbound `
        -Action Allow `
        -Protocol TCP `
        -LocalPort 9000 `
        -RemoteAddress Any `
        -Profile Domain,Private,Public `
        -Enabled $true `
        -ErrorAction Stop | Out-Null
    Write-Host "    ? OUTBOUND rule created" -ForegroundColor Green
}
catch {
    Write-Host "    ? OUTBOUND rule failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Step 4: Verify
Write-Host "[Step 4] Verifying rules..." -ForegroundColor Yellow
$inbound = Get-NetFirewallRule -DisplayName "ChatAppServer" -Direction Inbound -ErrorAction SilentlyContinue
$outbound = Get-NetFirewallRule -DisplayName "ChatAppServer (Out)" -Direction Outbound -ErrorAction SilentlyContinue

if ($inbound) {
    Write-Host "    ? Inbound rule EXISTS" -ForegroundColor Green
} else {
    Write-Host "    ? Inbound rule MISSING" -ForegroundColor Red
}

if ($outbound) {
    Write-Host "    ? Outbound rule EXISTS" -ForegroundColor Green
} else {
    Write-Host "    ? Outbound rule MISSING" -ForegroundColor Red
}
Write-Host ""

# Step 5: Show rules
Write-Host "[Step 5] Detailed rule information:" -ForegroundColor Yellow
Write-Host ""
Get-NetFirewallRule -DisplayName "ChatAppServer*" | Select-Object DisplayName, Direction, Action, Enabled | Format-Table -AutoSize
Write-Host ""

# Check RemoteAddress for Outbound
Write-Host "[Step 6] Verifying Outbound rule properties:" -ForegroundColor Yellow
$outboundRule = Get-NetFirewallRule -DisplayName "ChatAppServer (Out)" -Direction Outbound -ErrorAction SilentlyContinue
if ($outboundRule) {
    $addressFilter = $outboundRule | Get-NetFirewallAddressFilter
    Write-Host "    Remote Address: $($addressFilter.RemoteAddress)" -ForegroundColor Green
    if ($addressFilter.RemoteAddress -eq "Any") {
        Write-Host "    ? RemoteAddress is correctly set to 'Any'" -ForegroundColor Green
    } else {
        Write-Host "    ?? RemoteAddress is '$($addressFilter.RemoteAddress)' - should be 'Any'" -ForegroundColor Yellow
    }
}
Write-Host ""

Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host "    SUCCESS! Rules have been created." -ForegroundColor Green
Write-Host "============================================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Open Command Prompt or PowerShell" -ForegroundColor White
Write-Host "  2. Run Server: dotnet run --project ChatAppServer/ChatAppServer.csproj" -ForegroundColor White
Write-Host "  3. Open another terminal and run Client: dotnet run --project ChatAppClient/ChatAppClient.csproj" -ForegroundColor White
Write-Host "  4. Login to test the connection" -ForegroundColor White
Write-Host ""
