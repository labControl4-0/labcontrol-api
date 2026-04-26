using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using LabControlApi.DTOs.Plant;
using LabControlApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabControlApi.Controllers
{
    [ApiController]
    [Route("api/plants")]
    [Authorize]
    public class PlantsController : ControllerBase
    {
        private readonly IPlantService _plantService;

        public PlantsController(IPlantService plantService)
        {
            _plantService = plantService;
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlantResponseDto>>> GetPlants()
        {
            var userId = GetUserId();
            var plants = await _plantService.GetPlants(userId);
            return Ok(plants);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PlantResponseDto>> GetPlant(Guid id)
        {
            var userId = GetUserId();
            var plant = await _plantService.GetPlant(id, userId);
            if (plant == null)
            {
                return NotFound();
            }
            return Ok(plant);
        }

        [HttpPost]
        public async Task<ActionResult<PlantResponseDto>> CreatePlant(CreatePlantDto createPlantDto)
        {
            var userId = GetUserId();
            var newPlant = await _plantService.CreatePlant(createPlantDto, userId);
            return CreatedAtAction(nameof(GetPlant), new { id = newPlant.Id }, newPlant);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePlant(Guid id, UpdatePlantDto updatePlantDto)
        {
            var userId = GetUserId();
            await _plantService.UpdatePlant(id, updatePlantDto, userId);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlant(Guid id)
        {
            var userId = GetUserId();
            await _plantService.DeletePlant(id, userId);
            return NoContent();
        }
    }
}
