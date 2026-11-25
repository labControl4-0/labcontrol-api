namespace LabControlApi.Models;

public class PushDelivery
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EventId { get; set; }
    public MachineEvent Event { get; set; } = null!;
    public Guid UserId { get; set; }
    public Guid RegistrationId { get; set; }
    public DateTime NextAttemptAt { get; set; } = DateTime.UtcNow;
    public int Attempts { get; set; }
    public string? TicketId { get; set; }
    public string? Outcome { get; set; }
}
