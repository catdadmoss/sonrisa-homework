# API Testing with curl

## Base URL
BASE_URL="https://localhost:5001/api"

## 1. Create an Email Subscription
```bash
curl -k -X POST "$BASE_URL/subscriptions" \
  -H "Content-Type: application/json" \
  -d '{
    "userIdentifier": "user@example.com",
    "channel": "Email",
    "destination": "user@example.com",
    "alertTypeFilter": null
  }'
```

## 2. Create a Slack Subscription
```bash
curl -k -X POST "$BASE_URL/subscriptions" \
  -H "Content-Type: application/json" \
  -d '{
    "userIdentifier": "team-alerts",
    "channel": "Slack",
    "destination": "https://hooks.slack.com/services/YOUR/WEBHOOK/URL",
    "alertTypeFilter": "MarketMovement"
  }'
```

## 3. Create Breaking News Alert
```bash
curl -k -X POST "$BASE_URL/alerts" \
  -H "Content-Type: application/json" \
  -d '{
    "type": "BreakingNews",
    "title": "Major Development in Tech Sector",
    "message": "A significant announcement was made today affecting the technology industry..."
  }'
```

## 4. Create Market Movement Alert
```bash
curl -k -X POST "$BASE_URL/alerts" \
  -H "Content-Type: application/json" \
  -d '{
    "type": "MarketMovement",
    "title": "Stock Market Update",
    "message": "The market experienced a 5% increase today..."
  }'
```

## 5. Create Natural Disaster Alert
```bash
curl -k -X POST "$BASE_URL/alerts" \
  -H "Content-Type: application/json" \
  -d '{
    "type": "NaturalDisaster",
    "title": "Weather Alert",
    "message": "Severe weather warning issued for the region..."
  }'
```

## 6. Get All Alerts
```bash
curl -k -X GET "$BASE_URL/alerts"
```

## 7. Get Pending Alerts Only
```bash
curl -k -X GET "$BASE_URL/alerts?status=Pending"
```

## 8. Get Alerts by Type
```bash
curl -k -X GET "$BASE_URL/alerts?type=BreakingNews"
```

## 9. Get Alert Statistics
```bash
curl -k -X GET "$BASE_URL/alerts/statistics"
```

## 10. Get All Subscriptions
```bash
curl -k -X GET "$BASE_URL/subscriptions"
```

## 11. Get Notification Logs
```bash
curl -k -X GET "$BASE_URL/notificationlogs"
```

## 12. Get Logs for Specific Alert (replace {ALERT_ID})
```bash
curl -k -X GET "$BASE_URL/notificationlogs?alertId=1"
```

## 13. Update an Alert (replace {ALERT_ID})
```bash
curl -k -X PUT "$BASE_URL/alerts/{ALERT_ID}" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Updated Alert Title",
    "message": "Updated message content",
    "metadata": null
  }'
```

## 14. Delete a Subscription (replace {SUBSCRIPTION_ID})
```bash
curl -k -X DELETE "$BASE_URL/subscriptions/{SUBSCRIPTION_ID}"
```

## 15. Delete an Alert (replace {ALERT_ID})
```bash
curl -k -X DELETE "$BASE_URL/alerts/{ALERT_ID}"
```

---

## Full Workflow Example

```bash
# Step 1: Create a subscription
SUB_RESPONSE=$(curl -k -s -X POST "https://localhost:5001/api/subscriptions" \
  -H "Content-Type: application/json" \
  -d '{"userIdentifier":"test","channel":"Email","destination":"test@example.com"}')
echo "Subscription created: $SUB_RESPONSE"

# Step 2: Create an alert
ALERT_RESPONSE=$(curl -k -s -X POST "https://localhost:5001/api/alerts" \
  -H "Content-Type: application/json" \
  -d '{"type":"BreakingNews","title":"Test","message":"Test message"}')
echo "Alert created: $ALERT_RESPONSE"

# Step 3: Wait for background job (30 seconds)
echo "Waiting 30 seconds for background job to process..."
sleep 30

# Step 4: Check notification logs
curl -k -X GET "https://localhost:5001/api/alerts/statistics"
```

---

## Notes

- `-k` flag skips SSL certificate validation (for local development only)
- Replace `{ALERT_ID}` and `{SUBSCRIPTION_ID}` with actual IDs
- Add `| jq '.'` at the end for formatted JSON output (requires jq)
- For Windows PowerShell, use the test-api.ps1 script instead
