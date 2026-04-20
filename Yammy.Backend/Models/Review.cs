namespace Yammy.Backend.Models;

public class Review
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int RestaurantId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public User? User { get; set; }
    public Restaurant? Restaurant { get; set; }
}