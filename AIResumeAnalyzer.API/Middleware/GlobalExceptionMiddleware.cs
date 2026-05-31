using AIResumeAnalyzer.Application.Common.Exceptions;
using AIResumeAnalyzer.Application.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                "One or more validation errors occurred.",
                (object?)validationEx.Errors),

            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                notFoundEx.Message,
                (object?)null),

            BadRequestException badRequestEx => (
                HttpStatusCode.BadRequest,
                badRequestEx.Message,
                (object?)null),

            UnauthorizedAccessException unauthorizedEx => (
                HttpStatusCode.Unauthorized,
                unauthorizedEx.Message,
                (object?)null),

            InvalidOperationException invalidOpEx => (
                HttpStatusCode.BadRequest,
                invalidOpEx.Message,
                (object?)null),

            _ => (
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred. Please try again later.",
                (object?)null)
        };

        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            success = false,
            message,
            errors,
            statusCode = (int)statusCode
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}
