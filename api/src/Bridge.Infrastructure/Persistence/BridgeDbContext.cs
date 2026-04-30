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
}