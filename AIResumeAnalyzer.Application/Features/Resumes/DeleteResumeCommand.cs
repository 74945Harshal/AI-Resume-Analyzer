using AIResumeAnalyzer.Application.Common.Exceptions;
using AIResumeAnalyzer.Application.Common.Interfaces;
using AIResumeAnalyzer.Domain.Entities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Application.Features.Resumes;

public record DeleteResumeCommand(int Id, int UserId) : IRequest<bool>;

public class DeleteResumeCommandHandler : IRequestHandler<DeleteResumeCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteResumeCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteResumeCommand request, CancellationToken cancellationToken)
    {
        var resume = await _unitOfWork.Resumes.GetByIdAsync(request.Id, cancellationToken);
        if (resume == null)
        {
            throw new NotFoundException(nameof(Resume), request.Id);
        }

        if (resume.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this resume.");
        }

        _unitOfWork.Resumes.Delete(resume);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
