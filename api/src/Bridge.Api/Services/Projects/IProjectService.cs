using Bridge.Api.Dtos.Projects;

namespace Bridge.Api.Services.Projects;

public interface IProjectService
{
    Task<ProjectListResponse> ListAsync(ProjectListQuery query);
    Task<ProjectDetailResponse?> GetByIdAsync(int id);
    Task<ProjectDetailResponse> CreateAsync(CreateProjectRequest request, int ownerSalesId);
    Task<ProjectDetailResponse?> UpdateAsync(int id, UpdateProjectRequest request);
    Task<ProjectDeleteResult> SoftDeleteAsync(int id);
}

public enum ProjectDeleteResult
{
    Deleted,
    NotFound,
    HasAssignments,
}

public class ProjectListQuery
{
    public int? OwnerSalesId { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 20;
}