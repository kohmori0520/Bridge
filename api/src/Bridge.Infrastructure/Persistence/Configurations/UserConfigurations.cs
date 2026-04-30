using Bridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bridge.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(512);

        builder.Property(u => u.Role)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);
    }
}

public class SalesConfiguration : IEntityTypeConfiguration<Sales>
{
    public void Configure(EntityTypeBuilder<Sales> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Department).HasMaxLength(100);

        builder.HasOne(s => s.User)
            .WithOne(u => u.Sales!)
            .HasForeignKey<Sales>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.UserId).IsUnique();
    }
}

public class EngineerConfiguration : IEntityTypeConfiguration<Engineer>
{
    public void Configure(EntityTypeBuilder<Engineer> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Bio).HasColumnType("text");
        builder.Property(e => e.AvoidedWorkNote).HasColumnType("text");

        builder.HasOne(e => e.User)
            .WithOne(u => u.Engineer!)
            .HasForeignKey<Engineer>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.PrimarySales)
            .WithMany(s => s.Engineers)
            .HasForeignKey(e => e.PrimarySalesId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.UserId).IsUnique();
        builder.HasIndex(e => e.PrimarySalesId);
    }
}