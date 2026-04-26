using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LabControlApi.DTOs.Machine;
using LabControlApi.Models;
using LabControlApi.Repositories.Interfaces;
using LabControlApi.Services.Interfaces;

namespace LabControlApi.Services
{
    public class MachineService : IMachineService
    {
        private readonly IMachineRepository _machineRepository;
        private readonly IPlantRepository _plantRepository;

        public MachineService(IMachineRepository machineRepository, IPlantRepository plantRepository)
        {
            _machineRepository = machineRepository;
            _plantRepository = plantRepository;
        }

        public async Task<IEnumerable<MachineResponseDto>> GetMachines(Guid plantId, Guid userId)
        {
            var plant = await _plantRepository.GetByIdAsync(plantId, userId);
            if (plant == null) throw new Exception("Plant not found or user does not have access");

            var machines = await _machineRepository.GetByPlantIdAsync(plantId);
            return machines.Select(m => new MachineResponseDto
            {
                Id = m.Id,
                PlantId = m.PlantId,
                SectorId = m.SectorId,
                Name = m.Name,
                Model = m.Model,
                PosX = m.PosX,
                PosY = m.PosY,
                Status = m.Status,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt
            });
        }

        public async Task<MachineResponseDto> CreateMachine(CreateMachineDto dto, Guid userId)
        {
            var plant = await _plantRepository.GetByIdAsync(dto.PlantId, userId);
            if (plant == null) throw new Exception("Plant not found or user does not have access");

            var machine = new Machine
            {
                Id = Guid.NewGuid(),
                PlantId = dto.PlantId,
                SectorId = dto.SectorId,
                Name = dto.Name,
                Model = dto.Model,
                PosX = dto.PosX,
                PosY = dto.PosY,
                Status = dto.Status
            };

            var newMachine = await _machineRepository.AddAsync(machine);

            return new MachineResponseDto
            {
                Id = newMachine.Id,
                PlantId = newMachine.PlantId,
                SectorId = newMachine.SectorId,
                Name = newMachine.Name,
                Model = newMachine.Model,
                PosX = newMachine.PosX,
                PosY = newMachine.PosY,
                Status = newMachine.Status,
                CreatedAt = newMachine.CreatedAt,
                UpdatedAt = newMachine.UpdatedAt
            };
        }

        public async Task UpdateMachine(Guid id, UpdateMachineDto dto, Guid userId)
        {
            var machine = await _machineRepository.GetByIdAsync(id);
            if (machine == null) throw new Exception("Machine not found");

            var plant = await _plantRepository.GetByIdAsync(machine.PlantId, userId);
            if (plant == null) throw new Exception("Plant not found or user does not have access");

            machine.SectorId = dto.SectorId;
            machine.Name = dto.Name;
            machine.Model = dto.Model;
            machine.PosX = dto.PosX;
            machine.PosY = dto.PosY;
            machine.Status = dto.Status;
            machine.UpdatedAt = DateTime.UtcNow;

            await _machineRepository.UpdateAsync(machine);
        }

        public async Task DeleteMachine(Guid id, Guid userId)
        {
            var machine = await _machineRepository.GetByIdAsync(id);
            if (machine == null) return;

            var plant = await _plantRepository.GetByIdAsync(machine.PlantId, userId);
            if (plant == null) throw new Exception("Plant not found or user does not have access");

            await _machineRepository.DeleteAsync(id);
        }
    }
}
