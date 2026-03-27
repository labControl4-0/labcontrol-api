using LabControlApi.Data;
using LabControlApi.Models;
using LabControlApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LabControlApi.Repositories
{
    public class PlantVersionRepository : IPlantVersionRepository
    {
        private readonly AppDbContext _context;

        public PlantVersionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PlantVersion>> GetAllAsync()
        {
            return await _context.PlantVersions.ToListAsync();
        }

        public async Task<PlantVersion?> GetByIdAsync(Guid id)
        {
            return await _context.PlantVersions.FindAsync(id);
        }

        public async Task CreateAsync(PlantVersion plantVersion)
        {
            await _context.PlantVersions.AddAsync(plantVersion);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PlantVersion plantVersion)
        {
            _context.PlantVersions.Update(plantVersion);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var plantVersion = await GetByIdAsync(id);
            if (plantVersion != null)
            {
                _context.PlantVersions.Remove(plantVersion);
                await _context.SaveChangesAsync();
            }
        }
    }
}
