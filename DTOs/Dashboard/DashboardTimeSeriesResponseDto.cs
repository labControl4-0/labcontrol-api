using System;
using System.Collections.Generic;

namespace LabControlApi.DTOs.Dashboard
{
    public class DashboardTimeSeriesResponseDto
    {
        public Guid? PlantId { get; set; }
        public Guid? MachineId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Interval { get; set; } = "hour"; // "hour", "day"

        // Unidades explicitamente definidas no contrato tipado
        public Dictionary<string, string> Units { get; set; } = new()
        {
            { "temperature", "°C" },
            { "vibration", "mm/s" },
            { "rpm", "RPM" },
            { "energyUsage", "kWh" },
            { "power", "kW" }
        };

        public List<MetricTimeSeriesPointDto> Series { get; set; } = new();
    }
}

