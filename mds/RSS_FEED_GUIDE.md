# RSS Feed Integration Guide

## Overview

The Alert Notification System includes automated RSS/Atom feed monitoring that creates alerts from new feed items. This allows you to automatically notify subscribers about breaking news, weather updates, market movements, or any content available via RSS.

## How It Works

1. **Feed Configuration**: Add RSS feeds via the admin UI or API
2. **Background Polling**: A Quartz job checks active feeds every 5 minutes
3. **Alert Creation**: New feed items are converted to alerts with the feed's default type
4. **Automatic Distribution**: Alerts are sent to all matching subscriptions via the normal notification flow

## Adding an RSS Feed

### Via Admin UI

1. Navigate to **RSS Feeds** in the admin menu
2. Click **Add RSS Feed**
3. Fill in the form:
   - **Name**: Descriptive name (e.g., "BBC News", "Weather Alerts")
   - **URL**: Full RSS/Atom feed URL
   - **Default Alert Type**: What type of alerts to create (BreakingNews, MarketMovement, NaturalDisaster)
   - **Check Interval**: How often to poll this feed (default: 15 minutes, global minimum: 5 minutes)
   - **Active**: Enable/disable without deleting
4. Click **Save**

### Via API

```bash
POST /api/rssfeeds
{
  "name": "BBC News",
  "url": "http://feeds.bbci.co.uk/news/rss.xml",
  "defaultAlertType": "BreakingNews",
  "checkIntervalMinutes": 15,
  "isActive": true
}
```

## Popular RSS Feeds to Try

### Testing & Development
- **Lorem RSS (High Volume Test Feed)**: `http://lorem-rss.herokuapp.com/`
  - **Purpose**: Dummy feed for testing, generates 10 new entries per minute
  - **Use Case**: Perfect for testing alert creation, notification delivery, and system performance
  - **Warning**: High volume - use short check intervals (5-10 min) and monitor your notification quotas

### News
- **BBC News**: `http://feeds.bbci.co.uk/news/rss.xml`
- **Reuters Top News**: `https://www.reutersagency.com/feed/`
- **NPR News**: `https://feeds.npr.org/1001/rss.xml`

### Weather
- **NOAA Alerts**: `https://alerts.weather.gov/cap/us.php?x=0` (Atom)
- **Weather Channel**: Various regional feeds available

### Technology
- **Ars Technica**: `https://feeds.arstechnica.com/arstechnica/index`
- **TechCrunch**: `https://techcrunch.com/feed/`
- **Hacker News**: `https://news.ycombinator.com/rss`

### Finance
- **Yahoo Finance**: `https://finance.yahoo.com/news/rssindex`
- **MarketWatch**: `https://www.marketwatch.com/rss/`

## Feed Management

### Monitoring Feed Health

The RSS Feeds page shows:
- **Last Check**: When the feed was last polled
- **Items Processed**: Total number of alerts created from this feed
- **Status**: Active/Inactive
- **Errors**: If a feed fails, the error message is displayed

### Manual Check

Click **Check Now** on any feed to trigger an immediate poll (useful for testing).

### Editing Feeds

1. Click **Edit** on any feed
2. Modify settings (URL, type, interval, etc.)
3. Click **Save**

### Deactivating vs Deleting

- **Deactivate**: Uncheck "Active" to pause polling without losing configuration
- **Delete**: Permanently remove the feed (alerts already created remain)

## How Alerts Are Created

For each new RSS item:
1. **Deduplication**: System checks if an alert already exists for this item (by ID/link)
2. **Time Filter**: Only processes items published after the feed's last check time
3. **Alert Creation**:
   - **Title**: RSS item title
   - **Message**: RSS item summary/description (or link if no description)
   - **Type**: Feed's default alert type
   - **Metadata**: Includes feed info, item link, publish date (JSON)
4. **Status**: Alert starts as "Pending" and enters the normal notification flow

## Background Job Schedule

- **Job**: `RssFeedCheckJob`
- **Frequency**: Every 5 minutes (global setting)
- **Per-Feed Interval**: Configured individually (defaults to 15 minutes)
- **Concurrency**: Disabled (one check at a time)

The global job runs every 5 minutes but only processes feeds whose individual check interval has elapsed.

## Troubleshooting

### Feed Not Creating Alerts

1. **Check "Active" status**: Ensure feed is enabled
2. **Verify URL**: Test the RSS URL in a browser
3. **Check Last Error**: Error messages appear below the feed in the UI
4. **Check Last Check Time**: Ensure the job is running
5. **Publish Dates**: Feeds only create alerts for items newer than the last check

### Common Errors

- **404 Not Found**: Feed URL changed or removed
- **SSL/Certificate Errors**: Feed requires HTTPS with valid cert
- **Timeout**: Feed server slow or unreachable
- **Malformed XML**: Feed doesn't conform to RSS/Atom standards

### Testing

**Quick Test with Lorem RSS:**

1. Add the Lorem RSS feed:
   - Name: "Test Feed"
   - URL: `http://lorem-rss.herokuapp.com/`
   - Alert Type: BreakingNews
   - Check Interval: 5 minutes
2. Click **Check Now** to trigger an immediate check
3. Navigate to **Alerts** - you should see ~10 new alerts created
4. Create a subscription for "BreakingNews" alerts
5. Watch the **Notification Logs** page for deliveries
6. Wait 5-10 minutes and check again - more alerts should appear

**Testing with Real Feeds:**

1. Add a feed with a short check interval (5-10 minutes)
2. Use **Check Now** to test immediately
3. Create a subscription for the feed's alert type
4. Watch the **Notification Logs** page for deliveries

## API Endpoints

```
GET    /api/rssfeeds           - List all feeds
GET    /api/rssfeeds/{id}      - Get feed details
POST   /api/rssfeeds           - Create new feed
PUT    /api/rssfeeds/{id}      - Update feed
DELETE /api/rssfeeds/{id}      - Delete feed
POST   /api/rssfeeds/{id}/check - Trigger manual check
```

## Database Schema

The `RssFeed` table stores:
- `Id`, `Name`, `Url`
- `DefaultAlertType`: Enum converted to string
- `IsActive`: Boolean
- `CheckIntervalMinutes`: Per-feed polling interval
- `LastCheckedAt`: Timestamp of last successful check
- `CreatedAt`: Feed creation time
- `LastError`: Last error message (null if healthy)
- `ItemsProcessed`: Counter of alerts created

## Best Practices

1. **Start with longer intervals** (15-30 min) to avoid overwhelming the system
2. **Use specific alert types** so subscribers can filter effectively
3. **Monitor errors** and disable broken feeds
4. **Test with Lorem RSS first** (`http://lorem-rss.herokuapp.com/`) - generates 10 entries/min for immediate testing
5. **Set up subscriptions** before adding high-volume feeds to see notifications in action
6. **Use "Check Now"** for immediate testing during setup
7. **Watch notification quotas** when using high-volume test feeds

## Integration with Notification Flow

RSS-generated alerts are identical to manually created alerts:
- Same database table (`Alerts`)
- Same processing job (`NotificationJob`)
- Same delivery strategies (Email, Slack)
- Same subscription filtering (by alert type)

The only difference is the `Metadata` field includes RSS source information.

## Future Enhancements

Possible improvements:
- Feed-specific filters (keywords, regex)
- Per-feed rate limiting
- Feed health scoring
- Custom parsing for non-standard feeds
- OPML import/export
- Feed categories
