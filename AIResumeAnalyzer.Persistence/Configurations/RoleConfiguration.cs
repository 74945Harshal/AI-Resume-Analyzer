using AIResumeAnalyzer.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace AIResumeAnalyzer.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Name).IsRequired().HasMaxLength(50);

        builder.HasData(
            new Role { Id = 1, Name = "Admin", CreatedDate = new DateTime(2026, 5, 31, 0, 0, 0, DateTimeKind.Utc) },
            new Role { Id = 2, Name = "User", CreatedDate = new DateTime(2026, 5, 31, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}
