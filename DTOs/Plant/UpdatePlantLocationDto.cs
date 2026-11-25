using System.ComponentModel.DataAnnotations;

namespace LabControlApi.DTOs.Plant
{
    public class UpdatePlantLocationDto
    {
        [Required]
        [Range(-90d, 90d)]
        public double? Latitude { get; set; }

        [Required]
        [Range(-180d, 180d)]
        public double? Longitude { get; set; }
    }
}
