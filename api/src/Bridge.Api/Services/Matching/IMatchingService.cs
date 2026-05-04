using Bridge.Api.Dtos.Matching;

namespace Bridge.Api.Services.Matching;

public interface IMatchingService
{
    Task<ProjectMatchesResponse?> GetMatchesForProjectAsync(int projectId, int page, int limit);
    Task<EngineerMatchesResponse?> GetMatchesForEngineerAsync(int engineerId, int page, int limit);
}