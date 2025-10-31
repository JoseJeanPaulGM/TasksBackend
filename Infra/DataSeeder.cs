namespace Infra;

using BCrypt.Net;
using Domain;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!db.Users.Any())
        {
            var admin = new User
            {
                Username = "admin",
                PasswordHash = BCrypt.HashPassword("Admin123!"),
                Role = "Admin"
            };
            var user = new User
            {
                Username = "user",
                PasswordHash = BCrypt.HashPassword("User123!"),
                Role = "User"
            };
            db.Users.AddRange(admin, user);
            await db.SaveChangesAsync();

            db.Tasks.AddRange(
                new TaskItem { Title = "Revisar backlog", UserId = admin.Id },
                new TaskItem { Title = "Configurar Angular", UserId = user.Id },
                new TaskItem { Title = "Diseñar API", UserId = user.Id, IsComplete = true, CompletedAt = DateTimeOffset.UtcNow }
            );
            await db.SaveChangesAsync();
        }
    }
}
