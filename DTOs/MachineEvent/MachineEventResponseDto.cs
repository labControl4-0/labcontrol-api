using System;

namespace LabControlApi.DTOs.Event
{
    public class MachineEventResponseDto
    {
        public Guid Id { get; set; }
        public Guid MachineId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
}
