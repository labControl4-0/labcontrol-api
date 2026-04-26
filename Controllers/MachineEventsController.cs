using LabControlApi.DTOs.MachineEvent;
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
    [Route("api/machine-events")]
    [Authorize]
    public class MachineEventsController : ControllerBase
    {
        private readonly IMachineEventService _eventService;

        public MachineEventsController(IMachineEventService eventService)
        {
            _eventService = eventService;
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
        public async Task<ActionResult<IEnumerable<MachineEventResponseDto>>> GetEvents(Guid machineId)
        {
            var userId = GetUserId();
            var events = await _eventService.GetEvents(machineId, userId);
            return Ok(events);
        }

        [HttpPost]
        public async Task<ActionResult<MachineEventResponseDto>> CreateEvent(CreateMachineEventDto dto)
        {
            var userId = Guid.Parse(User.Claims.First(c => c.Type == "id").Value);
            var newEvent = await _eventService.CreateEvent(dto, userId);
            return CreatedAtAction(nameof(GetEvents), new { machineId = newEvent.MachineId }, newEvent);
        }

        [HttpPatch("{id}/resolve")]
        public async Task<ActionResult<MachineEventResponseDto>> ResolveEvent(Guid id)
        {
            var userId = GetUserId();
            var resolvedEvent = await _eventService.ResolveEvent(id, userId);
            return Ok(resolvedEvent);
        }
    }
}
