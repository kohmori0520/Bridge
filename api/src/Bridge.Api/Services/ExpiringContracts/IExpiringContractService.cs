using Bridge.Api.Dtos.ExpiringContracts;

namespace Bridge.Api.Services.ExpiringContracts;

public interface IExpiringContractService
{
    /// <summary>
    /// 指定の期間内に契約が終了する案件を取得する。
    /// </summary>
    /// <param name="days">残日数のしきい値(これ以下を返す)</param>
    /// <param name="filterBySalesId">指定された場合、その営業の担当エンジニアのみに絞る</param>
    Task<ExpiringContractsResponse> GetAsync(int days, int? filterBySalesId);
}