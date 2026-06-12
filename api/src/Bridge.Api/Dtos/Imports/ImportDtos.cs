namespace Bridge.Api.Dtos.Imports;

public class ImportResultResponse
{
    public bool DryRun { get; set; }
    public bool Valid { get; set; }
    public List<ImportSheetSummary> Sheets { get; set; } = [];
    public List<ImportRowError> Errors { get; set; } = [];
    /// <summary>dryRun=false で取り込みが成功した場合のみ。初期パスワードはこのレスポンスでしか返らない。</summary>
    public List<CreatedAccountResponse> CreatedAccounts { get; set; } = [];
}

public class ImportSheetSummary
{
    public string Sheet { get; set; } = string.Empty;
    public int NewCount { get; set; }
    public int UpdateCount { get; set; }
    public int ErrorCount { get; set; }
}

public class ImportRowError
{
    public string Sheet { get; set; } = string.Empty;
    public int Row { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class CreatedAccountResponse
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string InitialPassword { get; set; } = string.Empty;
}
