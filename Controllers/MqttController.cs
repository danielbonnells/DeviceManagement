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
using System.Text.Json.Serialization;

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

    [HttpPost("auth")]
    public async Task<IResult> Auth([FromBody] MqttAuthRequest request)
    {

        var uniqueId = request.Username;
        var apiKey = request.Password;

        if (request.Password == "SpecialHardcodedKeyphrase")
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

    [HttpPost("superuser")]
    public IActionResult CheckSuperuser([FromBody] MqttSuperuserRequest request)
    {
        // Only TestUser is superuser
        //return Ok(new { superuser = request.Username });
        return Ok(new { superuser = false });
    }

[HttpPost("acl")]
public async Task<IActionResult> CheckACL([FromBody] MqttAclRequest request)
{
    // --- START: RAW REQUEST BODY LOGGING FOR DEBUGGING 400 ERROR ---
    Request.EnableBuffering();
    using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
    {
        string rawBody = await reader.ReadToEndAsync();
        Request.Body.Position = 0; // Reset the stream for model binding
    }
    // --- END: RAW REQUEST BODY LOGGING ---

    // 1. Validate mandatory inputs (prevents 500 error on nulls)
    if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Topic))
    {
        return StatusCode(403, new { granted = false }); // Always return denial payload
    }
    
    // Check if Acc can be parsed (keep this check to prevent internal errors)
    if (!int.TryParse(request.Acc, out int accessCode))
    {
        // This is the source of your 400 error. Check the rawBody log for what was sent!
        return BadRequest(new { error = "Invalid 'Acc' format." }); 
    }

    // 2. Define the topic the user OWNS and grant explicit access
    string requiredTopicPrefix = $"devices/{request.Username}/";
    
    if (request.Topic.StartsWith(requiredTopicPrefix))
    {
        // --- SECURITY FIX: Return 200 OK with explicit grant ---
        return Ok(new { granted = true });
    }
    
    // 3. Handle Other Authorizations
    
    // Check if the user has database read access to a subway stop topic
    if (await IsAuthorizedForRead(request.Username, request.Topic))
    {
        return Ok(new { granted = true }); 
    }

    // 4. Default Denial
    return StatusCode(403, new { granted = false }); // Return denial status code (403) with denial payload
}
   
   private async Task<bool> IsAuthorizedForRead(string username, string requestedTopic)
    {
            var topicSegments = requestedTopic.Split('/');

            // 1. Structure Check: Verify the topic starts with something like "MTA/stops/"
            if (topicSegments.Length < 3 || topicSegments[1] != "stops")
            {
                return false; // Deny if structure is not as expected
            }

            // 2. Extract and Clean Stop ID
            var stopIdWithWildcard = topicSegments[2];
            var cleanStopId = stopIdWithWildcard.Replace("+", "").Replace("#", "");

            // 3. Validation: Deny if the topic was only a generic wildcard (e.g., "MTA/stops/#")
            if (string.IsNullOrEmpty(cleanStopId))
            {
                return false; 
            }

            // 4. Execute the Authorization Check against the database
            bool hasAccess = await _context.Devices
                .Where(d => d.UniqueId == username)
                .AnyAsync(d => d.Stops.Any(s => s.StopId == cleanStopId));

        return hasAccess;
}
}


public class MqttAuthRequest
{
    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }

    [JsonPropertyName("clientid")]
    public string ClientId { get; set; }
}
public class MqttSuperuserRequest
{
   [JsonPropertyName("username")]
    public string Username { get; set; }
}
public class MqttAclRequest
{
    [JsonPropertyName("username")] 
    public string Username { get; set; }

    [JsonPropertyName("clientid")]
    public string ClientId { get; set; } 

    [JsonPropertyName("topic")]
    public string Topic { get; set; }

    [JsonPropertyName("acc")] 
    public string Acc { get; set; }
}