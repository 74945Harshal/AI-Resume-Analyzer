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

public record MatchResumeWithJobCommand(
    int ResumeId,
    int UserId,
    string JobTitle,
    string JobDescriptionText) : IRequest<ResumeAnalysisDto>;

public class MatchResumeWithJobCommandHandler : IRequestHandler<MatchResumeWithJobCommand, ResumeAnalysisDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAIResumeAnalyzerService _aiService;
    private readonly IMapper _mapper;

    public MatchResumeWithJobCommandHandler(
        IUnitOfWork unitOfWork,
        IAIResumeAnalyzerService aiService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _aiService = aiService;
        _mapper = mapper;
    }

    public async Task<ResumeAnalysisDto> Handle(MatchResumeWithJobCommand request, CancellationToken cancellationToken)
    {
        var resume = await _unitOfWork.Resumes.GetByIdAsync(request.ResumeId, cancellationToken);
        if (resume == null)
            throw new NotFoundException(nameof(Resume), request.ResumeId);

        if (resume.UserId != request.UserId)
            throw new UnauthorizedAccessException("You are not authorized to analyze this resume.");

        // Save job description
        var jobDescription = new JobDescription
        {
            Title = request.JobTitle,
            DescriptionText = request.JobDescriptionText
        };
        await _unitOfWork.JobDescriptions.AddAsync(jobDescription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Run AI analysis
        var skills = await _aiService.ExtractSkillsAsync(resume.ExtractedText, cancellationToken);
        var summary = await _aiService.GenerateSummaryAsync(resume.ExtractedText, cancellationToken);
        var (matchScore, missingSkills) = await _aiService.CompareResumeWithJobAsync(
            resume.ExtractedText, request.JobDescriptionText, cancellationToken);

        var analysis = new ResumeAnalysis
        {
            ResumeId = resume.Id,
            JobDescriptionId = jobDescription.Id,
            Summary = summary,
            MatchScore = matchScore
        };

        await _unitOfWork.ResumeAnalyses.AddAsync(analysis, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Add extracted skills (present)
        foreach (var skillName in skills)
        {
            await _unitOfWork.Skills.AddAsync(new Skill
            {
                Name = skillName,
                IsMissing = false,
                ResumeAnalysisId = analysis.Id
            }, cancellationToken);
        }

        // Add missing skills
        foreach (var missingSkill in missingSkills)
        {
            await _unitOfWork.Skills.AddAsync(new Skill
            {
                Name = missingSkill,
                IsMissing = true,
                ResumeAnalysisId = analysis.Id
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var fullAnalysis = await _unitOfWork.ResumeAnalyses.GetAnalysisDetailsAsync(analysis.Id, cancellationToken);
        return _mapper.Map<ResumeAnalysisDto>(fullAnalysis);
    }
}
