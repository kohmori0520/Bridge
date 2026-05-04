using Bridge.Api.Dtos.Skills;

namespace Bridge.Api.Services.Skills;

public interface ISkillService
{
    Task<List<SkillResponse>> ListAsync();
}