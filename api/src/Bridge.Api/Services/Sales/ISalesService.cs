using Bridge.Api.Dtos.Sales;

namespace Bridge.Api.Services.Sales;

public interface ISalesService
{
    Task<List<SalesResponse>> ListAsync();
    Task<SalesResponse?> GetByIdAsync(int id);
    Task<SalesResponse?> UpdateAsync(int id, UpdateSalesRequest request);
}