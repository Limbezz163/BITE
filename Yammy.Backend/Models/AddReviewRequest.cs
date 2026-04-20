namespace Yammy.Backend.Models;

public class AddReviewRequest
{
    public int RestaurantId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}