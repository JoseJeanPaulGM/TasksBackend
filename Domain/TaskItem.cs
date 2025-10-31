namespace Domain;
using System.ComponentModel.DataAnnotations;

public class TaskItem
{
    public long Id { get; set; }

    [Required, MaxLength(255)]
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsComplete { get; set; } = false;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }

    public long UserId { get; set; }
    public User? User { get; set; }
}