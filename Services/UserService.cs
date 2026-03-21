using LabControlApi.DTOs;
using LabControlApi.Models;
using LabControlApi.Repositories.Interfaces;
using LabControlApi.Services.Interfaces;

namespace LabControlApi.Services
{
	public class UserService : IUserService
	{
		private readonly IUserRepository _repository;

		public UserService(IUserRepository repository)
		{
			_repository = repository;
		}

		public async Task<IEnumerable<UserResponseDto>> GetAllAsync()
		{
			var users = await _repository.GetAllAsync();
			return users.Select(u => ToDto(u));
		}

		public async Task<UserResponseDto?> GetByIdAsync(int id)
		{
			var user = await _repository.GetByIdAsync(id);
			return user == null ? null : ToDto(user);
		}

		public async Task<UserResponseDto> CreateAsync(CreateUserDto dto)
		{
			var user = new User
			{
				Name = dto.Name,
				Email = dto.Email
			};
			var created = await _repository.AddAsync(user);
			return ToDto(created);
		}

		public async Task<UserResponseDto?> UpdateAsync(int id, UpdateUserDto dto)
		{
			var user = new User
			{
				Id = id,
				Name = dto.Name,
				Email = dto.Email
			};
			var updated = await _repository.UpdateAsync(user);
			return updated == null ? null : ToDto(updated);
		}

		public async Task<bool> DeleteAsync(int id)
		{
			return await _repository.DeleteAsync(id);
		}

		private static UserResponseDto ToDto(User user)
		{
			return new UserResponseDto
			{
				Id = user.Id,
				Name = user.Name,
				Email = user.Email,
				CreatedAt = user.CreatedAt
			};
		}
	}
}
