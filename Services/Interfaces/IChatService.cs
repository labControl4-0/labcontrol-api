using LabControlApi.DTOs.Chat;

namespace LabControlApi.Services.Interfaces
{
    public interface IChatService
    {
        Task<ChatResponseDto> Chat(string message, Guid userId);
    }
}
