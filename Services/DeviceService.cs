using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LabControlApi.DTOs.Device;
using LabControlApi.Models;
using LabControlApi.Repositories.Interfaces;
using LabControlApi.Services.Interfaces;

namespace LabControlApi.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly IDeviceRepository _deviceRepository;

        public DeviceService(IDeviceRepository deviceRepository)
        {
            _deviceRepository = deviceRepository;
        }

        public async Task<DeviceResponseDto> RegisterDeviceAsync(Guid userId, RegisterDeviceDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.DeviceToken))
            {
                throw new ArgumentException("O token do dispositivo não pode ser vazio.");
            }

            var device = new UserDevice
            {
                UserId = userId,
                DeviceToken = dto.DeviceToken.Trim(),
                Platform = dto.Platform?.Trim().ToLowerInvariant(),
                DeviceName = dto.DeviceName?.Trim()
            };

            var saved = await _deviceRepository.AddOrUpdateAsync(device);

            return new DeviceResponseDto
            {
                Id = saved.Id,
                DeviceToken = saved.DeviceToken,
                Platform = saved.Platform,
                DeviceName = saved.DeviceName,
                IsActive = saved.IsActive,
                CreatedAt = saved.CreatedAt,
                UpdatedAt = saved.UpdatedAt
            };
        }

        public async Task<bool> UnregisterDeviceAsync(Guid userId, string deviceToken)
        {
            if (string.IsNullOrWhiteSpace(deviceToken)) return false;
            return await _deviceRepository.DeactivateOrRemoveAsync(userId, deviceToken.Trim());
        }

        public async Task<IEnumerable<DeviceResponseDto>> GetUserDevicesAsync(Guid userId)
        {
            var devices = await _deviceRepository.GetDevicesByUserIdAsync(userId);
            return devices.Select(d => new DeviceResponseDto
            {
                Id = d.Id,
                DeviceToken = d.DeviceToken,
                Platform = d.Platform,
                DeviceName = d.DeviceName,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            });
        }
    }
}

