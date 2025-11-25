using System.ComponentModel.DataAnnotations.Schema;

namespace DeviceManagement.Models;

public class Device
{
    public Device()
    {
        //For setting the clock 
        TimeZoneId = "America/New_York";
    }

    public void AddStop(Stop stop){
        if(!Stops.Any(s => s.StopId == stop.StopId))
        {
            Stops.Add(stop);
        }
    }
    public int Id { get; set; }
    public string UniqueId { get; set; }
    public string Name { get; set; }
    public int UserId { get; set; }

    //API Key used to access MQTT
    public string ApiKey { get; set; }

    private string TimeZoneId { get; set; }

    [NotMapped] // Tells EF Core to ignore this property
    public TimeZoneInfo TimeZone
    {
        get 
        {
            try 
            {
                return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                // Fallback to UTC if the ID is invalid or missing
                return TimeZoneInfo.Utc;
            }
        }
        set 
        {
            if (value != null)
            {
                TimeZoneId = value.Id;
            }
        }
    }

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    public List<Stop>? Stops { get; set; } = new();
}
