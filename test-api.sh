#!/bin/bash

# Test the Alert Notification System
# Make sure the application is running first: dotnet run

BASE_URL="https://localhost:5001/api"

echo "====================================="
echo "Alert Notification System - API Test"
echo "====================================="
echo ""

# Test 1: Create a subscription
echo "1. Creating Email Subscription..."
SUBSCRIPTION_RESPONSE=$(curl -k -s -X POST "$BASE_URL/subscriptions" \
  -H "Content-Type: application/json" \
  -d '{
    "userIdentifier": "testuser",
    "channel": "Email",
    "destination": "test@example.com",
    "alertTypeFilter": null
  }')

if [ $? -eq 0 ]; then
  SUBSCRIPTION_ID=$(echo $SUBSCRIPTION_RESPONSE | grep -o '"id":[0-9]*' | grep -o '[0-9]*')
  echo "   ✓ Subscription created: ID=$SUBSCRIPTION_ID"
else
  echo "   ✗ Failed to create subscription"
  echo "   Make sure the app is running at $BASE_URL"
  exit 1
fi

echo ""

# Test 2: Create an alert
echo "2. Creating Breaking News Alert..."
ALERT_RESPONSE=$(curl -k -s -X POST "$BASE_URL/alerts" \
  -H "Content-Type: application/json" \
  -d '{
    "type": "BreakingNews",
    "title": "Test Alert - Market Update",
    "message": "This is a test notification from the Alert Notification System.",
    "metadata": null
  }')

ALERT_ID=$(echo $ALERT_RESPONSE | grep -o '"id":[0-9]*' | grep -o '[0-9]*')
echo "   ✓ Alert created: ID=$ALERT_ID"

echo ""

# Test 3: Get statistics
echo "3. Fetching Alert Statistics..."
curl -k -s -X GET "$BASE_URL/alerts/statistics" | jq '.' 2>/dev/null || echo "   (Install jq for formatted output)"

echo ""

# Wait for background job
echo "4. Waiting for background job to process alert (30 seconds)..."
echo "   The Quartz job runs every 30 seconds to process pending alerts"
sleep 32

# Test 4: Check notification logs
echo "5. Checking Notification Logs..."
curl -k -s -X GET "$BASE_URL/notificationlogs?alertId=$ALERT_ID" | jq '.' 2>/dev/null || curl -k -s -X GET "$BASE_URL/notificationlogs?alertId=$ALERT_ID"

echo ""
echo "====================================="
echo "Test Complete!"
echo "====================================="
echo ""
echo "Next Steps:"
echo "  1. Configure Email settings in appsettings.json"
echo "  2. Add Slack webhook subscriptions"
echo "  3. Explore Swagger UI at https://localhost:5001/swagger"
echo ""
