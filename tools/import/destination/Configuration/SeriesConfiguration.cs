namespace TNO.Tools.Import.Destination.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TNO.Tools.Import.Destination.Entities;

public class SeriesConfiguration : IEntityTypeConfiguration<Series>
{
    public virtual void Configure(EntityTypeBuilder<Series> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(m => m.Name).IsRequired();
    }
}