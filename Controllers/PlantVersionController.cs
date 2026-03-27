using LabControlApi.DTOs;
using LabControlApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LabControlApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlantVersionController : ControllerBase
    {
        private readonly IPlantVersionService _service;

        public PlantVersionController(IPlantVersionService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var plantVersions = await _service.GetAllAsync();
            return Ok(plantVersions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var plantVersion = await _service.GetByIdAsync(id);
            if (plantVersion == null) return NotFound();
            return Ok(plantVersion);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePlantVersionDto createDto)
        {
            await _service.CreateAsync(createDto);
            return CreatedAtAction(nameof(GetById), new { id = createDto.Name }, createDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, CreatePlantVersionDto updateDto)
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
