using LabControlApi.Models;

namespace LabControlApi.Repositories.Interfaces
{
    public interface ISectorRepository
    {
        Task<IEnumerable<Sector>> GetByPlantVersionIdAsync(Guid plantVersionId);
        Task<Sector?> GetByIdAsync(Guid id);
        Task CreateAsync(Sector sector);
        Task UpdateAsync(Sector sector);
        Task DeleteAsync(Guid id);
    }
}
