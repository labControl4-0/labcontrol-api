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
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string PointsJson { get; set; } = string.Empty; // Store polygon points as JSON
        public decimal MinX { get; set; }
        public decimal MinY { get; set; }
        public decimal MaxX { get; set; }
        public decimal MaxY { get; set; }
        public decimal? AreaM2 { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public PlantVersion PlantVersion { get; set; } = null!;
        public ICollection<Machine> Machines { get; set; } = new List<Machine>();
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
