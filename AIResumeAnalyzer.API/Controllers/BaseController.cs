using AIResumeAnalyzer.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AIResumeAnalyzer.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    protected int CurrentUserId
    {
        get
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub");
            return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
        }
    }

    protected string CurrentUserEmail
        => User.FindFirst(ClaimTypes.Email)?.Value
        ?? User.FindFirst("email")?.Value
        ?? string.Empty;

    protected IActionResult OkResponse<T>(T data, string message = "Operation Successful")
        => Ok(ApiResponse<T>.SuccessResult(data, message));

    protected IActionResult CreatedResponse<T>(T data, string message = "Created Successfully")
        => StatusCode(201, ApiResponse<T>.SuccessResult(data, message));
}
