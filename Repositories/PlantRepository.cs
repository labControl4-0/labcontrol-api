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
    public class PlantRepository : IPlantRepository
    {
        private readonly AppDbContext _context;

        public PlantRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Plant>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Plants.Where(p => p.UserId == userId).ToListAsync();
        }

        public async Task<Plant?> GetByIdAsync(Guid id, Guid userId)
        {
            return await _context.Plants.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        }

        public async Task<Plant> AddAsync(Plant plant)
        {
            _context.Plants.Add(plant);
            await _context.SaveChangesAsync();
            return plant;
        }

        public async Task UpdateAsync(Plant plant)
        {
            _context.Entry(plant).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id, Guid userId)
        {
            var plant = await GetByIdAsync(id, userId);
            if (plant != null)
            {
                _context.Plants.Remove(plant);
                await _context.SaveChangesAsync();
            }
        }
    }
}
