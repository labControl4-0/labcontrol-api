
using LabControlApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LabControlApi.Repositories.Interfaces
{
	public interface IUserRepository
	{
		Task<IEnumerable<User>> GetAllAsync();
		Task<User?> GetByIdAsync(int id);
		Task<User> AddAsync(User user);
		Task<User?> UpdateAsync(User user);
		Task<bool> DeleteAsync(int id);
	}
}
