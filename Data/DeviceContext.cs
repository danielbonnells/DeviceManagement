using Microsoft.EntityFrameworkCore;
using DeviceManagement.Models;

namespace DeviceManagement.Data;

public class DeviceContext : DbContext
{
    public DeviceContext(DbContextOptions<DeviceContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Device> Devices => Set<Device>();
}
