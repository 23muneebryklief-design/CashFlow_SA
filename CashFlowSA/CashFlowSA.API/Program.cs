using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Settings;
using CashFlowSA.Infrastructure.Data;
using CashFlowSA.Infrastructure.Services;
using FluentValidation;
using CashFlowSA.Application.Common.Behaviors;
using MediatR;
using Scalar.AspNetCore;
using CashFlowSA.API.Filters;

var builder = WebApplication.CreateBuilder(args);

// ---- Configuration binding ----
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<SupabaseStorageSettings>(builder.Configuration.GetSection("SupabaseStorage"));
//ollama
builder.Services.Configure<OllamaSettings>(
    builder.Configuration.GetSection("Ollama"));


// ---- Database ----
builder.Services.AddDbContext<CashFlowDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<CashFlowDbContext>());

// ---- AutoMapper ----
builder.Services.AddAutoMapper(cfg => { }, typeof(ITokenService).Assembly);

// ---- Application services ----
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IFileStorage, SupabaseFileStorage>();

builder.Services.AddHttpClient<IRiskExplanationService, OllamaRiskExplanationService>(
    (provider, client) =>
    {
        var settings = provider
            .GetRequiredService<Microsoft.Extensions.Options.IOptions<OllamaSettings>>()
            .Value;

        client.BaseAddress = new Uri(settings.BaseUrl);
        client.Timeout = TimeSpan.FromMinutes(5);
    });
builder.Services.AddHttpClient<IRiskScoringService, OllamaRiskScoringService>(
    (provider, client) =>
    {
        var settings = provider
            .GetRequiredService<Microsoft.Extensions.Options.IOptions<OllamaSettings>>()
            .Value;

        client.BaseAddress = new Uri(settings.BaseUrl);
        client.Timeout = TimeSpan.FromMinutes(5);
    });
// ---- Background services ----
builder.Services.AddHostedService<CashFlowSA.API.Services.AuctionCloseBackgroundService>();

// ---- CORS ----
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// ---- Authentication ----
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt configuration section is missing.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true, 
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.Key)),
        ClockSkew = TimeSpan.Zero
    };
});

// ---- Controllers & Authorization ----
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiErrorResultFilter>();
});
builder.Services.AddAuthorization();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
// ---- OpenAPI ----
builder.Services.AddOpenApi();

// ---- MediatR ----
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(ITokenService).Assembly));

// ---- FluentValidation ----
builder.Services.AddValidatorsFromAssembly(typeof(ITokenService).Assembly);

// ---- Pipeline behaviors ----
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

var app = builder.Build();

// ---- Seed development/demo accounts ----
// Demo users are intentionally limited to Development so they can never be
// created accidentally in a production deployment. AdminSeeder remains the
// production-safe bootstrap for the first SuperAdmin.
if (app.Environment.IsDevelopment())
{
    await CashFlowSA.Infrastructure.Data.AdminSeeder.SeedAsync(app.Services);
    await CashFlowSA.Infrastructure.Data.DemoUserSeeder.SeedAsync(app.Services);
}

// ---- Development tools ----
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// ---- Middleware ----
app.UseMiddleware<CashFlowSA.API.Middleware.ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

// Enable CORS BEFORE Authentication & Authorization
app.UseCors("AllowReact");

app.UseAuthentication();
app.UseAuthorization();

// ---- Endpoints ----
app.MapControllers();

app.Run();

public partial class Program { }