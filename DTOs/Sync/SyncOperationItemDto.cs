using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace LabControlApi.DTOs.Sync
{
    public class SyncOperationItemDto
    {
        [Required]
        public Guid ClientOperationId { get; set; }

        [Required]
        public string EntityType { get; set; } = string.Empty; // "Plant", "Machine", "Sector", "MachineEvent"

        [Required]
        public string Action { get; set; } = string.Empty; // "CREATE", "UPDATE", "DELETE"

        public Guid? EntityId { get; set; }

        public DateTime ClientTimestamp { get; set; } = DateTime.UtcNow;

        public JsonElement? Data { get; set; }
    }
}

