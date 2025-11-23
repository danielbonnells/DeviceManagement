
//This is sent from the frontend after a user is logged in
public class RegisterDeviceDto
{
    //Device custom name from user
    public string Name { get; set; } = "";

    //Device temp code to bind to user. User submits this and
    //it gets checked against the registration table
    //the registration table will confirm if the code is expired,
    //if not, it provides the unique device id/mac address and creates a device
    public string TempCode { get; set; }

}