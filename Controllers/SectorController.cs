using LabControlApi.DTOs;
using LabControlApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LabControlApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SectorController : ControllerBase
    {
        private readonly ISectorService _service;

        public SectorController(ISectorService service)
        {
            _service = service;
        }

        [HttpGet("plant/{plantVersionId}")]
        public async Task<IActionResult> GetByPlantVersionId(Guid plantVersionId)
        {
            var sectors = await _service.GetByPlantVersionIdAsync(plantVersionId);
            return Ok(sectors);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var sector = await _service.GetByIdAsync(id);
            if (sector == null) return NotFound();
            return Ok(sector);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSectorDto createDto)
        {
            await _service.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = createDto.PlantVersionId }, createDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateSectorDto updateDto)
        {
            await _service.UpdateAsync(id, updateDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
