using System.Text;
using Bridge.Api.Services.Auth;
using Bridge.Api.Services.Users;
using Bridge.Api.Services.Sales;
using Bridge.Api.Services.Engineers;
using Bridge.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
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
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
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