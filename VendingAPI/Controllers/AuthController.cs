using Microsoft.AspNetCore.Mvc;
using VendingAPI.Services;
using VendingAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace VendingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;
    private readonly JwtService _jwt;
    private readonly VendingDbContext _db;

    public AuthController(AuthService auth, JwtService jwt, VendingDbContext db)
    {
        _auth = auth; _jwt = jwt; _db = db;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _auth.AuthenticateAsync(dto.Email, dto.Password);
        if (user == null) return Unauthorized(new { message = "Неверные учетные данные" });
        var token = _jwt.GenerateToken(user.UserID, user.Email ?? "", user.Role);
        return Ok(new { token, user.UserID, user.FullName, user.Role, user.PhotoUrl });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest(new { message = "Email уже используется" });
        var user = new Models.User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role ?? "Оператор",
            PhotoUrl = dto.PhotoUrl,
            TabNumber = dto.TabNumber
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Ok(new { user.UserID });
    }
}

public class LoginDto { public string Email { get; set; } = ""; public string Password { get; set; } = ""; }
public class RegisterDto { public string FullName { get; set; } = ""; public string Email { get; set; } = ""; public string Password { get; set; } = ""; public string? Role { get; set; } public string? PhotoUrl { get; set; } public string? TabNumber { get; set; } }
