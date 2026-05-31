using AIResumeAnalyzer.Domain.Common;

namespace AIResumeAnalyzer.Domain.Entities;

public class InterviewQuestion : BaseEntity
{
    public string Question { get; set; } = string.Empty;
    public string AnswerHint { get; set; } = string.Empty;

    public int ResumeAnalysisId { get; set; }
    public ResumeAnalysis ResumeAnalysis { get; set; } = null!;
}
