using System;

namespace LabControlApi.DTOs.Plant
{
    public class PlantResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Scale { get; set; }
        public decimal WidthUnits { get; set; }
        public decimal HeightUnits { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
