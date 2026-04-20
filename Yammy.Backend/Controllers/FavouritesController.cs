using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Yammy.Backend.Services;

namespace Yammy.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FavouritesController : ControllerBase
{
    private readonly DatabaseService _databaseService;

    public FavouritesController(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetFavourites()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var favourites = await _databaseService.GetFavouritesByUserIdAsync(userId);
        return Ok(favourites);
    }

    [HttpPost("{restaurantId}")]
    public async Task<IActionResult> AddFavourite(int restaurantId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _databaseService.AddFavouriteAsync(userId, restaurantId);
        if (result)
            return Ok(new { message = "Добавлено в избранное" });
        return BadRequest(new { message = "Уже в избранном" });
    }

    [HttpDelete("{restaurantId}")]
    public async Task<IActionResult> RemoveFavourite(int restaurantId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _databaseService.RemoveFavouriteAsync(userId, restaurantId);
        if (result)
            return Ok(new { message = "Удалено из избранного" });
        return NotFound(new { message = "Не найдено в избранном" });
    }

    [HttpGet("check/{restaurantId}")]
    public async Task<IActionResult> IsFavourite(int restaurantId)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var isFavourite = await _databaseService.IsFavouriteAsync(userId, restaurantId);
        return Ok(new { isFavourite });
    }
}