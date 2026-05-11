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

        public async Task<IEnumerable<Sector>> GetByPlantIdAsync(Guid plantId)
        {
            return await _context.Sectors.Where(s => s.PlantId == plantId).ToListAsync();
        }

        public async Task<IEnumerable<Sector>> GetByPlantVersionIdAsync(Guid plantVersionId)
        {
            return await _context.Sectors.Where(s => s.PlantId == plantVersionId).ToListAsync();
        }

        public async Task<Sector?> GetByIdAsync(Guid id)
        {
            return await _context.Sectors.FindAsync(id);
        }

        public async Task<Sector> AddAsync(Sector sector)
        {
            _context.Sectors.Add(sector);
            await _context.SaveChangesAsync();
            return sector;
        }

        public async Task UpdateAsync(Sector sector)
        {
            _context.Sectors.Update(sector);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var sector = await _context.Sectors
                .Include(s => s.Machines) // Include the machines in the sector
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sector != null)
            {
                // First, remove all machines within the sector
                if (sector.Machines != null && sector.Machines.Any())
                {
                    _context.Machines.RemoveRange(sector.Machines);
                }

                // Then, remove the sector itself
                _context.Sectors.Remove(sector);
                await _context.SaveChangesAsync();
            }
        }
    }
}
