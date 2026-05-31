using AIResumeAnalyzer.Domain.Common;
using System.Collections.Generic;

namespace AIResumeAnalyzer.Domain.Entities;

public class ResumeAnalysis : BaseEntity
{
    public string Summary { get; set; } = string.Empty;
    public double MatchScore { get; set; }

    public int ResumeId { get; set; }
    public Resume Resume { get; set; } = null!;

    public int? JobDescriptionId { get; set; }
    public JobDescription? JobDescription { get; set; }

    public ICollection<Skill> Skills { get; set; } = new List<Skill>();
    public ICollection<InterviewQuestion> InterviewQuestions { get; set; } = new List<InterviewQuestion>();
}
