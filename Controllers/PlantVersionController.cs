using LabControlApi.DTOs.PlantVersion;
using LabControlApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LabControlApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlantVersionController : ControllerBase
    {
        private readonly IPlantVersionService _plantVersionService;

        public PlantVersionController(IPlantVersionService plantVersionService)
        {
            _plantVersionService = plantVersionService;
        }

        [HttpGet("{plantId}")]
        public async Task<IActionResult> GetVersions(Guid plantId)
        {
            var userId = Guid.Parse(User.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            var versions = await _plantVersionService.GetVersions(plantId, userId);
            return Ok(versions);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreatePlantVersionDto dto)
        {
            var userId = Guid.Parse(User.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            var version = await _plantVersionService.CreateVersion(dto, userId);
            return Ok(version);
        }

        [HttpPost("{versionId}/clone")]
        public async Task<IActionResult> Clone(Guid versionId)
        {
            var userId = Guid.Parse(User.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            var version = await _plantVersionService.CloneVersion(versionId, userId);

            if (version == null)
                return NotFound();

            return Ok(version);
        }

        [HttpPost("{versionId}/activate")]
        public async Task<IActionResult> Activate(Guid versionId)
        {
            var userId = Guid.Parse(User.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier).Value);
            await _plantVersionService.ActivateVersion(versionId, userId);
            return NoContent();
        }
    }
}