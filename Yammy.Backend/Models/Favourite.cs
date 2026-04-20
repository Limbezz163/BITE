namespace Yammy.Backend.Models;

public class Favourite
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int RestaurantId { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public Restaurant? Restaurant { get; set; }
}