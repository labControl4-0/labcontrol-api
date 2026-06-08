using LabControlApi.DTOs.Sector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LabControlApi.Services.Interfaces
{
    public interface ISectorService
    {
        Task<IEnumerable<SectorResponseDto>> GetSectors(Guid versionId, Guid userId);
        Task<SectorResponseDto> CreateSector(CreateSectorDto createDto, Guid userId);
        Task UpdateSector(Guid id, UpdateSectorDto updateDto, Guid userId);
        Task DeleteSector(Guid id, Guid userId);
    }
}
