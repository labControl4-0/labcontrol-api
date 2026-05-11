using LabControlApi.Models;

namespace LabControlApi.Repositories.Interfaces
{
    public interface ISectorRepository
    {
        Task<IEnumerable<Sector>> GetByPlantIdAsync(Guid plantId);
        Task<Sector?> GetByIdAsync(Guid id);
        Task<Sector> AddAsync(Sector sector);
        Task UpdateAsync(Sector sector);
        Task DeleteAsync(Guid id);
    }
}
