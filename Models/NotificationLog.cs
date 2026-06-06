namespace AlertNotificationSystem.Models;

public class NotificationLog
{
	public int Id { get; set; }
	public int AlertId { get; set; }
	public int SubscriptionId { get; set; }
	public NotificationChannel Channel { get; set; }
	public bool Success { get; set; }
	public string? ErrorMessage { get; set; }
	public DateTime SentAt { get; set; } = DateTime.UtcNow;

	public Alert? Alert { get; set; }
	public Subscription? Subscription { get; set; }
}
