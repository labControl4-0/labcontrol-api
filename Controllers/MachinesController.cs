using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using LabControlApi.DTOs.Machine;
using LabControlApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabControlApi.Controllers
{
    [ApiController]
    [Route("api/machines")]
    [Authorize]
    public class MachinesController : ControllerBase
    {
        private readonly IMachineService _machineService;

        public MachinesController(IMachineService machineService)
        {
            _machineService = machineService;
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

        [HttpGet("plant/{plantId}")]
        public async Task<ActionResult<IEnumerable<MachineResponseDto>>> GetMachines(Guid plantId)
        {
            var userId = GetUserId();
            var machines = await _machineService.GetMachines(plantId, userId);
            return Ok(machines);
        }

        [HttpPost]
        public async Task<ActionResult<MachineResponseDto>> CreateMachine(CreateMachineDto createDto)
        {
            var userId = GetUserId();
            var newMachine = await _machineService.CreateMachine(createDto, userId);
            return Ok(newMachine);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMachine(Guid id, UpdateMachineDto updateDto)
        {
            var userId = GetUserId();
            await _machineService.UpdateMachine(id, updateDto, userId);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMachine(Guid id)
        {
            var userId = GetUserId();
            await _machineService.DeleteMachine(id, userId);
            return NoContent();
        }
    }
}
