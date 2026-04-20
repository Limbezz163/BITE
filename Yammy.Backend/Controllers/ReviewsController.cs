using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Yammy.Backend.Models;
using Yammy.Backend.Services;

namespace Yammy.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly DatabaseService _databaseService;

    public ReviewsController(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [HttpGet("restaurant/{restaurantId}")]
    public async Task<IActionResult> GetReviewsByRestaurant(int restaurantId)
    {
        var reviews = await _databaseService.GetReviewsByRestaurantIdAsync(restaurantId);
        return Ok(reviews);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddReview([FromBody] AddReviewRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        if (userId == 0)
            return Unauthorized(new { message = "Необходима авторизация" });
        
        try
        {
            var review = await _databaseService.AddReviewAsync(userId, request.RestaurantId, request.Rating, request.Comment);
            return Ok(review);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}