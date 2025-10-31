
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Infra;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _db;
    public TasksController(AppDbContext db) { _db = db; }

    private (long userId, bool isAdmin) GetUser()
    {
        var id = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");
        return (id, isAdmin);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<TaskDto>>> Get([FromQuery] TasksQuery q)
    {
        var (userId, isAdmin) = GetUser();
        var query = _db.Tasks.AsNoTracking();

        if (!isAdmin) query = query.Where(t => t.UserId == userId);
        else if (q.UserId.HasValue) query = query.Where(t => t.UserId == q.UserId.Value);

        query = q.Status switch
        {
            "pending" => query.Where(t => !t.IsComplete),
            "done" => query.Where(t => t.IsComplete),
            _ => query
        };

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim().ToLower();
            query = query.Where(t =>
                EF.Functions.Like(t.Title.ToLower(), $"%{term}%") ||
                (t.Description != null && EF.Functions.Like(t.Description.ToLower(), $"%{term}%")));
        }

        query = (q.Sort, q.Dir.ToLower()) switch
        {
            ("title", "asc") => query.OrderBy(t => t.Title),
            ("title", "desc") => query.OrderByDescending(t => t.Title),
            ("status", "asc") => query.OrderBy(t => t.IsComplete),
            ("status", "desc") => query.OrderByDescending(t => t.IsComplete),
            ("createdAt", "asc") => query.OrderBy(t => t.CreatedAt),
            _ => query.OrderByDescending(t => t.CreatedAt)
        };

        var total = await query.CountAsync();
        var page = Math.Max(q.Page, 1);
        var size = Math.Clamp(q.PageSize, 1, 100);

        var data = await query.Skip((page - 1) * size).Take(size).ToListAsync();
        var meta = new PaginationMeta(page, size, total,
            Math.Max(1, (int)Math.Ceiling(total / (double)size)),
            page * size < total, page > 1);

        return Ok(new PagedResult<TaskDto>(data.Select(x => x.ToDto()), meta));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<TaskDto>> GetById(long id)
    {
        var (userId, isAdmin) = GetUser();
        var t = await _db.Tasks.FindAsync(id);
        if (t is null) return NotFound();
        if (!isAdmin && t.UserId != userId) return Forbid();
        return Ok(t.ToDto());
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create([FromBody] TaskCreateDto dto)
    {
        var (userId, _) = GetUser();
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new ErrorResponse(400, "Bad Request", "El título es obligatorio."));

        var entity = new Domain.TaskItem
        {
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            IsComplete = dto.IsComplete,
            UserId = userId
        };
        _db.Tasks.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity.ToDto());
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<TaskDto>> Update(long id, [FromBody] TaskUpdateDto dto)
    {
        var (userId, isAdmin) = GetUser();
        var t = await _db.Tasks.FindAsync(id);
        if (t is null) return NotFound();
        if (!isAdmin && t.UserId != userId) return Forbid();
        if (string.IsNullOrWhiteSpace(dto.Title))
            return BadRequest(new ErrorResponse(400, "Bad Request", "El título es obligatorio."));

        var before = t.IsComplete;
        t.Title = dto.Title.Trim();
        t.Description = dto.Description?.Trim();
        t.IsComplete = dto.IsComplete;
        if (before != t.IsComplete) t.CompletedAt = t.IsComplete ? DateTimeOffset.UtcNow : null;

        await _db.SaveChangesAsync();
        return Ok(t.ToDto());
    }

    [HttpDelete("{id:long}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(long id)
    {
        var t = await _db.Tasks.FindAsync(id);
        if (t is null) return NotFound();
        _db.Tasks.Remove(t);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id:long}/toggle")]
    public async Task<ActionResult<TaskDto>> Toggle(long id)
    {
        var (userId, isAdmin) = GetUser();
        var t = await _db.Tasks.FindAsync(id);
        if (t is null) return NotFound();
        if (!isAdmin && t.UserId != userId) return Forbid();
        t.IsComplete = !t.IsComplete;
        t.CompletedAt = t.IsComplete ? DateTimeOffset.UtcNow : null;
        await _db.SaveChangesAsync();
        return Ok(t.ToDto());
    }
}













/*using Microsoft.AspNetCore.Mvc;

namespace Gestor_de_Tareas.Controllers;

[ApiController]
[Route("[controller]")]
public class TasksController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<TasksController> _logger;

    public TasksController(ILogger<TasksController> logger)
    {
        _logger = logger;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}*/
