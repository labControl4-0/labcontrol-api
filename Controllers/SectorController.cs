using LabControlApi.DTOs.Sector;
using LabControlApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LabControlApi.Controllers
{
    [ApiController]
    [Route("api/sectors")]
    public class SectorController : ControllerBase
    {
        private readonly ISectorService _sectorService;

        public SectorController(ISectorService sectorService)
        {
            _sectorService = sectorService;
        }

        [HttpGet("plant/{plantId}")]
        public async Task<ActionResult<IEnumerable<SectorResponseDto>>> GetSectorsByPlant(Guid plantId)
        {
            var userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            var sectors = await _sectorService.GetSectors(plantId, userId);
            return Ok(sectors);
        }

        [HttpPost]
        public async Task<ActionResult<SectorResponseDto>> CreateSector(CreateSectorDto createDto)
        {
            var userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            var newSector = await _sectorService.CreateSector(createDto, userId);
            return CreatedAtAction(nameof(GetSectorsByPlant), new { plantId = newSector.PlantId }, newSector);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSector(Guid id, UpdateSectorDto updateDto)
        {
            var userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            await _sectorService.UpdateSector(id, updateDto, userId);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSector(Guid id)
        {
            var userId = Guid.Parse(User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            await _sectorService.DeleteSector(id, userId);
            return NoContent();
        }
    }
}
