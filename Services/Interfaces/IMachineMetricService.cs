using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LabControlApi.DTOs.MachineMetric;

namespace LabControlApi.Services.Interfaces
{
    public interface IMachineMetricService
    {
        Task<IEnumerable<MachineMetricResponseDto>> GetMetrics(Guid machineId, Guid userId);
        Task<MachineMetricResponseDto> AddMetric(CreateMachineMetricDto createDto, Guid userId);
    }
}
