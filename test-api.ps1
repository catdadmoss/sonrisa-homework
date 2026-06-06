# Test the Alert Notification System

# This PowerShell script demonstrates the API usage
# Make sure the application is running first: dotnet run

$baseUrl = "https://localhost:5001/api"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Alert Notification System - API Test" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Test 1: Create a subscription
Write-Host "1. Creating Email Subscription..." -ForegroundColor Yellow
$subscription = @{
    userIdentifier = "testuser"
    channel = "Email"
    destination = "test@example.com"
    alertTypeFilter = $null
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/subscriptions" -Method Post -Body $subscription -ContentType "application/json" -SkipCertificateCheck
    Write-Host "   ✓ Subscription created: ID=$($response.id)" -ForegroundColor Green
    $subscriptionId = $response.id
} catch {
    Write-Host "   ✗ Failed to create subscription" -ForegroundColor Red
    Write-Host "   Make sure the app is running at $baseUrl" -ForegroundColor Red
    exit
}

Write-Host ""

# Test 2: Create an alert
Write-Host "2. Creating Breaking News Alert..." -ForegroundColor Yellow
$alert = @{
    type = "BreakingNews"
    title = "Test Alert - Market Update"
    message = "This is a test notification from the Alert Notification System. The market has experienced significant movement."
    metadata = $null
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/alerts" -Method Post -Body $alert -ContentType "application/json" -SkipCertificateCheck
    Write-Host "   ✓ Alert created: ID=$($response.id), Status=$($response.status)" -ForegroundColor Green
    $alertId = $response.id
} catch {
    Write-Host "   ✗ Failed to create alert" -ForegroundColor Red
}

Write-Host ""

# Test 3: Get all subscriptions
Write-Host "3. Fetching All Subscriptions..." -ForegroundColor Yellow
try {
    $subs = Invoke-RestMethod -Uri "$baseUrl/subscriptions" -Method Get -SkipCertificateCheck
    Write-Host "   ✓ Found $($subs.Count) subscription(s)" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Failed to fetch subscriptions" -ForegroundColor Red
}

Write-Host ""

# Test 4: Get alert statistics
Write-Host "4. Fetching Alert Statistics..." -ForegroundColor Yellow
try {
    $stats = Invoke-RestMethod -Uri "$baseUrl/alerts/statistics" -Method Get -SkipCertificateCheck
    Write-Host "   ✓ Total Alerts: $($stats.totalAlerts)" -ForegroundColor Green
    Write-Host "   ✓ Pending: $($stats.pendingAlerts), Sent: $($stats.sentAlerts), Failed: $($stats.failedAlerts)" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Failed to fetch statistics" -ForegroundColor Red
}

Write-Host ""

# Wait for background job
Write-Host "5. Waiting for background job to process alert (30 seconds)..." -ForegroundColor Yellow
Write-Host "   The Quartz job runs every 30 seconds to process pending alerts" -ForegroundColor Gray
Start-Sleep -Seconds 32

# Test 5: Check notification logs
Write-Host "6. Checking Notification Logs..." -ForegroundColor Yellow
try {
    $logs = Invoke-RestMethod -Uri "$baseUrl/notificationlogs?alertId=$alertId" -Method Get -SkipCertificateCheck
    if ($logs.Count -gt 0) {
        Write-Host "   ✓ Found $($logs.Count) notification log(s) for Alert ID $alertId" -ForegroundColor Green
        foreach ($log in $logs) {
            $status = if ($log.success) { "SUCCESS" } else { "FAILED" }
            $color = if ($log.success) { "Green" } else { "Red" }
            Write-Host "     - Channel: $($log.channel), Status: $status" -ForegroundColor $color
            if ($log.errorMessage) {
                Write-Host "       Error: $($log.errorMessage)" -ForegroundColor Red
            }
        }
    } else {
        Write-Host "   ⚠ No logs found yet - background job may still be processing" -ForegroundColor Yellow
    }
} catch {
    Write-Host "   ✗ Failed to fetch logs" -ForegroundColor Red
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Test Complete!" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor White
Write-Host "  1. Configure Email settings in appsettings.json to send real emails" -ForegroundColor Gray
Write-Host "  2. Add Slack webhook subscriptions for Slack notifications" -ForegroundColor Gray
Write-Host "  3. Explore the Swagger UI at https://localhost:5001/swagger" -ForegroundColor Gray
Write-Host ""
