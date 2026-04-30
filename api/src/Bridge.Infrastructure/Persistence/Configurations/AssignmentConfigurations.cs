using Bridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bridge.Infrastructure.Persistence.Configurations;

public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.HasOne(a => a.Engineer)
            .WithMany(e => e.Assignments)
            .HasForeignKey(a => a.EngineerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Project)
            .WithMany(p => p.Assignments)
            .HasForeignKey(a => a.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        // 同じエンジニアを同じ案件に2回アサインできない
        builder.HasIndex(a => new { a.EngineerId, a.ProjectId }).IsUnique();
        builder.HasIndex(a => a.Status);
    }
}

public class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.ContractType)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.HasOne(c => c.Assignment)
            .WithMany(a => a.Contracts)
            .HasForeignKey(c => c.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.AssignmentId);
        builder.HasIndex(c => c.PeriodTo);  // 更新間近検索の高速化
    }
}