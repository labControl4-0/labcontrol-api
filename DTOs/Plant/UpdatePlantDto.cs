namespace LabControlApi.DTOs.Plant
{
    public class UpdatePlantDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Scale { get; set; }
        public decimal WidthUnits { get; set; }
        public decimal HeightUnits { get; set; }
    }
}
