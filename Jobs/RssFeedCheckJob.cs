using Quartz;
using AlertNotificationSystem.Services;

namespace AlertNotificationSystem.Jobs;

[DisallowConcurrentExecution]
public class RssFeedCheckJob : IJob
{
	private readonly RssFeedService _rssFeedService;
	private readonly ILogger<RssFeedCheckJob> _logger;

	public RssFeedCheckJob(RssFeedService rssFeedService, ILogger<RssFeedCheckJob> logger)
	{
		_rssFeedService = rssFeedService;
		_logger = logger;
	}

	public async Task Execute(IJobExecutionContext context)
	{
		_logger.LogInformation("Starting RSS feed check job");

		try
		{
			await _rssFeedService.CheckFeedsAsync();
			_logger.LogInformation("RSS feed check job completed successfully");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "RSS feed check job failed: {Error}", ex.Message);
			throw;
		}
	}
}
