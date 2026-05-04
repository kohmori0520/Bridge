using Bridge.Api.Dtos.EngineerSkills;
using Bridge.Api.Dtos.Engineers;

namespace Bridge.Api.Services.EngineerSkills;

public interface IEngineerSkillService
{
    Task<List<EngineerSkillDto>?> GetAsync(int engineerId);
    Task<List<EngineerSkillDto>?> UpdateAsync(int engineerId, UpdateEngineerSkillsRequest request);
}