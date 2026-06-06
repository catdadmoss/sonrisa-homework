namespace AlertNotificationSystem.Models;

public class Alert
{
	public int Id { get; set; }
	public AlertType Type { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Message { get; set; } = string.Empty;
	public AlertStatus Status { get; set; } = AlertStatus.Pending;
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public DateTime? ProcessedAt { get; set; }
	public string? Metadata { get; set; }
}
