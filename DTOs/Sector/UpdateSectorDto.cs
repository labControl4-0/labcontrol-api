using System.Collections.Generic;
using LabControlApi.DTOs;

namespace LabControlApi.DTOs.Sector
{
    public class UpdateSectorDto
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public List<PointDto> Points { get; set; } = new();
        public decimal AreaM2 { get; set; }
    }
}
