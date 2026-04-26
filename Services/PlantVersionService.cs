using LabControlApi.DTOs.PlantVersion;
using LabControlApi.Models;
using LabControlApi.Repositories.Interfaces;
using LabControlApi.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LabControlApi.Services
{
    public class PlantVersionService : IPlantVersionService
    {
        private readonly IPlantVersionRepository _plantVersionRepository;
        private readonly IPlantRepository _plantRepository;

        public PlantVersionService(IPlantVersionRepository plantVersionRepository, IPlantRepository plantRepository)
        {
            _plantVersionRepository = plantVersionRepository;
            _plantRepository = plantRepository;
        }

        public async Task<IEnumerable<PlantVersionResponseDto>> GetVersions(Guid plantId, Guid userId)
        {
            var plant = await _plantRepository.GetByIdAsync(plantId, userId);
            if (plant == null)
            {
                // Ou lançar uma exceção
                return new List<PlantVersionResponseDto>();
            }

            var versions = await _plantVersionRepository.GetByPlantIdAsync(plantId);
            return versions.Select(v => new PlantVersionResponseDto
            {
                Id = v.Id,
                PlantId = v.PlantId,
                VersionNumber = v.VersionNumber,
                IsActive = v.IsActive,
                CreatedAt = v.CreatedAt,
                CreatedBy = v.CreatedBy
            });
        }

        public async Task<PlantVersionResponseDto> CreateVersion(CreatePlantVersionDto dto, Guid userId)
        {
            var plant = await _plantRepository.GetByIdAsync(dto.PlantId, userId);
            if (plant == null)
            {
                throw new Exception("Plant not found or user does not have access.");
            }

            var latestVersion = (await _plantVersionRepository.GetByPlantIdAsync(dto.PlantId))
                                .OrderByDescending(v => v.VersionNumber)
                                .FirstOrDefault();

            var newVersion = new PlantVersion
            {
                Id = Guid.NewGuid(),
                PlantId = dto.PlantId,
                VersionNumber = (latestVersion?.VersionNumber ?? 0) + 1,
                IsActive = false, // Sempre começa como inativa
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow
            };

            var createdVersion = await _plantVersionRepository.AddAsync(newVersion);

            return new PlantVersionResponseDto
            {
                Id = createdVersion.Id,
                PlantId = createdVersion.PlantId,
                VersionNumber = createdVersion.VersionNumber,
                IsActive = createdVersion.IsActive,
                CreatedAt = createdVersion.CreatedAt,
                CreatedBy = createdVersion.CreatedBy
            };
        }

        public async Task<PlantVersionResponseDto?> CloneVersion(Guid versionId, Guid userId)
        {
            var originalVersion = await _plantVersionRepository.GetByIdAsync(versionId);
            if (originalVersion == null)
            {
                return null;
            }

            var plant = await _plantRepository.GetByIdAsync(originalVersion.PlantId, userId);
            if (plant == null)
            {
                // Tentativa de clonar uma versão de uma planta que não pertence ao usuário
                return null;
            }

            var latestVersion = (await _plantVersionRepository.GetByPlantIdAsync(originalVersion.PlantId))
                                .OrderByDescending(v => v.VersionNumber)
                                .FirstOrDefault();

            var newVersion = new PlantVersion
            {
                Id = Guid.NewGuid(),
                PlantId = originalVersion.PlantId,
                VersionNumber = (latestVersion?.VersionNumber ?? 0) + 1,
                IsActive = false,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                Sectors = originalVersion.Sectors.Select(s => new Sector
                {
                    Id = Guid.NewGuid(),
                    PlantVersionId = Guid.NewGuid(), // Será preenchido pelo EF? Não, precisa ser o Id da newVersion
                    Name = s.Name,
                    Type = s.Type,
                    Color = s.Color,
                    PointsJson = s.PointsJson,
                    AreaM2 = s.AreaM2
                }).ToList()
            };
            
            foreach (var sector in newVersion.Sectors)
            {
                sector.PlantVersionId = newVersion.Id;
            }

            var createdVersion = await _plantVersionRepository.AddAsync(newVersion);

            return new PlantVersionResponseDto
            {
                Id = createdVersion.Id,
                PlantId = createdVersion.PlantId,
                VersionNumber = createdVersion.VersionNumber,
                IsActive = createdVersion.IsActive,
                CreatedAt = createdVersion.CreatedAt,
                CreatedBy = createdVersion.CreatedBy
            };
        }

        public async Task ActivateVersion(Guid versionId, Guid userId)
        {
            var versionToActivate = await _plantVersionRepository.GetByIdAsync(versionId);
            if (versionToActivate == null)
            {
                return; // Ou lançar exceção
            }

            var plant = await _plantRepository.GetByIdAsync(versionToActivate.PlantId, userId);
            if (plant == null)
            {
                return; // Ou lançar exceção de não autorizado
            }

            var allVersions = await _plantVersionRepository.GetByPlantIdAsync(versionToActivate.PlantId);
            foreach (var v in allVersions)
            {
                v.IsActive = false;
                await _plantVersionRepository.UpdateAsync(v);
            }

            versionToActivate.IsActive = true;
            versionToActivate.ActivatedAt = DateTime.UtcNow;
            versionToActivate.ActivatedBy = userId;
            await _plantVersionRepository.UpdateAsync(versionToActivate);
        }
    }
}
