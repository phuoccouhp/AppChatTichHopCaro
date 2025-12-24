# ============================================================================
# PowerShell Script: Debug Firewall Rules
# M?c ?ích: Ki?m tra và debug l?i Outbound rule không ???c t?o
# ============================================================================

param(
    [string]$RuleName = "ChatAppServer",
    [int]$Port = 9000
)

# Yêu c?u Admin
If (-NOT ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Host "ERROR: Script requires Administrator privileges!" -ForegroundColor Red
    Write-Host "Please run: Right-click PowerShell -> Run as administrator" -ForegroundColor Yellow
    exit 1
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   FIREWALL RULES DEBUG TOOL" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ============================================================================
# STEP 1: Ki?m tra Inbound Rule
# ============================================================================
Write-Host "[1] Checking INBOUND Rule: $RuleName" -ForegroundColor Yellow
$inbound = Get-NetFirewallRule -DisplayName $RuleName -ErrorAction SilentlyContinue
if ($inbound) {
    Write-Host "? Inbound rule EXISTS" -ForegroundColor Green
    $inbound | Format-Table -AutoSize
    
    $inboundDetails = Get-NetFirewallPortFilter -AssociatedNetFirewallRule $inbound
    Write-Host "Details:" -ForegroundColor Cyan
    $inboundDetails | Format-Table -AutoSize
} else {
    Write-Host "? Inbound rule NOT FOUND" -ForegroundColor Red
}
Write-Host ""

# ============================================================================
# STEP 2: Ki?m tra Outbound Rule
# ============================================================================
Write-Host "[2] Checking OUTBOUND Rule: $RuleName (Out)" -ForegroundColor Yellow
$outbound = Get-NetFirewallRule -DisplayName "$RuleName (Out)" -ErrorAction SilentlyContinue
if ($outbound) {
    Write-Host "? Outbound rule EXISTS" -ForegroundColor Green
    $outbound | Format-Table -AutoSize
    
    $outboundDetails = Get-NetFirewallPortFilter -AssociatedNetFirewallRule $outbound
    Write-Host "Details:" -ForegroundColor Cyan
    $outboundDetails | Format-Table -AutoSize
} else {
    Write-Host "? Outbound rule NOT FOUND" -ForegroundColor Red
    Write-Host "This is the problem! Outbound rule must be created." -ForegroundColor Red
}
Write-Host ""

# ============================================================================
# STEP 3: Th? t?o Outbound Rule n?u không t?n t?i
# ============================================================================
if (-NOT $outbound) {
    Write-Host "[3] Attempting to CREATE Outbound Rule..." -ForegroundColor Yellow
    try {
        # Delete old one first
        Remove-NetFirewallRule -DisplayName "$RuleName (Out)" -ErrorAction SilentlyContinue | Out-Null
        
        # Create new outbound rule WITH remoteip=any
        New-NetFirewallRule -DisplayName "$RuleName (Out)" `
            -Direction Outbound `
            -Action Allow `
            -Protocol TCP `
            -LocalPort $Port `
            -RemoteAddress Any `
            -Profile Domain,Private,Public `
            -Enabled $true | Out-Null
        
        Write-Host "? Outbound rule created successfully!" -ForegroundColor Green
        
        # Verify
        $outbound = Get-NetFirewallRule -DisplayName "$RuleName (Out)" -ErrorAction SilentlyContinue
        if ($outbound) {
            Write-Host "? Verification: Rule now EXISTS" -ForegroundColor Green
            $outbound | Format-Table -AutoSize
        }
    } catch {
        Write-Host "? Error creating outbound rule: $_" -ForegroundColor Red
    }
} else {
    Write-Host "[3] Outbound rule already exists - skipping creation" -ForegroundColor Cyan
}
Write-Host ""

# ============================================================================
# STEP 4: Ki?m tra t?t c? rules liên quan ??n port 9000
# ============================================================================
Write-Host "[4] Listing ALL rules for Port $Port" -ForegroundColor Yellow
$allRules = Get-NetFirewallRule | Where-Object {
    $filter = $_ | Get-NetFirewallPortFilter -ErrorAction SilentlyContinue
    $filter -and ($filter.LocalPort -eq $Port -or $filter.LocalPort -contains $Port)
}

if ($allRules) {
    $allRules | Format-Table DisplayName, Direction, Action, Enabled -AutoSize
} else {
    Write-Host "No firewall rules found for port $Port" -ForegroundColor Yellow
}
Write-Host ""

# ============================================================================
# STEP 5: Netsh verification (nh? code C# dùng)
# ============================================================================
Write-Host "[5] Netsh Verification (as used in C# code)" -ForegroundColor Yellow

Write-Host "Inbound netsh output:" -ForegroundColor Cyan
& netsh advfirewall firewall show rule name="$RuleName" dir=in
Write-Host ""

Write-Host "Outbound netsh output:" -ForegroundColor Cyan
& netsh advfirewall firewall show rule name="$RuleName (Out)" dir=out
Write-Host ""

# ============================================================================
# STEP 6: Test k?t n?i
# ============================================================================
Write-Host "[6] Testing Connection to Localhost:$Port" -ForegroundColor Yellow
$testResult = Test-NetConnection -ComputerName localhost -Port $Port
Write-Host "TcpTestSucceeded: $($testResult.TcpTestSucceeded)" -ForegroundColor Cyan
Write-Host ""

# ============================================================================
# SUMMARY
# ============================================================================
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   SUMMARY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$inOK = ($inbound -ne $null)
$outOK = ($outbound -ne $null)

Write-Host "Inbound Rule:  $(if ($inOK) { '? OK' } else { '? MISSING' })" -ForegroundColor $(if ($inOK) { 'Green' } else { 'Red' })
Write-Host "Outbound Rule: $(if ($outOK) { '? OK' } else { '? MISSING' })" -ForegroundColor $(if ($outOK) { 'Green' } else { 'Red' })

if ($inOK -and $outOK) {
    Write-Host ""
    Write-Host "? Both rules are properly configured!" -ForegroundColor Green
    Write-Host "  Server should be able to accept and reply to client connections." -ForegroundColor Green
} elseif ($inOK -and -NOT $outOK) {
    Write-Host ""
    Write-Host "? CRITICAL: Outbound rule is missing!" -ForegroundColor Red
    Write-Host "  Client can connect but Server cannot send replies." -ForegroundColor Red
    Write-Host "  Try running: New-NetFirewallRule -DisplayName '$RuleName (Out)' -Direction Outbound -Action Allow -Protocol TCP -LocalPort $Port -RemoteAddress Any -Profile Domain,Private,Public" -ForegroundColor Yellow
} else {
    Write-Host ""
    Write-Host "? ERROR: Inbound rule is missing!" -ForegroundColor Red
}

Write-Host ""
Write-Host "Press any key to exit..."
[Console]::ReadKey() | Out-Null
