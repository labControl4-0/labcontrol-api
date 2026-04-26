using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LabControlApi.DTOs.Plant;
using LabControlApi.Models;
using LabControlApi.Repositories.Interfaces;
using LabControlApi.Services.Interfaces;

namespace LabControlApi.Services
{
    public class PlantService : IPlantService
    {
        private readonly IPlantRepository _plantRepository;

        public PlantService(IPlantRepository plantRepository)
        {
            _plantRepository = plantRepository;
        }

        public async Task<IEnumerable<PlantResponseDto>> GetPlants(Guid userId)
        {
            var plants = await _plantRepository.GetByUserIdAsync(userId);
            return plants.Select(p => new PlantResponseDto
            {
                Id = p.Id,
                UserId = p.UserId,
                Name = p.Name,
                Description = p.Description,
                Scale = p.Scale,
                WidthUnits = p.WidthUnits,
                HeightUnits = p.HeightUnits,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            });
        }

        public async Task<PlantResponseDto?> GetPlant(Guid id, Guid userId)
        {
            var plant = await _plantRepository.GetByIdAsync(id, userId);
            if (plant == null) return null;

            return new PlantResponseDto
            {
                Id = plant.Id,
                UserId = plant.UserId,
                Name = plant.Name,
                Description = plant.Description,
                Scale = plant.Scale,
                WidthUnits = plant.WidthUnits,
                HeightUnits = plant.HeightUnits,
                CreatedAt = plant.CreatedAt,
                UpdatedAt = plant.UpdatedAt
            };
        }

        public async Task<PlantResponseDto> CreatePlant(CreatePlantDto dto, Guid userId)
        {
            var plant = new Plant
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = dto.Name,
                Description = dto.Description,
                Scale = dto.Scale,
                WidthUnits = dto.WidthUnits,
                HeightUnits = dto.HeightUnits
            };

            var newPlant = await _plantRepository.AddAsync(plant);

            return new PlantResponseDto
            {
                Id = newPlant.Id,
                UserId = newPlant.UserId,
                Name = newPlant.Name,
                Description = newPlant.Description,
                Scale = newPlant.Scale,
                WidthUnits = newPlant.WidthUnits,
                HeightUnits = newPlant.HeightUnits,
                CreatedAt = newPlant.CreatedAt,
                UpdatedAt = newPlant.UpdatedAt
            };
        }

        public async Task UpdatePlant(Guid id, UpdatePlantDto dto, Guid userId)
        {
            var plant = await _plantRepository.GetByIdAsync(id, userId);
            if (plant == null)
            {
                // Or throw a custom not found exception
                return;
            }

            plant.Name = dto.Name;
            plant.Description = dto.Description;
            plant.Scale = dto.Scale;
            plant.WidthUnits = dto.WidthUnits;
            plant.HeightUnits = dto.HeightUnits;
            plant.UpdatedAt = DateTime.UtcNow;

            await _plantRepository.UpdateAsync(plant);
        }

        public async Task DeletePlant(Guid id, Guid userId)
        {
            await _plantRepository.DeleteAsync(id, userId);
        }
    }
}
