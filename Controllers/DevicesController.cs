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

    [HttpPost("register")]
    public async Task<IActionResult> RegisterDevice([FromBody] Device device)
    {
        var userId = int.Parse(User.Claims.First(c => c.Type == "userId").Value);
        device.UserId = userId;

        _context.Devices.Add(device);
        await _context.SaveChangesAsync();
        return Ok(device);
    }
}
