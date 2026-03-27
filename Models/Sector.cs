using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Linq;

namespace LabControlApi.Models
{
    public class Sector
    {
        public Guid Id { get; set; }
        public Guid PlantVersionId { get; set; }
        public PlantVersion PlantVersion { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string? Color { get; set; }
        public string PointsJson { get; set; } = string.Empty;
        public decimal MinX { get; set; }
        public decimal MinY { get; set; }
        public decimal MaxX { get; set; }
        public decimal MaxY { get; set; }
        public decimal? AreaM2 { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public List<PointDto> GetPoints()
        {
            return JsonSerializer.Deserialize<List<PointDto>>(PointsJson) ?? new List<PointDto>();
        }

        public void SetPoints(List<PointDto> points)
        {
            PointsJson = JsonSerializer.Serialize(points);
            MinX = points.Min(p => p.X);
            MinY = points.Min(p => p.Y);
            MaxX = points.Max(p => p.X);
            MaxY = points.Max(p => p.Y);
        }
    }

    public static class SectorExtensions
    {
        public static decimal CalculatePolygonArea(List<PointDto> points)
        {
            decimal area = 0;
            for (int i = 0; i < points.Count; i++)
            {
                var j = (i + 1) % points.Count;
                area += points[i].X * points[j].Y;
                area -= points[j].X * points[i].Y;
            }
            return Math.Abs(area / 2);
        }
    }

    public class PointDto
    {
        public decimal X { get; set; }
        public decimal Y { get; set; }
    }
}
