using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeviceManagement.Data;
using Microsoft.AspNetCore.Authorization;

namespace DeviceManagement.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly DeviceContext _context;

    public UsersController(DeviceContext context)
    {
        _context = context;
    }
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = int.Parse(User.Claims.First(c => c.Type == "userId").Value);

        var user = await _context.Users
            .Include(u => u.Devices)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return NotFound();

        return Ok(new {
            user.Id,
            user.Email,
            Devices = user.Devices.Select(d => new { d.Id, d.Name })
        });
    }
}
