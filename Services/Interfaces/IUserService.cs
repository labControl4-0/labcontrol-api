using LabControlApi.DTOs;

namespace LabControlApi.Services.Interfaces
{
	public interface IUserService
	{
		Task<IEnumerable<UserResponseDto>> GetAllAsync();
		Task<UserResponseDto?> GetByIdAsync(int id);
		Task<UserResponseDto> CreateAsync(CreateUserDto dto);
		Task<UserResponseDto?> UpdateAsync(int id, UpdateUserDto dto);
		Task<bool> DeleteAsync(int id);
	}
}
