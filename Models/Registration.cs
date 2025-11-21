namespace DeviceManagement.Models;

public class Registration
{
    public string Id { get; set; }
    public string UniqueId { get; set; }

    public string TempCode { get; set; }

    public DateTime CreatedAt = DateTime.UtcNow;
}