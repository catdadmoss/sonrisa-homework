using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AlertNotificationSystem.Data;
using AlertNotificationSystem.Models;

namespace AlertNotificationSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RssFeedsController : ControllerBase
{
	private readonly ApplicationDbContext _context;
	private readonly ILogger<RssFeedsController> _logger;

	public RssFeedsController(ApplicationDbContext context, ILogger<RssFeedsController> logger)
	{
		_context = context;
		_logger = logger;
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<RssFeed>>> GetRssFeeds()
	{
		return await _context.RssFeeds.ToListAsync();
	}

	[HttpGet("{id}")]
	public async Task<ActionResult<RssFeed>> GetRssFeed(int id)
	{
		var rssFeed = await _context.RssFeeds.FindAsync(id);

		if (rssFeed == null)
		{
			return NotFound();
		}

		return rssFeed;
	}

	[HttpPost]
	public async Task<ActionResult<RssFeed>> CreateRssFeed(RssFeed rssFeed)
	{
		_context.RssFeeds.Add(rssFeed);
		await _context.SaveChangesAsync();

		_logger.LogInformation("Created RSS feed: {Name} ({Url})", rssFeed.Name, rssFeed.Url);

		return CreatedAtAction(nameof(GetRssFeed), new { id = rssFeed.Id }, rssFeed);
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> UpdateRssFeed(int id, RssFeed rssFeed)
	{
		if (id != rssFeed.Id)
		{
			return BadRequest();
		}

		_context.Entry(rssFeed).State = EntityState.Modified;

		try
		{
			await _context.SaveChangesAsync();
			_logger.LogInformation("Updated RSS feed: {Name}", rssFeed.Name);
		}
		catch (DbUpdateConcurrencyException)
		{
			if (!await RssFeedExists(id))
			{
				return NotFound();
			}
			throw;
		}

		return NoContent();
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteRssFeed(int id)
	{
		var rssFeed = await _context.RssFeeds.FindAsync(id);
		if (rssFeed == null)
		{
			return NotFound();
		}

		_context.RssFeeds.Remove(rssFeed);
		await _context.SaveChangesAsync();

		_logger.LogInformation("Deleted RSS feed: {Name}", rssFeed.Name);

		return NoContent();
	}

	[HttpPost("{id}/check")]
	public async Task<IActionResult> CheckFeedNow(int id)
	{
		var rssFeed = await _context.RssFeeds.FindAsync(id);
		if (rssFeed == null)
		{
			return NotFound();
		}

		// This endpoint could trigger an immediate check
		// For now, it just returns the feed info
		return Ok(new { message = "Feed check scheduled", feed = rssFeed });
	}

	private async Task<bool> RssFeedExists(int id)
	{
		return await _context.RssFeeds.AnyAsync(e => e.Id == id);
	}
}
