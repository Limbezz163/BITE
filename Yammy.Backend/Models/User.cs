namespace Yammy.Backend.Models;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Role { get; set; } = "user";
    public DateTime CreatedAt { get; set; }
    public string AvatarColor { get; set; } = "#3498db";
}