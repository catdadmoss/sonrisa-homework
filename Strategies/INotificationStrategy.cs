using AlertNotificationSystem.Models;

namespace AlertNotificationSystem.Strategies;

public interface INotificationStrategy
{
	NotificationChannel Channel { get; }
	Task<bool> SendNotificationAsync(Alert alert, string destination);
}
