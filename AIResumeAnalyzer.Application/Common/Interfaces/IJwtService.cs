using AIResumeAnalyzer.Domain.Entities;
using System.Security.Claims;

namespace AIResumeAnalyzer.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
