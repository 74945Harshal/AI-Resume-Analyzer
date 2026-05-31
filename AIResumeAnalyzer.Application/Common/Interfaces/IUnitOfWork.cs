using AIResumeAnalyzer.Domain.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IResumeRepository Resumes { get; }
    IResumeAnalysisRepository ResumeAnalyses { get; }
    IGenericRepository<RefreshToken> RefreshTokens { get; }
    IGenericRepository<JobDescription> JobDescriptions { get; }
    IGenericRepository<Skill> Skills { get; }
    IGenericRepository<InterviewQuestion> InterviewQuestions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
