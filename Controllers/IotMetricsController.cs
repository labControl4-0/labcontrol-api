using Microsoft.AspNetCore.Authorization;
using LabControlApi.DTOs;
using LabControlApi.DTOs.MachineMetric;
using LabControlApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace LabControlApi.Controllers
{
    [ApiController]
    [Route("api/iot/metrics")]
    [AllowAnonymous]
    public class IotMetricsController : ControllerBase
    {
        private readonly IMachineMetricService _metricService;

        public IotMetricsController(IMachineMetricService metricService)
        {
            _metricService = metricService;
        }

        [HttpPost]
        public async Task<IActionResult> AddMetric([FromBody] CreateIotMetricDto createDto)
        {
            if (!Guid.TryParse(createDto.MachineId, out var machineId))
            {
                return BadRequest("Invalid MachineId format.");
            }

            var temperatureDto = new CreateMachineMetricDto
            {
                MachineId = machineId,
                Name = "temperature",
                Value = createDto.Temperature,
                Timestamp = createDto.Timestamp
            };

            var vibrationDto = new CreateMachineMetricDto
            {
                MachineId = machineId,
                Name = "vibration",
                Value = createDto.Vibration,
                Timestamp = createDto.Timestamp
            };

            await _metricService.AddMetric(temperatureDto, Guid.Empty); 
            await _metricService.AddMetric(vibrationDto, Guid.Empty);

            return Ok();
        }
    }
}
