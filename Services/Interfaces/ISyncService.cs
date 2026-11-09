using System;
using System.Threading.Tasks;
using LabControlApi.DTOs.Sync;

namespace LabControlApi.Services.Interfaces
{
    public interface ISyncService
    {
        Task<SyncBatchResponseDto> ProcessBatchAsync(Guid userId, SyncBatchRequestDto batchDto);
    }
}

