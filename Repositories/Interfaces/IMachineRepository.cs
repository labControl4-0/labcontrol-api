using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LabControlApi.Models;

namespace LabControlApi.Repositories.Interfaces
{
    public interface IMachineRepository
    {
        Task<IEnumerable<Machine>> GetByPlantIdAsync(Guid plantId);
        Task<Machine?> GetByIdAsync(Guid id);
        Task<Machine> AddAsync(Machine machine);
        Task UpdateAsync(Machine machine);
        Task DeleteAsync(Guid id);
        Task DeleteBySectorIdAsync(Guid sectorId);
    }
}
