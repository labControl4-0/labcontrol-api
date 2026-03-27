namespace LabControlApi.DTOs
{
    public class PlantVersionResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal WidthUnits { get; set; }
        public decimal HeightUnits { get; set; }
        public decimal Scale { get; set; }
        public int VersionNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
