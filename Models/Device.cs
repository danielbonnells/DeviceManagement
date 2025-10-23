namespace DeviceManagement.Models;

public class Device
{
    public int Id { get; set; }
    public string UniqueId { get; set; } = "";
    public string Name { get; set; } = "";
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}
