using Microsoft.AspNetCore.Diagnostics;

namespace Bridge.Api.Middleware;

/// <summary>
/// 未処理例外を構造化エラーレスポンス(Docs/api-design.md §3.1)に変換する。
/// 例外の詳細はログにのみ出力し、クライアントには汎用メッセージを返す。
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception,
            "Unhandled exception for {Method} {Path}",
            httpContext.Request.Method,
            httpContext.Request.Path);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var response = new
        {
            error = new
            {
                code = "INTERNAL_SERVER_ERROR",
                message = "サーバー内部でエラーが発生しました",
                // 例外の詳細は開発環境でのみ返す(本番は情報漏洩防止のためログのみ)
                details = _environment.IsDevelopment()
                    ? new[] { new { field = "exception", message = exception.Message } }
                    : null,
            }
        };

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
        return true;
    }
}
