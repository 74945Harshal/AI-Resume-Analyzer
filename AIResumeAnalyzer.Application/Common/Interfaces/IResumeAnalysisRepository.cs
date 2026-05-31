using AIResumeAnalyzer.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Application.Common.Interfaces;

public interface IResumeAnalysisRepository : IGenericRepository<ResumeAnalysis>
{
    Task<ResumeAnalysis?> GetAnalysisDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ResumeAnalysis>> GetUserAnalysisHistoryAsync(int userId, CancellationToken cancellationToken = default);
}
