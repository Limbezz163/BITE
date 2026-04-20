using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Yammy.Backend.Models;
using Yammy.Backend.Services;

namespace Yammy.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")] 
public class RestaurantsController : ControllerBase
{
    private readonly DatabaseService _databaseService;

    public RestaurantsController(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRestaurants(
        [FromQuery] string? search,
        [FromQuery] string? cuisineType,
        [FromQuery] string? restaurantType,
        [FromQuery] string? priceRange,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] double? minRating)
    {
        var restaurants = await _databaseService.GetFilteredRestaurantsAsync(
            search, cuisineType, restaurantType, priceRange, minPrice, maxPrice, minRating);
        return Ok(restaurants);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRestaurantById(int id)
    {
        var restaurant = await _databaseService.GetRestaurantByIdAsync(id);
        if (restaurant == null)
            return NotFound(new { message = "Ресторан не найден" });
        return Ok(restaurant);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchRestaurants([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            return Ok(new List<Restaurant>());
        var results = await _databaseService.SearchRestaurantsByNameAsync(q);
        return Ok(results);
    }

    [HttpPost]
    
    public async Task<IActionResult> AddRestaurant([FromBody] AddRestaurantRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var restaurant = new Restaurant
        {
            Name = request.Name,
            Description = request.Description,
            CuisineType = request.CuisineType,
            RestaurantType = request.RestaurantType,
            PriceRange = request.PriceRange,
            Phone = request.Phone,
            City = request.City,
            Street = request.Street,
            House = request.House,
            OpeningDate = string.IsNullOrEmpty(request.OpeningDate) ? null : DateTime.Parse(request.OpeningDate),
            Features = request.Features != null && request.Features.Any() ? string.Join(",", request.Features) : null,
            ImageUrl = request.ImageUrl ?? "https://via.placeholder.com/300x200?text=No+Image"
        };
        
        var created = await _databaseService.AddRestaurantAsync(restaurant);
        return Ok(created);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteRestaurant(int id)
    {
        var result = await _databaseService.DeleteRestaurantAsync(id);
        if (!result)
            return NotFound(new { message = "Ресторан не найден" });
        return Ok(new { message = "Ресторан удален" });
    }
}