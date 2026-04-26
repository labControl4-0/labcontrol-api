using System;

namespace LabControlApi.DTOs.MachineEvent
{
    public class CreateMachineEventDto
    {
        public Guid MachineId { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
