using LabControlApi.Models;

namespace LabControlApi.Repositories.Interfaces
{
    public interface IPlantVersionRepository
    {
        Task<IEnumerable<PlantVersion>> GetByPlantIdAsync(Guid plantId);
        Task<PlantVersion?> GetByIdAsync(Guid id);
        Task<PlantVersion> AddAsync(PlantVersion plantVersion);
        Task UpdateAsync(PlantVersion plantVersion);
    }
}
