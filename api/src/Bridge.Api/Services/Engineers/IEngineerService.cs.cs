using Bridge.Api.Dtos.Engineers;

namespace Bridge.Api.Services.Engineers;

public interface IEngineerService
{
    Task<EngineerDetailResponse?> GetByIdAsync(int id);
    Task<EngineerListResponse> ListAsync(EngineerListQuery query);
    Task<EngineerDetailResponse?> UpdateAsync(int id, UpdateEngineerRequest request);
}

public class EngineerListQuery
{
    public int? PrimarySalesId { get; set; }
    public bool? Available { get; set; }
    public int? SkillId { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
}