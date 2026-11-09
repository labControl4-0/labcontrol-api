using System;

namespace LabControlApi.DTOs.Dashboard
{
    public class MetricTimeSeriesPointDto
    {
        public DateTime Timestamp { get; set; }
        public string TimeLabel { get; set; } = string.Empty; // Ex: "09/11 14:00" ou "09/11"

        // Temperatura
        public double AvgTemperature { get; set; }
        public double MaxTemperature { get; set; }

        // Vibração
        public double AvgVibration { get; set; }
        public double MaxVibration { get; set; }

        // RPM
        public double AvgRpm { get; set; }

        // Energia Acumulada no período (kWh)
        public double TotalEnergyUsage { get; set; }

        // Potência média no intervalo (kW)
        public double AveragePower { get; set; }

        public int SampleCount { get; set; }
    }
}

