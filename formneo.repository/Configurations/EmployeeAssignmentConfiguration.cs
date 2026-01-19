using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using formneo.core.Models;

namespace formneo.repository.Configurations
{
    public class EmployeeAssignmentConfiguration : IEntityTypeConfiguration<EmployeeAssignment>
    {
        public void Configure(EntityTypeBuilder<EmployeeAssignment> builder)
        {
            builder.HasKey(e => e.Id);

            // User ilişkisi
            builder.HasOne(e => e.User)
                .WithMany(u => u.EmployeeAssignments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrgUnit ilişkisi
            builder.HasOne(e => e.OrgUnit)
                .WithMany()
                .HasForeignKey(e => e.OrgUnitId)
                .OnDelete(DeleteBehavior.Restrict);

            // Position ilişkisi
            builder.HasOne(e => e.Position)
                .WithMany()
                .HasForeignKey(e => e.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Manager ilişkisi
            builder.HasOne(e => e.Manager)
                .WithMany()
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // AssignmentType enum conversion
            builder.Property(e => e.AssignmentType)
                .HasConversion<int>();

            // Indexes
            builder.HasIndex(e => e.UserId);
            builder.HasIndex(e => new { e.UserId, e.EndDate }); // Aktif atamaları hızlı bulmak için
            builder.HasIndex(e => new { e.OrgUnitId, e.EndDate }); // Departman bazlı sorgular için
            builder.HasIndex(e => new { e.StartDate, e.EndDate }); // Tarih bazlı sorgular için

            // Table name
            builder.ToTable("EmployeeAssignments");
        }
    }
}


