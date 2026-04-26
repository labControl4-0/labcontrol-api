using Microsoft.EntityFrameworkCore;
using LabControlApi.Models;

namespace LabControlApi.Data
{
	public class AppDbContext : DbContext
	{
		public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

		public DbSet<User> Users { get; set; }
        public DbSet<Plant> Plants { get; set; }
        public DbSet<PlantVersion> PlantVersions { get; set; }
        public DbSet<Sector> Sectors { get; set; }
        public DbSet<Machine> Machines { get; set; }
        public DbSet<MachineMetric> MachineMetrics { get; set; }
        public DbSet<MachineEvent> MachineEvents { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
            base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<User>(entity =>
			{
				entity.HasKey(u => u.Id);
				entity.Property(u => u.Id).ValueGeneratedOnAdd();
                entity.HasMany(u => u.Plants)
                      .WithOne(p => p.User)
                      .HasForeignKey(p => p.UserId);
			});

            modelBuilder.Entity<Plant>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Scale).HasPrecision(10, 2);
                entity.Property(p => p.WidthUnits).HasPrecision(10, 2);
                entity.Property(p => p.HeightUnits).HasPrecision(10, 2);
                entity.HasIndex(p => p.UserId);

                entity.HasMany(p => p.Versions)
                      .WithOne(pv => pv.Plant)
                      .HasForeignKey(pv => pv.PlantId);
                
                entity.HasMany(p => p.Machines)
                        .WithOne(m => m.Plant)
                        .HasForeignKey(m => m.PlantId);
            });

            modelBuilder.Entity<PlantVersion>(entity =>
            {
                entity.HasKey(pv => pv.Id);
                entity.HasIndex(pv => pv.PlantId);
                entity.HasIndex(pv => new { pv.PlantId, pv.VersionNumber }).IsUnique();

                entity.HasOne(pv => pv.Creator)
                      .WithMany()
                      .HasForeignKey(pv => pv.CreatedBy);

                entity.HasOne(pv => pv.Plant)
                      .WithMany(p => p.Versions)
                      .HasForeignKey(pv => pv.PlantId);

                entity.HasMany(pv => pv.Sectors)
                      .WithOne(s => s.PlantVersion)
                      .HasForeignKey(s => s.PlantVersionId);
            });

            modelBuilder.Entity<Sector>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.HasIndex(s => s.PlantVersionId);
            });

            modelBuilder.Entity<Machine>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.HasIndex(m => m.PlantId);
                
                entity.HasMany(m => m.Metrics)
                      .WithOne(mm => mm.Machine)
                      .HasForeignKey(mm => mm.MachineId);

                entity.HasMany(m => m.Events)
                        .WithOne(me => me.Machine)
                        .HasForeignKey(me => me.MachineId);
            });

            modelBuilder.Entity<MachineMetric>(entity =>
            {
                entity.HasKey(mm => mm.Id);
                entity.HasIndex(mm => new { mm.MachineId, mm.CollectedAt });
            });

            modelBuilder.Entity<MachineEvent>(entity =>
            {
                entity.HasKey(me => me.Id);
                entity.HasIndex(me => me.MachineId);
            });
		}
	}
}
