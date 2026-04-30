using Bridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bridge.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title).IsRequired().HasMaxLength(200);
        builder.Property(p => p.ClientName).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).HasColumnType("text");

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.HasOne(p => p.OwnerSales)
            .WithMany(s => s.OwnedProjects)
            .HasForeignKey(p => p.OwnerSalesId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.OwnerSalesId);
        builder.HasIndex(p => p.Status);
    }
}

public class ProjectRequiredSkillConfiguration : IEntityTypeConfiguration<ProjectRequiredSkill>
{
    public void Configure(EntityTypeBuilder<ProjectRequiredSkill> builder)
    {
        builder.HasKey(prs => prs.Id);

        builder.Property(prs => prs.RequirementType)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.HasOne(prs => prs.Project)
            .WithMany(p => p.RequiredSkills)
            .HasForeignKey(prs => prs.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(prs => prs.Skill)
            .WithMany(s => s.ProjectRequiredSkills)
            .HasForeignKey(prs => prs.SkillId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(prs => new { prs.ProjectId, prs.SkillId }).IsUnique();
    }
}