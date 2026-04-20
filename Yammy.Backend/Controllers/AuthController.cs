using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Yammy.Backend.Models;
using Yammy.Backend.Services;

namespace Yammy.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly DatabaseService _databaseService;
    private readonly IConfiguration _configuration;

    public AuthController(DatabaseService databaseService, IConfiguration configuration)
    {
        _databaseService = databaseService;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _databaseService.GetUserByEmailAsync(request.Email);
        if (user == null)
            return Unauthorized(new { message = "Неверный email или пароль" });
        
        if (request.Password != user.PasswordHash)
            return Unauthorized(new { message = "Неверный email или пароль" });
        
        var token = GenerateJwtToken(user);
        return Ok(new AuthResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            Token = token,
            AvatarColor = user.AvatarColor
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var existingUser = await _databaseService.GetUserByEmailAsync(request.Email);
        if (existingUser != null)
            return BadRequest(new { message = "Пользователь с таким email уже существует" });
        
        // Сохраняем пароль как есть (для простоты, без хеширования)
        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            PasswordHash = request.Password, // ВНИМАНИЕ: небезопасно, но для лабораторной работы
            Role = "user",
            AvatarColor = GetRandomColor()
        };
        await _databaseService.CreateUserAsync(user, user.PasswordHash);
        
        var token = GenerateJwtToken(user);
        return Ok(new AuthResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            Token = token,
            AvatarColor = user.AvatarColor
        });
    }
    
    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "YammySecretKey2025ForJWTTokenAuthenticationVeryLong"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role)
        };
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    private string GetRandomColor()
    {
        var colors = new[] { "#3498db", "#e74c3c", "#2ecc71", "#f39c12", "#9b59b6", "#1abc9c" };
        var random = new Random();
        return colors[random.Next(colors.Length)];
    }
}