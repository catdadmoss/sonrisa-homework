using AlertNotificationSystem.Models;
using Slack.Webhooks;
using Microsoft.Extensions.Configuration;

namespace AlertNotificationSystem.Strategies;

public class SlackNotificationStrategy : INotificationStrategy
{
	private readonly IConfiguration _configuration;
	private readonly ILogger<SlackNotificationStrategy> _logger;

	public NotificationChannel Channel => NotificationChannel.Slack;

	public SlackNotificationStrategy(IConfiguration configuration, ILogger<SlackNotificationStrategy> logger)
	{
		_configuration = configuration;
		_logger = logger;
	}

	public async Task<bool> SendNotificationAsync(Alert alert, string destination)
	{
		try
		{
			var webhookUrl = destination;

			if (string.IsNullOrEmpty(webhookUrl))
			{
				_logger.LogWarning("Slack webhook URL is empty for alert {AlertId}", alert.Id);
				return false;
			}

			var slackClient = new SlackClient(webhookUrl);

			var color = alert.Type switch
			{
				AlertType.BreakingNews => "#FF6B6B",
				AlertType.MarketMovement => "#4ECDC4",
				AlertType.NaturalDisaster => "#FF8C42",
				_ => "#95A5A6"
			};

			var slackMessage = new SlackMessage
			{
				Text = $"*{alert.Type}*: {alert.Title}",
				Attachments = new List<SlackAttachment>
				{
					new SlackAttachment
					{
						Color = color,
						Title = alert.Title,
						Text = alert.Message,
						Fields = new List<SlackField>
						{
							new SlackField
							{
								Title = "Alert Type",
								Value = alert.Type.ToString(),
								Short = true
							},
							new SlackField
							{
								Title = "Time",
								Value = alert.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss UTC"),
								Short = true
							}
						},
						Timestamp = (int)((DateTimeOffset)alert.CreatedAt).ToUnixTimeSeconds()
					}
				}
			};

			var result = await slackClient.PostAsync(slackMessage);

			if (result)
			{
				_logger.LogInformation("Slack notification sent successfully for alert {AlertId}", alert.Id);
			}
			else
			{
				_logger.LogWarning("Slack notification failed for alert {AlertId}", alert.Id);
			}

			return result;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send Slack notification for alert {AlertId}", alert.Id);
			return false;
		}
	}
}
