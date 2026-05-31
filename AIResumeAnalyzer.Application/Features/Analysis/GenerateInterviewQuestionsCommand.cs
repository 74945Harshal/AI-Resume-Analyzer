using AIResumeAnalyzer.Application.Common.DTOs;
using AIResumeAnalyzer.Application.Common.Exceptions;
using AIResumeAnalyzer.Application.Common.Interfaces;
using AIResumeAnalyzer.Domain.Entities;
using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Application.Features.Analysis;

public record GenerateInterviewQuestionsCommand(int AnalysisId, int UserId) : IRequest<List<InterviewQuestionDto>>;

public class GenerateInterviewQuestionsCommandHandler
    : IRequestHandler<GenerateInterviewQuestionsCommand, List<InterviewQuestionDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAIResumeAnalyzerService _aiService;
    private readonly IMapper _mapper;

    public GenerateInterviewQuestionsCommandHandler(
        IUnitOfWork unitOfWork,
        IAIResumeAnalyzerService aiService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _aiService = aiService;
        _mapper = mapper;
    }

    public async Task<List<InterviewQuestionDto>> Handle(
        GenerateInterviewQuestionsCommand request, CancellationToken cancellationToken)
    {
        var analysis = await _unitOfWork.ResumeAnalyses.GetAnalysisDetailsAsync(request.AnalysisId, cancellationToken);
        if (analysis == null)
            throw new NotFoundException(nameof(ResumeAnalysis), request.AnalysisId);

        if (analysis.Resume.UserId != request.UserId)
            throw new UnauthorizedAccessException("You are not authorized to access this analysis.");

        var skillNames = analysis.Skills
            .Where(s => !s.IsMissing)
            .Select(s => s.Name)
            .ToList();

        if (!skillNames.Any())
            throw new BadRequestException("No skills found in this analysis to generate interview questions.");

        var questions = await _aiService.GenerateInterviewQuestionsAsync(skillNames, cancellationToken);

        var questionEntities = new List<InterviewQuestion>();
        foreach (var (question, answerHint) in questions)
        {
            var entity = new InterviewQuestion
            {
                Question = question,
                AnswerHint = answerHint,
                ResumeAnalysisId = analysis.Id
            };
            await _unitOfWork.InterviewQuestions.AddAsync(entity, cancellationToken);
            questionEntities.Add(entity);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<List<InterviewQuestionDto>>(questionEntities);
    }
}
