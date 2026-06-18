using System.Security.Claims;
using LabControlApi.DTOs.Chat;
using LabControlApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LabControlApi.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        private Guid GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User not authenticated.");
            return new Guid(userId);
        }

        [HttpPost]
        public async Task<ActionResult<ChatResponseDto>> Chat(ChatRequestDto request)
        {
            var userId = GetUserId();
            var result = await _chatService.Chat(request.Message, userId);
            return Ok(result);
        }
    }
}
