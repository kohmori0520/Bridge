using System.Text.Json;
using Bridge.Api.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bridge.Tests.Middleware;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_Returns500WithStructuredError()
    {
        var (context, body) = CreateHttpContext();
        var handler = CreateHandler("Production");

        var handled = await handler.TryHandleAsync(context, new InvalidOperationException("boom"), CancellationToken.None);

        handled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

        var error = ReadErrorObject(body);
        error.GetProperty("code").GetString().Should().Be("INTERNAL_SERVER_ERROR");
        error.GetProperty("message").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task TryHandleAsync_HidesExceptionDetails_OutsideDevelopment()
    {
        var (context, body) = CreateHttpContext();
        var handler = CreateHandler("Production");

        await handler.TryHandleAsync(context, new InvalidOperationException("secret detail"), CancellationToken.None);

        var error = ReadErrorObject(body);
        error.GetProperty("details").ValueKind.Should().Be(JsonValueKind.Null);
        body.Position = 0;
        new StreamReader(body).ReadToEnd().Should().NotContain("secret detail");
    }

    [Fact]
    public async Task TryHandleAsync_IncludesExceptionDetails_InDevelopment()
    {
        var (context, body) = CreateHttpContext();
        var handler = CreateHandler("Development");

        await handler.TryHandleAsync(context, new InvalidOperationException("boom"), CancellationToken.None);

        var error = ReadErrorObject(body);
        error.GetProperty("details")[0].GetProperty("message").GetString().Should().Be("boom");
    }

    private static GlobalExceptionHandler CreateHandler(string environmentName)
        => new(NullLogger<GlobalExceptionHandler>.Instance, new StubHostEnvironment(environmentName));

    private static (DefaultHttpContext Context, MemoryStream Body) CreateHttpContext()
    {
        var body = new MemoryStream();
        var context = new DefaultHttpContext();
        context.Response.Body = body;
        return (context, body);
    }

    private static JsonElement ReadErrorObject(MemoryStream body)
    {
        body.Position = 0;
        using var document = JsonDocument.Parse(body.ToArray());
        return document.RootElement.GetProperty("error").Clone();
    }

    private sealed class StubHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "Bridge.Tests";
        public string ContentRootPath { get; set; } = ".";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
