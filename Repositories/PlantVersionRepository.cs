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

        public async Task<IEnumerable<PlantVersion>> GetByPlantIdAsync(Guid plantId)
        {
            return await _context.PlantVersions.Where(pv => pv.PlantId == plantId).ToListAsync();
        }

        public async Task<PlantVersion?> GetByIdAsync(Guid id)
        {
            return await _context.PlantVersions.Include(pv => pv.Sectors).FirstOrDefaultAsync(pv => pv.Id == id);
        }

        public async Task<PlantVersion> AddAsync(PlantVersion plantVersion)
        {
            _context.PlantVersions.Add(plantVersion);
            await _context.SaveChangesAsync();
            return plantVersion;
        }

        public async Task UpdateAsync(PlantVersion plantVersion)
        {
            _context.Entry(plantVersion).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
