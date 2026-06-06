namespace AlertNotificationSystem.Models;

public class Subscription
{
	public int Id { get; set; }
	public string UserIdentifier { get; set; } = string.Empty;
	public NotificationChannel Channel { get; set; }
	public string Destination { get; set; } = string.Empty;
	public AlertType? AlertTypeFilter { get; set; }
	public bool IsActive { get; set; } = true;
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
