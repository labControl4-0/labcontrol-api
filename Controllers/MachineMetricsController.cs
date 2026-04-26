using LabControlApi.DTOs.MachineMetric;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using LabControlApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabControlApi.Controllers
{
    [ApiController]
    [Route("api/machine-metrics")]
    [Authorize]
    public class MachineMetricsController : ControllerBase
    {
        private readonly IMachineMetricService _metricService;

        public MachineMetricsController(IMachineMetricService metricService)
        {
            _metricService = metricService;
        }

        private Guid GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User not authenticated.");
            }
            return new Guid(userId);
        }

        [HttpGet("machine/{machineId}")]
        public async Task<ActionResult<IEnumerable<MachineMetricResponseDto>>> GetMetrics(Guid machineId)
        {
            var userId = GetUserId();
            var metrics = await _metricService.GetMetrics(machineId, userId);
            return Ok(metrics);
        }

        [HttpPost]
        public async Task<ActionResult<MachineMetricResponseDto>> AddMetric(CreateMachineMetricDto createDto)
        {
            var userId = GetUserId();
            var newMetric = await _metricService.AddMetric(createDto, userId);
            return Ok(newMetric);
        }
    }
}
