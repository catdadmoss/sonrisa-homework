# Quick Start Guide - 5 Minutes to Running

## Step 1: Run the Application (30 seconds)
```bash
dotnet restore
dotnet run
```

Open your browser to: **https://localhost:5001/swagger**

## Step 2: Create Your First Subscription (1 minute)

Click on `POST /api/subscriptions` in Swagger, then "Try it out":

```json
{
  "userIdentifier": "testuser",
  "channel": "Email",
  "destination": "your-email@example.com",
  "alertTypeFilter": null
}
```

Click "Execute"

## Step 3: Create an Alert (30 seconds)

Click on `POST /api/alerts`, then "Try it out":

```json
{
  "type": "BreakingNews",
  "title": "Test Alert",
  "message": "This is a test alert notification"
}
```

Click "Execute"

## Step 4: Watch it Work! (30 seconds)

The background job runs every 30 seconds. Within 30 seconds:
- Go to `GET /api/notificationlogs`
- Click "Try it out" then "Execute"
- You'll see the notification attempt logged

## Optional: Configure Email

To actually send emails, edit `appsettings.json`:

```json
"Email": {
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": "587",
  "SmtpUser": "your-email@gmail.com",
  "SmtpPassword": "your-gmail-app-password",
  "FromEmail": "your-email@gmail.com",
  "FromName": "Alert System"
}
```

For Gmail App Passwords: https://support.google.com/accounts/answer/185833

## Optional: Add Slack Notification

1. Create a Slack Incoming Webhook: https://api.slack.com/messaging/webhooks
2. Create a subscription with your webhook:

```json
{
  "userIdentifier": "slack-team",
  "channel": "Slack",
  "destination": "https://hooks.slack.com/services/YOUR/WEBHOOK/URL"
}
```

## That's It!

You now have a fully functional alert notification system running locally.

### What's Happening Behind the Scenes?

1. Quartz.NET background job checks for pending alerts every 30 seconds
2. Finds all matching subscriptions based on alert type filter
3. Uses Strategy Pattern to send via Email or Slack
4. Logs every notification attempt to the database

### Next Steps

- Check out the full README.md for advanced usage
- Explore the API endpoints in Swagger
- Add more subscriptions with different alert type filters
- Try different alert types: BreakingNews, MarketMovement, NaturalDisaster
