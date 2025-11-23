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
[Route("/[controller]")]
public class MqttController : ControllerBase
{
    private readonly DeviceContext _context;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;

    public MqttController(DeviceContext context, IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _config = config;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("auth")]
    public async Task<IResult> Auth([FromQuery] string username, [FromQuery] string password)
    {

        var uniqueId = username;
        var apiKey = password;

        if (password == "SpecialHardcodedKeyphrase")
        {
            //check registrations for the uniqueID
            //if registration exists and is PENDING_PAIRING

            var existingRegistration = await _context.Registrations
                .Where(d => d.UniqueId == uniqueId && d.Status == "PENDING_PAIRING")
                .FirstOrDefaultAsync();

            //check if expired
            if (existingRegistration == null || existingRegistration.CreatedAt < DateTime.UtcNow.AddMinutes(-10))
            {
                return Results.Content("Forbidden", "text/plain", statusCode: 403);
            }
        
            //Allow through
            return Results.Content("OK", "text/plain", statusCode: 200);

        }

        if (string.IsNullOrWhiteSpace(uniqueId) || string.IsNullOrWhiteSpace(apiKey))
            return Results.Content("Forbidden", "text/plain", statusCode: 403);

        // Look up device in DB
        var device = await _context.Devices
            .Where(d => d.UniqueId == uniqueId)
            .FirstOrDefaultAsync();

        if (device is null)
            return Results.Content("Forbidden", "text/plain", statusCode: 403);

        if (device.ApiKey != apiKey)
            return Results.Content("Forbidden", "text/plain", statusCode: 403);

        // SUCCESS
        return Results.Content("OK", "text/plain", statusCode: 200);

    }

    // [HttpGet("superuser")]
    // public IActionResult Superuser([FromQuery] string username)
    // {
    //     // Only TestUser is superuser
    //     return Ok(new { superuser = username == TestUser });
    // }

    [HttpGet("acl")]
    public IActionResult Acl([FromQuery] string username, [FromQuery] string topic, [FromQuery] string acc)
    {
      
    
        return Ok(new { allow = false });
    }
   
}

