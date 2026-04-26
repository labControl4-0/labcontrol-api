using System;

namespace LabControlApi.DTOs.Machine
{
    public class CreateMachineDto
    {
        public Guid PlantId { get; set; }
        public Guid? SectorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public double PosX { get; set; }
        public double PosY { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
