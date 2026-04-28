using LabControlApi.DTOs;
using LabControlApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("plantVersion/{plantVersionId}")]
        public async Task<ActionResult<IEnumerable<SectorResponseDto>>> GetSectorsByPlantVersion(Guid plantVersionId)
        {
            var userId = Guid.Parse(User.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            var sectors = await _sectorService.GetSectors(plantVersionId, userId);
            return Ok(sectors);
        }

        [HttpPost]
        public async Task<ActionResult<SectorResponseDto>> CreateSector(CreateSectorDto createDto)
        {
            var userId = Guid.Parse(User.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            var newSector = await _sectorService.CreateSector(createDto, userId);
            return CreatedAtAction(nameof(GetSectorsByPlantVersion), new { plantVersionId = newSector.PlantVersionId }, newSector);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSector(Guid id, UpdateSectorDto updateDto)
        {
            var userId = Guid.Parse(User.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            await _sectorService.UpdateSector(id, updateDto, userId);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSector(Guid id)
        {
            var userId = Guid.Parse(User.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            await _sectorService.DeleteSector(id, userId);
            return NoContent();
        }
    }
}
