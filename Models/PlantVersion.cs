using System;
using System.Collections.Generic;

namespace LabControlApi.Models
{
    public class PlantVersion
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal WidthUnits { get; set; }
        public decimal HeightUnits { get; set; }
        public decimal Scale { get; set; }
        public int VersionNumber { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Sector> Sectors { get; set; } = new List<Sector>();
    }
}
