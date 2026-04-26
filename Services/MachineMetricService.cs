using LabControlApi.DTOs.MachineMetric;
using LabControlApi.Models;
using LabControlApi.Repositories.Interfaces;
using LabControlApi.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LabControlApi.Services
{
    public class MachineMetricService : IMachineMetricService
    {
        private readonly IMachineMetricRepository _metricRepository;
        private readonly IMachineRepository _machineRepository;

        public MachineMetricService(IMachineMetricRepository metricRepository, IMachineRepository machineRepository)
        {
            _metricRepository = metricRepository;
            _machineRepository = machineRepository;
        }

        public async Task<IEnumerable<MachineMetricResponseDto>> GetMetrics(Guid machineId, Guid userId)
        {
            var machine = await _machineRepository.GetByIdAsync(machineId);
            if (machine == null) return new List<MachineMetricResponseDto>();

            // Proper authorization check needed

            var metrics = await _metricRepository.GetByMachineIdAsync(machineId);
            var response = new List<MachineMetricResponseDto>();

            foreach (var metric in metrics)
            {
                response.Add(new MachineMetricResponseDto { Id = metric.Id, MachineId = metric.MachineId, Name = "Temperature", Value = metric.Temperature, Timestamp = metric.CollectedAt });
                response.Add(new MachineMetricResponseDto { Id = metric.Id, MachineId = metric.MachineId, Name = "Rpm", Value = metric.Rpm, Timestamp = metric.CollectedAt });
                response.Add(new MachineMetricResponseDto { Id = metric.Id, MachineId = metric.MachineId, Name = "Vibration", Value = metric.Vibration, Timestamp = metric.CollectedAt });
                response.Add(new MachineMetricResponseDto { Id = metric.Id, MachineId = metric.MachineId, Name = "EnergyUsage", Value = metric.EnergyUsage, Timestamp = metric.CollectedAt });
            }
            return response;
        }

        public async Task<MachineMetricResponseDto> AddMetric(CreateMachineMetricDto createDto, Guid userId)
        {
            var machine = await _machineRepository.GetByIdAsync(createDto.MachineId);
            if (machine == null) throw new Exception("Machine not found");

            // Proper authorization check needed

            var metric = new MachineMetric
            {
                MachineId = createDto.MachineId,
                Temperature = createDto.Name == "Temperature" ? createDto.Value : 0,
                Rpm = createDto.Name == "Rpm" ? createDto.Value : 0,
                Vibration = createDto.Name == "Vibration" ? createDto.Value : 0,
                EnergyUsage = createDto.Name == "EnergyUsage" ? createDto.Value : 0,
                CollectedAt = createDto.Timestamp
            };

            var newMetric = await _metricRepository.AddAsync(metric);

            return new MachineMetricResponseDto
            {
                Id = newMetric.Id,
                MachineId = newMetric.MachineId,
                Name = createDto.Name,
                Value = createDto.Value,
                Timestamp = newMetric.CollectedAt
            };
        }
    }
}
