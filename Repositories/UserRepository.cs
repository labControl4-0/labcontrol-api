
using LabControlApi.Models;
using LabControlApi.Data;
using LabControlApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LabControlApi.Repositories
{
	public class UserRepository : IUserRepository
	{
		private readonly AppDbContext _context;

		public UserRepository(AppDbContext context)
		{
			_context = context;
		}

		public async Task<IEnumerable<User>> GetAllAsync()
		{
			return await _context.Users.ToListAsync();
		}

		public async Task<User?> GetByIdAsync(int id)
		{
			return await _context.Users.FindAsync(id);
		}

		public async Task<User> AddAsync(User user)
		{
			_context.Users.Add(user);
			await _context.SaveChangesAsync();
			return user;
		}

		public async Task<User?> UpdateAsync(User user)
		{
			var existing = await _context.Users.FindAsync(user.Id);
			if (existing == null) return null;
			existing.Name = user.Name;
			existing.Email = user.Email;
			await _context.SaveChangesAsync();
			return existing;
		}

		public async Task<bool> DeleteAsync(int id)
		{
			var user = await _context.Users.FindAsync(id);
			if (user == null) return false;
			_context.Users.Remove(user);
			await _context.SaveChangesAsync();
			return true;
		}
	}
}
