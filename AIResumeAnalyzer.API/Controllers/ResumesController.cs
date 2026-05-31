using AIResumeAnalyzer.Application.Features.Resumes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.API.Controllers;

/// <summary>
/// Resume upload and management endpoints.
/// </summary>
[Tags("Resumes")]
[Authorize]
public class ResumesController : BaseController
{
    private readonly IMediator _mediator;

    public ResumesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Upload a PDF resume (max 10 MB).
    /// </summary>
    [HttpPost("upload")]
    [ProducesResponseType(typeof(object), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> UploadResume(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { success = false, message = "No file was uploaded." });

        using var stream = file.OpenReadStream();
        var command = new UploadResumeCommand(file.FileName, stream, CurrentUserId);
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedResponse(result, "Resume uploaded successfully.");
    }

    /// <summary>
    /// Get all resumes for the current user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetMyResumes(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserResumesQuery(CurrentUserId), cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Get a specific resume by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetResumeById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetResumeByIdQuery(id, CurrentUserId), cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Delete a resume by ID.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteResume(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteResumeCommand(id, CurrentUserId), cancellationToken);
        return OkResponse(true, "Resume deleted successfully.");
    }
}
