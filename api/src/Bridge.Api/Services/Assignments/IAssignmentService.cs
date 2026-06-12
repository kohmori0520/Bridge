using Bridge.Api.Dtos.Assignments;

namespace Bridge.Api.Services.Assignments;

public interface IAssignmentService
{
    Task<CreateAssignmentResult> CreateAsync(CreateAssignmentRequest request);
    Task<AssignmentDetailResponse?> GetByIdAsync(int id);
    Task<UpdateAssignmentResult> UpdateStatusAsync(int id, UpdateAssignmentRequest request);
    Task<EngineerAssignmentsResponse?> GetByEngineerIdAsync(int engineerId);
    Task<CreateContractResult> AddContractAsync(int assignmentId, CreateContractRequest request);
    Task<List<ContractItem>?> GetContractsByAssignmentIdAsync(int assignmentId);
}