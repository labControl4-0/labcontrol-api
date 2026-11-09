using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using LabControlApi.DTOs.Device;
using LabControlApi.DTOs.Push;
using LabControlApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabControlApi.Controllers
{
    [ApiController]
    [Route("api/devices")]
    [Authorize]
    public class DevicesController : ControllerBase
    {
        private readonly IDeviceService _deviceService;
        private readonly IPushNotificationService _pushService;

        public DevicesController(IDeviceService deviceService, IPushNotificationService pushService)
        {
            _deviceService = deviceService;
            _pushService = pushService;
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DeviceResponseDto>>> GetMyDevices()
        {
            var userId = GetUserId();
            var devices = await _deviceService.GetUserDevicesAsync(userId);
            return Ok(devices);
        }

        [HttpPost]
        public async Task<ActionResult<DeviceResponseDto>> RegisterDevice([FromBody] RegisterDeviceDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            var device = await _deviceService.RegisterDeviceAsync(userId, dto);
            return Ok(device);
        }

        [HttpDelete]
        public async Task<IActionResult> UnregisterDevice([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest("O token é obrigatório.");
            }

            var userId = GetUserId();
            var success = await _deviceService.UnregisterDeviceAsync(userId, token);

            if (!success)
            {
                return NotFound("Dispositivo não encontrado ou não pertence a este usuário.");
            }

            return NoContent();
        }

        [HttpPost("test-push")]
        public async Task<IActionResult> TestPush([FromBody] SendPushNotificationDto dto)
        {
            var userId = GetUserId();
            var success = await _pushService.SendPushToUserAsync(userId, dto);

            if (!success)
            {
                return BadRequest(new { message = "Não foi possível enviar o push. Verifique se o usuário possui dispositivos ativos." });
            }

            return Ok(new { message = "Notificação de teste enviada com sucesso para os dispositivos do usuário." });
        }
    }
}

