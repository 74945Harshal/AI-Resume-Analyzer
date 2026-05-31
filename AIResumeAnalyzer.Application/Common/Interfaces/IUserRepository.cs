using AIResumeAnalyzer.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Application.Common.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetUserWithRoleAsync(int id, CancellationToken cancellationToken = default);
}
