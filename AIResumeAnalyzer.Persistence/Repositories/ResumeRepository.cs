using AIResumeAnalyzer.Application.Common.Interfaces;
using AIResumeAnalyzer.Domain.Entities;
using AIResumeAnalyzer.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Persistence.Repositories;

public class ResumeRepository : GenericRepository<Resume>, IResumeRepository
{
    public ResumeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Resume>> GetUserResumesAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(r => r.UserId == userId).ToListAsync(cancellationToken);
    }

    public async Task<Resume?> GetResumeWithAnalysesAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Include(r => r.Analyses)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }
}
