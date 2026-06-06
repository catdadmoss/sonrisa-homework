# System Design Document
## Alert Notification System

**Version**: 1.0  
**Date**: 2026-06-06  
**Technology**: .NET 8 Web API  
**Status**: Production Ready

---

## Table of Contents
1. [System Overview](#1-system-overview)
2. [Architecture](#2-architecture)
3. [Design Patterns](#3-design-patterns)
4. [Data Model](#4-data-model)
5. [Component Design](#5-component-design)
6. [API Design](#6-api-design)
7. [Security Design](#7-security-design)
8. [Performance & Scalability](#8-performance--scalability)
9. [Technology Stack](#9-technology-stack)
10. [Deployment Architecture](#10-deployment-architecture)

---

## 1. System Overview

### 1.1 Purpose
The Alert Notification System is a .NET 8 Web API designed to manage and distribute alerts across multiple communication channels. It provides a centralized platform for creating, managing, and delivering notifications to subscribed users through their preferred channels.

### 1.2 Key Objectives
- **Multi-Channel Support**: Email, Slack, and extensible to SMS, Push, Teams
- **Asynchronous Processing**: Background job processing for scalability
- **Extensibility**: Easy addition of new notification channels
- **Reliability**: Persistent storage and delivery tracking
- **Developer-Friendly**: RESTful API with Swagger documentation

### 1.3 Scope
- Alert creation and management
- User subscription management
- Multi-channel notification delivery
- Notification history and audit logging
- Admin API for system management

---

## 2. Architecture

### 2.1 High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      Client Layer                            │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐                  │
│  │ Swagger  │  │  HTTP    │  │  Any     │                  │
│  │   UI     │  │  Clients │  │  REST    │                  │
│  │          │  │ (curl,   │  │  Client  │                  │
│  │          │  │ Postman) │  │          │                  │
│  └──────────┘  └──────────┘  └──────────┘                  │
└────────────────────┬────────────────────────────────────────┘
                     │ HTTPS/REST
┌────────────────────▼────────────────────────────────────────┐
│                   API Layer (ASP.NET Core)                   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │   Alerts     │  │Subscriptions │  │Notification  │      │
│  │  Controller  │  │  Controller  │  │     Logs     │      │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘      │
└─────────┼──────────────────┼──────────────────┼─────────────┘
          │                  │                  │
┌─────────▼──────────────────▼──────────────────▼─────────────┐
│                    Service Layer                             │
│  ┌───────────────────────────────────────────────────┐      │
│  │         NotificationService                        │      │
│  │  • Orchestrates notification sending              │      │
│  │  • Manages alert processing lifecycle             │      │
│  │  • Coordinates strategies                         │      │
│  └────┬──────────────────────────────────────────────┘      │
│       │                                                      │
│  ┌────▼──────────────────────────────────────────────┐      │
│  │         Strategy Pattern (INotificationStrategy)   │      │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐        │      │
│  │  │  Email   │  │  Slack   │  │  Future  │        │      │
│  │  │ Strategy │  │ Strategy │  │ Strategy │        │      │
│  │  └──────────┘  └──────────┘  └──────────┘        │      │
│  └───────────────────────────────────────────────────┘      │
└─────────┬────────────────────────────────────────────────────┘
          │
┌─────────▼────────────────────────────────────────────────────┐
│                  Background Jobs (Quartz.NET)                │
│  ┌───────────────────────────────────────────────────┐      │
│  │         NotificationJob                            │      │
│  │  • Runs every 30 seconds (configurable)           │      │
│  │  • Processes pending alerts                       │      │
│  │  • Triggers NotificationService                   │      │
│  └───────────────────────────────────────────────────┘      │
└──────────────────────────────────────────────────────────────┘
          │
┌─────────▼────────────────────────────────────────────────────┐
│              Data Access Layer (EF Core)                     │
│  ┌───────────────────────────────────────────────────┐      │
│  │         ApplicationDbContext                       │      │
│  │  • DbSet<Alert>                                   │      │
│  │  • DbSet<Subscription>                            │      │
│  │  • DbSet<NotificationLog>                         │      │
│  └────┬──────────────────────────────────────────────┘      │
└───────┼──────────────────────────────────────────────────────┘
        │
┌───────▼──────────────────────────────────────────────────────┐
│               Database Layer (SQLite)                        │
│  ┌─────────┐  ┌──────────────┐  ┌──────────────┐           │
│  │ Alerts  │  │Subscriptions │  │Notification  │           │
│  │  Table  │  │    Table     │  │  Logs Table  │           │
│  └─────────┘  └──────────────┘  └──────────────┘           │
└──────────────────────────────────────────────────────────────┘
          │
┌─────────▼────────────────────────────────────────────────────┐
│              External Services                               │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐                  │
│  │  SMTP    │  │  Slack   │  │  Future  │                  │
│  │  Server  │  │ Webhook  │  │ Services │                  │
│  └──────────┘  └──────────┘  └──────────┘                  │
└──────────────────────────────────────────────────────────────┘
```

### 2.2 Architectural Principles

#### Separation of Concerns
- **Controllers**: Handle HTTP requests/responses only
- **Services**: Contain business logic
- **Strategies**: Implement channel-specific notification logic
- **Data Layer**: Manage database operations

#### Dependency Injection
All components use .NET's built-in DI container for loose coupling:
```csharp
// Services registered in Program.cs
builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<INotificationStrategy, EmailNotificationStrategy>();
builder.Services.AddScoped<INotificationStrategy, SlackNotificationStrategy>();
```

#### Single Responsibility Principle
Each class has a single, well-defined purpose:
- `NotificationService` → Orchestrates notification workflow
- `EmailNotificationStrategy` → Sends emails only
- `SlackNotificationStrategy` → Sends Slack messages only
- `NotificationJob` → Triggers background processing

---

## 3. Design Patterns

### 3.1 Strategy Pattern

**Purpose**: Enable runtime selection of notification channels without modifying core logic.

**Implementation**:
```csharp
// Strategy Interface
public interface INotificationStrategy
{
    NotificationChannel Channel { get; }
    Task<bool> SendNotificationAsync(Alert alert, string destination);
}

// Concrete Strategies
public class EmailNotificationStrategy : INotificationStrategy
{
    public NotificationChannel Channel => NotificationChannel.Email;
    // Email-specific implementation
}

public class SlackNotificationStrategy : INotificationStrategy
{
    public NotificationChannel Channel => NotificationChannel.Slack;
    // Slack-specific implementation
}
```

**Benefits**:
- Easy to add new channels (SMS, Teams, Push, etc.)
- No modification to existing code (Open/Closed Principle)
- Testable in isolation
- Runtime channel selection

**Usage Example**:
```csharp
// NotificationService automatically uses correct strategy
var strategy = _strategies.FirstOrDefault(s => s.Channel == subscription.Channel);
await strategy.SendNotificationAsync(alert, subscription.Destination);
```

### 3.2 Repository Pattern (via EF Core)

**Purpose**: Abstract data access and provide a collection-like interface.

**Implementation**:
```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<Alert> Alerts { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<NotificationLog> NotificationLogs { get; set; }
}
```

**Benefits**:
- Database-agnostic (can swap SQLite for SQL Server, PostgreSQL)
- Centralized query logic
- Unit testing support (mock DbContext)

### 3.3 Dependency Injection Pattern

**Purpose**: Achieve loose coupling and testability.

**Implementation**:
```csharp
public class NotificationService
{
    private readonly IEnumerable<INotificationStrategy> _strategies;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IEnumerable<INotificationStrategy> strategies,
        ApplicationDbContext context,
        ILogger<NotificationService> logger)
    {
        _strategies = strategies;
        _context = context;
        _logger = logger;
    }
}
```

**Benefits**:
- Constructor injection for explicit dependencies
- Easy mocking for unit tests
- Centralized configuration in `Program.cs`

### 3.4 Factory Pattern (Implicit via DI)

**Purpose**: Create strategy instances on demand.

**Implementation**: .NET DI container acts as a factory:
```csharp
// DI container creates and injects all registered strategies
IEnumerable<INotificationStrategy> _strategies
```

---

## 4. Data Model

### 4.1 Entity-Relationship Diagram

```
┌────────────────────┐
│      Alert         │
├────────────────────┤
│ Id (PK)            │─┐
│ Type               │ │
│ Title              │ │
│ Message            │ │
│ Status             │ │
│ CreatedAt          │ │
│ ProcessedAt        │ │
│ Metadata           │ │
└────────────────────┘ │
                       │
                       │ 1:N
                       │
         ┌─────────────▼────────────────┐
         │    NotificationLog           │
         ├──────────────────────────────┤
         │ Id (PK)                      │
         │ AlertId (FK) ───────────────┐│
         │ SubscriptionId (FK) ────┐   ││
         │ Channel                 │   ││
         │ Destination             │   ││
         │ Success                 │   ││
         │ ErrorMessage            │   ││
         │ SentAt                  │   ││
         └─────────────────────────┼───┼┘
                           N:1     │   │
                                   │   │
                      ┌────────────▼───┼─────┐
                      │  Subscription      1  │
                      ├────────────────────────┤
                      │ Id (PK)                │
                      │ UserIdentifier         │
                      │ Channel                │
                      │ Destination            │
                      │ AlertTypeFilter        │
                      │ IsActive               │
                      │ CreatedAt              │
                      └────────────────────────┘
```

### 4.2 Entity Descriptions

#### Alert
**Purpose**: Represents a notification event to be sent.

```csharp
public class Alert
{
    public int Id { get; set; }                      // Primary key
    public AlertType Type { get; set; }              // BreakingNews, MarketMovement, NaturalDisaster
    public string Title { get; set; }                // Short headline (max 200 chars)
    public string Message { get; set; }              // Full message body
    public AlertStatus Status { get; set; }          // Pending, Processing, Sent, Failed
    public DateTime CreatedAt { get; set; }          // When alert was created (UTC)
    public DateTime? ProcessedAt { get; set; }       // When processing completed (UTC)
    public string? Metadata { get; set; }            // Optional JSON for extensibility
}
```

**Indexes**:
- `Status` → Fast queries for pending alerts
- `CreatedAt` → Chronological processing

#### Subscription
**Purpose**: Represents a user's notification preferences.

```csharp
public class Subscription
{
    public int Id { get; set; }                      // Primary key
    public string UserIdentifier { get; set; }       // User email, ID, or username
    public NotificationChannel Channel { get; set; } // Email, Slack
    public string Destination { get; set; }          // Email address or webhook URL
    public AlertType? AlertTypeFilter { get; set; }  // null = all types
    public bool IsActive { get; set; }               // Enable/disable subscription
    public DateTime CreatedAt { get; set; }          // When subscription created (UTC)
}
```

**Indexes**:
- `IsActive` → Fast filtering of active subscriptions
- `(UserIdentifier, Channel)` → Prevent duplicates, fast user lookups

#### NotificationLog
**Purpose**: Audit trail of all notification attempts.

```csharp
public class NotificationLog
{
    public int Id { get; set; }                      // Primary key
    public int AlertId { get; set; }                 // Foreign key to Alert
    public int SubscriptionId { get; set; }          // Foreign key to Subscription
    public NotificationChannel Channel { get; set; } // Which channel was used
    public string Destination { get; set; }          // Where it was sent
    public bool Success { get; set; }                // Delivery success/failure
    public string? ErrorMessage { get; set; }        // Error details if failed
    public DateTime SentAt { get; set; }             // When attempt was made (UTC)

    // Navigation properties
    public Alert Alert { get; set; }
    public Subscription Subscription { get; set; }
}
```

**Indexes**:
- `AlertId` → Find all notifications for an alert
- `SubscriptionId` → Find all notifications for a subscription
- `SentAt` → Chronological queries

### 4.3 Enumerations

```csharp
public enum AlertType
{
    BreakingNews,      // Urgent news updates
    MarketMovement,    // Financial market alerts
    NaturalDisaster    // Weather/disaster warnings
}

public enum NotificationChannel
{
    Email,             // SMTP email
    Slack              // Slack webhook
    // Future: SMS, Push, Teams, Discord, etc.
}

public enum AlertStatus
{
    Pending,           // Created, awaiting processing
    Processing,        // Currently being sent
    Sent,              // Successfully delivered to all subscribers
    Failed             // Delivery failed
}
```

### 4.4 Database Constraints

**Alert Table**:
- `Title`: Required, Max 200 characters
- `Message`: Required
- `Status`: Default = Pending
- `CreatedAt`: Default = UTC Now

**Subscription Table**:
- `UserIdentifier`: Required, Max 100 characters
- `Destination`: Required, Max 255 characters
- `IsActive`: Default = true
- `CreatedAt`: Default = UTC Now

**NotificationLog Table**:
- `AlertId`: Required, Cascade delete
- `SubscriptionId`: Required, Cascade delete
- `SentAt`: Default = UTC Now

---

## 5. Component Design

### 5.1 Controllers Layer

#### AlertsController
**Responsibilities**:
- HTTP request/response handling
- Input validation
- DTO transformation
- Delegating to service layer

**Key Endpoints**:
```csharp
GET    /api/alerts              // List all alerts
GET    /api/alerts/{id}         // Get single alert
POST   /api/alerts              // Create new alert
PUT    /api/alerts/{id}         // Update alert
DELETE /api/alerts/{id}         // Delete alert
GET    /api/alerts/statistics   // Get statistics
```

**Design Decisions**:
- RESTful conventions
- Async/await for all operations
- Standard HTTP status codes (200, 201, 404, 400, 500)
- No business logic in controller

#### SubscriptionsController
**Responsibilities**:
- Manage user subscriptions
- Filter by user or channel
- CRUD operations

**Key Endpoints**:
```csharp
GET    /api/subscriptions                      // List all
GET    /api/subscriptions/{id}                 // Get by ID
GET    /api/subscriptions/user/{identifier}    // Get user's subscriptions
POST   /api/subscriptions                      // Create
PUT    /api/subscriptions/{id}                 // Update
DELETE /api/subscriptions/{id}                 // Delete
```

#### NotificationLogsController
**Responsibilities**:
- Read-only access to logs
- Filtering by alert or subscription

**Key Endpoints**:
```csharp
GET /api/notificationlogs                      // List all logs
GET /api/notificationlogs/alert/{id}           // Logs for alert
GET /api/notificationlogs/subscription/{id}    // Logs for subscription
```

### 5.2 Service Layer

#### NotificationService
**Purpose**: Core business logic for notification processing.

**Key Methods**:
```csharp
Task ProcessPendingAlertsAsync()
// - Queries all pending alerts
// - Processes each sequentially
// - Updates alert status

Task ProcessAlertAsync(Alert alert)
// - Finds active subscriptions (with optional filtering)
// - Sends via appropriate strategy
// - Logs each attempt
// - Updates alert status

Task SendNotificationAsync(Alert alert, Subscription subscription)
// - Selects correct strategy by channel
// - Calls strategy's SendNotificationAsync
// - Creates NotificationLog entry
// - Handles errors gracefully
```

**State Management**:
```csharp
Alert.Status Lifecycle:
Pending → Processing → Sent (all successful)
Pending → Processing → Failed (any failure)
```

**Error Handling**:
- Try/catch around each notification
- Log errors but continue processing
- Update alert status appropriately
- Create log entry even on failure

### 5.3 Strategy Layer

#### INotificationStrategy Interface
```csharp
public interface INotificationStrategy
{
    NotificationChannel Channel { get; }
    Task<bool> SendNotificationAsync(Alert alert, string destination);
}
```

#### EmailNotificationStrategy
**Technology**: MailKit (modern, cross-platform SMTP library)

**Configuration**:
```csharp
_configuration["Email:SmtpHost"]      // smtp.gmail.com
_configuration["Email:SmtpPort"]      // 587 (TLS)
_configuration["Email:SmtpUser"]      // account@gmail.com
_configuration["Email:SmtpPassword"]  // App Password
_configuration["Email:FromEmail"]     // sender address
_configuration["Email:FromName"]      // display name
```

**Message Format**:
- **Subject**: `[{AlertType}] {Title}`
- **HTML Body**: Formatted with alert details
- **Plain Text**: Fallback for non-HTML clients

**Error Handling**:
- SMTP connection failures
- Authentication errors
- Invalid email addresses
- Timeout issues

#### SlackNotificationStrategy
**Technology**: Slack.Webhooks NuGet package

**Configuration**:
- Webhook URL stored in `Subscription.Destination`

**Message Format**:
```json
{
  "text": "🚨 {AlertType}: {Title}",
  "blocks": [
    {
      "type": "section",
      "text": { "type": "mrkdwn", "text": "{Message}" }
    }
  ]
}
```

**Error Handling**:
- Invalid webhook URL
- Network failures
- Slack API errors (rate limits, deleted webhooks)

### 5.4 Background Jobs

#### NotificationJob (Quartz.NET)
**Purpose**: Scheduled background processing of pending alerts.

**Schedule**: Every 30 seconds (configurable in `Program.cs`)

**Implementation**:
```csharp
public async Task Execute(IJobExecutionContext context)
{
    _logger.LogInformation("NotificationJob started");

    try
    {
        await _notificationService.ProcessPendingAlertsAsync();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error processing notifications");
    }

    _logger.LogInformation("NotificationJob completed");
}
```

**Design Decisions**:
- Hosted service (runs with application lifecycle)
- Scoped dependencies (new DbContext per execution)
- Graceful error handling (log but don't crash)
- Configurable interval via `WithIntervalInSeconds(30)`

**Scalability Considerations**:
- Single instance processes alerts to avoid duplicates
- For multi-instance, use distributed locking (Redis, SQL)
- Quartz.NET supports clustered mode with shared state

---

## 6. API Design

### 6.1 RESTful Principles

**Resource-Based URLs**:
```
/api/alerts              // Alert collection
/api/alerts/{id}         // Single alert
/api/subscriptions       // Subscription collection
/api/notificationlogs    // Log collection
```

**HTTP Verbs**:
- `GET` → Retrieve (idempotent)
- `POST` → Create (non-idempotent)
- `PUT` → Update (idempotent)
- `DELETE` → Remove (idempotent)

**Status Codes**:
- `200 OK` → Successful GET/PUT/DELETE
- `201 Created` → Successful POST
- `204 No Content` → Successful DELETE (optional)
- `400 Bad Request` → Validation errors
- `404 Not Found` → Resource doesn't exist
- `500 Internal Server Error` → Server-side failures

### 6.2 Request/Response Format

**Content Type**: `application/json`

**Create Alert Request**:
```json
POST /api/alerts
{
  "type": "BreakingNews",
  "title": "Major Event Occurring",
  "message": "Detailed information about the event...",
  "metadata": "{\"source\":\"system\",\"priority\":1}"
}
```

**Create Alert Response** (201 Created):
```json
{
  "id": 42,
  "type": "BreakingNews",
  "title": "Major Event Occurring",
  "message": "Detailed information about the event...",
  "status": "Pending",
  "createdAt": "2024-01-15T10:30:00Z",
  "processedAt": null,
  "metadata": "{\"source\":\"system\",\"priority\":1}"
}
```

**Create Subscription Request**:
```json
POST /api/subscriptions
{
  "userIdentifier": "john.doe@example.com",
  "channel": "Email",
  "destination": "john.doe@example.com",
  "alertTypeFilter": "BreakingNews"
}
```

### 6.3 Filtering & Pagination

**Statistics Endpoint**:
```csharp
GET /api/alerts/statistics

Response:
{
  "totalAlerts": 1250,
  "pendingAlerts": 5,
  "sentAlerts": 1200,
  "failedAlerts": 45,
  "byType": {
    "BreakingNews": 500,
    "MarketMovement": 400,
    "NaturalDisaster": 350
  }
}
```

**User Subscriptions**:
```csharp
GET /api/subscriptions/user/{userIdentifier}

Response: Subscription[]
```

### 6.4 Error Handling

**Validation Error**:
```json
{
  "error": "Validation failed",
  "details": {
    "title": ["Title is required"],
    "destination": ["Invalid email format"]
  }
}
```

**Not Found**:
```json
{
  "error": "Alert with ID 999 not found"
}
```

### 6.5 Swagger/OpenAPI

**URL**: `/swagger`

**Features**:
- Interactive API testing
- Request/response schemas
- Endpoint descriptions
- Try-it-out functionality

**Configuration** (Program.cs):
```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Alert Notification System", 
        Version = "v1" 
    });
});
```

---

## 7. Security Design

### 7.1 Configuration Security

**User Secrets (Development)**:
```
Location: %APPDATA%\Microsoft\UserSecrets\{UserSecretsId}\secrets.json
Purpose: Store credentials outside source control
Usage: Automatic in Development environment
```

**Environment Variables (Production)**:
```csharp
Environment.GetEnvironmentVariable("EMAIL_SMTP_PASSWORD")
```

**Azure Key Vault (Recommended for Production)**:
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri(keyVaultEndpoint),
    new DefaultAzureCredential());
```

### 7.2 HTTPS Configuration

**Development**:
- Port 5000 (HTTP) → Redirects to 5001 (HTTPS)
- Port 5001 (HTTPS) → Development certificate required

**Production**:
- Force HTTPS via `app.UseHttpsRedirection()`
- Use reverse proxy (Nginx, IIS) for TLS termination

### 7.3 Input Validation

**Model Validation**:
```csharp
[Required]
[MaxLength(200)]
public string Title { get; set; }

[Required]
[EmailAddress]
public string Destination { get; set; }
```

**Controller Validation**:
```csharp
if (!ModelState.IsValid)
    return BadRequest(ModelState);
```

### 7.4 SQL Injection Prevention

**Entity Framework Core** parameterizes all queries automatically:
```csharp
// Safe - EF Core uses parameters
var alert = await _context.Alerts
    .FirstOrDefaultAsync(a => a.Id == id);
```

### 7.5 Secrets Management

**Never Commit**:
- ❌ SMTP passwords
- ❌ API keys
- ❌ Connection strings with credentials

**Safe to Commit**:
- ✅ `appsettings.json` with placeholders
- ✅ `.csproj` with `UserSecretsId`
- ✅ Configuration structure

### 7.6 Future Security Enhancements

**Authentication**:
- JWT bearer tokens
- API keys
- OAuth 2.0 / OpenID Connect

**Authorization**:
- Role-based (Admin, User)
- Claim-based permissions
- Subscription ownership validation

**Rate Limiting**:
- Per-user rate limits
- Global rate limits
- Distributed rate limiting (Redis)

---

## 8. Performance & Scalability

### 8.1 Performance Characteristics

**Database Queries**:
- Indexed columns: Status, CreatedAt, IsActive
- Composite index: (UserIdentifier, Channel)
- EF Core query optimization with `.AsNoTracking()` for read-only

**Asynchronous Processing**:
- All I/O operations are async (DB, SMTP, HTTP)
- Non-blocking background jobs
- Parallel notification sending within alerts

**Caching Opportunities** (Future):
```csharp
// Cache active subscriptions
IMemoryCache _cache.GetOrCreateAsync("active-subscriptions", ...)
```

### 8.2 Scalability Design

**Horizontal Scaling**:
```
Current: Single instance
Future: Multiple instances with:
  - Shared database
  - Distributed locks (Redis) for job coordination
  - Quartz.NET clustered mode
```

**Vertical Scaling**:
- Increase CPU for parallel notification sending
- Increase memory for caching subscriptions
- Increase database IOPS

**Queue-Based Architecture** (Future Enhancement):
```
Producer (API) → Message Queue (RabbitMQ/Azure Service Bus) → Consumer (Workers)
Benefits:
  - Decoupled components
  - Better load distribution
  - Retry mechanisms
  - Dead-letter queues
```

### 8.3 Database Optimization

**SQLite → Production Database Migration**:
```csharp
// Development
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Production
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString) 
           // or options.UseNpgsql(connectionString)
);
```

**Connection Pooling**:
- Enabled by default in EF Core
- Configurable via connection string

**Query Optimization**:
```csharp
// Avoid N+1 queries
var logs = await _context.NotificationLogs
    .Include(l => l.Alert)
    .Include(l => l.Subscription)
    .ToListAsync();
```

### 8.4 Monitoring & Metrics

**Application Insights** (Azure):
```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

**Metrics to Track**:
- Alerts created per minute
- Notifications sent per minute
- Average processing time
- Success/failure rates
- Background job execution time

**Logging Levels**:
- Information: Job start/stop, notifications sent
- Warning: SMTP failures, missing configuration
- Error: Unhandled exceptions
- Debug: Strategy selection, subscription filtering

---

## 9. Technology Stack

### 9.1 Core Framework

**ASP.NET Core 8.0**
- **Why**: Modern, cross-platform, high-performance
- **Alternatives Considered**: Node.js (rejected - team .NET expertise)

**Entity Framework Core 8.0**
- **Why**: ORM with LINQ, migrations, database-agnostic
- **Alternatives Considered**: Dapper (rejected - needed migrations)

### 9.2 Database

**SQLite (Development/Small Scale)**
- **Pros**: Zero configuration, portable, file-based
- **Cons**: Limited concurrency, not for high-scale production
- **Production Alternative**: SQL Server, PostgreSQL, MySQL

### 9.3 Background Jobs

**Quartz.NET 3.8.0**
- **Why**: Mature, feature-rich, supports clustering
- **Alternatives Considered**: Hangfire (similar, chose Quartz for flexibility)

### 9.4 Notification Libraries

**MailKit 4.16.0**
- **Why**: Modern, secure, supports SMTP/IMAP/POP3
- **Alternatives Considered**: SmtpClient (deprecated), SendGrid SDK (requires account)

**Slack.Webhooks 1.1.5**
- **Why**: Simple, webhooks-based (no OAuth needed)
- **Alternatives Considered**: Slack.NET (overkill for webhook-only)

### 9.5 API Documentation

**Swashbuckle.AspNetCore 6.5.0**
- **Why**: Industry standard, Swagger/OpenAPI compliance
- **Alternatives Considered**: NSwag (similar, chose Swashbuckle for simplicity)

### 9.6 Dependency Tree

```
AlertNotificationSystem
├── Microsoft.NET.Sdk.Web (8.0)
│   ├── ASP.NET Core Runtime
│   └── Kestrel Web Server
├── Microsoft.EntityFrameworkCore.Sqlite (8.0.0)
│   └── SQLite Native Library
├── Microsoft.EntityFrameworkCore.Design (8.0.0)
│   └── EF Core Tools
├── Quartz (3.8.0)
│   └── Quartz.Extensions.Hosting (3.8.0)
├── MailKit (4.16.0)
│   └── MimeKit
├── Slack.Webhooks (1.1.5)
├── Swashbuckle.AspNetCore (6.5.0)
└── Security Updates (via NuGet)
    ├── Newtonsoft.Json (13.0.1)
    ├── System.Net.Http (4.3.4)
    ├── System.Text.Json (8.0.5)
    └── Others (see PROJECT_DELIVERABLES.md)
```

---

## 10. Deployment Architecture

### 10.1 Development Environment

```
Developer Machine
├── Visual Studio 2026
├── .NET 8 SDK
├── SQLite (file: alerts.db)
├── User Secrets (credentials)
└── HTTPS Dev Certificate
```

**Startup**:
```bash
dotnet restore
dotnet run
# → https://localhost:5001
# → Swagger UI at /swagger
```

### 10.2 Production Deployment Options

#### Option A: Azure App Service
```
Azure Resources:
├── App Service (Linux/Windows)
│   ├── .NET 8 Runtime
│   ├── Environment Variables (secrets)
│   └── Application Insights
├── Azure SQL Database
│   └── Connection String in Key Vault
└── Azure Key Vault
    ├── SMTP Password
    └── Database Connection String
```

**Benefits**: Managed service, auto-scaling, built-in monitoring

#### Option B: Docker Container
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY ./publish .
ENTRYPOINT ["dotnet", "AlertNotificationSystem.dll"]
```

**Kubernetes Deployment**:
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: alert-notification-system
spec:
  replicas: 3
  template:
    spec:
      containers:
      - name: api
        image: alert-notification-system:latest
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: db-secret
              key: connection-string
```

#### Option C: IIS (Windows Server)
```
IIS Configuration:
├── Application Pool (.NET CLR: No Managed Code)
├── Site Bindings (HTTPS:443)
├── Environment Variables (System or App Pool level)
└── SQL Server (local or remote)
```

### 10.3 CI/CD Pipeline

**GitHub Actions Example**:
```yaml
name: Build and Deploy

on:
  push:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
    - name: Restore
      run: dotnet restore
    - name: Build
      run: dotnet build --no-restore
    - name: Test
      run: dotnet test --no-build
    - name: Publish
      run: dotnet publish -c Release -o ./publish
    - name: Deploy to Azure
      uses: azure/webapps-deploy@v2
      with:
        app-name: alert-notification-api
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./publish
```

### 10.4 Environment Configuration

**Development** (`appsettings.Development.json`):
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com"
  }
}
```

**Production** (`appsettings.Production.json`):
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "#{DatabaseConnectionString}#"  // Replaced in CI/CD
  }
}
```

**Environment Variables** (Production):
```bash
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection="Server=...;Database=...;User=...;Password=..."
Email__SmtpPassword="..."
```

### 10.5 Health Checks

**Future Enhancement**:
```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>()
    .AddSmtpHealthCheck(smtpOptions => { ... });

app.MapHealthChecks("/health");
```

**Monitoring Endpoint**: `/health`
- Database connectivity
- External service availability
- Disk space, memory usage

---

## 11. Future Enhancements

### 11.1 Near Term
- [ ] **SMS Notifications** via Twilio
- [ ] **Microsoft Teams** integration
- [ ] **Push Notifications** (FCM, APNs)
- [ ] **Rate Limiting** per user/IP
- [ ] **Authentication** (JWT/API keys)

### 11.2 Medium Term
- [ ] **Message Templates** with placeholders
- [ ] **Scheduling** (send alerts at specific time)
- [ ] **User Preferences** (quiet hours, frequency limits)
- [ ] **Alert Prioritization** (urgent, normal, low)
- [ ] **Retry Logic** for failed notifications

### 11.3 Long Term
- [ ] **Multi-Tenant Support**
- [ ] **Analytics Dashboard**
- [ ] **Webhook Callbacks** for delivery status
- [ ] **A/B Testing** for message content
- [ ] **Machine Learning** for optimal send times

---

## 12. Conclusion

The Alert Notification System demonstrates a well-architected, production-ready solution using modern .NET 8 practices. Key design strengths:

✅ **Extensibility**: Strategy pattern enables easy addition of notification channels  
✅ **Scalability**: Asynchronous design with background jobs  
✅ **Maintainability**: Clean separation of concerns, SOLID principles  
✅ **Security**: User Secrets, HTTPS, no hardcoded credentials  
✅ **Reliability**: Comprehensive logging, error handling, audit trail  
✅ **Developer Experience**: Swagger UI, clear documentation, scripts  

The architecture supports growth from a simple notification service to a high-scale, multi-tenant enterprise platform with minimal refactoring required.

---

**Document Version**: 1.0  
**Last Updated**: 2024  
**Authors**: Development Team  
**Status**: Production Ready
