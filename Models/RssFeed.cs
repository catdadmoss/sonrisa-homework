namespace AlertNotificationSystem.Models;

public class RssFeed
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public string Url { get; set; } = string.Empty;
	public AlertType DefaultAlertType { get; set; } = AlertType.BreakingNews;
	public bool IsActive { get; set; } = true;
	public int CheckIntervalMinutes { get; set; } = 15;
	public DateTime? LastCheckedAt { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	public string? LastError { get; set; }
	public int ItemsProcessed { get; set; } = 0;
}
