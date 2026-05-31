using AIResumeAnalyzer.Application.Common.DTOs;
using AIResumeAnalyzer.Application.Common.Exceptions;
using AIResumeAnalyzer.Application.Common.Interfaces;
using AIResumeAnalyzer.Domain.Entities;
using AutoMapper;
using MediatR;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Application.Features.Resumes;

public record UploadResumeCommand(
    string FileName,
    Stream FileStream,
    int UserId) : IRequest<ResumeDto>;

public class UploadResumeCommandHandler : IRequestHandler<UploadResumeCommand, ResumeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPdfParserService _pdfParserService;
    private readonly IMapper _mapper;

    public UploadResumeCommandHandler(
        IUnitOfWork unitOfWork,
        IPdfParserService pdfParserService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _pdfParserService = pdfParserService;
        _mapper = mapper;
    }

    public async Task<ResumeDto> Handle(UploadResumeCommand request, CancellationToken cancellationToken)
    {
        var userExists = await _unitOfWork.Users.ExistsAsync(request.UserId, cancellationToken);
        if (!userExists)
        {
            throw new NotFoundException(nameof(User), request.UserId);
        }

        string extractedText;
        try
        {
            extractedText = await _pdfParserService.ExtractTextAsync(request.FileStream, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new BadRequestException($"Failed to extract text from PDF: {ex.Message}");
        }

        if (string.IsNullOrWhiteSpace(extractedText))
        {
            throw new BadRequestException("The uploaded PDF resume contains no readable text.");
        }

        var folderPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "resumes");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(request.FileName)}";
        var fullPath = Path.Combine(folderPath, uniqueFileName);

        request.FileStream.Position = 0;
        using (var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
        {
            await request.FileStream.CopyToAsync(fileStream, cancellationToken);
        }

        var resume = new Resume
        {
            FileName = request.FileName,
            FilePath = Path.Combine("resumes", uniqueFileName),
            ExtractedText = extractedText,
            UserId = request.UserId
        };

        await _unitOfWork.Resumes.AddAsync(resume, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ResumeDto>(resume);
    }
}
