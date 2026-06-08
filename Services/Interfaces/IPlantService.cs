using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LabControlApi.DTOs.Plant;

namespace LabControlApi.Services.Interfaces
{
    public interface IPlantService
    {
        Task<IEnumerable<PlantResponseDto>> GetPlants(Guid userId);
        Task<PlantResponseDto?> GetPlant(Guid id, Guid userId);
        Task<PlantResponseDto> CreatePlant(CreatePlantDto dto, Guid userId);
        Task UpdatePlant(Guid id, UpdatePlantDto dto, Guid userId);
        Task DeletePlant(Guid id, Guid userId);
    }
}
