# Alert Notification System

> **📋 Submission Note**: This project was built from an intentionally vague brief as an AI-assisted development exercise.  
> **Start here for evaluation**: [SUBMISSION_OVERVIEW.md](SUBMISSION_OVERVIEW.md) | [DECISION_LOG.md](DECISION_LOG.md)

A .NET 8 Web API with Blazor Server admin interface for managing and distributing alert notifications across multiple channels (Email, Slack) with extensibility for additional channels.

## 📚 Documentation Index

### For Evaluators
- **[SUBMISSION_OVERVIEW.md](SUBMISSION_OVERVIEW.md)** - Submission summary and navigation
- **[DECISION_LOG.md](DECISION_LOG.md)** ⭐ - Complete process documentation and AI analysis
- **[PROMPT_HISTORY.md](PROMPT_HISTORY.md)** - Conversation history with GitHub Copilot
- **[SUBMISSION_CHECKLIST.md](SUBMISSION_CHECKLIST.md)** - Pre-submission validation

### Technical Documentation
- **[PROJECT_DELIVERABLES.md](PROJECT_DELIVERABLES.md)** - Project scope and deliverables
- **[SYSTEM_DESIGN.md](SYSTEM_DESIGN.md)** - Architecture and design decisions
- **[GETTING_STARTED.md](GETTING_STARTED.md)** - Detailed setup instructions
- **[BLAZOR_UI_GUIDE.md](BLAZOR_UI_GUIDE.md)** - Admin interface walkthrough
- **[RSS_FEED_GUIDE.md](RSS_FEED_GUIDE.md)** - RSS automation feature guide
- **[USER_SECRETS_SETUP.md](USER_SECRETS_SETUP.md)** - Secure credential configuration

## Features

- **Blazor Admin UI**: Modern web-based admin dashboard
- **RSS Feed Integration**: Automatically create alerts from RSS/Atom feeds
- **Multiple Alert Types**: Breaking News, Market Movements, Natural Disasters
- **Multi-Channel Delivery**: Email (SMTP) and Slack webhooks
- **Strategy Pattern**: Easy to extend with new notification channels
- **Background Processing**: Quartz.NET for async alert processing and RSS polling
- **REST API**: Full CRUD operations via API endpoints
- **SQLite Database**: Lightweight, zero-configuration database
- **Swagger UI**: Interactive API documentation

## Technology Stack (100% Free)

- .NET 8
- Blazor Server
- Entity Framework Core + SQLite
- Quartz.NET (background jobs)
- MailKit (email)
- Slack.Webhooks
- Swashbuckle (API docs)

## Quick Start

### 1. Configure Email (Optional but recommended)

Use **User Secrets** for secure credential storage:

```bash
dotnet user-secrets set "Email:SmtpUser" "your-email@gmail.com"
dotnet user-secrets set "Email:SmtpPassword" "your-app-password"
dotnet user-secrets set "Email:FromEmail" "your-email@gmail.com"
```

Or run the setup script: `.\setup-user-secrets-manual.ps1`

**For Gmail**: Use an [App Password](https://support.google.com/accounts/answer/185833)

### 2. Run the Application

```bash
dotnet restore
dotnet run
```

The application will start at `https://localhost:5001`

### 3. Access the Admin Dashboard

Navigate to: **`https://localhost:5001`** (Blazor UI)

Or use Swagger for API: **`https://localhost:5001/swagger`**

## Admin Dashboard

The Blazor UI provides:

- **Dashboard**: View statistics (total alerts, pending, sent, failed)
- **Alerts Management**: Create, edit, delete alerts
- **Subscriptions**: Manage user notification subscriptions
- **Notification Logs**: View delivery history and errors
- **RSS Feeds**: Subscribe to RSS/Atom feeds that automatically generate alerts
- **Quick Actions**: Direct links to common tasks

### RSS Feed Feature

Subscribe to news, weather, or any RSS/Atom feed and automatically create alerts:

1. Navigate to **RSS Feeds** in the admin menu
2. Click **Add RSS Feed**
3. Enter feed details:
   - Name (e.g., "BBC News")
   - Feed URL (e.g., "http://feeds.bbci.co.uk/news/rss.xml")
   - Default Alert Type (BreakingNews, MarketMovement, or NaturalDisaster)
   - Check Interval (how often to poll, in minutes)
4. The background job will check feeds every 5 minutes and create alerts from new items
5. Alerts are then distributed to all subscriptions matching the alert type

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
