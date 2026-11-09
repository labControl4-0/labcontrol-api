using System;
using System.Threading.Tasks;
using LabControlApi.DTOs.Push;

namespace LabControlApi.Services.Interfaces
{
    public interface IPushNotificationService
    {
        Task<bool> SendPushToUserAsync(Guid userId, SendPushNotificationDto notification);
        Task<int> SendPushToAllUsersAsync(SendPushNotificationDto notification);
    }
}

