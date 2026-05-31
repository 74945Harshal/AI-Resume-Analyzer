using AIResumeAnalyzer.Domain.Common;
using System.Collections.Generic;

namespace AIResumeAnalyzer.Domain.Entities;

public class JobDescription : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string DescriptionText { get; set; } = string.Empty;

    public ICollection<ResumeAnalysis> Analyses { get; set; } = new List<ResumeAnalysis>();
}
