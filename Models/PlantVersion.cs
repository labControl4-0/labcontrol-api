using System;
using System.Collections.Generic;

namespace LabControlApi.Models
{
    public class PlantVersion
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int WidthUnits { get; set; }
        public int HeightUnits { get; set; }
        public double Scale { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid PlantId { get; set; }
        public int VersionNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ActivatedAt { get; set; }
        public Guid? ActivatedBy { get; set; }
        public Guid CreatedBy { get; set; }

        // Navigation properties
        public Plant Plant { get; set; } = null!;
        public User Creator { get; set; } = null!;
        public User? Activator { get; set; }
        public ICollection<Sector> Sectors { get; set; } = new List<Sector>();
    }
}
