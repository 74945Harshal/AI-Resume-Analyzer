using AIResumeAnalyzer.Application.Common.DTOs;
using AIResumeAnalyzer.Application.Common.Interfaces;
using AIResumeAnalyzer.Application.Common.Models;
using AutoMapper;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Application.Features.Analysis;

public record GetAnalysisHistoryQuery(
    int UserId,
    int PageNumber = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    string? SortBy = null,
    bool SortDescending = true) : IRequest<PaginatedList<ResumeAnalysisDto>>;

public class GetAnalysisHistoryQueryHandler
    : IRequestHandler<GetAnalysisHistoryQuery, PaginatedList<ResumeAnalysisDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAnalysisHistoryQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PaginatedList<ResumeAnalysisDto>> Handle(
        GetAnalysisHistoryQuery request, CancellationToken cancellationToken)
    {
        var analyses = await _unitOfWork.ResumeAnalyses.GetUserAnalysisHistoryAsync(request.UserId, cancellationToken);

        var query = analyses.AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(a =>
                a.Summary.ToLower().Contains(term) ||
                (a.JobDescription != null && a.JobDescription.Title.ToLower().Contains(term)) ||
                a.Resume.FileName.ToLower().Contains(term));
        }

        // Sort
        query = request.SortBy?.ToLower() switch
        {
            "matchscore" => request.SortDescending
                ? query.OrderByDescending(a => a.MatchScore)
                : query.OrderBy(a => a.MatchScore),
            "filename" => request.SortDescending
                ? query.OrderByDescending(a => a.Resume.FileName)
                : query.OrderBy(a => a.Resume.FileName),
            _ => request.SortDescending
                ? query.OrderByDescending(a => a.CreatedDate)
                : query.OrderBy(a => a.CreatedDate)
        };

        var totalCount = query.Count();
        var items = query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var dtos = _mapper.Map<List<ResumeAnalysisDto>>(items);
        return new PaginatedList<ResumeAnalysisDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}
