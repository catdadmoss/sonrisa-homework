using AlertNotificationSystem.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace AlertNotificationSystem.Strategies;

public class EmailNotificationStrategy : INotificationStrategy
{
	private readonly IConfiguration _configuration;
	private readonly ILogger<EmailNotificationStrategy> _logger;

	public NotificationChannel Channel => NotificationChannel.Email;

	public EmailNotificationStrategy(IConfiguration configuration, ILogger<EmailNotificationStrategy> logger)
	{
		_configuration = configuration;
		_logger = logger;
	}

	public async Task<bool> SendNotificationAsync(Alert alert, string destination)
	{
		try
		{
			var smtpHost = _configuration["Email:SmtpHost"];
			var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
			var smtpUser = _configuration["Email:SmtpUser"];
			var smtpPassword = _configuration["Email:SmtpPassword"];
			var fromEmail = _configuration["Email:FromEmail"];
			var fromName = _configuration["Email:FromName"] ?? "Alert Notification System";

			if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser) || 
				string.IsNullOrEmpty(smtpPassword) || string.IsNullOrEmpty(fromEmail))
			{
				_logger.LogWarning("Email configuration is incomplete");
				return false;
			}

			var message = new MimeMessage();
			message.From.Add(new MailboxAddress(fromName, fromEmail));
			message.To.Add(new MailboxAddress("", destination));
			message.Subject = $"[{alert.Type}] {alert.Title}";

			var bodyBuilder = new BodyBuilder
			{
				HtmlBody = $@"
					<h2>{alert.Title}</h2>
					<p><strong>Type:</strong> {alert.Type}</p>
					<p><strong>Time:</strong> {alert.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC</p>
					<hr>
					<p>{alert.Message}</p>
				",
				TextBody = $"{alert.Title}\n\nType: {alert.Type}\nTime: {alert.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC\n\n{alert.Message}"
			};

			message.Body = bodyBuilder.ToMessageBody();

			using var client = new SmtpClient();
			await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
			await client.AuthenticateAsync(smtpUser, smtpPassword);
			await client.SendAsync(message);
			await client.DisconnectAsync(true);

			_logger.LogInformation("Email sent successfully to {Destination} for alert {AlertId}", destination, alert.Id);
			return true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Failed to send email to {Destination} for alert {AlertId}", destination, alert.Id);
			return false;
		}
	}
}
