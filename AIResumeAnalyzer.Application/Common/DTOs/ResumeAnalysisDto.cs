using System;
using System.Collections.Generic;

namespace AIResumeAnalyzer.Application.Common.DTOs;

public class ResumeAnalysisDto
{
    public int Id { get; set; }
    public string Summary { get; set; } = string.Empty;
    public double MatchScore { get; set; }
    public int ResumeId { get; set; }
    public JobDescriptionDto? JobDescription { get; set; }
    public DateTime CreatedDate { get; set; }

    public List<SkillDto> Skills { get; set; } = new();
    public List<InterviewQuestionDto> InterviewQuestions { get; set; } = new();
}
