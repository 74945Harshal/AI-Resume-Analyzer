using AIResumeAnalyzer.Application.Common.DTOs;
using AIResumeAnalyzer.Application.Common.Exceptions;
using AIResumeAnalyzer.Application.Common.Interfaces;
using AIResumeAnalyzer.Domain.Entities;
using AutoMapper;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Application.Features.Resumes;

public record GetResumeByIdQuery(int Id, int UserId) : IRequest<ResumeDto>;

public class GetResumeByIdQueryHandler : IRequestHandler<GetResumeByIdQuery, ResumeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetResumeByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ResumeDto> Handle(GetResumeByIdQuery request, CancellationToken cancellationToken)
    {
        var resume = await _unitOfWork.Resumes.GetByIdAsync(request.Id, cancellationToken);
        if (resume == null)
        {
            throw new NotFoundException(nameof(Resume), request.Id);
        }

        if (resume.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("You are not authorized to access this resume.");
        }

        return _mapper.Map<ResumeDto>(resume);
    }
}
