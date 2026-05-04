using Bridge.Api.Dtos.Assignments;
using Bridge.Domain.Entities;
using Bridge.Domain.Enums;
using Bridge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bridge.Api.Services.Assignments;

public class AssignmentService : IAssignmentService
{
    private readonly BridgeDbContext _db;

    public AssignmentService(BridgeDbContext db)
    {
        _db = db;
    }

    // ---- アサイン作成(初回契約も同時に作る) ----
    public async Task<CreateAssignmentResult> CreateAsync(CreateAssignmentRequest request)
    {
        // 入力検証
        if (request.InitialContract.PeriodFrom > request.InitialContract.PeriodTo)
        {
            return Fail("INVALID_PERIOD", "契約期間が不正です(開始日が終了日より後)");
        }

        // Engineer/Project の存在確認
        var engineerExists = await _db.Engineers.AnyAsync(e => e.Id == request.EngineerId);
        if (!engineerExists) return Fail("ENGINEER_NOT_FOUND", "指定されたエンジニアが存在しません");

        var projectExists = await _db.Projects.AnyAsync(p => p.Id == request.ProjectId);
        if (!projectExists) return Fail("PROJECT_NOT_FOUND", "指定された案件が存在しません");

        // 同じエンジニア×同じ案件の重複アサインを禁止(DB側にも UK 制約あり)
        var duplicateExists = await _db.Assignments
            .AnyAsync(a => a.EngineerId == request.EngineerId && a.ProjectId == request.ProjectId);
        if (duplicateExists)
        {
            return Fail("DUPLICATE_ASSIGNMENT", "このエンジニアは既にこの案件にアサインされています");
        }

        await using var tx = await _db.Database.BeginTransactionAsync();

        var assignment = new Assignment
        {
            EngineerId = request.EngineerId,
            ProjectId = request.ProjectId,
            AssignedAt = request.AssignedAt,
            Status = AssignmentStatus.Active,
        };
        _db.Assignments.Add(assignment);
        await _db.SaveChangesAsync();

        var contract = new Contract
        {
            AssignmentId = assignment.Id,
            PeriodFrom = request.InitialContract.PeriodFrom,
            PeriodTo = request.InitialContract.PeriodTo,
            UnitPrice = request.InitialContract.UnitPrice,
            ContractType = ContractType.Initial,
        };
        _db.Contracts.Add(contract);
        await _db.SaveChangesAsync();

        await tx.CommitAsync();

        var detail = await GetByIdAsync(assignment.Id);
        return new CreateAssignmentResult { Success = true, Assignment = detail };
    }

    // ---- 単一アサインの詳細取得 ----
    public async Task<AssignmentDetailResponse?> GetByIdAsync(int id)
    {
        var assignment = await _db.Assignments
            .Include(a => a.Engineer)
            .Include(a => a.Project)
            .Include(a => a.Contracts)
            .AsSplitQuery()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (assignment is null) return null;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return new AssignmentDetailResponse
        {
            Id = assignment.Id,
            EngineerId = assignment.EngineerId,
            EngineerName = assignment.Engineer.Name,
            ProjectId = assignment.ProjectId,
            ProjectTitle = assignment.Project.Title,
            ClientName = assignment.Project.ClientName,
            Status = assignment.Status.ToString().ToLowerInvariant(),
            AssignedAt = assignment.AssignedAt,
            Contracts = MapContracts(assignment.Contracts, today),
        };
    }

    // ---- アサインのステータス更新(active → ended など) ----
    public async Task<AssignmentDetailResponse?> UpdateStatusAsync(int id, UpdateAssignmentRequest request)
    {
        var assignment = await _db.Assignments.FirstOrDefaultAsync(a => a.Id == id);
        if (assignment is null) return null;

        if (!Enum.TryParse<AssignmentStatus>(request.Status, ignoreCase: true, out var newStatus))
            return null;

        assignment.Status = newStatus;
        assignment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    // ---- エンジニアの全アサイン+契約履歴(API 設計書 5.2 節) ----
    public async Task<EngineerAssignmentsResponse?> GetByEngineerIdAsync(int engineerId)
    {
        var engineerExists = await _db.Engineers.AnyAsync(e => e.Id == engineerId);
        if (!engineerExists) return null;

        var assignments = await _db.Assignments
            .Include(a => a.Project)
            .Include(a => a.Contracts)
            .AsSplitQuery()
            .Where(a => a.EngineerId == engineerId)
            .OrderByDescending(a => a.AssignedAt)
            .ToListAsync();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return new EngineerAssignmentsResponse
        {
            Assignments = assignments.Select(a => new AssignmentWithContracts
            {
                Id = a.Id,
                ProjectId = a.ProjectId,
                ProjectTitle = a.Project.Title,
                ClientName = a.Project.ClientName,
                Status = a.Status.ToString().ToLowerInvariant(),
                AssignedAt = a.AssignedAt,
                Contracts = MapContracts(a.Contracts, today),
            }).ToList(),
        };
    }

    // ---- 契約追加(更新契約)----
    public async Task<CreateContractResult> AddContractAsync(int assignmentId, CreateContractRequest request)
    {
        // 期間検証
        if (request.PeriodFrom > request.PeriodTo)
        {
            return new CreateContractResult
            {
                Success = false,
                ErrorCode = "INVALID_PERIOD",
                ErrorMessage = "契約期間が不正です",
            };
        }

        var assignment = await _db.Assignments
            .Include(a => a.Contracts)
            .FirstOrDefaultAsync(a => a.Id == assignmentId);

        if (assignment is null)
        {
            return new CreateContractResult
            {
                Success = false,
                ErrorCode = "ASSIGNMENT_NOT_FOUND",
                ErrorMessage = "指定されたアサインが存在しません",
            };
        }

        // 期間重複チェック:既存の契約と1日でも重なるなら拒否
        var hasOverlap = assignment.Contracts.Any(c =>
            request.PeriodFrom <= c.PeriodTo && c.PeriodFrom <= request.PeriodTo);

        if (hasOverlap)
        {
            return new CreateContractResult
            {
                Success = false,
                ErrorCode = "PERIOD_OVERLAP",
                ErrorMessage = "既存の契約と期間が重複しています",
            };
        }

        // 既存契約があれば renewal、なければ initial(通常 initial は CreateAssignment で作るので
        // ここに来る場合は renewal だが、念のため判定する)
        var contractType = assignment.Contracts.Any() ? ContractType.Renewal : ContractType.Initial;

        var contract = new Contract
        {
            AssignmentId = assignmentId,
            PeriodFrom = request.PeriodFrom,
            PeriodTo = request.PeriodTo,
            UnitPrice = request.UnitPrice,
            ContractType = contractType,
        };
        _db.Contracts.Add(contract);

        assignment.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new CreateContractResult
        {
            Success = true,
            Contract = new ContractItem
            {
                Id = contract.Id,
                PeriodFrom = contract.PeriodFrom,
                PeriodTo = contract.PeriodTo,
                UnitPrice = contract.UnitPrice,
                ContractType = contract.ContractType.ToString().ToLowerInvariant(),
                IsCurrent = IsCurrentContract(contract, today),
            },
        };
    }

    // ---- 単一アサインの契約履歴 ----
    public async Task<List<ContractItem>?> GetContractsByAssignmentIdAsync(int assignmentId)
    {
        var assignment = await _db.Assignments
            .Include(a => a.Contracts)
            .FirstOrDefaultAsync(a => a.Id == assignmentId);

        if (assignment is null) return null;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return MapContracts(assignment.Contracts, today);
    }

    // ---- ヘルパー ----
    private static List<ContractItem> MapContracts(IEnumerable<Contract> contracts, DateOnly today)
    {
        return contracts
            .OrderByDescending(c => c.PeriodFrom)
            .Select(c => new ContractItem
            {
                Id = c.Id,
                PeriodFrom = c.PeriodFrom,
                PeriodTo = c.PeriodTo,
                UnitPrice = c.UnitPrice,
                ContractType = c.ContractType.ToString().ToLowerInvariant(),
                IsCurrent = IsCurrentContract(c, today),
            })
            .ToList();
    }

    private static bool IsCurrentContract(Contract c, DateOnly today)
    {
        return c.PeriodFrom <= today && today <= c.PeriodTo;
    }

    private static CreateAssignmentResult Fail(string code, string message)
    {
        return new CreateAssignmentResult
        {
            Success = false,
            ErrorCode = code,
            ErrorMessage = message,
        };
    }
}