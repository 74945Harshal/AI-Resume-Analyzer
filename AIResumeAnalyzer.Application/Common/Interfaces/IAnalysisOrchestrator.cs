using System.Threading.Tasks;

namespace AIResumeAnalyzer.Application.Common.Interfaces;

public interface IAnalysisOrchestrator
{
    Task ProcessResumeAnalysisAsync(int resumeId, int analysisId);
    Task ProcessJobMatchAnalysisAsync(int resumeId, int jobDescriptionId, int analysisId);
    Task ProcessInterviewQuestionsGenerationAsync(int analysisId);
}
