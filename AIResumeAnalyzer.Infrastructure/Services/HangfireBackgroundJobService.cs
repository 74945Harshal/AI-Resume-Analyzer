using AIResumeAnalyzer.Application.Common.Interfaces;
using Hangfire;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Infrastructure.Services;

public class HangfireBackgroundJobService : IBackgroundJobService
{
    public string Enqueue(Expression<Func<Task>> methodCall)
    {
        return BackgroundJob.Enqueue(methodCall);
    }

    public string Enqueue<T>(Expression<Func<T, Task>> methodCall)
    {
        return BackgroundJob.Enqueue(methodCall);
    }
}
