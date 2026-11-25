using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LabControlApi.Data;
using LabControlApi.Models;
using LabControlApi.Repositories.Interfaces;
using LabControlApi.Services;
using Microsoft.EntityFrameworkCore;

namespace LabControlApi.Repositories
{
    public class MachineEventRepository : IMachineEventRepository
    {
        private readonly AppDbContext _context;

        public MachineEventRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MachineEvent>> GetByMachineIdAsync(Guid machineId)
        {
            return await _context.MachineEvents
                .Where(e => e.MachineId == machineId)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<MachineEvent?> GetByIdAsync(Guid id)
        {
            return await _context.MachineEvents.FindAsync(id);
        }

        public async Task<MachineEvent> AddAsync(MachineEvent machineEvent)
        {
            _context.MachineEvents.Add(machineEvent);
            if (PushCategories.FromEvent(machineEvent.EventType) != null)
            {
                var owner = _context.Machines.Where(machine => machine.Id == machineEvent.MachineId)
                    .Select(machine => machine.Plant.UserId);
                var recipients = await _context.PushDevices
                    .Where(device => owner.Contains(device.UserId))
                    .Select(device => new { device.UserId, device.RegistrationId })
                    .ToListAsync();
                foreach (var device in recipients)
                {
                    _context.PushDeliveries.Add(new PushDelivery
                    {
                        Event = machineEvent,
                        UserId = device.UserId,
                        RegistrationId = device.RegistrationId
                    });
                }
            }

            await _context.SaveChangesAsync();
            return machineEvent;
        }

        public async Task UpdateAsync(MachineEvent machineEvent)
        {
            _context.Entry(machineEvent).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
