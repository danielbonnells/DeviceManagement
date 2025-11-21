using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeviceManagement.Data;
using DeviceManagement.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace DeviceManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class DevicesController : ControllerBase
{
    private readonly DeviceContext _context;

    public DevicesController(DeviceContext context)
    {
        _context = context;
        
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetMyDevices()
    {
        var userId = int.Parse(User.Claims.First(c => c.Type == "userId").Value);
        var devices = await _context.Devices.Where(d => d.UserId == userId).ToListAsync();
        return Ok(devices);
    }

    // [HttpPost("register")]
    // public async Task<IActionResult> RegisterDevice([FromForm] Device device)
    // {
    //     var userId = int.Parse(User.Claims.First(c => c.Type == "userId").Value);
    //     device.UserId = userId;

    //     _context.Devices.Add(device);
    //     await _context.SaveChangesAsync();
    //     return Ok(device);
    // }

    [HttpPost("register")]
public async Task<IActionResult> RegisterDevice([FromForm] Device device)
{
    // 1. Get the current user's ID from the JWT claim
    if (!int.TryParse(User.Claims.First(c => c.Type == "userId").Value, out int currentUserId))
    {
        return Unauthorized("User ID claim is missing or invalid.");
    }

    // 2. Check if a device with this UniqueId already exists
    var existingDevice = await _context.Devices
        .Where(d => d.UniqueId == device.UniqueId)
        .FirstOrDefaultAsync(); 

    // 3. Handle Validation Scenarios
    if (existingDevice != null)
    {
        // 3a. Device exists and belongs to *another* user
        if (existingDevice.UserId != currentUserId)
        {
            // Return a 409 Conflict error for unauthorized access attempt
            return Conflict(new 
            {
                Title = "Device Conflict",
                Status = 409,
                Message = "This device is already registered to another user."
            });
        }
        
        // 3b. Device exists and belongs to the *current* user
        // You can treat this as a success or an update. For registration, 
        // we'll treat it as already registered and return success or a status update.
        return Ok(new 
        {
            Message = "Device is already registered to this user.",
            Device = existingDevice
        });
    }

    // 4. Proceed with new registration (Device is unique and not registered)
    device.UserId = currentUserId;
    device.RegisteredAt = DateTime.UtcNow; // Ensure this is set correctly

    _context.Devices.Add(device);
    await _context.SaveChangesAsync();
    
    // Return 201 Created for a new resource
    return CreatedAtAction(nameof(RegisterDevice), new { id = device.Id }, device);
}
}
