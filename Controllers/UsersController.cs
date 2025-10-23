using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeviceManagement.Data;
using DeviceManagement.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Text;
using BCrypt.Net;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;

namespace DeviceManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly DeviceContext _context;
    private readonly IConfiguration _config;

    public UsersController(DeviceContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterRequest request)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);

    // Check for duplicate email
    bool emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email);
    if (emailExists)
        return BadRequest(new { message = "Email already in use." });

    // Create new user
    var user = new User
    {
        Email = request.Email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
    };

    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    // Return limited info
    return Ok(new { user.Id, user.Email });
}

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
            return Unauthorized(new { message = "Invalid credentials." });

        // Verify the password hash
        bool validPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!validPassword)
            return Unauthorized(new { message = "Invalid credentials." });

        // Generate JWT
        var token = GenerateJwtToken(user);

        return Ok(new
        {
            token,
            user = new { user.Id, user.Email }
        });
    }
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = GetUserIdFromClaims();
        if (userId == null) return Unauthorized();

        var user = await _context.Users
            .Include(u => u.Devices)
            .FirstOrDefaultAsync(u => u.Id == userId);

        return user == null ? NotFound() : Ok(user);
    }

    private int? GetUserIdFromClaims()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == "userId");
        return claim != null ? int.Parse(claim.Value) : null;
    }

    // private string GenerateJwtToken(User user)
    // {
    //     var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
    //     var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    //     var claims = new[]
    //     {
    //         new Claim(JwtRegisteredClaimNames.Sub, user.Email),
    //         new Claim("userId", user.Id.ToString())
    //     };
    //     var token = new JwtSecurityToken(
    //         _config["Jwt:Issuer"],
    //         _config["Jwt:Audience"],
    //         claims,
    //         expires: DateTime.UtcNow.AddDays(7),
    //         signingCredentials: credentials
    //     );
    //     return new JwtSecurityTokenHandler().WriteToken(token);
    // }

    private string GenerateJwtToken(User user)
{
    var claims = new[]
    {
        new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Sub, user.Email),
        new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim("userId", user.Id.ToString())
      };

    var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(_config["Jwt:Key"])
    );

    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: _config["Jwt:Issuer"],
        audience: _config["Jwt:Audience"],
        claims: claims,
        expires: DateTime.UtcNow.AddHours(12),
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
}
