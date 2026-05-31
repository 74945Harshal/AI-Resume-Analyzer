using AIResumeAnalyzer.Domain.Common;
using System.Collections.Generic;

namespace AIResumeAnalyzer.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Resume> Resumes { get; set; } = new List<Resume>();
}
