using AlertNotificationSystem.Models;
using AlertNotificationSystem.Strategies;
using AlertNotificationSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace AlertNotificationSystem.Services;

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

	public async Task ProcessPendingAlertsAsync()
	{
		var pendingAlerts = await _context.Alerts
			.Where(a => a.Status == AlertStatus.Pending)
			.OrderBy(a => a.CreatedAt)
			.ToListAsync();

		_logger.LogInformation("Found {Count} pending alerts to process", pendingAlerts.Count);

		foreach (var alert in pendingAlerts)
		{
			await ProcessAlertAsync(alert);
		}
	}

	public async Task ProcessAlertAsync(Alert alert)
	{
		try
		{
			alert.Status = AlertStatus.Processing;
			await _context.SaveChangesAsync();

			var subscriptions = await _context.Subscriptions
				.Where(s => s.IsActive && (s.AlertTypeFilter == null || s.AlertTypeFilter == alert.Type))
				.ToListAsync();

			_logger.LogInformation("Processing alert {AlertId} for {Count} subscriptions", alert.Id, subscriptions.Count);

			var tasks = new List<Task>();

			foreach (var subscription in subscriptions)
			{
				tasks.Add(SendNotificationAsync(alert, subscription));
			}

			await Task.WhenAll(tasks);

			alert.Status = AlertStatus.Sent;
			alert.ProcessedAt = DateTime.UtcNow;
			await _context.SaveChangesAsync();

			_logger.LogInformation("Alert {AlertId} processed successfully", alert.Id);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to process alert {AlertId}", alert.Id);
			alert.Status = AlertStatus.Failed;
			await _context.SaveChangesAsync();
		}
	}

	private async Task SendNotificationAsync(Alert alert, Subscription subscription)
	{
		var strategy = _strategies.FirstOrDefault(s => s.Channel == subscription.Channel);

		if (strategy == null)
		{
			_logger.LogWarning("No strategy found for channel {Channel}", subscription.Channel);
			await LogNotification(alert.Id, subscription.Id, subscription.Channel, false, "Strategy not found");
			return;
		}

		var success = await strategy.SendNotificationAsync(alert, subscription.Destination);
		await LogNotification(alert.Id, subscription.Id, subscription.Channel, success, success ? null : "Notification failed");
	}

	private async Task LogNotification(int alertId, int subscriptionId, NotificationChannel channel, bool success, string? errorMessage)
	{
		var log = new NotificationLog
		{
			AlertId = alertId,
			SubscriptionId = subscriptionId,
			Channel = channel,
			Success = success,
			ErrorMessage = errorMessage,
			SentAt = DateTime.UtcNow
		};

		_context.NotificationLogs.Add(log);
		await _context.SaveChangesAsync();
	}
}
