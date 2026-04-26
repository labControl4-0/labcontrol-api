using System;
using System.Collections.Generic;

namespace LabControlApi.Models
{
    public class Machine
    {
        public Guid Id { get; set; }
        public Guid PlantId { get; set; }
        public Guid? SectorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public double PosX { get; set; }
        public double PosY { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Plant Plant { get; set; } = null!;
        public Sector? Sector { get; set; }
        public ICollection<MachineMetric> Metrics { get; set; } = new List<MachineMetric>();
        public ICollection<MachineEvent> Events { get; set; } = new List<MachineEvent>();
    }
}
