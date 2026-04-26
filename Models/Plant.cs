using System;
using System.Collections.Generic;

namespace LabControlApi.Models
{
    public class Plant
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Scale { get; set; }
        public decimal WidthUnits { get; set; }
        public decimal HeightUnits { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User User { get; set; } = null!;
        public ICollection<PlantVersion> Versions { get; set; } = new List<PlantVersion>();
        public ICollection<Machine> Machines { get; set; } = new List<Machine>();
    }
}
