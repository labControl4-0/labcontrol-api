using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LabControlApi.Data;
using LabControlApi.Models;
using LabControlApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LabControlApi.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        private readonly AppDbContext _context;

        public DeviceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserDevice?> GetByTokenAsync(string deviceToken)
        {
            return await _context.UserDevices
                .FirstOrDefaultAsync(d => d.DeviceToken == deviceToken && d.IsActive);
        }

        public async Task<UserDevice?> GetUserDeviceAsync(Guid userId, string deviceToken)
        {
            return await _context.UserDevices
                .FirstOrDefaultAsync(d => d.UserId == userId && d.DeviceToken == deviceToken);
        }

        public async Task<IEnumerable<UserDevice>> GetDevicesByUserIdAsync(Guid userId)
        {
            return await _context.UserDevices
                .Where(d => d.UserId == userId && d.IsActive)
                .OrderByDescending(d => d.UpdatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserDevice>> GetAllActiveDevicesAsync()
        {
            return await _context.UserDevices
                .Where(d => d.IsActive)
                .ToListAsync();
        }

        public async Task<UserDevice> AddOrUpdateAsync(UserDevice device)
        {
            var existing = await _context.UserDevices
                .FirstOrDefaultAsync(d => d.UserId == device.UserId && d.DeviceToken == device.DeviceToken);

            if (existing != null)
            {
                existing.IsActive = true;
                existing.Platform = device.Platform ?? existing.Platform;
                existing.DeviceName = device.DeviceName ?? existing.DeviceName;
                existing.UpdatedAt = DateTime.UtcNow;
                _context.UserDevices.Update(existing);
                await _context.SaveChangesAsync();
                return existing;
            }

            device.Id = Guid.NewGuid();
            device.CreatedAt = DateTime.UtcNow;
            device.UpdatedAt = DateTime.UtcNow;
            device.IsActive = true;
            _context.UserDevices.Add(device);
            await _context.SaveChangesAsync();
            return device;
        }

        public async Task<bool> DeactivateOrRemoveAsync(Guid userId, string deviceToken)
        {
            var existing = await GetUserDeviceAsync(userId, deviceToken);
            if (existing == null) return false;

            existing.IsActive = false;
            existing.UpdatedAt = DateTime.UtcNow;
            _context.UserDevices.Update(existing);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task DeactivateTokensAsync(IEnumerable<string> invalidTokens)
        {
            var tokensList = invalidTokens.ToList();
            if (!tokensList.Any()) return;

            var devices = await _context.UserDevices
                .Where(d => tokensList.Contains(d.DeviceToken))
                .ToListAsync();

            foreach (var dev in devices)
            {
                dev.IsActive = false;
                dev.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}

