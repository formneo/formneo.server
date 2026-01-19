using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using formneo.core.Models;

namespace formneo.repository.Configurations
{
    internal class OrgUnitConfiguration : IEntityTypeConfiguration<OrgUnit>
    {
        public void Configure(EntityTypeBuilder<OrgUnit> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
            builder.Property(x => x.Code).HasMaxLength(50);
            builder.Property(x => x.Type).HasConversion<int>();

            builder.ToTable("OrgUnits");
        }
    }
}

