using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlertNotificationSystem.Data;
using AlertNotificationSystem.Models;

namespace AlertNotificationSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationLogsController : ControllerBase
{
	private readonly ApplicationDbContext _context;

	public NotificationLogsController(ApplicationDbContext context)
	{
		_context = context;
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<NotificationLog>>> GetLogs(
		[FromQuery] int? alertId = null,
		[FromQuery] int skip = 0,
		[FromQuery] int take = 100)
	{
		var query = _context.NotificationLogs
			.Include(l => l.Alert)
			.Include(l => l.Subscription)
			.AsQueryable();

		if (alertId.HasValue)
		{
			query = query.Where(l => l.AlertId == alertId.Value);
		}

		var logs = await query
			.OrderByDescending(l => l.SentAt)
			.Skip(skip)
			.Take(take)
			.ToListAsync();

		return Ok(logs);
	}
}
