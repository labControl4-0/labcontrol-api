namespace LabControlApi.DTOs.PlantVersion
{
    public class CreatePlantVersionDto
    {
        public string Name { get; set; } = string.Empty;
        public Guid PlantId { get; set; }
    }
}
