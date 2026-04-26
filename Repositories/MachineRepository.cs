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
    public class MachineRepository : IMachineRepository
    {
        private readonly AppDbContext _context;

        public MachineRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Machine>> GetByPlantIdAsync(Guid plantId)
        {
            return await _context.Machines.Where(m => m.PlantId == plantId).ToListAsync();
        }

        public async Task<Machine?> GetByIdAsync(Guid id)
        {
            return await _context.Machines.FindAsync(id);
        }

        public async Task<Machine> AddAsync(Machine machine)
        {
            _context.Machines.Add(machine);
            await _context.SaveChangesAsync();
            return machine;
        }

        public async Task UpdateAsync(Machine machine)
        {
            _context.Entry(machine).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var machine = await GetByIdAsync(id);
            if (machine != null)
            {
                _context.Machines.Remove(machine);
                await _context.SaveChangesAsync();
            }
        }
    }
}
