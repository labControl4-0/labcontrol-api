using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LabControlApi.Models;

namespace LabControlApi.Repositories.Interfaces
{
    public interface IMachineEventRepository
    {
        Task<IEnumerable<MachineEvent>> GetByMachineIdAsync(Guid machineId);
        Task<MachineEvent?> GetByIdAsync(Guid id);
        Task<MachineEvent> AddAsync(MachineEvent machineEvent);
        Task UpdateAsync(MachineEvent machineEvent);
    }
}
