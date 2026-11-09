using System;
using System.Threading.Tasks;
using LabControlApi.DTOs.Dashboard;

namespace LabControlApi.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardPlantSummaryDto> GetPlantSummaryAsync(Guid plantId, Guid userId);
        Task<DashboardTimeSeriesResponseDto> GetAggregatedMetricsAsync(
            Guid? plantId,
            Guid? machineId,
            DateTime? startDate,
            DateTime? endDate,
            string? interval,
            Guid userId);
    }
}

