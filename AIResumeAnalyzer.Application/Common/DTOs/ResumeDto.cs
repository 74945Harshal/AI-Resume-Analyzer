using System;

namespace AIResumeAnalyzer.Application.Common.DTOs;

public class ResumeDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ExtractedText { get; set; } = string.Empty;
    public int UserId { get; set; }
    public DateTime CreatedDate { get; set; }
}
