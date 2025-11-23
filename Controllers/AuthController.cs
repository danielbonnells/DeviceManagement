// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using DeviceManagement.Data;
// using DeviceManagement.Models;
// using Microsoft.IdentityModel.Tokens;
// using System.IdentityModel.Tokens.Jwt;
// using System.Security.Claims;
// using System.Text;
// using Google.Apis.Auth;
// using BCrypt.Net;
// using Microsoft.AspNetCore.Identity.Data;

// namespace DeviceManagement.Controllers;

// [ApiController]
// [Route("api/[controller]")]
// public class AuthController : ControllerBase
// {
//     private readonly DeviceContext _context;
//     private readonly IConfiguration _config;

//     public AuthController(DeviceContext context, IConfiguration config)
//     {
//         _context = context;
//         _config = config;
//     }

//     // ----------------------
//     // Email/Password Signup
//     // ----------------------
//     [HttpPost("register")]
//     public async Task<IActionResult> Register([FromBody] RegisterRequest request)
//     {
//         if (!ModelState.IsValid)
//             return BadRequest(ModelState);

//         if (await _context.Users.AnyAsync(u => u.Email == request.Email))
//             return BadRequest(new { message = "Email already in use." });

//         var user = new User
//         {
//             Email = request.Email,
//             PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
//         };

//         _context.Users.Add(user);
//         await _context.SaveChangesAsync();

//         return Ok(new { user.Id, user.Email });
//     }

//     // ----------------------
//     // Email/Password Login
//     // ----------------------
//     [HttpPost("login")]
//     public async Task<IActionResult> Login([FromBody] LoginRequest request)
//     {
//         var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
//         if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
//             return Unauthorized(new { message = "Invalid credentials." });

//         var token = GenerateJwtToken(user);

//         SetTokenCookie(token);

//         return Ok(new { user = new { user.Id, user.Email } });
//     }

//     // ----------------------
//     // Google Login
//     // ----------------------
//     [HttpPost("google")]
//     public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
//     {
//         GoogleJsonWebSignature.Payload payload;
//         try
//         {
//             payload = await GoogleJsonWebSignature.ValidateAsync(request.Credential, new GoogleJsonWebSignature.ValidationSettings
//             {
//                 Audience = new[] { _config["Google:ClientId"] }
//             });
//         }
//         catch (Exception)
//         {
//             return Unauthorized(new { message = "Invalid Google token." });
//         }

//         // Check if user already exists
//         var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);
//         if (user == null)
//         {
//             user = new User
//             {
//                 Email = payload.Email,
//                 PasswordHash = null // No password for Google accounts
//             };
//             _context.Users.Add(user);
//             await _context.SaveChangesAsync();
//         }

//         var token = GenerateJwtToken(user);
//         SetTokenCookie(token);
//         return Ok(new { user = new { user.Id, user.Email } });
//     }

//     // ----------------------
//     // Helpers
//     // ----------------------
//     private string GenerateJwtToken(User user)
//     {
//         var claims = new[]
//         {
//             new Claim(JwtRegisteredClaimNames.Sub, user.Email),
//             new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
//             new Claim("userId", user.Id.ToString())
//         };

//         var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
//         var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

//         var token = new JwtSecurityToken(
//             issuer: _config["Jwt:Issuer"],
//             audience: _config["Jwt:Audience"],
//             claims: claims,
//             expires: DateTime.UtcNow.AddDays(30),
//             signingCredentials: creds
//         );

//         return new JwtSecurityTokenHandler().WriteToken(token);
//     }

//     private void SetTokenCookie(string token)
//     {
//         var cookieOptions = new CookieOptions
//         {
//             HttpOnly = true,          // JS cannot read the cookie
//             Secure = true,            // Only send over HTTPS
//             SameSite = SameSiteMode.Lax,
//             Expires = DateTime.UtcNow.AddDays(30)
//         };
//         Response.Cookies.Append("session", token, cookieOptions);
//     }
// }

// public class GoogleLoginRequest
// {
//     public string Credential { get; set; } = string.Empty;
// }

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeviceManagement.Data;
using DeviceManagement.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;
using System.Net.Http.Json;
using BCrypt.Net;
using Microsoft.AspNetCore.Identity.Data;

namespace DeviceManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly DeviceContext _context;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory; // 👇 CHANGED

    public AuthController(DeviceContext context, IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _config = config;
        _httpClientFactory = httpClientFactory; // 👇 CHANGED
    }

    // ----------------------
    // Email/Password Signup
    // ----------------------
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest(new { message = "Email already in use." });

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { user.Id, user.Email });
    }

    // ----------------------
    // Email/Password Login
    // ----------------------
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid credentials." });

        var token = GenerateJwtToken(user);

        // 👇 CHANGED — set HttpOnly cookie instead of returning raw token
        Response.Cookies.Append("session", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(30)
        });

        return Ok(new { user = new { user.Id, user.Email } });
    }

    // ----------------------
    // Google Login (OAuth Code Flow)
    // ----------------------
    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleCodeRequest request)
    {
        try
        {
            // 👇 CHANGED — exchange code for tokens
            var client = _httpClientFactory.CreateClient();

            var payload = new
            {
                code = request.Code,
                client_id = _config["Google:ClientIdFrontend"],
                client_secret = _config["Google:ClientSecretFrontend"],
                redirect_uri = "postmessage", // 👈 use same as frontend
                grant_type = "authorization_code"
            };

            var tokenResponse = await client.PostAsJsonAsync("https://oauth2.googleapis.com/token", payload);
            if (!tokenResponse.IsSuccessStatusCode)
                return Unauthorized(new { message = "Failed to exchange authorization code." });

            var tokenData = await tokenResponse.Content.ReadFromJsonAsync<GoogleTokenResponse>();
            var content = await tokenResponse.Content.ReadAsStringAsync();
            Console.WriteLine(content);
            if (tokenData == null || string.IsNullOrEmpty(tokenData.IdToken))
                return Unauthorized(new { message = "Missing ID token from Google." });

            // 👇 CHANGED — verify the ID token
            var googlePayload = await GoogleJsonWebSignature.ValidateAsync(tokenData.IdToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _config["Google:ClientIdFrontend"] }
            });

            // 👇 CHANGED — find or create local user
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == googlePayload.Email);
            if (user == null)
            {
                user = new User
                {
                    Email = googlePayload.Email,
                    PasswordHash = "" // Google accounts have no password
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            var appToken = GenerateJwtToken(user);

            // 👇 CHANGED — set secure HttpOnly cookie
            Response.Cookies.Append("session", appToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(30)
            });

            return Ok(new { user = new { user.Id, user.Email } });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Google login error: {ex.Message}");
            return Unauthorized(new { message = "Google authentication failed." });
        }
    }

    //Device Pre-Registration
    [HttpPost("getCode")]
    public async Task<IActionResult> GetCode([FromBody] RegistrationDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var registration = new Registration
        {
            UniqueId = request.UniqueId,
            TempCode = GenerateTempCode(9, 3)
        };

        _context.Registrations.Add(registration);
        await _context.SaveChangesAsync();

        return Ok(new { registration.UniqueId, registration.TempCode, registration.CreatedAt });
    }

    // ----------------------
    // Helpers
    // ----------------------
    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("userId", user.Id.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates a temporary registration code in a hyphenated format (e.g., "XXX-XXX-XXX").
    /// </summary>
    /// <param name="totalLength">The total number of characters in the code (excluding hyphens).</param>
    /// <param name="groupSize">The number of characters per group.</param>
    /// <returns>The generated code string.</returns>
    public static string GenerateTempCode(int totalLength = 9, int groupSize = 3)
    {
        const string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Excludes 0, 1, I, O for clarity
        
        if (totalLength <= 0 || groupSize <= 0 || totalLength % groupSize != 0)
        {
            throw new ArgumentException("Total length must be positive and divisible by group size.");
        }

        var random = new Random();
        var codeBuilder = new StringBuilder();
        int groups = totalLength / groupSize;

        for (int i = 0; i < groups; i++)
        {
            // Generate characters for one group
            for (int j = 0; j < groupSize; j++)
            {
                codeBuilder.Append(Chars[random.Next(Chars.Length)]);
            }

            // Append a hyphen if it's not the last group
            if (i < groups - 1)
            {
                codeBuilder.Append('-');
            }
        }

        return codeBuilder.ToString();
    }

}

// 👇 CHANGED — new request/response models
public class GoogleCodeRequest
{
    public string Code { get; set; } = string.Empty;
}

public class GoogleTokenResponse
{
    [System.Text.Json.Serialization.JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("scope")]
    public string? Scope { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("id_token")]
    public string? IdToken { get; set; }
}

