using System;

namespace LabControlApi.DTOs
{
    public class CreateIotMetricDto
    {
        public string MachineId { get; set; }
        public double Temperature { get; set; }
        public double Vibration { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
