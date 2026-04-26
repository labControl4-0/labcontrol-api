using LabControlApi.DTOs.PlantVersion;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LabControlApi.Services.Interfaces
{
    public interface IPlantVersionService
    {
        Task<IEnumerable<PlantVersionResponseDto>> GetVersions(Guid plantId, Guid userId);
        Task<PlantVersionResponseDto> CreateVersion(CreatePlantVersionDto dto, Guid userId);
        Task<PlantVersionResponseDto?> CloneVersion(Guid versionId, Guid userId);
        Task ActivateVersion(Guid versionId, Guid userId);
    }
}
