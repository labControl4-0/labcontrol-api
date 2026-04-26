namespace LabControlApi.Controllers
{
    [ApiController]
    [Route("api/plants/{plantId}/versions")]
    public class PlantVersionsController : ControllerBase
    {
        private readonly IPlantVersionService _plantVersionService;
        private readonly ILogger<PlantVersionsController> _logger;

        public PlantVersionsController(IPlantVersionService plantVersionService, ILogger<PlantVersionsController> logger)
        {
            _plantVersionService = plantVersionService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlantVersionResponseDto>>> GetVersions(Guid plantId)
        {
            var userId = Guid.Parse(User.Claims.First(c => c.Type == "id").Value);
            var versions = await _plantVersionService.GetVersions(plantId, userId);
            return Ok(versions);
        }

        [HttpPost]
        public async Task<ActionResult<PlantVersionResponseDto>> CreateVersion(Guid plantId, CreatePlantVersionDto dto)
        {
            var userId = Guid.Parse(User.Claims.First(c => c.Type == "id").Value);
            dto.PlantId = plantId;
            var newVersion = await _plantVersionService.CreateVersion(dto, userId);
            return CreatedAtAction(nameof(GetVersions), new { plantId = newVersion.PlantId }, newVersion);
        }

        [HttpPost("{versionId}/clone")]
        public async Task<ActionResult<PlantVersionResponseDto>> CloneVersion(Guid versionId)
        {
            var userId = Guid.Parse(User.Claims.First(c => c.Type == "id").Value);
            var clonedVersion = await _plantVersionService.CloneVersion(versionId, userId);
            if (clonedVersion == null)
            {
                return NotFound();
            }
            return Ok(clonedVersion);
        }

        [HttpPatch("{versionId}/activate")]
        public async Task<IActionResult> ActivateVersion(Guid versionId)
        {
            var userId = Guid.Parse(User.Claims.First(c => c.Type == "id").Value);
            await _plantVersionService.ActivateVersion(versionId, userId);
            return NoContent();
        }
    }
}
