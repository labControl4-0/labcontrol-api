using LabControlApi.DTOs;

namespace LabControlApi.Services.Interfaces
{
    public interface ISectorService
    {
        Task<IEnumerable<SectorResponseDto>> GetByPlantVersionIdAsync(Guid plantVersionId);
        Task<SectorResponseDto?> GetByIdAsync(Guid id);
        Task CreateAsync(CreateSectorDto createDto);
        Task UpdateAsync(Guid id, UpdateSectorDto updateDto);
        Task DeleteAsync(Guid id);
    }
}
