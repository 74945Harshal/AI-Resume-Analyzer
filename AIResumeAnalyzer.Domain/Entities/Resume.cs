using AIResumeAnalyzer.Domain.Common;
using System.Collections.Generic;

namespace AIResumeAnalyzer.Domain.Entities;

public class Resume : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ExtractedText { get; set; } = string.Empty;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<ResumeAnalysis> Analyses { get; set; } = new List<ResumeAnalysis>();
}
