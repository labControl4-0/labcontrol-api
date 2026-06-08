using LabControlApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabControlApi.Data
{
    public class ModelConfiguration : IEntityTypeConfiguration<Sector>
    {
        public void Configure(EntityTypeBuilder<Sector> builder)
        {
            builder.HasKey(s => s.Id);
            builder.Property(s => s.Name).IsRequired();
            builder.Property(s => s.MinX).HasPrecision(18, 2);
            builder.Property(s => s.MinY).HasPrecision(18, 2);
            builder.Property(s => s.MaxX).HasPrecision(18, 2);
            builder.Property(s => s.MaxY).HasPrecision(18, 2);
            builder.Property(s => s.AreaM2).HasPrecision(18, 2);
        }
    }
}
