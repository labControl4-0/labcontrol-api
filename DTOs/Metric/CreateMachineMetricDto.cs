using System;

namespace LabControlApi.DTOs.Metric
{
    public class CreateMachineMetricDto
    {
        public Guid MachineId { get; set; }
        public double Temperature { get; set; }
        public double Rpm { get; set; }
        public double Vibration { get; set; }
        public double EnergyUsage { get; set; }
    }
}
