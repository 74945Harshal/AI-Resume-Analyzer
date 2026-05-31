using AIResumeAnalyzer.Application.Features.Analysis;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.API.Controllers;

/// <summary>
/// AI Resume Analysis endpoints.
/// </summary>
[Tags("Analysis")]
[Authorize]
public class AnalysisController : BaseController
{
    private readonly IMediator _mediator;

    public AnalysisController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Analyze a resume using AI - extracts skills and generates a professional summary.
    /// </summary>
    [HttpPost("analyze/{resumeId:int}")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AnalyzeResume(int resumeId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new AnalyzeResumeCommand(resumeId, CurrentUserId), cancellationToken);
        return OkResponse(result, "Resume analyzed successfully.");
    }

    /// <summary>
    /// Match a resume against a job description - generates match score and missing skills.
    /// </summary>
    [HttpPost("match")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> MatchResumeWithJob(
        [FromBody] MatchResumeWithJobRequest request,
        CancellationToken cancellationToken)
    {
        var command = new MatchResumeWithJobCommand(
            request.ResumeId,
            CurrentUserId,
            request.JobTitle,
            request.JobDescriptionText);

        var result = await _mediator.Send(command, cancellationToken);
        return OkResponse(result, "Resume matched with job description successfully.");
    }

    /// <summary>
    /// Generate 10 technical interview questions based on an analysis.
    /// </summary>
    [HttpPost("{analysisId:int}/interview-questions")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GenerateInterviewQuestions(
        int analysisId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GenerateInterviewQuestionsCommand(analysisId, CurrentUserId), cancellationToken);
        return OkResponse(result, "Interview questions generated successfully.");
    }

    /// <summary>
    /// Get a specific analysis by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetAnalysisById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAnalysisByIdQuery(id, CurrentUserId), cancellationToken);
        return OkResponse(result);
    }

    /// <summary>
    /// Get analysis history with pagination, search, and sorting.
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(object), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetAnalysisHistory(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetAnalysisHistoryQuery(CurrentUserId, pageNumber, pageSize, searchTerm, sortBy, sortDescending),
            cancellationToken);
        return OkResponse(result);
    }
}

/// <summary>
/// Request model for matching a resume with a job description.
/// </summary>
public class MatchResumeWithJobRequest
{
    public int ResumeId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string JobDescriptionText { get; set; } = string.Empty;
}
