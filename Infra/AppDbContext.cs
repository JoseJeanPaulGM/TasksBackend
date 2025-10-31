namespace Infra;

using Domain;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<User> Users => Set<User>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasIndex(x => x.Username).IsUnique();
            e.Property(x => x.Role).HasMaxLength(20);
        });

        b.Entity<TaskItem>(e =>
        {
            e.ToTable("tasks");
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(255);
            e.HasIndex(x => x.IsComplete);
            e.HasIndex(x => x.CreatedAt);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(b);
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var e in ChangeTracker.Entries<TaskItem>())
        {
            if (e.State == EntityState.Added)
            {
                e.Entity.CreatedAt = now; e.Entity.UpdatedAt = now;
                if (e.Entity.IsComplete && e.Entity.CompletedAt is null) e.Entity.CompletedAt = now;
            }
            else if (e.State == EntityState.Modified)
            {
                e.Entity.UpdatedAt = now;
                if (e.Property(p => p.IsComplete).IsModified)
                    e.Entity.CompletedAt = e.Entity.IsComplete ? now : null;
            }
        }
        return base.SaveChangesAsync(ct);
    }
}
