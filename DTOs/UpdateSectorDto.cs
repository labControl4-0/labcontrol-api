using LabControlApi.DTOs;

namespace LabControlApi.DTOs
{
    public class UpdateSectorDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string? Color { get; set; }
        public List<PointDto> Points { get; set; } = new List<PointDto>();
    }
}
