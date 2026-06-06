using Quartz;
using AlertNotificationSystem.Services;

namespace AlertNotificationSystem.Jobs;

public class NotificationJob : IJob
{
	private readonly NotificationService _notificationService;
	private readonly ILogger<NotificationJob> _logger;

	public NotificationJob(NotificationService notificationService, ILogger<NotificationJob> logger)
	{
		_notificationService = notificationService;
		_logger = logger;
	}

	public async Task Execute(IJobExecutionContext context)
	{
		_logger.LogInformation("NotificationJob started at {Time}", DateTime.UtcNow);

		try
		{
			await _notificationService.ProcessPendingAlertsAsync();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error occurred while processing notifications");
		}

		_logger.LogInformation("NotificationJob completed at {Time}", DateTime.UtcNow);
	}
}
