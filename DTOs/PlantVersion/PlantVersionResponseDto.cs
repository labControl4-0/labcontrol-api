using System;

namespace LabControlApi.DTOs.PlantVersion
{
    public class PlantVersionResponseDto
    {
        public Guid Id { get; set; }
        public Guid PlantId { get; set; }
        public int VersionNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
