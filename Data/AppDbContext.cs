
using Microsoft.EntityFrameworkCore;
using LabControlApi.Models;

namespace LabControlApi.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<User> Users { get; set; }
	}
}
