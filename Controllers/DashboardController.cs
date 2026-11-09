using System;
using System.Security.Claims;
using System.Threading.Tasks;
using LabControlApi.DTOs.Dashboard;
using LabControlApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabControlApi.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        private Guid GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Usuário não autenticado.");
            }
            return new Guid(userId);
        }

        /// <summary>
        /// Retorna os indicadores consolidados de uma planta (total de máquinas, ativas, em alerta e médias das últimas 24h).
        /// </summary>
        [HttpGet("plants/{plantId}/summary")]
        public async Task<ActionResult<DashboardPlantSummaryDto>> GetPlantSummary(Guid plantId)
        {
            try
            {
                var userId = GetUserId();
                var summary = await _dashboardService.GetPlantSummaryAsync(plantId, userId);
                return Ok(summary);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Retorna série temporal agregada de métricas (por hora ou por dia) para exibição em gráficos.
        /// </summary>
        [HttpGet("metrics/aggregated")]
        public async Task<ActionResult<DashboardTimeSeriesResponseDto>> GetAggregatedMetrics(
            [FromQuery] Guid? plantId,
            [FromQuery] Guid? machineId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? interval = "hour")
        {
            var userId = GetUserId();
            var result = await _dashboardService.GetAggregatedMetricsAsync(
                plantId,
                machineId,
                startDate,
                endDate,
                interval,
                userId);

            return Ok(result);
        }
    }
}

