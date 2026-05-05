using System.Text;
using Bridge.Api.Services.Auth;
using Bridge.Api.Services.Users;
using Bridge.Api.Services.Sales;
using Bridge.Api.Services.Engineers;
using Bridge.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Bridge.Api.Services.Projects;
using Bridge.Api.Services.EngineerPreferences;
using Bridge.Api.Services.EngineerSkills;
using Bridge.Api.Services.Assignments;
using Bridge.Api.Services.Skills;
using Bridge.Api.Services.ExpiringContracts;
using Bridge.Api.Services.Matching;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// CORS
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();
var allowedOriginsFromEnv = (builder.Configuration["Cors:AllowedOriginsCsv"] ?? string.Empty)
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

allowedOrigins = allowedOrigins
    .Concat(allowedOriginsFromEnv)
    .Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

if (allowedOrigins.Length == 0 && builder.Environment.IsDevelopment())
{
    allowedOrigins = ["http://localhost:5173"];
}

if (allowedOrigins.Length == 0)
    throw new InvalidOperationException("Cors:AllowedOrigins configuration is missing.");

var allowVercelPreviewOrigins = builder.Configuration.GetValue("Cors:AllowVercelPreviewOrigins", true);

builder.Services.AddCors(options =>
{
    options.AddPolicy("BridgeCors", policy =>
    {
        policy.SetIsOriginAllowed(origin => IsAllowedOrigin(origin, allowedOrigins, allowVercelPreviewOrigins))
            .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
            .WithHeaders("Authorization", "Content-Type");
    });
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// Swagger(JWT 入力欄付き)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.FullName?.Replace('+', '.') ?? type.Name);

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT を入力(Bearer プレフィックスは不要)"
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document, null),
            new List<string>()
        }
    });
});

// DbContext
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' not found.");

builder.Services.AddDbContext<BridgeDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseSnakeCaseNamingConvention());

// JWT Options
var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()
    ?? throw new InvalidOperationException("Jwt configuration is missing.");
ValidateJwtOptions(jwtOptions);
builder.Services.AddSingleton(jwtOptions);

// Service registrations
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddScoped<IEngineerService, EngineerService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<IEngineerPreferenceService, EngineerPreferenceService>();
builder.Services.AddScoped<IEngineerSkillService, EngineerSkillService>();
builder.Services.AddScoped<ISkillService, SkillService>();
builder.Services.AddScoped<IExpiringContractService, ExpiringContractService>();
builder.Services.AddScoped<IMatchingService, MatchingService>();
// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SalesOnly", p => p.RequireRole("Sales", "Admin"));
    options.AddPolicy("EngineerOnly", p => p.RequireRole("Engineer", "Admin"));
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

var app = builder.Build();

// Pipeline
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("BridgeCors");
app.UseAuthentication();   // ← JWT 検証(必ず Authorization の前)
app.UseAuthorization();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BridgeDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await DemoUserSeeder.SeedAsync(db, hasher);
}

app.Run();

static void ValidateJwtOptions(JwtOptions options)
{
    if (string.IsNullOrWhiteSpace(options.Secret))
        throw new InvalidOperationException("Jwt:Secret configuration is missing.");

    if (options.Secret == "REPLACE_IN_PRODUCTION")
        throw new InvalidOperationException("Jwt:Secret must be replaced with a secure value.");

    if (Encoding.UTF8.GetByteCount(options.Secret) < 32)
        throw new InvalidOperationException("Jwt:Secret must be at least 32 bytes for HS256.");

    if (string.IsNullOrWhiteSpace(options.Issuer))
        throw new InvalidOperationException("Jwt:Issuer configuration is missing.");

    if (string.IsNullOrWhiteSpace(options.Audience))
        throw new InvalidOperationException("Jwt:Audience configuration is missing.");

    if (options.ExpiryHours <= 0)
        throw new InvalidOperationException("Jwt:ExpiryHours must be greater than 0.");
}

static bool IsAllowedOrigin(string origin, string[] allowedOrigins, bool allowVercelPreviewOrigins)
{
    if (allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
        return true;

    if (!allowVercelPreviewOrigins)
        return false;

    return Uri.TryCreate(origin, UriKind.Absolute, out var uri)
        && uri.Scheme == Uri.UriSchemeHttps
        && uri.Host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase);
}
