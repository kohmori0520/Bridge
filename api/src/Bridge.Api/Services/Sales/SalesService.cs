using Bridge.Api.Dtos.Sales;
using Bridge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Bridge.Api.Services.Sales;

public class SalesService : ISalesService
{
    private readonly BridgeDbContext _db;

    public SalesService(BridgeDbContext db)
    {
        _db = db;
    }

    public async Task<List<SalesResponse>> ListAsync()
    {
        return await _db.Sales
            .AsNoTracking()
            .OrderBy(s => s.Id)
            .Select(s => new SalesResponse
            {
                Id = s.Id,
                Name = s.Name,
                Department = s.Department,
                Email = s.User.Email,
            })
            .ToListAsync();
    }

    public async Task<SalesResponse?> GetByIdAsync(int id)
    {
        return await _db.Sales
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new SalesResponse
            {
                Id = s.Id,
                Name = s.Name,
                Department = s.Department,
                Email = s.User.Email,
            })
            .FirstOrDefaultAsync();
    }

    public async Task<SalesResponse?> UpdateAsync(int id, UpdateSalesRequest request)
    {
        var sales = await _db.Sales
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sales is null) return null;

        sales.Name = request.Name;
        sales.Department = request.Department;
        sales.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return new SalesResponse
        {
            Id = sales.Id,
            Name = sales.Name,
            Department = sales.Department,
            Email = sales.User.Email,
        };
    }
}