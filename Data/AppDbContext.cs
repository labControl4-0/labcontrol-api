using Microsoft.EntityFrameworkCore;
using LabControlApi.Models;

namespace LabControlApi.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<User> Users { get; set; }
		public DbSet<PlantVersion> PlantVersions { get; set; } = null!;
		public DbSet<Sector> Sectors { get; set; } = null!;

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfiguration<PlantVersion>(new ModelConfiguration());
			modelBuilder.ApplyConfiguration<Sector>(new ModelConfiguration());
		}
	}
}
