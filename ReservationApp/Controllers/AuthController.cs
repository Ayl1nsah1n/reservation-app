using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservationApp.Data;
using ReservationApp.Dtos;
using ReservationApp.Enums;
using ReservationApp.Models;
using ReservationApp.Services;

namespace ReservationApp.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenSvc;
    private readonly IConfiguration _cfg;

    public AuthController(AppDbContext db, TokenService tokenSvc, IConfiguration cfg)
    {
        _db = db;
        _tokenSvc = tokenSvc;
        _cfg = cfg;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
            return Conflict("Email already exists");

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = Role.User
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return StatusCode(201);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginDto dto)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized();

        var jwt = _cfg.GetSection("Jwt");
        var token = _tokenSvc.Create(
            user,
            jwt["Issuer"]!, jwt["Audience"]!, jwt["Key"]!, int.Parse(jwt["ExpiryMinutes"]!)
        );

        return Ok(new AuthResponse(token, user.FullName, user.Role.ToString()));
    }

    
    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout() => Ok(new { message = "Logged out. Delete your token on client." });
}
