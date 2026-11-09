using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LabControlApi.DTOs.Sync
{
    public class SyncBatchRequestDto
    {
        [Required]
        public Guid ClientId { get; set; }

        public List<SyncOperationItemDto> Operations { get; set; } = new();
    }
}

