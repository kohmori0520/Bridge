using Bridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Bridge.Infrastructure.Persistence.SeedData;

namespace Bridge.Infrastructure.Persistence.Configurations;

public class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(s => s.Name).IsUnique();

        builder.Property(s => s.Category)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(30);

        builder.HasData(SkillSeedData.GetSeedSkills());
    }
}

public class EngineerSkillConfiguration : IEntityTypeConfiguration<EngineerSkill>
{
    public void Configure(EntityTypeBuilder<EngineerSkill> builder)
    {
        builder.HasKey(es => es.Id);

        builder.HasOne(es => es.Engineer)
            .WithMany(e => e.Skills)
            .HasForeignKey(es => es.EngineerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(es => es.Skill)
            .WithMany(s => s.EngineerSkills)
            .HasForeignKey(es => es.SkillId)
            .OnDelete(DeleteBehavior.Restrict);

        // 同じエンジニア×同じスキルの重複を禁止
        builder.HasIndex(es => new { es.EngineerId, es.SkillId }).IsUnique();
    }
}

public class EngineerPreferredSkillConfiguration : IEntityTypeConfiguration<EngineerPreferredSkill>
{
    public void Configure(EntityTypeBuilder<EngineerPreferredSkill> builder)
    {
        builder.HasKey(eps => eps.Id);

        builder.HasOne(eps => eps.Engineer)
            .WithMany(e => e.PreferredSkills)
            .HasForeignKey(eps => eps.EngineerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(eps => eps.Skill)
            .WithMany(s => s.EngineerPreferredSkills)
            .HasForeignKey(eps => eps.SkillId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(eps => new { eps.EngineerId, eps.SkillId }).IsUnique();
    }
}

public class EngineerPreferredCategoryConfiguration : IEntityTypeConfiguration<EngineerPreferredCategory>
{
    public void Configure(EntityTypeBuilder<EngineerPreferredCategory> builder)
    {
        builder.HasKey(epc => epc.Id);

        builder.Property(epc => epc.Category)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(30);

        builder.HasOne(epc => epc.Engineer)
            .WithMany(e => e.PreferredCategories)
            .HasForeignKey(epc => epc.EngineerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(epc => new { epc.EngineerId, epc.Category }).IsUnique();

    }
}