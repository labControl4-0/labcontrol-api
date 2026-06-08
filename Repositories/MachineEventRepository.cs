using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LabControlApi.Data;
using LabControlApi.Models;
using LabControlApi.Repositories.Interfaces;
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
