namespace Yammy.Backend.Models;

public class AddRestaurantRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CuisineType { get; set; } = string.Empty;
    public string RestaurantType { get; set; } = string.Empty;
    public string PriceRange { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string House { get; set; } = string.Empty;
    public string? OpeningDate { get; set; }
    public List<string> Features { get; set; } = new();
    public string? ImageUrl { get; set; }
}