using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LabControlApi.Models;

namespace LabControlApi.Repositories.Interfaces
{
    public interface IMachineMetricRepository
    {
        Task<IEnumerable<MachineMetric>> GetByMachineIdAsync(Guid machineId);
        Task<MachineMetric> AddAsync(MachineMetric metric);
    }
}
