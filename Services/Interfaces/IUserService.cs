using LabControlApi.DTOs;

namespace LabControlApi.Services.Interfaces
{
	public interface IUserService
	{
		Task<IEnumerable<UserResponseDto>> GetAllAsync();
		Task<UserResponseDto?> GetByIdAsync(Guid id);
		Task<UserResponseDto> CreateAsync(CreateUserDto dto);
		Task<UserResponseDto?> UpdateAsync(Guid id, UpdateUserDto dto);
		Task<bool> DeleteAsync(Guid id);
	}
}
