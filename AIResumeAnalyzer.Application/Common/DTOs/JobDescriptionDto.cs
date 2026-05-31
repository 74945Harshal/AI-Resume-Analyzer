namespace AIResumeAnalyzer.Application.Common.DTOs;

public class JobDescriptionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DescriptionText { get; set; } = string.Empty;
}
