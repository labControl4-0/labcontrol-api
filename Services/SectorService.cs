using LabControlApi.DTOs;
using LabControlApi.Models;
using LabControlApi.Repositories.Interfaces;
using LabControlApi.Services.Interfaces;
using System.Text.Json;

namespace LabControlApi.Services
{
    public class SectorService : ISectorService
    {
        private readonly ISectorRepository _repository;

        public SectorService(ISectorRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<SectorResponseDto>> GetByPlantVersionIdAsync(Guid plantVersionId)
        {
            var sectors = await _repository.GetByPlantVersionIdAsync(plantVersionId);
            return sectors.Select(s => new SectorResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                Type = s.Type,
                Color = s.Color,
                MinX = s.MinX,
                MinY = s.MinY,
                MaxX = s.MaxX,
                MaxY = s.MaxY,
                AreaM2 = s.AreaM2,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            });
        }

        public async Task<SectorResponseDto?> GetByIdAsync(Guid id)
        {
            var sector = await _repository.GetByIdAsync(id);
            if (sector == null) return null;

            return new SectorResponseDto
            {
                Id = sector.Id,
                Name = sector.Name,
                Type = sector.Type,
                Color = sector.Color,
                MinX = sector.MinX,
                MinY = sector.MinY,
                MaxX = sector.MaxX,
                MaxY = sector.MaxY,
                AreaM2 = sector.AreaM2,
                CreatedAt = sector.CreatedAt,
                UpdatedAt = sector.UpdatedAt
            };
        }

        public async Task CreateAsync(CreateSectorDto createDto)
        {
            var points = createDto.Points.Select(p => new Models.PointDto { X = p.X, Y = p.Y }).ToList();
            var sector = new Sector
            {
                Id = Guid.NewGuid(),
                PlantVersionId = createDto.PlantVersionId,
                Name = createDto.Name,
                Type = createDto.Type,
                Color = createDto.Color,
                PointsJson = JsonSerializer.Serialize(points),
                MinX = points.Min(p => p.X),
                MinY = points.Min(p => p.Y),
                MaxX = points.Max(p => p.X),
                MaxY = points.Max(p => p.Y),
                AreaM2 = SectorExtensions.CalculatePolygonArea(points)
            };

            await _repository.CreateAsync(sector);
        }

        public async Task UpdateAsync(Guid id, UpdateSectorDto updateDto)
        {
            var sector = await _repository.GetByIdAsync(id);
            if (sector == null) return;

            var points = updateDto.Points.Select(p => new Models.PointDto { X = p.X, Y = p.Y }).ToList();
            sector.Name = updateDto.Name;
            sector.Type = updateDto.Type;
            sector.Color = updateDto.Color;
            sector.PointsJson = JsonSerializer.Serialize(points);
            sector.MinX = points.Min(p => p.X);
            sector.MinY = points.Min(p => p.Y);
            sector.MaxX = points.Max(p => p.X);
            sector.MaxY = points.Max(p => p.Y);
            sector.AreaM2 = SectorExtensions.CalculatePolygonArea(points);
            sector.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(sector);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }
    }
}
