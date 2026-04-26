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
    public class MachineMetricRepository : IMachineMetricRepository
    {
        private readonly AppDbContext _context;

        public MachineMetricRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MachineMetric>> GetByMachineIdAsync(Guid machineId)
        {
            return await _context.MachineMetrics
                .Where(m => m.MachineId == machineId)
                .OrderByDescending(m => m.CollectedAt)
                .ToListAsync();
        }

        public async Task<MachineMetric> AddAsync(MachineMetric metric)
        {
            _context.MachineMetrics.Add(metric);
            await _context.SaveChangesAsync();
            return metric;
        }
    }
}
