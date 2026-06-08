using System;

namespace LabControlApi.DTOs.MachineMetric
{
    public class MachineMetricResponseDto
    {
        public long Id { get; set; }
        public Guid MachineId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Value { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
