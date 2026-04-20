namespace Yammy.Backend.Models;

public class Restaurant
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public string? CuisineType { get; set; }
    public string? RestaurantType { get; set; }
    public string? PriceRange { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Street { get; set; }
    public string? House { get; set; }
    public DateTime? OpeningDate { get; set; }
    public decimal Rating { get; set; }
    public string? ImageUrl { get; set; }
    public string? Features { get; set; }
    public DateTime CreatedAt { get; set; }
}