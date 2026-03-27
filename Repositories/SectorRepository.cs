using LabControlApi.Data;
using LabControlApi.Models;
using LabControlApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LabControlApi.Repositories
{
    public class SectorRepository : ISectorRepository
    {
        private readonly AppDbContext _context;

        public SectorRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Sector>> GetByPlantVersionIdAsync(Guid plantVersionId)
        {
            return await _context.Sectors.Where(s => s.PlantVersionId == plantVersionId).ToListAsync();
        }

        public async Task<Sector?> GetByIdAsync(Guid id)
        {
            return await _context.Sectors.FindAsync(id);
        }

        public async Task CreateAsync(Sector sector)
        {
            await _context.Sectors.AddAsync(sector);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Sector sector)
        {
            _context.Sectors.Update(sector);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var sector = await GetByIdAsync(id);
            if (sector != null)
            {
                _context.Sectors.Remove(sector);
                await _context.SaveChangesAsync();
            }
        }
    }
}
