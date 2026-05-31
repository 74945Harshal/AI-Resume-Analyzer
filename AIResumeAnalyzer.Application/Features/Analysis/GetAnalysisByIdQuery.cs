using AIResumeAnalyzer.Application.Common.DTOs;
using AIResumeAnalyzer.Application.Common.Exceptions;
using AIResumeAnalyzer.Application.Common.Interfaces;
using AIResumeAnalyzer.Domain.Entities;
using AutoMapper;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Application.Features.Analysis;

public record GetAnalysisByIdQuery(int Id, int UserId) : IRequest<ResumeAnalysisDto>;

public class GetAnalysisByIdQueryHandler : IRequestHandler<GetAnalysisByIdQuery, ResumeAnalysisDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAnalysisByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ResumeAnalysisDto> Handle(GetAnalysisByIdQuery request, CancellationToken cancellationToken)
    {
        var analysis = await _unitOfWork.ResumeAnalyses.GetAnalysisDetailsAsync(request.Id, cancellationToken);
        if (analysis == null)
            throw new NotFoundException(nameof(ResumeAnalysis), request.Id);

        if (analysis.Resume.UserId != request.UserId)
            throw new UnauthorizedAccessException("You are not authorized to access this analysis.");

        return _mapper.Map<ResumeAnalysisDto>(analysis);
    }
}
