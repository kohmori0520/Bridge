namespace Bridge.Domain.Entities;

/// <summary>
/// CreatedAt / UpdatedAt を持つエンティティ。
/// UpdatedAt は BridgeDbContext.SaveChanges 時に自動更新される。
/// </summary>
public interface ITimestamped
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
}
