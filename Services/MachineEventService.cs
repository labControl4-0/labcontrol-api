using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LabControlApi.DTOs.Event;
using LabControlApi.DTOs.MachineEvent;
using LabControlApi.Models;
using LabControlApi.Repositories.Interfaces;
using LabControlApi.Services.Interfaces;

namespace LabControlApi.Services
{
    public class MachineEventService : IMachineEventService
    {
        private readonly IMachineEventRepository _eventRepository;
        private readonly IMachineRepository _machineRepository;
        private readonly IPlantRepository _plantRepository;

        public MachineEventService(IMachineEventRepository eventRepository, IMachineRepository machineRepository, IPlantRepository plantRepository)
        {
            _eventRepository = eventRepository;
            _machineRepository = machineRepository;
            _plantRepository = plantRepository;
        }

        public async Task<IEnumerable<MachineEventResponseDto>> GetEvents(Guid machineId, Guid userId)
        {
            var machine = await _machineRepository.GetByIdAsync(machineId);
            if (machine == null) throw new Exception("Machine not found");

            var plant = await _plantRepository.GetByIdAsync(machine.PlantId, userId);
            if (plant == null) throw new Exception("Plant not found or user does not have access");

            var events = await _eventRepository.GetByMachineIdAsync(machineId);
            return events.Select(e => new MachineEventResponseDto
            {
                Id = e.Id,
                MachineId = e.MachineId,
                EventType = e.EventType,
                Message = e.Message,
                CreatedAt = e.CreatedAt,
                ResolvedAt = e.ResolvedAt
            });
        }

        public async Task<MachineEventResponseDto> CreateEvent(CreateMachineEventDto dto, Guid userId)
        {
            var machine = await _machineRepository.GetByIdAsync(dto.MachineId);
            if (machine == null) throw new Exception("Machine not found");

            var plant = await _plantRepository.GetByIdAsync(machine.PlantId, userId);
            if (plant == null) throw new Exception("Plant not found or user does not have access");

            var machineEvent = new MachineEvent
            {
                MachineId = dto.MachineId,
                EventType = dto.EventType,
                Message = dto.Message
            };

            var newEvent = await _eventRepository.AddAsync(machineEvent);

            return new MachineEventResponseDto
            {
                Id = newEvent.Id,
                MachineId = newEvent.MachineId,
                EventType = newEvent.EventType,
                Message = newEvent.Message,
                CreatedAt = newEvent.CreatedAt,
                ResolvedAt = newEvent.ResolvedAt
            };
        }

        public async Task<MachineEventResponseDto> ResolveEvent(Guid id, Guid userId)
        {
            var machineEvent = await _eventRepository.GetByIdAsync(id);
            if (machineEvent == null)
            {
                throw new Exception("Event not found");
            }

            var machine = await _machineRepository.GetByIdAsync(machineEvent.MachineId);
            if (machine == null)
            {
                throw new Exception("Machine not found");
            }

            var plant = await _plantRepository.GetByIdAsync(machine.PlantId, userId);
            if (plant == null)
            {
                throw new Exception("User does not have access to this plant");
            }

            machineEvent.ResolvedAt = DateTime.UtcNow;
            await _eventRepository.UpdateAsync(machineEvent);

            return new MachineEventResponseDto
            {
                Id = machineEvent.Id,
                MachineId = machineEvent.MachineId,
                EventType = machineEvent.EventType,
                Message = machineEvent.Message,
                CreatedAt = machineEvent.CreatedAt,
                ResolvedAt = machineEvent.ResolvedAt
            };
        }
    }
}
