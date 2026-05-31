using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;

namespace AIResumeAnalyzer.API.Configuration;

/// <summary>
/// Restricts Hangfire dashboard access to Admin role in production.
/// In development, allows all access.
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Allow in development
        var env = httpContext.RequestServices
            .GetService(typeof(Microsoft.Extensions.Hosting.IHostEnvironment))
            as Microsoft.Extensions.Hosting.IHostEnvironment;

        if (env?.IsDevelopment() == true)
            return true;

        // In production, require Admin role
        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole("Admin");
    }
}
