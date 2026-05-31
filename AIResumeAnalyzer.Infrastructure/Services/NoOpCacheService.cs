using AIResumeAnalyzer.Application.Common.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Infrastructure.Services;

/// <summary>
/// No-operation cache service used as fallback when Redis is unavailable.
/// </summary>
public class NoOpCacheService : ICacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        => Task.FromResult<T?>(default);

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
