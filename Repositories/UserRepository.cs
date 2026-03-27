using System;
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

		public async Task<User?> GetByIdAsync(Guid id)
		{
			return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
		}

		public async Task<User> AddAsync(User user)
		{
			_context.Users.Add(user);
			await _context.SaveChangesAsync();
			return user;
		}

		public async Task<User?> UpdateAsync(User user)
		{
			var existingUser = await GetByIdAsync(user.Id);
			if (existingUser == null) return null;

			existingUser.Name = user.Name;
			existingUser.Email = user.Email;
			existingUser.UpdatedAt = DateTime.UtcNow;

			_context.Users.Update(existingUser);
			await _context.SaveChangesAsync();

			return existingUser;
		}

		public async Task<bool> DeleteAsync(Guid id)
		{
			var user = await GetByIdAsync(id);
			if (user == null) return false;

			_context.Users.Remove(user);
			await _context.SaveChangesAsync();

			return true;
		}

		public async Task CreateAsync(User user)
		{
			await _context.Users.AddAsync(user);
			await _context.SaveChangesAsync();
		}

		public async Task<User?> GetByEmailAsync(string email)
		{
			return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
		}
	}
}
