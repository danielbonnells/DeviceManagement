namespace DeviceManagement.Models;

public class Stop
{
    public Stop(string stopId, string stopName)
    {
        StopId = stopId;
        StopName = stopName;
    }
    public int Id { get; set; }
    public string StopId { get; set; }
    public string StopName { get; set; }
    public float? StopLon { get; set; }
    public float? StopLat { get; set; }
    public string? StopType { get; set; }
    public string? ParentStation { get; set; }
    public List<Device>? Devices { get; set; } = new();

}