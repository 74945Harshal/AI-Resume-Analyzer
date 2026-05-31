namespace AIResumeAnalyzer.Application.Common.DTOs;

public class InterviewQuestionDto
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public string AnswerHint { get; set; } = string.Empty;
}
