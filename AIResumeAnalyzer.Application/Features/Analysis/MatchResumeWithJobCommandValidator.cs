using FluentValidation;

namespace AIResumeAnalyzer.Application.Features.Analysis;

public class MatchResumeWithJobCommandValidator : AbstractValidator<MatchResumeWithJobCommand>
{
    public MatchResumeWithJobCommandValidator()
    {
        RuleFor(x => x.ResumeId)
            .GreaterThan(0).WithMessage("Resume ID must be greater than 0.");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID must be greater than 0.");

        RuleFor(x => x.JobTitle)
            .NotEmpty().WithMessage("Job title is required.")
            .MaximumLength(200).WithMessage("Job title must not exceed 200 characters.");

        RuleFor(x => x.JobDescriptionText)
            .NotEmpty().WithMessage("Job description is required.")
            .MinimumLength(50).WithMessage("Job description must be at least 50 characters.");
    }
}
