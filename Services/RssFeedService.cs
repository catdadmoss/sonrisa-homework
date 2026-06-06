using System.ServiceModel.Syndication;
using System.Xml;
using AlertNotificationSystem.Data;
using AlertNotificationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AlertNotificationSystem.Services;

public class RssFeedService
{
	private readonly ApplicationDbContext _context;
	private readonly ILogger<RssFeedService> _logger;
	private readonly IHttpClientFactory _httpClientFactory;

	public RssFeedService(
		ApplicationDbContext context,
		ILogger<RssFeedService> logger,
		IHttpClientFactory httpClientFactory)
	{
		_context = context;
		_logger = logger;
		_httpClientFactory = httpClientFactory;
	}

	public async Task CheckFeedsAsync()
	{
		var activeFeeds = await _context.RssFeeds
			.Where(f => f.IsActive)
			.ToListAsync();

		_logger.LogInformation("Checking {Count} active RSS feeds", activeFeeds.Count);

		foreach (var feed in activeFeeds)
		{
			await CheckSingleFeedAsync(feed);
		}
	}

	private async Task CheckSingleFeedAsync(RssFeed feed)
	{
		try
		{
			_logger.LogInformation("Checking feed: {Name} ({Url})", feed.Name, feed.Url);

			var httpClient = _httpClientFactory.CreateClient();
			httpClient.Timeout = TimeSpan.FromSeconds(30);

			using var response = await httpClient.GetAsync(feed.Url);
			response.EnsureSuccessStatusCode();

			using var stream = await response.Content.ReadAsStreamAsync();
			using var xmlReader = XmlReader.Create(stream);

			var syndicationFeed = SyndicationFeed.Load(xmlReader);
			var newItemsCount = 0;

			foreach (var item in syndicationFeed.Items)
			{
				var itemId = item.Id ?? item.Links.FirstOrDefault()?.Uri.ToString() ?? string.Empty;
				if (string.IsNullOrEmpty(itemId))
				{
					_logger.LogWarning("RSS item has no ID or link, skipping");
					continue;
				}

				var publishDate = item.PublishDate.UtcDateTime;

				// Only process items published after last check
				if (feed.LastCheckedAt.HasValue && publishDate <= feed.LastCheckedAt.Value)
				{
					continue;
				}

				// Check if we already created an alert for this item
				var existingAlert = await _context.Alerts
					.Where(a => a.Metadata != null && a.Metadata.Contains(itemId))
					.FirstOrDefaultAsync();

				if (existingAlert != null)
				{
					continue;
				}

				// Create new alert from RSS item
				var alert = new Alert
				{
					Type = feed.DefaultAlertType,
					Title = item.Title?.Text ?? "No Title",
					Message = GetItemSummary(item),
					Status = AlertStatus.Pending,
					Metadata = System.Text.Json.JsonSerializer.Serialize(new
					{
						Source = "RSS",
						FeedId = feed.Id,
						FeedName = feed.Name,
						ItemId = itemId,
						Link = item.Links.FirstOrDefault()?.Uri.ToString(),
						PublishDate = publishDate
					})
				};

				_context.Alerts.Add(alert);
				newItemsCount++;
			}

			feed.LastCheckedAt = DateTime.UtcNow;
			feed.ItemsProcessed += newItemsCount;
			feed.LastError = null;

			await _context.SaveChangesAsync();

			_logger.LogInformation("Feed {Name} processed: {NewItems} new items", feed.Name, newItemsCount);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error checking feed {Name}: {Error}", feed.Name, ex.Message);
			feed.LastError = ex.Message;
			feed.LastCheckedAt = DateTime.UtcNow;
			await _context.SaveChangesAsync();
		}
	}

	private string GetItemSummary(SyndicationItem item)
	{
		if (item.Summary?.Text != null)
		{
			return item.Summary.Text;
		}

		if (item.Content is TextSyndicationContent textContent)
		{
			return textContent.Text;
		}

		var link = item.Links.FirstOrDefault()?.Uri.ToString();
		return link != null ? $"Read more: {link}" : "No description available";
	}
}
