using FluentValidation;

namespace AIResumeAnalyzer.Application.Features.Analysis;

public class AnalyzeResumeCommandValidator : AbstractValidator<AnalyzeResumeCommand>
{
    public AnalyzeResumeCommandValidator()
    {
        RuleFor(x => x.ResumeId)
            .GreaterThan(0).WithMessage("Resume ID must be greater than 0.");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID must be greater than 0.");
    }
}
