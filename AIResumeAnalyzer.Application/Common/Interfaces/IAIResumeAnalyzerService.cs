using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Application.Common.Interfaces;

public interface IAIResumeAnalyzerService
{
    Task<List<string>> ExtractSkillsAsync(string resumeText, CancellationToken cancellationToken = default);
    Task<string> GenerateSummaryAsync(string resumeText, CancellationToken cancellationToken = default);
    Task<(double MatchScore, List<string> MissingSkills)> CompareResumeWithJobAsync(string resumeText, string jobDescription, CancellationToken cancellationToken = default);
    Task<List<(string Question, string AnswerHint)>> GenerateInterviewQuestionsAsync(List<string> skills, CancellationToken cancellationToken = default);
}
