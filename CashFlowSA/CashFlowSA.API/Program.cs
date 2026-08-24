using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using CashFlowSA.Application.Common.Interfaces;
using CashFlowSA.Application.Common.Auditing;
using CashFlowSA.Application.Common.Settings;
using CashFlowSA.Infrastructure.Data;
using CashFlowSA.Infrastructure.Services;
using FluentValidation;
using CashFlowSA.Application.Common.Behaviors;
using MediatR;
using Scalar.AspNetCore;
using CashFlowSA.API.Filters;
using CashFlowSA.API.Services;

var builder = WebApplication.CreateBuilder(args);

// ---- Configuration binding ----
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

builder.Services.Configure<SupabaseStorageSettings>(
    builder.Configuration.GetSection("SupabaseStorage"));

// Ollama
builder.Services.Configure<OllamaSettings>(
    builder.Configuration.GetSection("Ollama"));

builder.Services.Configure<RabbitMqSettings>(
    builder.Configuration.GetSection("RabbitMQ"));

builder.Services.Configure<NotificationDeliverySettings>(
    builder.Configuration.GetSection("NotificationDelivery"));

// ---- Database ----
builder.Services.AddDbContext<CashFlowDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IApplicationDbContext>(provider =>
    provider.GetRequiredService<CashFlowDbContext>());

// ---- AutoMapper ----
builder.Services.AddAutoMapper(cfg => { }, typeof(ITokenService).Assembly);

// ---- Application services ----
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.AddScoped<IFileStorage, SupabaseFileStorage>();

builder.Services.AddSingleton<
    CashFlowSA.Application.Common.Ocr.IInvoiceOcrQueue,
    CashFlowSA.API.Services.InvoiceOcrQueue>();

builder.Services.AddScoped<
    CashFlowSA.Application.Common.Ocr.IInvoiceOcrService,
    PdfInvoiceOcrService>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<CashFlowSA.Application.Common.Payments.ISandboxPaymentGateway, CashFlowSA.API.Services.SandboxPaymentGateway>();

// ---- Notifications ----
builder.Services.AddScoped<
    CashFlowSA.Application.Common.Notifications.INotificationDispatcher,
    CashFlowSA.API.Services.NotificationDispatcher>();

builder.Services.AddScoped<
    CashFlowSA.API.Services.INotificationRealtimeService,
    CashFlowSA.API.Services.NotificationRealtimeService>();

builder.Services.AddScoped<
    CashFlowSA.Application.Common.Notifications.IEmailNotificationSender,
    CashFlowSA.API.Services.EmailNotificationSender>();

builder.Services.AddHttpClient<
    CashFlowSA.Application.Common.Notifications.ISmsNotificationSender,
    CashFlowSA.API.Services.TwilioSmsNotificationSender>();

// ---- Ollama ----
builder.Services.AddHttpClient<IRiskExplanationService, OllamaRiskExplanationService>(
    (provider, client) =>
    {
        var settings = provider
            .GetRequiredService<
                Microsoft.Extensions.Options.IOptions<OllamaSettings>>()
            .Value;

        client.BaseAddress = new Uri(settings.BaseUrl);
        client.Timeout = TimeSpan.FromMinutes(5);
    });

builder.Services.AddHttpClient<IRiskScoringService, OllamaRiskScoringService>(
    (provider, client) =>
    {
        var settings = provider
            .GetRequiredService<
                Microsoft.Extensions.Options.IOptions<OllamaSettings>>()
            .Value;

        client.BaseAddress = new Uri(settings.BaseUrl);
        client.Timeout = TimeSpan.FromMinutes(5);
    });

// ---- Background services ----
builder.Services.AddHostedService<
    CashFlowSA.API.Services.AuctionCloseBackgroundService>();

builder.Services.AddHostedService<
    CashFlowSA.API.Services.InvoiceOcrBackgroundService>();

// ---- CORS ----
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ---- Authentication ----
var jwtSettings = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "Jwt configuration section is missing.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"].FirstOrDefault();
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrWhiteSpace(accessToken) &&
                path.StartsWithSegments("/hubs/notifications"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };

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

// ---- SignalR ----
builder.Services.AddSignalR();

// ---- Controllers & Authorization ----
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiErrorResultFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter());
});

builder.Services.AddAuthorization(options =>
{
    // Secure-by-default: every API endpoint requires an authenticated user
    // unless it explicitly opts out with [AllowAnonymous].
    options.FallbackPolicy =
        new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
});

// ---- OpenAPI / Scalar ----
builder.Services.AddOpenApi();

// ---- MediatR ----
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(ITokenService).Assembly));

// ---- FluentValidation ----
builder.Services.AddValidatorsFromAssembly(
    typeof(ITokenService).Assembly);

// ---- Pipeline behaviors ----
builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));

var app = builder.Build();

// ---- Seed development/demo accounts ----
// Demo users are intentionally limited to Development so they can never be
// created accidentally in a production deployment.
if (app.Environment.IsDevelopment())
{
    await CashFlowSA.Infrastructure.Data.AdminSeeder
        .SeedAsync(app.Services);

    await CashFlowSA.Infrastructure.Data.DemoUserSeeder
        .SeedAsync(app.Services);
}

// ---- OpenAPI & Scalar ----
// Enabled regardless of environment so /scalar is available while testing.
app.MapOpenApi();
app.MapScalarApiReference();

// ---- Middleware ----
app.UseMiddleware<
    CashFlowSA.API.Middleware.ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

// Enable CORS BEFORE Authentication & Authorization
app.UseCors("AllowReact");

app.UseAuthentication();
app.UseAuthorization();

// ---- Endpoints ----
app.MapControllers();
app.MapHub<CashFlowSA.API.Hubs.NotificationHub>("/hubs/notifications");

app.Run();

public partial class Program { }
