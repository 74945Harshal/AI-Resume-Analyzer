using AIResumeAnalyzer.Domain.Common;

namespace AIResumeAnalyzer.Domain.Entities;

public class Skill : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsMissing { get; set; }

    public int ResumeAnalysisId { get; set; }
    public ResumeAnalysis ResumeAnalysis { get; set; } = null!;
}
