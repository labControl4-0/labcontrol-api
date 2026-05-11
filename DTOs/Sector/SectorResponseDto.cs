using System;
using System.Collections.Generic;

namespace LabControlApi.DTOs.Sector
{
    public class SectorResponseDto
    {
        public Guid Id { get; set; }
        public Guid PlantId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string PointsJson { get; set; } = string.Empty;
        public double AreaM2 { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
