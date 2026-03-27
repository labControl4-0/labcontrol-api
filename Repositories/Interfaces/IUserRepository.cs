using LabControlApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LabControlApi.Repositories.Interfaces
{
	public interface IUserRepository
	{
		Task<IEnumerable<User>> GetAllAsync();
		Task<User?> GetByIdAsync(Guid id);
		Task<User> AddAsync(User user);
		Task<User?> UpdateAsync(User user);
		Task<bool> DeleteAsync(Guid id);
		Task CreateAsync(User user);
		Task<User?> GetByEmailAsync(string email);
	}
}
