using LabControlApi.DTOs;
using LabControlApi.DTOs.Sector;
using LabControlApi.Models;
using LabControlApi.Repositories.Interfaces;
using LabControlApi.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace LabControlApi.Services
{
    public class SectorService : ISectorService
    {
        private readonly ISectorRepository _sectorRepository;
        private readonly IPlantVersionRepository _plantVersionRepository;
        private readonly IPlantRepository _plantRepository;

        public SectorService(ISectorRepository sectorRepository, IPlantVersionRepository plantVersionRepository, IPlantRepository plantRepository)
        {
            _sectorRepository = sectorRepository;
            _plantVersionRepository = plantVersionRepository;
            _plantRepository = plantRepository;
        }

        public async Task<IEnumerable<SectorResponseDto>> GetSectors(Guid versionId, Guid userId)
        {
            var version = await _plantVersionRepository.GetByIdAsync(versionId);
            if (version == null) return new List<SectorResponseDto>();

            var plant = await _plantRepository.GetByIdAsync(version.PlantId, userId);
            if (plant == null) return new List<SectorResponseDto>();

            var sectors = await _sectorRepository.GetByPlantVersionIdAsync(versionId);
            return sectors.Select(s => new SectorResponseDto
            {
                Id = s.Id,
                PlantVersionId = s.PlantVersionId,
                Name = s.Name,
                Type = s.Type,
                Color = s.Color,
                PointsJson = s.PointsJson,
                AreaM2 = (double)(s.AreaM2 ?? 0)
            });
        }

        public async Task<SectorResponseDto> CreateSector(CreateSectorDto createDto, Guid userId)
        {
            var version = await _plantVersionRepository.GetByIdAsync(createDto.PlantVersionId);
            if (version == null)
            {
                throw new Exception("Plant version not found");
            }

            var plant = await _plantRepository.GetByIdAsync(version.PlantId, userId);
            if (plant == null)
            {
                throw new Exception("User does not have access to this plant");
            }

            var sector = new Sector
            {
                Id = Guid.NewGuid(),
                PlantVersionId = createDto.PlantVersionId,
                Name = createDto.Name,
                Type = createDto.Type,
                Color = createDto.Color,
                PointsJson = JsonSerializer.Serialize(createDto.Points),
                AreaM2 = createDto.AreaM2,
                CreatedAt = DateTime.UtcNow
            };

            await _sectorRepository.AddAsync(sector);

            return new SectorResponseDto
            {
                Id = sector.Id,
                PlantVersionId = sector.PlantVersionId,
                Name = sector.Name,
                Type = sector.Type,
                Color = sector.Color,
                PointsJson = sector.PointsJson,
                AreaM2 = (double)sector.AreaM2,
                CreatedAt = sector.CreatedAt
            };
        }

        public async Task UpdateSector(Guid id, UpdateSectorDto updateDto, Guid userId)
        {
            var sector = await _sectorRepository.GetByIdAsync(id);
            if (sector == null)
            {
                throw new Exception("Sector not found");
            }

            var version = await _plantVersionRepository.GetByIdAsync(sector.PlantVersionId);
            if (version == null)
            {
                throw new Exception("Plant version not found");
            }

            var plant = await _plantRepository.GetByIdAsync(version.PlantId, userId);
            if (plant == null)
            {
                throw new Exception("User does not have access to this plant");
            }

            sector.Name = updateDto.Name;
            sector.Type = updateDto.Type;
            sector.Color = updateDto.Color;
            sector.PointsJson = JsonSerializer.Serialize(updateDto.Points);
            sector.AreaM2 = updateDto.AreaM2;

            await _sectorRepository.UpdateAsync(sector);
        }

        public async Task DeleteSector(Guid id, Guid userId)
        {
            var sector = await _sectorRepository.GetByIdAsync(id);
            if (sector == null)
            {
                throw new Exception("Sector not found");
            }

            var version = await _plantVersionRepository.GetByIdAsync(sector.PlantVersionId);
            if (version == null)
            {
                throw new Exception("Plant version not found");
            }

            var plant = await _plantRepository.GetByIdAsync(version.PlantId, userId);
            if (plant == null)
            {
                throw new Exception("User does not have access to this plant");
            }

            await _sectorRepository.DeleteAsync(id);
        }
    }
}
