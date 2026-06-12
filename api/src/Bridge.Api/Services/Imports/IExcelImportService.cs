using Bridge.Api.Dtos.Imports;

namespace Bridge.Api.Services.Imports;

public interface IExcelImportService
{
    /// <summary>取り込み用テンプレート xlsx を生成する(スキルマスタ・ドロップダウン付き)。</summary>
    Task<byte[]> BuildTemplateAsync();

    /// <summary>
    /// Excel を検証・取り込みする。1行でもエラーがあれば何も取り込まない(全件 or なし)。
    /// </summary>
    /// <param name="stream">xlsx ファイル</param>
    /// <param name="dryRun">true なら検証のみ</param>
    /// <param name="importerSalesId">実行者の SalesId。担当営業メール空欄時のフォールバック(Admin は null)</param>
    /// <exception cref="InvalidImportFileException">xlsx として読み込めない場合</exception>
    Task<ImportResultResponse> ImportAsync(Stream stream, bool dryRun, int? importerSalesId);
}

public class InvalidImportFileException : Exception
{
    public InvalidImportFileException(string message, Exception? inner = null) : base(message, inner) { }
}
