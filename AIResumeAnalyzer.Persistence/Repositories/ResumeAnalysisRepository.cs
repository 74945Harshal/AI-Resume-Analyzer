using AIResumeAnalyzer.Application.Common.Interfaces;
using AIResumeAnalyzer.Domain.Entities;
using AIResumeAnalyzer.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Persistence.Repositories;

public class ResumeAnalysisRepository : GenericRepository<ResumeAnalysis>, IResumeAnalysisRepository
{
    public ResumeAnalysisRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ResumeAnalysis?> GetAnalysisDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(ra => ra.Resume)
            .Include(ra => ra.JobDescription)
            .Include(ra => ra.Skills)
            .Include(ra => ra.InterviewQuestions)
            .FirstOrDefaultAsync(ra => ra.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ResumeAnalysis>> GetUserAnalysisHistoryAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(ra => ra.Resume)
            .Include(ra => ra.JobDescription)
            .Include(ra => ra.Skills)
            .Where(ra => ra.Resume.UserId == userId)
            .OrderByDescending(ra => ra.CreatedDate)
            .ToListAsync(cancellationToken);
    }
}
