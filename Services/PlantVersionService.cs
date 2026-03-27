using LabControlApi.DTOs;
using LabControlApi.Models;
using LabControlApi.Repositories.Interfaces;
using LabControlApi.Services.Interfaces;

namespace LabControlApi.Services
{
    public class PlantVersionService : IPlantVersionService
    {
        private readonly IPlantVersionRepository _repository;

        public PlantVersionService(IPlantVersionRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<PlantVersionResponseDto>> GetAllAsync()
        {
            var plantVersions = await _repository.GetAllAsync();
            return plantVersions.Select(p => new PlantVersionResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                WidthUnits = p.WidthUnits,
                HeightUnits = p.HeightUnits,
                Scale = p.Scale,
                VersionNumber = p.VersionNumber,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt
            });
        }

        public async Task<PlantVersionResponseDto?> GetByIdAsync(Guid id)
        {
            var plantVersion = await _repository.GetByIdAsync(id);
            if (plantVersion == null) return null;

            return new PlantVersionResponseDto
            {
                Id = plantVersion.Id,
                Name = plantVersion.Name,
                WidthUnits = plantVersion.WidthUnits,
                HeightUnits = plantVersion.HeightUnits,
                Scale = plantVersion.Scale,
                VersionNumber = plantVersion.VersionNumber,
                IsActive = plantVersion.IsActive,
                CreatedAt = plantVersion.CreatedAt
            };
        }

        public async Task CreateAsync(CreatePlantVersionDto createDto)
        {
            var plantVersion = new PlantVersion
            {
                Id = Guid.NewGuid(),
                Name = createDto.Name,
                WidthUnits = createDto.WidthUnits,
                HeightUnits = createDto.HeightUnits,
                Scale = createDto.Scale
            };

            await _repository.CreateAsync(plantVersion);
        }

        public async Task UpdateAsync(Guid id, CreatePlantVersionDto updateDto)
        {
            var plantVersion = await _repository.GetByIdAsync(id);
            if (plantVersion == null) return;

            plantVersion.Name = updateDto.Name;
            plantVersion.WidthUnits = updateDto.WidthUnits;
            plantVersion.HeightUnits = updateDto.HeightUnits;
            plantVersion.Scale = updateDto.Scale;

            await _repository.UpdateAsync(plantVersion);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}
