using System;
using System.Security.Claims;
using System.Threading.Tasks;
using LabControlApi.DTOs.Sync;
using LabControlApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabControlApi.Controllers
{
    [ApiController]
    [Route("api/sync")]
    [Authorize]
    public class SyncController : ControllerBase
    {
        private readonly ISyncService _syncService;

        public SyncController(ISyncService syncService)
        {
            _syncService = syncService;
        }

        private Guid GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Usuário não autenticado.");
            }
            return new Guid(userId);
        }

        /// <summary>
        /// Endpoint para envio em lote de operações realizadas offline pelo aplicativo mobile.
        /// Garante idempotência via ClientOperationId e detecta conflitos de concorrência.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<SyncBatchResponseDto>> SyncBatch([FromBody] SyncBatchRequestDto batchDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            var response = await _syncService.ProcessBatchAsync(userId, batchDto);
            return Ok(response);
        }
    }
}

