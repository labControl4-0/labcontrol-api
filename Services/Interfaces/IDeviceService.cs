using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LabControlApi.DTOs.Device;

namespace LabControlApi.Services.Interfaces
{
    public interface IDeviceService
    {
        Task<DeviceResponseDto> RegisterDeviceAsync(Guid userId, RegisterDeviceDto dto);
        Task<bool> UnregisterDeviceAsync(Guid userId, string deviceToken);
        Task<IEnumerable<DeviceResponseDto>> GetUserDevicesAsync(Guid userId);
    }
}

