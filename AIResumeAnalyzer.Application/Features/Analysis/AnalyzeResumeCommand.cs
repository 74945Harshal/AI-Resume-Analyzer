using AIResumeAnalyzer.Application.Common.DTOs;
using AIResumeAnalyzer.Application.Common.Exceptions;
using AIResumeAnalyzer.Application.Common.Interfaces;
using AIResumeAnalyzer.Domain.Entities;
using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Application.Features.Analysis;

public record AnalyzeResumeCommand(int ResumeId, int UserId) : IRequest<ResumeAnalysisDto>;

public class AnalyzeResumeCommandHandler : IRequestHandler<AnalyzeResumeCommand, ResumeAnalysisDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAIResumeAnalyzerService _aiService;
    private readonly IMapper _mapper;

    public AnalyzeResumeCommandHandler(
        IUnitOfWork unitOfWork,
        IAIResumeAnalyzerService aiService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _aiService = aiService;
        _mapper = mapper;
    }

    public async Task<ResumeAnalysisDto> Handle(AnalyzeResumeCommand request, CancellationToken cancellationToken)
    {
        var resume = await _unitOfWork.Resumes.GetByIdAsync(request.ResumeId, cancellationToken);
        if (resume == null)
            throw new NotFoundException(nameof(Resume), request.ResumeId);

        if (resume.UserId != request.UserId)
            throw new UnauthorizedAccessException("You are not authorized to analyze this resume.");

        // Run AI analysis
        var skills = await _aiService.ExtractSkillsAsync(resume.ExtractedText, cancellationToken);
        var summary = await _aiService.GenerateSummaryAsync(resume.ExtractedText, cancellationToken);

        var analysis = new ResumeAnalysis
        {
            ResumeId = resume.Id,
            Summary = summary,
            MatchScore = 0
        };

        await _unitOfWork.ResumeAnalyses.AddAsync(analysis, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Add extracted skills
        var skillEntities = new List<Skill>();
        foreach (var skillName in skills)
        {
            var skill = new Skill
            {
                Name = skillName,
                IsMissing = false,
                ResumeAnalysisId = analysis.Id
            };
            await _unitOfWork.Skills.AddAsync(skill, cancellationToken);
            skillEntities.Add(skill);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with full details
        var fullAnalysis = await _unitOfWork.ResumeAnalyses.GetAnalysisDetailsAsync(analysis.Id, cancellationToken);
        return _mapper.Map<ResumeAnalysisDto>(fullAnalysis);
    }
}
