using AIResumeAnalyzer.Application.Common.Interfaces;
using AIResumeAnalyzer.Domain.Entities;
using AIResumeAnalyzer.Persistence.Context;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private bool _disposed = false;

    private IUserRepository? _users;
    private IResumeRepository? _resumes;
    private IResumeAnalysisRepository? _resumeAnalyses;
    private IGenericRepository<RefreshToken>? _refreshTokens;
    private IGenericRepository<JobDescription>? _jobDescriptions;
    private IGenericRepository<Skill>? _skills;
    private IGenericRepository<InterviewQuestion>? _interviewQuestions;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IResumeRepository Resumes => _resumes ??= new ResumeRepository(_context);
    public IResumeAnalysisRepository ResumeAnalyses => _resumeAnalyses ??= new ResumeAnalysisRepository(_context);

    public IGenericRepository<RefreshToken> RefreshTokens => 
        _refreshTokens ??= new GenericRepository<RefreshToken>(_context);

    public IGenericRepository<JobDescription> JobDescriptions => 
        _jobDescriptions ??= new GenericRepository<JobDescription>(_context);

    public IGenericRepository<Skill> Skills => 
        _skills ??= new GenericRepository<Skill>(_context);

    public IGenericRepository<InterviewQuestion> InterviewQuestions => 
        _interviewQuestions ??= new GenericRepository<InterviewQuestion>(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
