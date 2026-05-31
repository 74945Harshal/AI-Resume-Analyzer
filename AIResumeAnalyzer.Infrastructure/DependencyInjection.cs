using AIResumeAnalyzer.Application.Common.Interfaces;
using AIResumeAnalyzer.Infrastructure.Services;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System;
using System.Text;

namespace AIResumeAnalyzer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // JWT Authentication
        services.AddJwtAuthentication(configuration);

        // Services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IPdfParserService, PdfParserService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAnalysisOrchestrator, AnalysisOrchestrator>();
        services.AddScoped<IBackgroundJobService, HangfireBackgroundJobService>();

        // Ollama AI Service
        services.AddHttpClient<IAIResumeAnalyzerService, OllamaAIService>(client =>
        {
            var baseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        // Redis Cache
        services.AddRedisCache(configuration);

        // Hangfire
        services.AddHangfireJobs(configuration);

        return services;
    }

    private static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey is not configured.");

        services.AddAuthentication(options =>
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
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
        });

        return services;
    }

    private static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? configuration["Redis:ConnectionString"]
            ?? "localhost:6379";

        try
        {
            var multiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
            services.AddSingleton<IConnectionMultiplexer>(multiplexer);
            services.AddScoped<ICacheService, RedisCacheService>();
        }
        catch
        {
            // If Redis is unavailable, register a no-op cache service
            services.AddScoped<ICacheService, NoOpCacheService>();
        }

        return services;
    }

    private static IServiceCollection AddHangfireJobs(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                  .UseSimpleAssemblyNameTypeSerializer()
                  .UseRecommendedSerializerSettings()
                  .UseSqlServerStorage(
                      configuration.GetConnectionString("DefaultConnection"),
                      new SqlServerStorageOptions
                      {
                          CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                          SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                          QueuePollInterval = TimeSpan.Zero,
                          UseRecommendedIsolationLevel = true,
                          DisableGlobalLocks = true
                      });
        });

        services.AddHangfireServer();

        return services;
    }
}
