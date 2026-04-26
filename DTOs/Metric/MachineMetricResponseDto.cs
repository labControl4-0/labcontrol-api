using System;

namespace LabControlApi.DTOs.Metric
{
    public class MachineMetricResponseDto
    {
        public long Id { get; set; }
        public Guid MachineId { get; set; }
        public double Temperature { get; set; }
        public double Rpm { get; set; }
        public double Vibration { get; set; }
        public double EnergyUsage { get; set; }
        public DateTime CollectedAt { get; set; }
    }
}
