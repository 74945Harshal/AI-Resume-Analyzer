using FluentValidation;
using System.IO;

namespace AIResumeAnalyzer.Application.Features.Resumes;

public class UploadResumeCommandValidator : AbstractValidator<UploadResumeCommand>
{
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

    public UploadResumeCommandValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required.")
            .Must(x => Path.GetExtension(x).ToLower() == ".pdf")
            .WithMessage("Only PDF resumes are allowed.");

        RuleFor(x => x.FileStream)
            .NotNull().WithMessage("File stream is required.")
            .Must(x => x.Length <= MaxFileSize)
            .WithMessage("File size must not exceed 10 MB.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
