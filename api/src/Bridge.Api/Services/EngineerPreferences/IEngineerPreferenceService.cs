using Bridge.Api.Dtos.EngineerPreferences;

namespace Bridge.Api.Services.EngineerPreferences;

public interface IEngineerPreferenceService
{
    Task<EngineerPreferencesResponse?> GetAsync(int engineerId);
    Task<EngineerPreferencesResponse?> UpdateAsync(int engineerId, UpdateEngineerPreferencesRequest request);
}