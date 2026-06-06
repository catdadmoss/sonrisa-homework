# Alert Notification System

A .NET 8 Web API for managing and distributing alert notifications across multiple channels (Email, Slack) with extensibility for additional channels.

## Features

- **Multiple Alert Types**: Breaking News, Market Movements, Natural Disasters
- **Multi-Channel Delivery**: Email (SMTP) and Slack webhooks
- **Strategy Pattern**: Easy to extend with new notification channels
- **Background Processing**: Quartz.NET for async alert processing
- **Admin API**: Full CRUD operations via REST API
- **SQLite Database**: Lightweight, zero-configuration database
- **Swagger UI**: Interactive API documentation

## Technology Stack (100% Free)

- .NET 8
- Entity Framework Core + SQLite
- Quartz.NET (background jobs)
- MailKit (email)
- Slack.Webhooks
- Swashbuckle (API docs)

## Quick Start

### 1. Configure Email (Optional but recommended)

Edit `appsettings.json` and update the Email section:

```json
"Email": {
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": "587",
  "SmtpUser": "your-email@gmail.com",
  "SmtpPassword": "your-app-password",
  "FromEmail": "your-email@gmail.com",
  "FromName": "Alert Notification System"
}
```

**For Gmail**: Use an [App Password](https://support.google.com/accounts/answer/185833)

### 2. Run the Application

```bash
dotnet restore
dotnet run
```

The API will start at `https://localhost:5001` (or check console output)

### 3. Access Swagger UI

Navigate to: `https://localhost:5001/swagger`

## API Usage

### Create a Subscription

```bash
POST /api/subscriptions
{
  "userIdentifier": "john@example.com",
  "channel": "Email",
  "destination": "john@example.com",
  "alertTypeFilter": null
}
```

**Channels**: `Email` | `Slack`  
**Alert Types**: `BreakingNews` | `MarketMovement` | `NaturalDisaster`

For Slack, use:
```json
{
  "userIdentifier": "team-alerts",
  "channel": "Slack",
  "destination": "https://hooks.slack.com/services/YOUR/WEBHOOK/URL"
}
```

### Create an Alert

```bash
POST /api/alerts
{
  "type": "BreakingNews",
  "title": "Major Market Event",
  "message": "Stock market experiences significant movement...",
  "metadata": null
}
```

The alert will be automatically processed within 30 seconds by the background job and sent to all matching subscriptions.

### View Alerts

```bash
GET /api/alerts
GET /api/alerts/{id}
GET /api/alerts?type=BreakingNews&status=Sent
GET /api/alerts/statistics
```

### View Notification Logs

```bash
GET /api/notificationlogs
GET /api/notificationlogs?alertId=1
```

## How It Works

1. **Create Alert**: POST to `/api/alerts` creates an alert with status `Pending`
2. **Background Job**: Quartz.NET job runs every 30 seconds
3. **Match Subscriptions**: System finds all active subscriptions matching the alert type
4. **Send Notifications**: Strategy pattern selects appropriate channel (Email/Slack)
5. **Log Results**: All attempts logged to `NotificationLogs` table

## Adding a New Notification Channel

1. Implement `INotificationStrategy` interface
2. Register in `Program.cs` dependency injection
3. Add new enum value to `NotificationChannel`

Example:
```csharp
public class SmsNotificationStrategy : INotificationStrategy
{
    public NotificationChannel Channel => NotificationChannel.Sms;

    public async Task<bool> SendNotificationAsync(Alert alert, string destination)
    {
        // Your SMS implementation
    }
}
```

## Database

SQLite database (`alerts.db`) is created automatically on first run. Located in project root.

### Tables
- **Alerts**: All alert records
- **Subscriptions**: User notification preferences
- **NotificationLogs**: Delivery history

## Configuration

Edit `appsettings.json`:

- **Background Job Interval**: Modify `Program.cs` line with `.WithIntervalInSeconds(30)`
- **Database Path**: Modify `ConnectionStrings:DefaultConnection`
- **Email Settings**: Update `Email` section

## Production Considerations

- Add authentication/authorization (JWT, API keys)
- Use a production database (SQL Server, PostgreSQL)
- Configure rate limiting
- Add retry logic for failed notifications
- Implement proper error handling and monitoring
- Secure sensitive configuration (Azure Key Vault, AWS Secrets Manager)

## License

MIT
