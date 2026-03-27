using LabControlApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabControlApi.Data
{
    public class ModelConfiguration : IEntityTypeConfiguration<PlantVersion>, IEntityTypeConfiguration<Sector>
    {
        public void Configure(EntityTypeBuilder<PlantVersion> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.Name).IsRequired();
            builder.Property(p => p.WidthUnits).HasPrecision(18, 2);
            builder.Property(p => p.HeightUnits).HasPrecision(18, 2);
            builder.Property(p => p.Scale).HasPrecision(18, 2);
            builder.HasMany(p => p.Sectors).WithOne(s => s.PlantVersion).HasForeignKey(s => s.PlantVersionId);
        }

        public void Configure(EntityTypeBuilder<Sector> builder)
        {
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Name).IsRequired();
            builder.Property(s => s.PointsJson).IsRequired();
            builder.Property(s => s.MinX).HasPrecision(18, 2);
            builder.Property(s => s.MinY).HasPrecision(18, 2);
            builder.Property(s => s.MaxX).HasPrecision(18, 2);
            builder.Property(s => s.MaxY).HasPrecision(18, 2);
            builder.Property(s => s.AreaM2).HasPrecision(18, 2);
            builder.HasIndex(s => s.PlantVersionId);
        }
    }
}
