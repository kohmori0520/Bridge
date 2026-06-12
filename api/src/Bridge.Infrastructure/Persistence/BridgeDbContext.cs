using Bridge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Bridge.Infrastructure.Persistence;

public class BridgeDbContext : DbContext
{
    public BridgeDbContext(DbContextOptions<BridgeDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Sales> Sales => Set<Sales>();
    public DbSet<Engineer> Engineers => Set<Engineer>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<EngineerSkill> EngineerSkills => Set<EngineerSkill>();
    public DbSet<EngineerPreferredSkill> EngineerPreferredSkills => Set<EngineerPreferredSkill>();
    public DbSet<EngineerPreferredCategory> EngineerPreferredCategories => Set<EngineerPreferredCategory>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectRequiredSkill> ProjectRequiredSkills => Set<ProjectRequiredSkill>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<Contract> Contracts => Set<Contract>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BridgeDbContext).Assembly);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        TouchUpdatedAt();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        TouchUpdatedAt();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    // 変更されたエンティティの UpdatedAt を自動更新する。
    // サービス側での手動代入は不要(子エンティティのみ変更時に親を「触る」目的の代入は引き続き有効)
    private void TouchUpdatedAt()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<ITimestamped>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}