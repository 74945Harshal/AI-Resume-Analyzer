using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;

namespace AIResumeAnalyzer.API.Configuration;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "AI Resume Analyzer API",
                Version = "v1",
                Description = """
                    A production-ready AI-powered Resume Analyzer built with ASP.NET Core 8.
                    
                    **Features:**
                    - Upload and parse PDF resumes
                    - AI-powered skill extraction using Ollama (local LLM)
                    - Professional resume summary generation
                    - Resume vs Job Description matching with score
                    - Missing skills identification
                    - Technical interview question generation
                    - Analysis history with pagination and search
                    
                    **Authentication:** Use the /api/auth/login endpoint to get a JWT token, then click 'Authorize' and enter: Bearer {your-token}
                    """,
                Contact = new OpenApiContact
                {
                    Name = "AI Resume Analyzer",
                    Email = "support@airesumeanalyzer.com"
                }
            });

            // JWT Bearer Authentication
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token. Example: Bearer eyJhbGciOiJIUzI1NiIs..."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new List<string>()
                }
            });

            // Include XML comments if available
            options.EnableAnnotations();
        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Resume Analyzer API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "AI Resume Analyzer API";
            options.DisplayRequestDuration();
            options.EnableDeepLinking();
        });

        return app;
    }
}
