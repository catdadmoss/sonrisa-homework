using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlertNotificationSystem.Data;
using AlertNotificationSystem.Models;

namespace AlertNotificationSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
	private readonly ApplicationDbContext _context;

	public SubscriptionsController(ApplicationDbContext context)
	{
		_context = context;
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<Subscription>>> GetSubscriptions()
	{
		return await _context.Subscriptions.ToListAsync();
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<Subscription>> GetSubscription(int id)
	{
		var subscription = await _context.Subscriptions.FindAsync(id);
		if (subscription == null) return NotFound();
		return subscription;
	}

	[HttpPost]
	public async Task<ActionResult<Subscription>> CreateSubscription(CreateSubscriptionRequest request)
	{
		var subscription = new Subscription
		{
			UserIdentifier = request.UserIdentifier,
			Channel = request.Channel,
			Destination = request.Destination,
			AlertTypeFilter = request.AlertTypeFilter,
			IsActive = true
		};

		_context.Subscriptions.Add(subscription);
		await _context.SaveChangesAsync();

		return CreatedAtAction(nameof(GetSubscription), new { id = subscription.Id }, subscription);
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteSubscription(int id)
	{
		var subscription = await _context.Subscriptions.FindAsync(id);
		if (subscription == null) return NotFound();

		_context.Subscriptions.Remove(subscription);
		await _context.SaveChangesAsync();
		return NoContent();
	}
}

public record CreateSubscriptionRequest(
	string UserIdentifier,
	NotificationChannel Channel,
	string Destination,
	AlertType? AlertTypeFilter = null
);
