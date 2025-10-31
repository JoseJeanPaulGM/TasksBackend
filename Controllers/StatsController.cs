using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Infra;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatsController : ControllerBase
{
    private readonly AppDbContext _db;
    public StatsController(AppDbContext db) { _db = db; }

    private (long userId, bool isAdmin) GetUser()
    {
        var id = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");
        return (id, isAdmin);
    }

    [HttpGet("tasks")]
    public async Task<ActionResult<StatsDto>> Get([FromQuery] int windowDays = 7, [FromQuery] long? userId = null)
    {
        windowDays = Math.Clamp(windowDays, 2, 60);
        var (currentUserId, isAdmin) = GetUser();
        var ownerId = isAdmin && userId.HasValue ? userId.Value : currentUserId;

        var today = DateTimeOffset.UtcNow.Date;
        var start = today.AddDays(-(windowDays - 1));
        var keys = Enumerable.Range(0, windowDays).Select(i => start.AddDays(i)).ToArray();

        var inWindow = await _db.Tasks.AsNoTracking()
            .Where(t => t.UserId == ownerId &&
                        t.CreatedAt.Date <= today &&
                        (t.CompletedAt == null || t.CompletedAt.Value.Date >= start))
            .ToListAsync();

        var created = new int[windowDays];
        var completed = new int[windowDays];

        foreach (var t in inWindow)
        {
            var cIdx = (int)(t.CreatedAt.Date - start).TotalDays;
            if (cIdx >= 0 && cIdx < windowDays) created[cIdx]++;
            if (t.CompletedAt.HasValue)
            {
                var fIdx = (int)(t.CompletedAt.Value.Date - start).TotalDays;
                if (fIdx >= 0 && fIdx < windowDays) completed[fIdx]++;
            }
        }

        var pendingBefore = await _db.Tasks.AsNoTracking()
            .Where(t => t.UserId == ownerId &&
                        t.CreatedAt.Date < start &&
                        !(t.CompletedAt != null && t.CompletedAt.Value.Date < start))
            .CountAsync();

        var pending = new int[windowDays];
        var run = pendingBefore;
        for (int i = 0; i < windowDays; i++) { run += created[i]; run -= completed[i]; pending[i] = Math.Max(0, run); }

        var total = await _db.Tasks.CountAsync(t => t.UserId == ownerId);
        var done = await _db.Tasks.CountAsync(t => t.UserId == ownerId && t.IsComplete);
        var labels = keys.Select(d => d.ToString("yyyy-MM-dd")).ToArray();

        return Ok(new StatsDto(total, done, total - done, windowDays, labels, created, completed, pending));
    }
}
