# Blazor Admin UI Guide

## Overview

The Alert Notification System includes a modern Blazor Server admin interface for managing alerts, subscriptions, and viewing notification logs.

## Accessing the UI

1. **Start the application**:
   ```bash
   dotnet run
   ```

2. **Open your browser** to: `https://localhost:5001`

3. **Default landing page**: Dashboard with system statistics

## Features

### 1. Dashboard (Home Page)

**URL**: `/` or `https://localhost:5001`

**Features**:
- **Statistics Cards**: Total alerts, pending, sent, failed
- **Alerts by Type**: Breakdown of BreakingNews, MarketMovement, NaturalDisaster
- **Quick Actions**: One-click navigation to create alerts, manage subscriptions, view logs

### 2. Alerts Management

**URL**: `/alerts` or use navigation menu

**Features**:
- **View All Alerts**: Table list with ID, Type, Title, Status, Timestamps
- **Create Alert**: Form with Type, Title, Message, Metadata fields
- **Edit Alert**: Click edit button to modify existing alerts
- **Delete Alert**: Remove alerts (with confirmation)
- **Status Badges**: Visual indicators for Pending, Processing, Sent, Failed
- **Real-time Updates**: Refresh to see background job processing results

**How to Create an Alert**:
1. Click "Create New Alert" button
2. Select Alert Type (BreakingNews, MarketMovement, NaturalDisaster)
3. Enter Title (max 200 characters)
4. Enter Message (notification body)
5. Optionally add JSON Metadata
6. Click "Save"
7. Alert will be shown as "Pending"
8. Background job processes it every 30 seconds
9. Status changes to "Sent" or "Failed"

### 3. Subscriptions Management

**URL**: `/subscriptions`

**Features**:
- **View All Subscriptions**: List with User, Channel, Destination, Filter, Status
- **Create Subscription**: Add new user notification preferences
- **Edit Subscription**: Modify existing subscriptions
- **Delete Subscription**: Remove subscriptions
- **Active/Inactive Toggle**: Enable or disable subscriptions
- **Alert Type Filtering**: Subscribe to all types or specific ones

**How to Create a Subscription**:

**For Email**:
1. Click "Create New Subscription"
2. User Identifier: `john@example.com`
3. Channel: `Email`
4. Destination: `john@example.com`
5. Alert Type Filter: `All Types` (or select specific type)
6. Active: Checked
7. Click "Save"

**For Slack**:
1. Create a Slack Incoming Webhook:
   - Go to https://api.slack.com/messaging/webhooks
   - Create webhook for your channel
   - Copy the webhook URL
2. In the UI:
   - User Identifier: `team-alerts`
   - Channel: `Slack`
   - Destination: `https://hooks.slack.com/services/YOUR/WEBHOOK/URL`
   - Alert Type Filter: Select or leave "All Types"
   - Active: Checked
3. Click "Save"

### 4. Notification Logs

**URL**: `/logs`

**Features**:
- **View All Logs**: Complete delivery history
- **Status Indicators**: Success (green check) or Failed (red X)
- **Error Details**: View error messages for failed notifications
- **Filter Statistics**: Summary of total, successful, failed notifications
- **Related Links**: Jump to associated alerts/subscriptions
- **Timestamp**: When each notification was attempted

**Reading Logs**:
- **Green rows**: Successful notifications
- **Red rows**: Failed notifications
- **Error button**: Click to view detailed error message in modal
- **Alert ID**: Links back to alerts page
- **Subscription ID**: Links back to subscriptions page

## Navigation

The sidebar menu provides:
- **Dashboard**: System overview
- **Alerts**: Manage alerts
- **Subscriptions**: Manage subscriptions
- **Notification Logs**: View history
- **API Docs (Swagger)**: Opens Swagger UI in new tab

## Tips & Best Practices

### Testing the System

1. **Create a Subscription** first (to yourself via email or Slack)
2. **Create an Alert** that matches the subscription filter
3. **Wait 30 seconds** for background job to process
4. **Check your email/Slack** for the notification
5. **View Logs** to confirm delivery status

### Common Workflows

**Scenario 1: Send Breaking News to All Subscribers**
1. Go to Alerts → Create New Alert
2. Type: BreakingNews
3. Title: "Major Event"
4. Message: "Details here..."
5. Save
6. All active subscriptions with no filter or BreakingNews filter will receive it

**Scenario 2: Subscribe User to Market Alerts Only**
1. Go to Subscriptions → Create New Subscription
2. User: `trader@example.com`
3. Channel: Email
4. Destination: `trader@example.com`
5. Filter: MarketMovement Only
6. Save
7. User only gets MarketMovement alerts

**Scenario 3: Troubleshoot Failed Notification**
1. Go to Notification Logs
2. Find red (failed) row
3. Click "View" error button
4. Read error message (e.g., "Invalid webhook URL", "SMTP authentication failed")
5. Fix the issue (update subscription, check email settings)
6. Create a new test alert

### Performance

- **Background Job**: Runs every 30 seconds
- **Create Multiple Alerts**: They queue as "Pending"
- **Bulk Processing**: All pending alerts processed in each job run
- **Parallel Sending**: Notifications sent concurrently within each alert

### Troubleshooting

**Alert stays "Pending"**:
- Check if background job is running (console output shows "NotificationJob started")
- Verify subscriptions exist and are active
- Check email/Slack configuration

**Notification marked "Sent" but not received**:
- Check spam folder (for email)
- Verify webhook URL is correct (for Slack)
- Check Notification Logs for actual success status

**UI not loading**:
- Ensure application is running: `dotnet run`
- Check browser console for errors
- Verify URL: `https://localhost:5001`
- Trust HTTPS certificate if prompted

**Form validation errors**:
- Title is required (max 200 chars)
- Message is required
- Email format validation for email destinations
- All required fields must be filled

## API Alternative

If you prefer programmatic access, use the REST API via Swagger:
- URL: `https://localhost:5001/swagger`
- Full CRUD operations available
- Test directly from Swagger UI
- Use for automation/scripting

## Next Steps

1. **Configure Email**: Set up User Secrets with your Gmail app password
2. **Create Slack Webhook**: Set up Slack integration if needed
3. **Add Test Subscriptions**: Subscribe yourself to test notifications
4. **Create Test Alert**: Verify end-to-end flow
5. **Monitor Logs**: Check delivery success/failure

## Security Note

**Never commit**:
- Email passwords in `appsettings.json`
- Slack webhook URLs in code
- Database files (`.db`)

**Use User Secrets** for all credentials in development.
**Use Environment Variables** or Azure Key Vault in production.

---

Enjoy managing your alert notifications! 🎉
