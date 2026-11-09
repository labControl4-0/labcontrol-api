using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LabControlApi.Models;

namespace LabControlApi.Repositories.Interfaces
{
    public interface IDeviceRepository
    {
        Task<UserDevice?> GetByTokenAsync(string deviceToken);
        Task<UserDevice?> GetUserDeviceAsync(Guid userId, string deviceToken);
        Task<IEnumerable<UserDevice>> GetDevicesByUserIdAsync(Guid userId);
        Task<IEnumerable<UserDevice>> GetAllActiveDevicesAsync();
        Task<UserDevice> AddOrUpdateAsync(UserDevice device);
        Task<bool> DeactivateOrRemoveAsync(Guid userId, string deviceToken);
        Task DeactivateTokensAsync(IEnumerable<string> invalidTokens);
    }
}

