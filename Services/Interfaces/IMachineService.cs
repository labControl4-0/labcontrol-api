using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LabControlApi.DTOs.Machine;

namespace LabControlApi.Services.Interfaces
{
    public interface IMachineService
    {
        Task<IEnumerable<MachineResponseDto>> GetMachines(Guid plantId, Guid userId);
        Task<MachineResponseDto> CreateMachine(CreateMachineDto dto, Guid userId);
        Task UpdateMachine(Guid id, UpdateMachineDto dto, Guid userId);
        Task DeleteMachine(Guid id, Guid userId);
    }
}
