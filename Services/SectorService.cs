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
        private readonly IPlantRepository _plantRepository;
        private readonly IMachineRepository _machineRepository;

        public SectorService(ISectorRepository sectorRepository, IPlantRepository plantRepository, IMachineRepository machineRepository)
        {
            _sectorRepository = sectorRepository;
            _plantRepository = plantRepository;
            _machineRepository = machineRepository;
        }

        public async Task<IEnumerable<SectorResponseDto>> GetSectors(Guid plantId, Guid userId)
        {
            var plant = await _plantRepository.GetByIdAsync(plantId, userId);
            if (plant == null) return new List<SectorResponseDto>();

            var sectors = await _sectorRepository.GetByPlantIdAsync(plantId);
            return sectors.Select(s => new SectorResponseDto
            {
                Id = s.Id,
                PlantId = s.PlantId,
                Name = s.Name,
                Type = s.Type,
                Color = s.Color,
                PointsJson = s.PointsJson,
                AreaM2 = (double)(s.AreaM2 ?? 0)
            });
        }

        public async Task<SectorResponseDto> CreateSector(CreateSectorDto createDto, Guid userId)
        {
            var plant = await _plantRepository.GetByIdAsync(createDto.PlantId, userId);
            if (plant == null)
            {
                throw new Exception("User does not have access to this plant");
            }

            var sector = new Sector
            {
                Id = Guid.NewGuid(),
                PlantId = createDto.PlantId,
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
                PlantId = sector.PlantId,
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

            var plant = await _plantRepository.GetByIdAsync(sector.PlantId, userId);
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

            var plant = await _plantRepository.GetByIdAsync(sector.PlantId, userId);
            if (plant == null)
            {
                throw new Exception("User does not have access to this plant");
            }

            await _sectorRepository.DeleteAsync(id);
        }
    }
}
