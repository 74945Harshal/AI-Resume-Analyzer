using FluentValidation;

namespace AIResumeAnalyzer.Application.Features.Analysis;

public class GenerateInterviewQuestionsCommandValidator : AbstractValidator<GenerateInterviewQuestionsCommand>
{
    public GenerateInterviewQuestionsCommandValidator()
    {
        RuleFor(x => x.AnalysisId)
            .GreaterThan(0).WithMessage("Analysis ID must be greater than 0.");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("User ID must be greater than 0.");
    }
}
