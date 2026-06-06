using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlertNotificationSystem.Data;
using AlertNotificationSystem.Models;

namespace AlertNotificationSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
	private readonly ApplicationDbContext _context;
	private readonly ILogger<AlertsController> _logger;

	public AlertsController(ApplicationDbContext context, ILogger<AlertsController> logger)
	{
		_context = context;
		_logger = logger;
	}

	/// <summary>
	/// Get all alerts with optional filtering
	/// </summary>
	[HttpGet]
	public async Task<ActionResult<IEnumerable<Alert>>> GetAlerts(
		[FromQuery] AlertType? type = null,
		[FromQuery] AlertStatus? status = null,
		[FromQuery] int skip = 0,
		[FromQuery] int take = 50)
	{
		var query = _context.Alerts.AsQueryable();

		if (type.HasValue)
		{
			query = query.Where(a => a.Type == type.Value);
		}

		if (status.HasValue)
		{
			query = query.Where(a => a.Status == status.Value);
		}

		var alerts = await query
			.OrderByDescending(a => a.CreatedAt)
			.Skip(skip)
			.Take(take)
			.ToListAsync();

		return Ok(alerts);
	}

	/// <summary>
	/// Get a specific alert by ID
	/// </summary>
	[HttpGet("{id}")]
	public async Task<ActionResult<Alert>> GetAlert(int id)
	{
		var alert = await _context.Alerts.FindAsync(id);

		if (alert == null)
		{
			return NotFound();
		}

		return Ok(alert);
	}

	/// <summary>
	/// Create a new alert
	/// </summary>
	[HttpPost]
	public async Task<ActionResult<Alert>> CreateAlert(CreateAlertRequest request)
	{
		var alert = new Alert
		{
			Type = request.Type,
			Title = request.Title,
			Message = request.Message,
			Metadata = request.Metadata,
			Status = AlertStatus.Pending,
			CreatedAt = DateTime.UtcNow
		};

		_context.Alerts.Add(alert);
		await _context.SaveChangesAsync();

		_logger.LogInformation("Created new alert {AlertId} of type {Type}", alert.Id, alert.Type);

		return CreatedAtAction(nameof(GetAlert), new { id = alert.Id }, alert);
	}

	/// <summary>
	/// Update an existing alert
	/// </summary>
	[HttpPut("{id}")]
	public async Task<IActionResult> UpdateAlert(int id, UpdateAlertRequest request)
	{
		var alert = await _context.Alerts.FindAsync(id);

		if (alert == null)
		{
			return NotFound();
		}

		alert.Title = request.Title;
		alert.Message = request.Message;
		alert.Metadata = request.Metadata;

		await _context.SaveChangesAsync();

		_logger.LogInformation("Updated alert {AlertId}", id);

		return NoContent();
	}

	/// <summary>
	/// Delete an alert
	/// </summary>
	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteAlert(int id)
	{
		var alert = await _context.Alerts.FindAsync(id);

		if (alert == null)
		{
			return NotFound();
		}

		_context.Alerts.Remove(alert);
		await _context.SaveChangesAsync();

		_logger.LogInformation("Deleted alert {AlertId}", id);

		return NoContent();
	}

	/// <summary>
	/// Get alert statistics
	/// </summary>
	[HttpGet("statistics")]
	public async Task<ActionResult<object>> GetStatistics()
	{
		var stats = new
		{
			TotalAlerts = await _context.Alerts.CountAsync(),
			PendingAlerts = await _context.Alerts.CountAsync(a => a.Status == AlertStatus.Pending),
			ProcessingAlerts = await _context.Alerts.CountAsync(a => a.Status == AlertStatus.Processing),
			SentAlerts = await _context.Alerts.CountAsync(a => a.Status == AlertStatus.Sent),
			FailedAlerts = await _context.Alerts.CountAsync(a => a.Status == AlertStatus.Failed),
			ByType = await _context.Alerts
				.GroupBy(a => a.Type)
				.Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
				.ToListAsync()
		};

		return Ok(stats);
	}
}

public record CreateAlertRequest(
	AlertType Type,
	string Title,
	string Message,
	string? Metadata = null
);

public record UpdateAlertRequest(
	string Title,
	string Message,
	string? Metadata = null
);
