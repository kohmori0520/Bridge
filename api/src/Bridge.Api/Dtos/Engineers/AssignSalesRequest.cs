namespace Bridge.Api.Dtos.Engineers;

public class AssignSalesRequest
{
    /// <summary>
    /// 担当営業の Sales.Id。null の場合は担当を外す。
    /// </summary>
    public int? PrimarySalesId { get; set; }
}
