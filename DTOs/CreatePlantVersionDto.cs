namespace LabControlApi.DTOs
{
    public class CreatePlantVersionDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal WidthUnits { get; set; }
        public decimal HeightUnits { get; set; }
        public decimal Scale { get; set; }
    }
}
