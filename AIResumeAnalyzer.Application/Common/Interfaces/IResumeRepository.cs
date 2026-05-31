using AIResumeAnalyzer.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Application.Common.Interfaces;

public interface IResumeRepository : IGenericRepository<Resume>
{
    Task<IEnumerable<Resume>> GetUserResumesAsync(int userId, CancellationToken cancellationToken = default);
    Task<Resume?> GetResumeWithAnalysesAsync(int id, CancellationToken cancellationToken = default);
}
