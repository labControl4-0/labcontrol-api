using LabControlApi.Models;

namespace LabControlApi.Repositories.Interfaces
{
    public interface IPlantVersionRepository
    {
        Task<IEnumerable<PlantVersion>> GetAllAsync();
        Task<PlantVersion?> GetByIdAsync(Guid id);
        Task CreateAsync(PlantVersion plantVersion);
        Task UpdateAsync(PlantVersion plantVersion);
        Task DeleteAsync(Guid id);
    }
}
