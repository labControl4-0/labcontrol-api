using System;

namespace LabControlApi.Models
{
    public class SyncOperationLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public Guid ClientOperationId { get; set; }
        public string EntityType { get; set; } = string.Empty; // "Plant", "Machine", "Sector", "MachineEvent"
        public Guid? EntityId { get; set; }
        public string Action { get; set; } = string.Empty; // "CREATE", "UPDATE", "DELETE"
        public string Status { get; set; } = string.Empty; // "SUCCESS", "CONFLICT", "ERROR", "ALREADY_PROCESSED"
        public string? ResponsePayloadJson { get; set; }
        public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public User User { get; set; } = null!;
    }
}

