using System;

namespace LabControlApi.DTOs.Sync
{
    public class SyncOperationResultDto
    {
        public Guid ClientOperationId { get; set; }
        public string Status { get; set; } = string.Empty; // "SUCCESS", "ALREADY_PROCESSED", "CONFLICT", "ERROR"
        public string? Message { get; set; }
        public Guid? EntityId { get; set; }
        public object? CurrentServerState { get; set; } // Retornado caso haja CONFLICT
    }

    public class SyncBatchResponseDto
    {
        public DateTime ServerTimestamp { get; set; } = DateTime.UtcNow;
        public int TotalProcessed { get; set; }
        public int SuccessCount { get; set; }
        public int ConflictCount { get; set; }
        public int ErrorCount { get; set; }
        public System.Collections.Generic.List<SyncOperationResultDto> Results { get; set; } = new();
    }
}

