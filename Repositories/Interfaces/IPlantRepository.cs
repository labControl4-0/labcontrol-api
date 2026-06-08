using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LabControlApi.Models;

namespace LabControlApi.Repositories.Interfaces
{
    public interface IPlantRepository
    {
        Task<IEnumerable<Plant>> GetByUserIdAsync(Guid userId);
        Task<Plant?> GetByIdAsync(Guid id, Guid userId);
        Task<Plant> AddAsync(Plant plant);
        Task UpdateAsync(Plant plant);
        Task DeleteAsync(Guid id, Guid userId);
    }
}
