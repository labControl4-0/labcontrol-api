using System;

namespace LabControlApi.DTOs.Dashboard
{
    public class DashboardPlantSummaryDto
    {
        public Guid PlantId { get; set; }
        public string PlantName { get; set; } = string.Empty;
        public int TotalMachines { get; set; }
        public int ActiveMachines { get; set; }
        public int AlertMachines { get; set; }
        public int OfflineMachines { get; set; }
        public int OpenEventsCount { get; set; }

        // Indicadores consolidados
        public double AverageTemperature { get; set; }
        public string TemperatureUnit { get; set; } = "°C";

        public double AverageVibration { get; set; }
        public string VibrationUnit { get; set; } = "mm/s";

        public double TotalEnergyAccumulated { get; set; }
        public string EnergyUnit { get; set; } = "kWh";

        public double AveragePower { get; set; }
        public string PowerUnit { get; set; } = "kW";
    }
}

