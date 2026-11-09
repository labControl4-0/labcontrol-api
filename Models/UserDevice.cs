using System;

namespace LabControlApi.Models
{
    public class UserDevice
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string DeviceToken { get; set; } = string.Empty;
        public string? Platform { get; set; } // "android", "ios", "web"
        public string? DeviceName { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public User User { get; set; } = null!;
    }
}

