using AIResumeAnalyzer.Application.Common.DTOs;
using AIResumeAnalyzer.Application.Common.Interfaces;
using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Application.Features.Resumes;

public record GetUserResumesQuery(int UserId) : IRequest<IEnumerable<ResumeDto>>;

public class GetUserResumesQueryHandler : IRequestHandler<GetUserResumesQuery, IEnumerable<ResumeDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetUserResumesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ResumeDto>> Handle(GetUserResumesQuery request, CancellationToken cancellationToken)
    {
        var resumes = await _unitOfWork.Resumes.GetUserResumesAsync(request.UserId, cancellationToken);
        return _mapper.Map<IEnumerable<ResumeDto>>(resumes);
    }
}
