namespace AIResumeAnalyzer.Application.Common.DTOs;

public class SkillDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsMissing { get; set; }
}
