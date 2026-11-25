namespace LabControlApi.Models;

public class PushDevice
{
    public string Token { get; set; } = "";
    public Guid UserId { get; set; }
    public Guid RegistrationId { get; set; }
    public string Platform { get; set; } = "";
}
