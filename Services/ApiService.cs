using AlertNotificationSystem.Models;
using System.Net.Http.Json;

namespace AlertNotificationSystem.Services;

public class ApiService
{
	private readonly HttpClient _httpClient;
	private readonly ILogger<ApiService> _logger;

	public ApiService(IHttpClientFactory httpClientFactory, ILogger<ApiService> logger)
	{
		_httpClient = httpClientFactory.CreateClient("API");
		_logger = logger;
	}

	// Alerts
	public async Task<List<Alert>> GetAlertsAsync()
	{
		try
		{
			return await _httpClient.GetFromJsonAsync<List<Alert>>("api/alerts") ?? new List<Alert>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error fetching alerts");
			return new List<Alert>();
		}
	}

	public async Task<Alert?> GetAlertAsync(int id)
	{
		try
		{
			return await _httpClient.GetFromJsonAsync<Alert>($"api/alerts/{id}");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error fetching alert {Id}", id);
			return null;
		}
	}

	public async Task<bool> CreateAlertAsync(Alert alert)
	{
		try
		{
			var response = await _httpClient.PostAsJsonAsync("api/alerts", alert);
			return response.IsSuccessStatusCode;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating alert");
			return false;
		}
	}

	public async Task<bool> UpdateAlertAsync(int id, Alert alert)
	{
		try
		{
			var response = await _httpClient.PutAsJsonAsync($"api/alerts/{id}", alert);
			return response.IsSuccessStatusCode;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating alert {Id}", id);
			return false;
		}
	}

	public async Task<bool> DeleteAlertAsync(int id)
	{
		try
		{
			var response = await _httpClient.DeleteAsync($"api/alerts/{id}");
			return response.IsSuccessStatusCode;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting alert {Id}", id);
			return false;
		}
	}

	public async Task<object?> GetAlertStatisticsAsync()
	{
		try
		{
			return await _httpClient.GetFromJsonAsync<object>("api/alerts/statistics");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error fetching alert statistics");
			return null;
		}
	}

	// Subscriptions
	public async Task<List<Subscription>> GetSubscriptionsAsync()
	{
		try
		{
			return await _httpClient.GetFromJsonAsync<List<Subscription>>("api/subscriptions") ?? new List<Subscription>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error fetching subscriptions");
			return new List<Subscription>();
		}
	}

	public async Task<Subscription?> GetSubscriptionAsync(int id)
	{
		try
		{
			return await _httpClient.GetFromJsonAsync<Subscription>($"api/subscriptions/{id}");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error fetching subscription {Id}", id);
			return null;
		}
	}

	public async Task<bool> CreateSubscriptionAsync(Subscription subscription)
	{
		try
		{
			var response = await _httpClient.PostAsJsonAsync("api/subscriptions", subscription);
			return response.IsSuccessStatusCode;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error creating subscription");
			return false;
		}
	}

	public async Task<bool> UpdateSubscriptionAsync(int id, Subscription subscription)
	{
		try
		{
			var response = await _httpClient.PutAsJsonAsync($"api/subscriptions/{id}", subscription);
			return response.IsSuccessStatusCode;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error updating subscription {Id}", id);
			return false;
		}
	}

	public async Task<bool> DeleteSubscriptionAsync(int id)
	{
		try
		{
			var response = await _httpClient.DeleteAsync($"api/subscriptions/{id}");
			return response.IsSuccessStatusCode;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error deleting subscription {Id}", id);
			return false;
		}
	}

	// Notification Logs
	public async Task<List<NotificationLog>> GetNotificationLogsAsync()
	{
		try
		{
			return await _httpClient.GetFromJsonAsync<List<NotificationLog>>("api/notificationlogs") ?? new List<NotificationLog>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error fetching notification logs");
			return new List<NotificationLog>();
		}
	}

	public async Task<List<NotificationLog>> GetLogsByAlertAsync(int alertId)
	{
		try
		{
			return await _httpClient.GetFromJsonAsync<List<NotificationLog>>($"api/notificationlogs/alert/{alertId}") ?? new List<NotificationLog>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error fetching logs for alert {AlertId}", alertId);
			return new List<NotificationLog>();
		}
	}

	public async Task<List<NotificationLog>> GetLogsBySubscriptionAsync(int subscriptionId)
	{
		try
		{
			return await _httpClient.GetFromJsonAsync<List<NotificationLog>>($"api/notificationlogs/subscription/{subscriptionId}") ?? new List<NotificationLog>();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error fetching logs for subscription {SubscriptionId}", subscriptionId);
			return new List<NotificationLog>();
		}
	}
}
