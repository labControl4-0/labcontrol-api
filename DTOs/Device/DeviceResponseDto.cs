using System;

namespace LabControlApi.DTOs.Device
{
    public class DeviceResponseDto
    {
        public Guid Id { get; set; }
        public string DeviceToken { get; set; } = string.Empty;
        public string? Platform { get; set; }
        public string? DeviceName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

