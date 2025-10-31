namespace Domain;

using System.ComponentModel.DataAnnotations;

public class User
{
    public long Id { get; set; }

    [Required, MaxLength(100)]
    public string Username { get; set; } = default!;

    [Required]
    public string PasswordHash { get; set; } = default!;

    [Required, MaxLength(20)]
    public string Role { get; set; } = "User"; // "Admin" o "User"
}
