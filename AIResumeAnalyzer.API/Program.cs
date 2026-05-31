using AIResumeAnalyzer.API.Configuration;
using AIResumeAnalyzer.API.Middleware;
using AIResumeAnalyzer.Application;
using AIResumeAnalyzer.Infrastructure;
using AIResumeAnalyzer.Persistence;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

// ─── Bootstrap Logger ────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting AI Resume Analyzer API...");

    var builder = WebApplication.CreateBuilder(args);

    // ─── Serilog ─────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration)
                     .ReadFrom.Services(services)
                     .Enrich.FromLogContext());

    // ─── Services ────────────────────────────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Layer registrations
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddPersistence(builder.Configuration);

    // Swagger
    builder.Services.AddSwaggerConfiguration();

    // File upload size limit (10 MB)
    builder.Services.Configure<FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit = 10 * 1024 * 1024;
    });

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader());
    });

    // ─── Build App ───────────────────────────────────────────────────────────
    var app = builder.Build();

    // ─── Middleware Pipeline ─────────────────────────────────────────────────
    app.UseMiddleware<GlobalExceptionMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwaggerConfiguration();
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");

    app.UseAuthentication();
    app.UseAuthorization();

    // Hangfire Dashboard (Admin only in production)
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        DashboardTitle = "AI Resume Analyzer - Background Jobs",
        Authorization = new[] { new HangfireAuthorizationFilter() }
    });

    app.UseStaticFiles();

    app.MapControllers();

    // ─── Ensure wwwroot/resumes directory exists ─────────────────────────────
    var resumesPath = System.IO.Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "resumes");
    if (!System.IO.Directory.Exists(resumesPath))
        System.IO.Directory.CreateDirectory(resumesPath);

    Log.Information("AI Resume Analyzer API started successfully.");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
