using LabControlApi.DTOs;

namespace LabControlApi.Services.Interfaces
{
    public interface IPlantVersionService
    {
        Task<IEnumerable<PlantVersionResponseDto>> GetAllAsync();
        Task<PlantVersionResponseDto?> GetByIdAsync(Guid id);
        Task CreateAsync(CreatePlantVersionDto createDto);
        Task UpdateAsync(Guid id, CreatePlantVersionDto updateDto);
        Task DeleteAsync(Guid id);
    }
}
