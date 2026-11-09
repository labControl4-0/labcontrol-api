using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LabControlApi.Data;
using LabControlApi.DTOs.Dashboard;
using LabControlApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LabControlApi.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardPlantSummaryDto> GetPlantSummaryAsync(Guid plantId, Guid userId)
        {
            // Valida se a planta pertence ao usuário autenticado
            var plant = await _context.Plants
                .Include(p => p.Machines)
                    .ThenInclude(m => m.MachineEvents.Where(e => e.ResolvedAt == null))
                .FirstOrDefaultAsync(p => p.Id == plantId && p.UserId == userId);

            if (plant == null)
            {
                throw new KeyNotFoundException("Planta não encontrada ou não pertence ao usuário.");
            }

            var machineIds = plant.Machines.Select(m => m.Id).ToList();

            // Totalizadores de máquinas
            int totalMachines = plant.Machines.Count;
            int alertMachines = plant.Machines.Count(m => m.MachineEvents.Any());
            int activeMachines = plant.Machines.Count(m => m.Status.Equals("Active", StringComparison.OrdinalIgnoreCase) ||
                                                          m.Status.Equals("Ativa", StringComparison.OrdinalIgnoreCase) ||
                                                          m.Status.Equals("Operando", StringComparison.OrdinalIgnoreCase));
            int offlineMachines = totalMachines - activeMachines;
            int totalOpenEvents = plant.Machines.Sum(m => m.MachineEvents.Count);

            // Médias consolidadas com base nas últimas 24 horas
            var cutoff = DateTime.UtcNow.AddHours(-24);
            var recentMetrics = await _context.MachineMetrics
                .Where(mm => machineIds.Contains(mm.MachineId) && mm.CollectedAt >= cutoff)
                .ToListAsync();

            double avgTemp = recentMetrics.Any(m => m.Temperature > 0) ? recentMetrics.Where(m => m.Temperature > 0).Average(m => m.Temperature) : 0;
            double avgVib = recentMetrics.Any(m => m.Vibration > 0) ? recentMetrics.Where(m => m.Vibration > 0).Average(m => m.Vibration) : 0;
            double totalEnergy = recentMetrics.Sum(m => m.EnergyUsage);
            double avgPower = recentMetrics.Any(m => m.EnergyUsage > 0) ? recentMetrics.Average(m => m.EnergyUsage) : 0;

            return new DashboardPlantSummaryDto
            {
                PlantId = plant.Id,
                PlantName = plant.Name,
                TotalMachines = totalMachines,
                ActiveMachines = activeMachines,
                AlertMachines = alertMachines,
                OfflineMachines = offlineMachines,
                OpenEventsCount = totalOpenEvents,
                AverageTemperature = Math.Round(avgTemp, 1),
                AverageVibration = Math.Round(avgVib, 2),
                TotalEnergyAccumulated = Math.Round(totalEnergy, 2),
                AveragePower = Math.Round(avgPower, 2)
            };
        }

        public async Task<DashboardTimeSeriesResponseDto> GetAggregatedMetricsAsync(
            Guid? plantId,
            Guid? machineId,
            DateTime? startDate,
            DateTime? endDate,
            string? interval,
            Guid userId)
        {
            var end = endDate ?? DateTime.UtcNow;
            var start = startDate ?? end.AddDays(-1);
            var groupInterval = string.Equals(interval, "day", StringComparison.OrdinalIgnoreCase) ? "day" : "hour";

            // Determinar a lista de MachineIds autorizados para este usuário
            var allowedMachineQuery = _context.Machines
                .Include(m => m.Plant)
                .Where(m => m.Plant.UserId == userId);

            if (plantId.HasValue)
            {
                allowedMachineQuery = allowedMachineQuery.Where(m => m.PlantId == plantId.Value);
            }

            if (machineId.HasValue)
            {
                allowedMachineQuery = allowedMachineQuery.Where(m => m.Id == machineId.Value);
            }

            var validMachineIds = await allowedMachineQuery.Select(m => m.Id).ToListAsync();

            if (!validMachineIds.Any())
            {
                return new DashboardTimeSeriesResponseDto
                {
                    PlantId = plantId,
                    MachineId = machineId,
                    StartDate = start,
                    EndDate = end,
                    Interval = groupInterval,
                    Series = new List<MetricTimeSeriesPointDto>()
                };
            }

            // Buscar métricas no intervalo
            var rawMetrics = await _context.MachineMetrics
                .Where(m => validMachineIds.Contains(m.MachineId) && m.CollectedAt >= start && m.CollectedAt <= end)
                .Select(m => new
                {
                    m.CollectedAt,
                    m.Temperature,
                    m.Vibration,
                    m.Rpm,
                    m.EnergyUsage
                })
                .ToListAsync();

            // Agrupamento por hora ou dia
            var grouped = rawMetrics
                .GroupBy(m => groupInterval == "day"
                    ? new DateTime(m.CollectedAt.Year, m.CollectedAt.Month, m.CollectedAt.Day, 0, 0, 0, DateTimeKind.Utc)
                    : new DateTime(m.CollectedAt.Year, m.CollectedAt.Month, m.CollectedAt.Day, m.CollectedAt.Hour, 0, 0, DateTimeKind.Utc))
                .OrderBy(g => g.Key)
                .Select(g =>
                {
                    var temps = g.Where(x => x.Temperature > 0).Select(x => x.Temperature).ToList();
                    var vibs = g.Where(x => x.Vibration > 0).Select(x => x.Vibration).ToList();
                    var rpms = g.Where(x => x.Rpm > 0).Select(x => x.Rpm).ToList();

                    return new MetricTimeSeriesPointDto
                    {
                        Timestamp = g.Key,
                        TimeLabel = groupInterval == "day"
                            ? g.Key.ToString("dd/MM")
                            : g.Key.ToString("dd/MM HH:00"),
                        AvgTemperature = temps.Any() ? Math.Round(temps.Average(), 1) : 0,
                        MaxTemperature = temps.Any() ? Math.Round(temps.Max(), 1) : 0,
                        AvgVibration = vibs.Any() ? Math.Round(vibs.Average(), 2) : 0,
                        MaxVibration = vibs.Any() ? Math.Round(vibs.Max(), 2) : 0,
                        AvgRpm = rpms.Any() ? Math.Round(rpms.Average(), 0) : 0,
                        TotalEnergyUsage = Math.Round(g.Sum(x => x.EnergyUsage), 2),
                        AveragePower = Math.Round(g.Average(x => x.EnergyUsage), 2),
                        SampleCount = g.Count()
                    };
                })
                .ToList();

            return new DashboardTimeSeriesResponseDto
            {
                PlantId = plantId,
                MachineId = machineId,
                StartDate = start,
                EndDate = end,
                Interval = groupInterval,
                Series = grouped
            };
        }
    }
}

