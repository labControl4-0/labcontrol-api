using System;

namespace LabControlApi.Models
{
    public class MachineEvent
    {
        public Guid Id { get; set; }
        public Guid MachineId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }

        // Navigation property
        public Machine Machine { get; set; } = null!;
    }
}
