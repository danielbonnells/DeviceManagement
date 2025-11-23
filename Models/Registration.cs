using System.Security.Cryptography;

namespace DeviceManagement.Models;

//This entity is created when the device requests a temp code
//Which it then shows to the user
public class Registration
{
    public Registration()
    {
        CreatedAt = DateTime.UtcNow;
        Status = "PENDING";
    }
    public int Id { get; set; }

    public string Status { get; set; }
    //Device MAC address
    public string UniqueId { get; set; }

    //Device temp code to bind to user
    public string TempCode { get; set; }

    //used for expiration
    public DateTime CreatedAt { get; set; }
}