using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LabControlApi.DTOs.Event;
using LabControlApi.DTOs.MachineEvent;

namespace LabControlApi.Services.Interfaces
{
    public interface IMachineEventService
    {
        Task<IEnumerable<MachineEventResponseDto>> GetEvents(Guid machineId, Guid userId);
        Task<MachineEventResponseDto> CreateEvent(CreateMachineEventDto dto, Guid userId);
        Task<MachineEventResponseDto> ResolveEvent(Guid id, Guid userId);
    }
}
