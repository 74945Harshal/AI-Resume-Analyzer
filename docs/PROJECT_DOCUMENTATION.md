# AI Resume Analyzer — Complete Project Documentation

> **Version:** 1.0.0 | **Framework:** ASP.NET Core 7.0 | **Architecture:** Clean Architecture + Modular Monolith

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Architecture Overview](#2-architecture-overview)
3. [Solution Structure](#3-solution-structure)
4. [Tech Stack & NuGet Packages](#4-tech-stack--nuget-packages)
5. [Domain Layer](#5-domain-layer)
6. [Application Layer](#6-application-layer)
7. [Persistence Layer](#7-persistence-layer)
8. [Infrastructure Layer](#8-infrastructure-layer)
9. [API Layer](#9-api-layer)
10. [Database Schema](#10-database-schema)
11. [CQRS — Commands & Queries Reference](#11-cqrs--commands--queries-reference)
12. [API Endpoints Reference](#12-api-endpoints-reference)
13. [Authentication & Authorization](#13-authentication--authorization)
14. [AI Integration — Ollama](#14-ai-integration--ollama)
15. [PDF Parsing](#15-pdf-parsing)
16. [Caching — Redis](#16-caching--redis)
17. [Background Jobs — Hangfire](#17-background-jobs--hangfire)
18. [Logging — Serilog](#18-logging--serilog)
19. [Error Handling](#19-error-handling)
20. [Configuration Reference](#20-configuration-reference)
21. [Getting Started](#21-getting-started)
22. [EF Core Migrations](#22-ef-core-migrations)
23. [Design Patterns Used](#23-design-patterns-used)
24. [API Response Format](#24-api-response-format)
25. [Security Considerations](#25-security-considerations)

---

## 1. Project Overview

**AI Resume Analyzer** is a production-ready REST API that uses a locally-running Large Language Model (Ollama) to analyze PDF resumes. It extracts skills, generates professional summaries, matches resumes against job descriptions with a scored result, identifies missing skills, and generates tailored technical interview questions — all without sending data to any external cloud AI service.

### Key Capabilities

| Capability | Description |
|---|---|
| Resume Upload | Upload PDF resumes (max 10 MB), extract and store text |
| Skill Extraction | AI identifies all technical and soft skills from resume text |
| Professional Summary | AI generates a 3–4 sentence career summary |
| Job Match Scoring | Compare resume against a job description, get a 0–100 match score |
| Missing Skills | Identify skills required by the job but absent from the resume |
| Interview Questions | Generate 10 targeted technical interview questions |
| Analysis History | Paginated, searchable, sortable history of all past analyses |
| JWT Auth | Secure registration, login, refresh tokens, role-based access |
| Background Jobs | Hangfire processes heavy AI tasks asynchronously |
| Redis Caching | Cache analysis results to avoid redundant AI calls |
| Soft Delete | All records support soft delete with audit timestamps |

---

## 2. Architecture Overview

The solution follows **Clean Architecture** (also known as Onion Architecture), enforcing strict dependency rules: outer layers depend on inner layers, never the reverse.

```
┌─────────────────────────────────────────────────────────┐
│                    API Layer                            │
│         (Controllers, Middleware, Swagger)              │
├─────────────────────────────────────────────────────────┤
│               Infrastructure Layer                      │
│   (JWT, Redis, Hangfire, Ollama, PDF, Email, Hashing)   │
├──────────────────────┬──────────────────────────────────┤
│   Persistence Layer  │                                  │
│  (EF Core, DbContext,│    Application Layer             │
│   Repositories, UoW) │  (CQRS, MediatR, Validators,    │
│                      │   DTOs, AutoMapper, Interfaces)  │
├──────────────────────┴──────────────────────────────────┤
│                    Domain Layer                         │
│          (Entities, BaseEntity, Interfaces)             │
└─────────────────────────────────────────────────────────┘
```

### Dependency Flow

```
Domain  ←  Application  ←  Persistence
                        ←  Infrastructure
                        ←  API
```

The **Domain** and **Application** layers have zero dependencies on frameworks or infrastructure. All infrastructure concerns (database, AI, cache, email) are abstracted behind interfaces defined in the Application layer and implemented in Infrastructure/Persistence.

---

## 3. Solution Structure

```
AI Resume Analyzer/
├── docs/
│   └── PROJECT_DOCUMENTATION.md       ← You are here
│
├── AIResumeAnalyzer.sln
│
├── AIResumeAnalyzer.Domain/            ← Core business entities
│   ├── Common/
│   │   ├── BaseEntity.cs
│   │   └── IAuditableEntity.cs
│   └── Entities/
│       ├── User.cs
│       ├── Role.cs
│       ├── RefreshToken.cs
│       ├── Resume.cs
│       ├── ResumeAnalysis.cs
│       ├── Skill.cs
│       ├── JobDescription.cs
│       └── InterviewQuestion.cs
│
├── AIResumeAnalyzer.Application/       ← Business logic, CQRS
│   ├── DependencyInjection.cs
│   ├── Common/
│   │   ├── Behaviors/
│   │   │   └── ValidationBehavior.cs
│   │   ├── DTOs/
│   │   │   ├── UserDto.cs
│   │   │   ├── ResumeDto.cs
│   │   │   ├── ResumeAnalysisDto.cs
│   │   │   ├── SkillDto.cs
│   │   │   ├── JobDescriptionDto.cs
│   │   │   └── InterviewQuestionDto.cs
│   │   ├── Exceptions/
│   │   │   ├── BadRequestException.cs
│   │   │   ├── NotFoundException.cs
│   │   │   └── ValidationException.cs
│   │   ├── Interfaces/
│   │   │   ├── IAIResumeAnalyzerService.cs
│   │   │   ├── IAnalysisOrchestrator.cs
│   │   │   ├── IBackgroundJobService.cs
│   │   │   ├── ICacheService.cs
│   │   │   ├── IEmailService.cs
│   │   │   ├── IGenericRepository.cs
│   │   │   ├── IJwtService.cs
│   │   │   ├── IPasswordHasher.cs
│   │   │   ├── IPdfParserService.cs
│   │   │   ├── IResumeAnalysisRepository.cs
│   │   │   ├── IResumeRepository.cs
│   │   │   ├── IUnitOfWork.cs
│   │   │   └── IUserRepository.cs
│   │   ├── Mappings/
│   │   │   └── MappingProfile.cs
│   │   └── Models/
│   │       ├── ApiResponse.cs
│   │       └── PaginatedList.cs
│   └── Features/
│       ├── Authentication/
│       │   ├── RegisterUserCommand.cs  (+ Handler)
│       │   ├── RegisterUserCommandValidator.cs
│       │   ├── LoginUserCommand.cs     (+ Handler)
│       │   ├── LoginUserCommandValidator.cs
│       │   ├── GetUserByIdQuery.cs     (+ Handler)
│       │   └── AuthResponse.cs
│       ├── Resumes/
│       │   ├── UploadResumeCommand.cs  (+ Handler)
│       │   ├── UploadResumeCommandValidator.cs
│       │   ├── DeleteResumeCommand.cs  (+ Handler)
│       │   ├── GetResumeByIdQuery.cs   (+ Handler)
│       │   └── GetUserResumesQuery.cs  (+ Handler)
│       └── Analysis/
│           ├── AnalyzeResumeCommand.cs           (+ Handler)
│           ├── AnalyzeResumeCommandValidator.cs
│           ├── MatchResumeWithJobCommand.cs       (+ Handler)
│           ├── MatchResumeWithJobCommandValidator.cs
│           ├── GenerateInterviewQuestionsCommand.cs (+ Handler)
│           ├── GenerateInterviewQuestionsCommandValidator.cs
│           ├── GetAnalysisByIdQuery.cs            (+ Handler)
│           └── GetAnalysisHistoryQuery.cs         (+ Handler)
│
├── AIResumeAnalyzer.Persistence/       ← EF Core, Repositories
│   ├── DependencyInjection.cs
│   ├── Context/
│   │   └── ApplicationDbContext.cs
│   ├── Configurations/
│   │   ├── UserConfiguration.cs
│   │   ├── RoleConfiguration.cs
│   │   ├── ResumeConfiguration.cs
│   │   ├── ResumeAnalysisConfiguration.cs
│   │   └── RefreshTokenConfiguration.cs
│   └── Repositories/
│       ├── GenericRepository.cs
│       ├── UserRepository.cs
│       ├── ResumeRepository.cs
│       ├── ResumeAnalysisRepository.cs
│       └── UnitOfWork.cs
│
├── AIResumeAnalyzer.Infrastructure/    ← External services
│   ├── DependencyInjection.cs
│   └── Services/
│       ├── JwtService.cs
│       ├── PasswordHasher.cs
│       ├── PdfParserService.cs
│       ├── OllamaAIService.cs
│       ├── RedisCacheService.cs
│       ├── NoOpCacheService.cs
│       ├── HangfireBackgroundJobService.cs
│       ├── EmailService.cs
│       └── AnalysisOrchestrator.cs
│
└── AIResumeAnalyzer.API/               ← HTTP entry point
    ├── Program.cs
    ├── appsettings.json
    ├── appsettings.Development.json
    ├── Configuration/
    │   ├── SwaggerConfiguration.cs
    │   └── HangfireAuthorizationFilter.cs
    ├── Controllers/
    │   ├── BaseController.cs
    │   ├── AuthController.cs
    │   ├── ResumesController.cs
    │   └── AnalysisController.cs
    └── Middleware/
        ├── GlobalExceptionMiddleware.cs
        └── RequestLoggingMiddleware.cs
```

---

## 4. Tech Stack & NuGet Packages

### Runtime

| Technology | Version | Purpose |
|---|---|---|
| ASP.NET Core | 7.0 | Web API framework |
| Entity Framework Core | 7.0.20 | ORM / Code-First migrations |
| SQL Server / LocalDB | — | Primary relational database |
| Ollama | Latest | Local LLM inference engine (free, no API key) |
| Redis | — | Distributed caching |

### NuGet Packages by Project

#### AIResumeAnalyzer.Application
| Package | Version | Purpose |
|---|---|---|
| MediatR | 12.1.1 | CQRS mediator pattern |
| FluentValidation | 11.9.0 | Command/query validation |
| FluentValidation.DependencyInjectionExtensions | 11.9.0 | Auto-register validators |
| AutoMapper | 12.0.1 | Object-to-object mapping |
| AutoMapper.Extensions.Microsoft.DependencyInjection | 12.0.1 | `AddAutoMapper()` DI extension |
| Microsoft.Extensions.Logging.Abstractions | 7.0.1 | Logging abstractions |

#### AIResumeAnalyzer.Persistence
| Package | Version | Purpose |
|---|---|---|
| Microsoft.EntityFrameworkCore | 7.0.20 | ORM core |
| Microsoft.EntityFrameworkCore.SqlServer | 7.0.20 | SQL Server provider |
| Microsoft.EntityFrameworkCore.Design | 7.0.20 | Migrations tooling |
| Microsoft.EntityFrameworkCore.Tools | 7.0.20 | CLI migration commands |

#### AIResumeAnalyzer.Infrastructure
| Package | Version | Purpose |
|---|---|---|
| Microsoft.AspNetCore.Authentication.JwtBearer | 7.0.20 | JWT middleware |
| System.IdentityModel.Tokens.Jwt | 7.0.3 | JWT token generation |
| StackExchange.Redis | 2.7.20 | Redis client |
| Hangfire.AspNetCore | 1.8.11 | Background job server |
| Hangfire.SqlServer | 1.8.11 | Hangfire SQL Server storage |
| itext7 | 7.2.5 | PDF text extraction (AGPL) |
| System.Net.Http.Json | 7.0.1 | HTTP JSON helpers |
| Microsoft.Extensions.Http | 7.0.0 | IHttpClientFactory |

#### AIResumeAnalyzer.API
| Package | Version | Purpose |
|---|---|---|
| Serilog.AspNetCore | 7.0.0 | Structured logging |
| Serilog.Sinks.Console | 4.1.0 | Console log output |
| Serilog.Sinks.File | 5.0.0 | Rolling file log output |
| Serilog.Enrichers.Environment | 2.3.0 | Machine name enrichment |
| Serilog.Enrichers.Thread | 3.1.0 | Thread ID enrichment |
| Swashbuckle.AspNetCore | 6.5.0 | Swagger/OpenAPI UI |
| Swashbuckle.AspNetCore.Annotations | 6.5.0 | Swagger annotations |

---

## 5. Domain Layer

The Domain layer contains pure C# classes with no framework dependencies. It is the innermost layer and never references any other project.

### BaseEntity

Every entity inherits from `BaseEntity`, which provides:

```csharp
public abstract class BaseEntity : IAuditableEntity
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public bool IsDeleted { get; set; }   // Soft delete flag
}
```

### IAuditableEntity

```csharp
public interface IAuditableEntity
{
    DateTime CreatedDate { get; set; }
    DateTime? UpdatedDate { get; set; }
}
```

`ApplicationDbContext.SaveChangesAsync` automatically sets `CreatedDate` on insert and `UpdatedDate` on every update via the `ApplyAuditInfo()` method.

### Entity Relationships

```
Role (1) ──────────── (N) User
User (1) ──────────── (N) Resume
User (1) ──────────── (N) RefreshToken
Resume (1) ─────────── (N) ResumeAnalysis
JobDescription (1) ─── (N) ResumeAnalysis
ResumeAnalysis (1) ──── (N) Skill
ResumeAnalysis (1) ──── (N) InterviewQuestion
```

---

## 6. Application Layer

The Application layer contains all business logic. It defines interfaces that outer layers must implement, and orchestrates use cases through CQRS handlers.

### CQRS with MediatR

Every use case is a **Command** (mutates state) or a **Query** (reads state). Each lives in its own file alongside its handler and validator.

```
Feature/
├── SomeCommand.cs          ← record + IRequestHandler<,> in same file
├── SomeCommandValidator.cs ← AbstractValidator<SomeCommand>
└── SomeQuery.cs            ← record + IRequestHandler<,> in same file
```

### Validation Pipeline

`ValidationBehavior<TRequest, TResponse>` is registered as a MediatR pipeline behavior. It runs **before** every handler. If any `IValidator<TRequest>` finds failures, it throws `ValidationException` — the global middleware catches it and returns HTTP 400 with the error dictionary.

```
Request → ValidationBehavior → Handler → Response
              ↓ (on failure)
         throws ValidationException → GlobalExceptionMiddleware → 400
```

### AutoMapper Profiles

`MappingProfile` defines all entity → DTO mappings:

| Source | Destination | Notes |
|---|---|---|
| `User` | `UserDto` | Maps `Role.Name` → `RoleName` |
| `Resume` | `ResumeDto` | Direct mapping |
| `ResumeAnalysis` | `ResumeAnalysisDto` | Includes nested Skills, Questions |
| `Skill` | `SkillDto` | Direct mapping |
| `JobDescription` | `JobDescriptionDto` | Direct mapping |
| `InterviewQuestion` | `InterviewQuestionDto` | Direct mapping |

### Key Interfaces (defined in Application, implemented elsewhere)

| Interface | Implemented In | Purpose |
|---|---|---|
| `IUnitOfWork` | Persistence | Coordinates all repositories + SaveChanges |
| `IGenericRepository<T>` | Persistence | CRUD for any entity |
| `IUserRepository` | Persistence | User-specific queries |
| `IResumeRepository` | Persistence | Resume-specific queries |
| `IResumeAnalysisRepository` | Persistence | Analysis queries with includes |
| `IAIResumeAnalyzerService` | Infrastructure | Ollama AI operations |
| `IPdfParserService` | Infrastructure | PDF text extraction |
| `IJwtService` | Infrastructure | JWT generation/validation |
| `IPasswordHasher` | Infrastructure | PBKDF2 password hashing |
| `ICacheService` | Infrastructure | Redis get/set/remove |
| `IBackgroundJobService` | Infrastructure | Hangfire job enqueue |
| `IEmailService` | Infrastructure | SMTP email sending |
| `IAnalysisOrchestrator` | Infrastructure | Background AI processing |

### Pagination Model

`PaginatedList<T>` wraps any list with pagination metadata:

```csharp
public class PaginatedList<T>
{
    public List<T> Items { get; }
    public int PageNumber { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }
    public bool HasPreviousPage { get; }
    public bool HasNextPage { get; }
}
```

---

## 7. Persistence Layer

### ApplicationDbContext

Extends `DbContext` with:
- **Global soft-delete query filter** — automatically appends `WHERE IsDeleted = 0` to every query for all entities inheriting `BaseEntity`
- **Audit info** — `SaveChangesAsync` sets `CreatedDate`/`UpdatedDate` automatically
- **Configuration discovery** — `ApplyConfigurationsFromAssembly` loads all `IEntityTypeConfiguration<T>` classes

### Generic Repository

`GenericRepository<T>` provides the standard CRUD contract:

```csharp
Task<T?> GetByIdAsync(int id, CancellationToken ct)
Task<IEnumerable<T>> GetAllAsync(CancellationToken ct)
Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct)
Task AddAsync(T entity, CancellationToken ct)
void Update(T entity)
void Delete(T entity)          // Sets IsDeleted = true (soft delete)
Task<bool> ExistsAsync(int id, CancellationToken ct)
```

### Specific Repositories

| Repository | Extra Methods |
|---|---|
| `UserRepository` | `GetByEmailAsync`, `GetUserWithRoleAsync` |
| `ResumeRepository` | `GetUserResumesAsync`, `GetResumeWithAnalysesAsync` |
| `ResumeAnalysisRepository` | `GetAnalysisDetailsAsync` (with all includes), `GetUserAnalysisHistoryAsync` |

### Unit of Work

`UnitOfWork` exposes all repositories as lazy-initialized properties and wraps `SaveChangesAsync`. This ensures all changes within a single request are committed atomically.

```csharp
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IResumeRepository Resumes { get; }
    IResumeAnalysisRepository ResumeAnalyses { get; }
    IGenericRepository<RefreshToken> RefreshTokens { get; }
    IGenericRepository<JobDescription> JobDescriptions { get; }
    IGenericRepository<Skill> Skills { get; }
    IGenericRepository<InterviewQuestion> InterviewQuestions { get; }
    Task<int> SaveChangesAsync(CancellationToken ct);
}
```

### EF Core Configurations

Each entity has a dedicated `IEntityTypeConfiguration<T>` class:

| Configuration | Key Rules |
|---|---|
| `UserConfiguration` | Unique index on Email, max lengths, FK to Role with Restrict delete |
| `RoleConfiguration` | Seeds Admin (Id=1) and User (Id=2) roles |
| `ResumeConfiguration` | FK to User with Cascade delete |
| `ResumeAnalysisConfiguration` | FK to Resume (Cascade), FK to JobDescription (SetNull), cascades to Skills and InterviewQuestions |
| `RefreshTokenConfiguration` | FK to User with Cascade delete |

---

## 8. Infrastructure Layer

### JwtService

Generates and validates JWT tokens using `System.IdentityModel.Tokens.Jwt`.

**Access Token Claims:**
- `sub` — User ID
- `email` — User email
- `unique_name` — Username
- `role` — Role name (Admin / User)
- `jti` — Unique token ID

**Refresh Token:** 64 cryptographically random bytes encoded as Base64.

**`GetPrincipalFromExpiredToken`:** Validates signature of an expired token (used for token refresh flows) without checking lifetime.

### PasswordHasher

Uses **PBKDF2 with SHA-256**, 100,000 iterations, 16-byte salt, 32-byte hash. The salt is prepended to the hash and stored as a single Base64 string. Verification uses `CryptographicOperations.FixedTimeEquals` to prevent timing attacks.

### PdfParserService

Uses **itext7** to extract text page-by-page from uploaded PDF files. The stream is first read into a `MemoryStream` (required for itext7's seekable reader), then processed synchronously inside `Task.Run` to avoid blocking the thread pool.

### OllamaAIService

Communicates with a locally-running Ollama instance via HTTP POST to `/api/generate`. All four AI operations follow the same pattern:

1. Build a plain-text prompt with clear instructions
2. POST to Ollama with `{ model, prompt, stream: false }`
3. Parse the JSON response field
4. Use regex to extract structured JSON from the AI's free-text response
5. Fall back gracefully if parsing fails

**Timeout:** 5 minutes per request (AI inference can be slow on CPU).

**Fallback:** If Ollama is unreachable, throws `InvalidOperationException` with a clear message telling the user to start Ollama.

### RedisCacheService / NoOpCacheService

`RedisCacheService` serializes values to JSON using `System.Text.Json` before storing in Redis. All operations are wrapped in try/catch — a Redis failure never crashes the application.

`NoOpCacheService` is registered automatically if Redis connection fails at startup. It silently no-ops all cache operations, allowing the app to run without Redis.

### AnalysisOrchestrator

Used by Hangfire background jobs to run the full AI pipeline asynchronously:
- `ProcessResumeAnalysisAsync` — extracts skills + summary, notifies user by email
- `ProcessJobMatchAnalysisAsync` — runs job comparison, saves missing skills
- `ProcessInterviewQuestionsGenerationAsync` — generates and saves 10 questions

### EmailService

Uses `System.Net.Mail.SmtpClient` to send HTML emails. Failures are logged but not rethrown — email is a non-critical notification path.

---

## 9. API Layer

### Program.cs — Middleware Pipeline Order

```
GlobalExceptionMiddleware       ← catches all unhandled exceptions
RequestLoggingMiddleware        ← logs method, path, status, duration
UseSerilogRequestLogging        ← structured Serilog HTTP log
UseSwagger / UseSwaggerUI       ← only in Development
UseHttpsRedirection
UseCors("AllowAll")
UseAuthentication               ← reads JWT from Authorization header
UseAuthorization                ← enforces [Authorize] attributes
UseHangfireDashboard("/hangfire")
UseStaticFiles                  ← serves wwwroot/resumes/
MapControllers
```

> **Order matters.** Authentication must come before Authorization. Exception middleware must be first so it catches errors from all subsequent middleware.

### BaseController

All controllers inherit `BaseController`, which provides:
- `CurrentUserId` — parsed from the `sub` JWT claim
- `CurrentUserEmail` — parsed from the `email` JWT claim
- `OkResponse<T>(data, message)` — wraps data in `ApiResponse<T>`
- `CreatedResponse<T>(data, message)` — returns HTTP 201

### Controllers

| Controller | Route | Auth |
|---|---|---|
| `AuthController` | `/api/auth` | Mixed (register/login = anonymous) |
| `ResumesController` | `/api/resumes` | `[Authorize]` |
| `AnalysisController` | `/api/analysis` | `[Authorize]` |

### Swagger

Available at `/swagger` in Development. Configured with:
- JWT Bearer authentication support (click **Authorize**, enter `Bearer <token>`)
- Full API description with feature summary
- Request duration display
- Deep linking enabled

### Hangfire Dashboard

Available at `/hangfire`. In Development, open to all. In Production, requires the `Admin` role.

---

## 10. Database Schema

### Tables

#### Users
| Column | Type | Constraints |
|---|---|---|
| Id | int | PK, Identity |
| Username | nvarchar(100) | NOT NULL |
| Email | nvarchar(255) | NOT NULL, UNIQUE |
| PasswordHash | nvarchar(max) | NOT NULL |
| RoleId | int | FK → Roles |
| CreatedDate | datetime2 | NOT NULL |
| UpdatedDate | datetime2 | NULL |
| IsDeleted | bit | NOT NULL, DEFAULT 0 |

#### Roles
| Column | Type | Constraints |
|---|---|---|
| Id | int | PK |
| Name | nvarchar(50) | NOT NULL |
| CreatedDate | datetime2 | NOT NULL |
| IsDeleted | bit | NOT NULL |

**Seeded data:** `Admin` (Id=1), `User` (Id=2)

#### RefreshTokens
| Column | Type | Constraints |
|---|---|---|
| Id | int | PK |
| Token | nvarchar(500) | NOT NULL |
| JwtId | nvarchar(100) | NOT NULL |
| ExpiryDate | datetime2 | NOT NULL |
| IsUsed | bit | NOT NULL |
| IsRevoked | bit | NOT NULL |
| UserId | int | FK → Users (Cascade) |
| CreatedDate | datetime2 | NOT NULL |
| IsDeleted | bit | NOT NULL |

#### Resumes
| Column | Type | Constraints |
|---|---|---|
| Id | int | PK |
| FileName | nvarchar(255) | NOT NULL |
| FilePath | nvarchar(500) | NOT NULL |
| ExtractedText | nvarchar(max) | NOT NULL |
| UserId | int | FK → Users (Cascade) |
| CreatedDate | datetime2 | NOT NULL |
| UpdatedDate | datetime2 | NULL |
| IsDeleted | bit | NOT NULL |

#### JobDescriptions
| Column | Type | Constraints |
|---|---|---|
| Id | int | PK |
| Title | nvarchar(200) | NOT NULL |
| DescriptionText | nvarchar(max) | NOT NULL |
| CreatedDate | datetime2 | NOT NULL |
| IsDeleted | bit | NOT NULL |

#### ResumeAnalyses
| Column | Type | Constraints |
|---|---|---|
| Id | int | PK |
| Summary | nvarchar(max) | NOT NULL |
| MatchScore | float | NOT NULL |
| ResumeId | int | FK → Resumes (Cascade) |
| JobDescriptionId | int | FK → JobDescriptions (SetNull), NULL |
| CreatedDate | datetime2 | NOT NULL |
| UpdatedDate | datetime2 | NULL |
| IsDeleted | bit | NOT NULL |

#### Skills
| Column | Type | Constraints |
|---|---|---|
| Id | int | PK |
| Name | nvarchar(max) | NOT NULL |
| IsMissing | bit | NOT NULL |
| ResumeAnalysisId | int | FK → ResumeAnalyses (Cascade) |
| CreatedDate | datetime2 | NOT NULL |
| IsDeleted | bit | NOT NULL |

#### InterviewQuestions
| Column | Type | Constraints |
|---|---|---|
| Id | int | PK |
| Question | nvarchar(max) | NOT NULL |
| AnswerHint | nvarchar(max) | NOT NULL |
| ResumeAnalysisId | int | FK → ResumeAnalyses (Cascade) |
| CreatedDate | datetime2 | NOT NULL |
| IsDeleted | bit | NOT NULL |

---

## 11. CQRS — Commands & Queries Reference

### Authentication Feature

#### RegisterUserCommand
- **Input:** `Username`, `Email`, `Password`
- **Validation:** Username ≤ 100 chars, valid email, password ≥ 6 chars
- **Logic:** Checks email uniqueness → hashes password → creates User with RoleId=2 (User) → generates JWT + refresh token
- **Output:** `AuthResponse` (token, refreshToken, username, email, role)

#### LoginUserCommand
- **Input:** `Email`, `Password`
- **Validation:** Non-empty email and password
- **Logic:** Finds user by email → verifies password hash → generates JWT + refresh token
- **Output:** `AuthResponse`

#### GetUserByIdQuery
- **Input:** `Id`
- **Logic:** Loads user with Role include → maps to `UserDto`
- **Output:** `UserDto`

### Resume Feature

#### UploadResumeCommand
- **Input:** `FileName`, `FileStream`, `UserId`
- **Validation:** `.pdf` extension only, file size ≤ 10 MB, non-empty stream
- **Logic:** Verifies user exists → extracts text via `IPdfParserService` → saves file to `wwwroot/resumes/` with GUID prefix → saves Resume entity
- **Output:** `ResumeDto`

#### DeleteResumeCommand
- **Input:** `Id`, `UserId`
- **Logic:** Loads resume → verifies ownership → soft deletes
- **Output:** `bool`

#### GetResumeByIdQuery
- **Input:** `Id`, `UserId`
- **Logic:** Loads resume → verifies ownership → maps to `ResumeDto`
- **Output:** `ResumeDto`

#### GetUserResumesQuery
- **Input:** `UserId`
- **Logic:** Returns all non-deleted resumes for the user
- **Output:** `IEnumerable<ResumeDto>`

### Analysis Feature

#### AnalyzeResumeCommand
- **Input:** `ResumeId`, `UserId`
- **Validation:** Both IDs > 0
- **Logic:** Verifies ownership → calls `ExtractSkillsAsync` + `GenerateSummaryAsync` → saves `ResumeAnalysis` + `Skill` entities
- **Output:** `ResumeAnalysisDto` (with skills, no match score)

#### MatchResumeWithJobCommand
- **Input:** `ResumeId`, `UserId`, `JobTitle`, `JobDescriptionText`
- **Validation:** JobTitle ≤ 200 chars, JobDescription ≥ 50 chars
- **Logic:** Saves `JobDescription` → runs full AI pipeline (skills + summary + match comparison) → saves analysis with match score, present skills, and missing skills
- **Output:** `ResumeAnalysisDto` (with match score, all skills flagged present/missing)

#### GenerateInterviewQuestionsCommand
- **Input:** `AnalysisId`, `UserId`
- **Logic:** Loads analysis with skills → verifies ownership → calls `GenerateInterviewQuestionsAsync` with present skill names → saves up to 10 `InterviewQuestion` entities
- **Output:** `List<InterviewQuestionDto>`

#### GetAnalysisByIdQuery
- **Input:** `Id`, `UserId`
- **Logic:** Loads full analysis with all includes → verifies ownership
- **Output:** `ResumeAnalysisDto`

#### GetAnalysisHistoryQuery
- **Input:** `UserId`, `PageNumber`, `PageSize`, `SearchTerm?`, `SortBy?`, `SortDescending`
- **Logic:** Loads all user analyses → applies search filter (summary, job title, filename) → sorts by `createddate`/`matchscore`/`filename` → paginates
- **Output:** `PaginatedList<ResumeAnalysisDto>`

---

## 12. API Endpoints Reference

### Authentication — `/api/auth`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/auth/register` | Anonymous | Register new user |
| POST | `/api/auth/login` | Anonymous | Login, get JWT |
| GET | `/api/auth/me` | Bearer | Get current user profile |
| GET | `/api/auth/{id}` | Admin only | Get user by ID |

**Register Request:**
```json
{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "SecurePass123"
}
```

**Login Request:**
```json
{
  "email": "john@example.com",
  "password": "SecurePass123"
}
```

**Auth Response:**
```json
{
  "success": true,
  "message": "Login successful.",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "base64string...",
    "username": "john_doe",
    "email": "john@example.com",
    "role": "User"
  }
}
```

### Resumes — `/api/resumes`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/resumes/upload` | Bearer | Upload PDF resume (multipart/form-data, field: `file`) |
| GET | `/api/resumes` | Bearer | Get all my resumes |
| GET | `/api/resumes/{id}` | Bearer | Get resume by ID |
| DELETE | `/api/resumes/{id}` | Bearer | Soft-delete resume |

**Upload:** `Content-Type: multipart/form-data`, field name `file`, PDF only, max 10 MB.

### Analysis — `/api/analysis`

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| POST | `/api/analysis/analyze/{resumeId}` | Bearer | Analyze resume (skills + summary) |
| POST | `/api/analysis/match` | Bearer | Match resume vs job description |
| POST | `/api/analysis/{analysisId}/interview-questions` | Bearer | Generate interview questions |
| GET | `/api/analysis/{id}` | Bearer | Get analysis by ID |
| GET | `/api/analysis/history` | Bearer | Paginated analysis history |

**Match Request:**
```json
{
  "resumeId": 1,
  "jobTitle": "Senior .NET Developer",
  "jobDescriptionText": "We are looking for a Senior .NET Developer with 5+ years experience in C#, ASP.NET Core, SQL Server, Docker, Kubernetes, Azure DevOps..."
}
```

**History Query Parameters:**
```
GET /api/analysis/history?pageNumber=1&pageSize=10&searchTerm=dotnet&sortBy=matchscore&sortDescending=true
```

---

## 13. Authentication & Authorization

### Flow

```
1. POST /api/auth/register  →  User created, JWT + RefreshToken returned
2. POST /api/auth/login     →  JWT + RefreshToken returned
3. All protected endpoints  →  Authorization: Bearer <JWT>
```

### JWT Structure

- **Algorithm:** HMAC-SHA256
- **Expiry:** 60 minutes (configurable via `JwtSettings:ExpiryMinutes`)
- **Claims:** `sub` (userId), `email`, `unique_name` (username), `role`, `jti`

### Roles

| Role | Id | Capabilities |
|---|---|---|
| Admin | 1 | All endpoints including `GET /api/auth/{id}` |
| User | 2 | Own resumes and analyses only |

Roles are seeded automatically via EF Core data seeding in `RoleConfiguration`. New registrations always get `RoleId = 2` (User).

### Refresh Token

Stored in the `RefreshTokens` table. Each login/register creates a new refresh token with a 7-day expiry. The `GetPrincipalFromExpiredToken` method in `JwtService` allows implementing a token refresh endpoint (not yet exposed as an API endpoint — can be added as a future enhancement).

### Authorization Policies

```csharp
options.AddPolicy("AdminOnly",    policy => policy.RequireRole("Admin"));
options.AddPolicy("UserOrAdmin",  policy => policy.RequireRole("User", "Admin"));
```

---

## 14. AI Integration — Ollama

### What is Ollama?

[Ollama](https://ollama.com) is a free, open-source tool that runs Large Language Models locally on your machine. No API key, no cloud, no cost. It exposes a simple HTTP API on `http://localhost:11434`.

### Setup

1. Download and install Ollama from https://ollama.com
2. Pull the model: `ollama pull llama3.2`
3. Ollama runs as a background service automatically

### Configuration

```json
"Ollama": {
  "BaseUrl": "http://localhost:11434",
  "Model": "llama3.2"
}
```

You can swap the model to any Ollama-compatible model (e.g., `mistral`, `codellama`, `phi3`).

### AI Operations

#### ExtractSkillsAsync
- **Prompt strategy:** Asks the model to return a JSON array of skill strings
- **Parsing:** Regex extracts `[...]` from response, deserializes as `List<string>`
- **Fallback:** If JSON parsing fails, splits response by newlines/commas

#### GenerateSummaryAsync
- **Prompt strategy:** Asks for a 3–4 sentence professional summary
- **Output:** Raw string returned as-is

#### CompareResumeWithJobAsync
- **Prompt strategy:** Asks for JSON with `matchScore` (0–100) and `missingSkills` array
- **Parsing:** Regex extracts `{...}`, deserializes as `MatchResult`
- **Output:** `(double MatchScore, List<string> MissingSkills)`

#### GenerateInterviewQuestionsAsync
- **Prompt strategy:** Asks for exactly 10 questions as a JSON array with `question` and `answerHint` fields
- **Parsing:** Regex extracts `[...]`, deserializes as `List<InterviewQuestionResult>`
- **Output:** `List<(string Question, string AnswerHint)>`

### Error Handling

If Ollama is not running, `OllamaAIService` throws `InvalidOperationException` with the message:
> "Failed to communicate with Ollama AI service. Ensure Ollama is running at http://localhost:11434."

This is caught by `GlobalExceptionMiddleware` and returned as HTTP 400.

---

## 15. PDF Parsing

### Library: itext7 (v7.2.5)

itext7 is used to extract text from uploaded PDF resumes.

**License:** AGPL-3.0. For commercial/closed-source distribution, a commercial itext license is required. For internal/open-source use, AGPL applies.

### Process

```
1. IFormFile received in ResumesController
2. Stream opened from IFormFile
3. UploadResumeCommand dispatched with stream
4. PdfParserService.ExtractTextAsync called:
   a. Stream copied to MemoryStream (itext7 needs seekable stream)
   b. PdfReader + PdfDocument opened from byte array
   c. LocationTextExtractionStrategy extracts text per page
   d. All pages concatenated and trimmed
5. Extracted text stored in Resume.ExtractedText
6. File saved to wwwroot/resumes/{guid}_{originalname}.pdf
```

### Validation

- Extension must be `.pdf` (checked by `UploadResumeCommandValidator`)
- File size must be ≤ 10 MB (checked by validator + `RequestSizeLimit` attribute)
- Extracted text must not be empty (checked in handler — rejects image-only PDFs)

---

## 16. Caching — Redis

### Configuration

```json
"ConnectionStrings": {
  "Redis": "localhost:6379"
}
```

### Resilience

If Redis is unavailable at startup, `DependencyInjection.AddRedisCache` catches the connection exception and registers `NoOpCacheService` instead. The application starts and runs normally — just without caching.

### ICacheService

```csharp
Task<T?> GetAsync<T>(string key, CancellationToken ct)
Task SetAsync<T>(string key, T value, TimeSpan? expiration, CancellationToken ct)
Task RemoveAsync(string key, CancellationToken ct)
```

Default expiration: **30 minutes** if not specified.

### Recommended Cache Keys (for future use)

```
analysis:{analysisId}           → ResumeAnalysisDto
history:{userId}:p{page}        → PaginatedList<ResumeAnalysisDto>
resume:{resumeId}               → ResumeDto
```

Cache invalidation should be triggered in command handlers after mutations (e.g., after `AnalyzeResumeCommand` completes, set the cache key; after `DeleteResumeCommand`, remove it).

---

## 17. Background Jobs — Hangfire

### Configuration

Hangfire uses SQL Server as its job storage (same database as the application). Jobs are persisted and survive application restarts.

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AIResumeAnalyzerDb;..."
}
```

### Dashboard

Available at `/hangfire`. Shows:
- Enqueued, processing, succeeded, failed jobs
- Job retry history
- Server status

### IBackgroundJobService

```csharp
string Enqueue(Expression<Func<Task>> methodCall)
string Enqueue<T>(Expression<Func<T, Task>> methodCall)
```

### IAnalysisOrchestrator (Background Job Target)

The `AnalysisOrchestrator` is designed to be called by Hangfire jobs:

```csharp
// Example: enqueue from a controller or handler
_backgroundJobService.Enqueue<IAnalysisOrchestrator>(
    o => o.ProcessResumeAnalysisAsync(resumeId, analysisId));
```

### Job Types

| Method | Trigger | Description |
|---|---|---|
| `ProcessResumeAnalysisAsync` | After resume upload | Runs AI skill extraction + summary, emails user |
| `ProcessJobMatchAnalysisAsync` | After job match request | Runs comparison, saves missing skills |
| `ProcessInterviewQuestionsGenerationAsync` | On demand | Generates and saves 10 interview questions |

---

## 18. Logging — Serilog

### Configuration (appsettings.json)

```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "WriteTo": [
    { "Name": "Console" },
    {
      "Name": "File",
      "Args": {
        "path": "Logs/log-.txt",
        "rollingInterval": "Day",
        "retainedFileCountLimit": 30
      }
    }
  ]
}
```

### Log Files

Rolling daily log files are written to `AIResumeAnalyzer.API/Logs/log-YYYYMMDD.txt`. Up to 30 days are retained.

### What Gets Logged

| Event | Level | Where |
|---|---|---|
| Application start/stop | Information | Bootstrap logger |
| Every HTTP request | Information | `UseSerilogRequestLogging` |
| Request start/end with duration | Information | `RequestLoggingMiddleware` |
| Validation failures | Warning | `ValidationBehavior` |
| Cache hits/misses | Warning | `RedisCacheService` |
| AI service errors | Error | `OllamaAIService` |
| Background job progress | Information | `AnalysisOrchestrator` |
| Unhandled exceptions | Error | `GlobalExceptionMiddleware` |
| Fatal startup errors | Fatal | Bootstrap logger |

---

## 19. Error Handling

### GlobalExceptionMiddleware

Catches all unhandled exceptions and maps them to structured JSON responses:

| Exception Type | HTTP Status | Notes |
|---|---|---|
| `ValidationException` | 400 Bad Request | Includes `errors` dictionary with field-level messages |
| `NotFoundException` | 404 Not Found | Entity name and key in message |
| `BadRequestException` | 400 Bad Request | Custom message |
| `UnauthorizedAccessException` | 401 Unauthorized | Ownership violations |
| `InvalidOperationException` | 400 Bad Request | Business rule violations |
| Any other `Exception` | 500 Internal Server Error | Generic message, full details in logs |

### Error Response Format

```json
{
  "success": false,
  "message": "One or more validation errors occurred.",
  "errors": {
    "Email": ["A valid email address is required."],
    "Password": ["Password must be at least 6 characters long."]
  },
  "statusCode": 400
}
```

---

## 20. Configuration Reference

### appsettings.json — Full Reference

```json
{
  "Serilog": { ... },                    // Logging configuration

  "ConnectionStrings": {
    "DefaultConnection": "...",          // SQL Server connection string
    "Redis": "localhost:6379"            // Redis connection string
  },

  "JwtSettings": {
    "SecretKey": "...",                  // Min 32 chars, keep secret
    "Issuer": "AIResumeAnalyzer",        // Token issuer claim
    "Audience": "AIResumeAnalyzerUsers", // Token audience claim
    "ExpiryMinutes": "60"               // Access token lifetime
  },

  "Ollama": {
    "BaseUrl": "http://localhost:11434", // Ollama server URL
    "Model": "llama3.2"                 // Model name to use
  },

  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",    // Use App Password for Gmail
    "FromEmail": "noreply@...",
    "FromName": "AI Resume Analyzer"
  }
}
```

### Environment-Specific Overrides

`appsettings.Development.json` overrides:
- Log level set to `Debug` for all namespaces
- EF Core SQL commands logged at `Information`
- JWT expiry extended to 120 minutes for convenience
- Uses a separate dev secret key

### Environment Variables

Any `appsettings.json` key can be overridden via environment variables using double-underscore notation:

```
JwtSettings__SecretKey=MyProductionSecret
ConnectionStrings__DefaultConnection=Server=prod-server;...
Ollama__Model=mistral
```

---

## 21. Getting Started

### Prerequisites

| Requirement | Version | Notes |
|---|---|---|
| .NET SDK | 7.0.x | `dotnet --version` to check |
| SQL Server | Any | LocalDB works for development |
| Ollama | Latest | https://ollama.com |
| Redis | 6+ | Optional — app runs without it |
| Visual Studio | 17.4+ | Or VS Code with C# extension |

### Step 1 — Clone and Open

```bash
git clone <repository-url>
cd "AI Resume Analyzer"
```

Open `AIResumeAnalyzer.sln` in Visual Studio.

### Step 2 — Configure Connection String

Edit `AIResumeAnalyzer.API/appsettings.Development.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AIResumeAnalyzerDb;Trusted_Connection=True;"
}
```

For a full SQL Server instance:
```
Server=localhost;Database=AIResumeAnalyzerDb;User Id=sa;Password=YourPassword;TrustServerCertificate=True;
```

### Step 3 — Run EF Core Migrations

Open **Package Manager Console** in Visual Studio, set Default Project to `AIResumeAnalyzer.Persistence`:

```powershell
Add-Migration InitialCreate -StartupProject AIResumeAnalyzer.API
Update-Database -StartupProject AIResumeAnalyzer.API
```

Or using the .NET CLI from the solution root:

```bash
dotnet ef migrations add InitialCreate --project AIResumeAnalyzer.Persistence --startup-project AIResumeAnalyzer.API
dotnet ef database update --project AIResumeAnalyzer.Persistence --startup-project AIResumeAnalyzer.API
```

This creates all tables and seeds the Admin and User roles.

### Step 4 — Start Ollama

```bash
# Install a model (one-time)
ollama pull llama3.2

# Ollama runs as a background service automatically after install
# Verify it's running:
curl http://localhost:11434/api/tags
```

### Step 5 — Run the API

Press **F5** in Visual Studio, or:

```bash
dotnet run --project AIResumeAnalyzer.API
```

The API starts at `https://localhost:58436`. Swagger UI is at `https://localhost:58436/swagger`.

### Step 6 — Test the API

1. Open Swagger at `https://localhost:58436/swagger`
2. Register: `POST /api/auth/register`
3. Login: `POST /api/auth/login` — copy the `token` from the response
4. Click **Authorize** in Swagger, enter `Bearer <your-token>`
5. Upload a PDF: `POST /api/resumes/upload` — use the file picker
6. Analyze: `POST /api/analysis/analyze/{resumeId}`
7. View results: `GET /api/analysis/{analysisId}`

---

## 22. EF Core Migrations

### Creating a New Migration

```bash
dotnet ef migrations add <MigrationName> \
  --project AIResumeAnalyzer.Persistence \
  --startup-project AIResumeAnalyzer.API
```

### Applying Migrations

```bash
dotnet ef database update \
  --project AIResumeAnalyzer.Persistence \
  --startup-project AIResumeAnalyzer.API
```

### Rolling Back

```bash
# Roll back to a specific migration
dotnet ef database update <PreviousMigrationName> \
  --project AIResumeAnalyzer.Persistence \
  --startup-project AIResumeAnalyzer.API

# Remove the last unapplied migration
dotnet ef migrations remove \
  --project AIResumeAnalyzer.Persistence \
  --startup-project AIResumeAnalyzer.API
```

### Generating SQL Script (for production deployments)

```bash
dotnet ef migrations script \
  --project AIResumeAnalyzer.Persistence \
  --startup-project AIResumeAnalyzer.API \
  --output migration.sql
```

---

## 23. Design Patterns Used

### Clean Architecture
The solution is divided into concentric layers. The Domain and Application layers have no dependencies on frameworks, databases, or external services. All dependencies point inward.

### CQRS (Command Query Responsibility Segregation)
Every use case is either a Command (write) or a Query (read). Commands and Queries are plain C# records dispatched through MediatR. This makes each use case independently testable and easy to locate.

### Mediator Pattern (MediatR)
Controllers never call services directly. They dispatch a Command or Query to MediatR, which routes it to the correct handler. This decouples the API layer from business logic.

### Repository Pattern
Data access is abstracted behind `IGenericRepository<T>` and specific repository interfaces. Handlers never use `DbContext` directly — they always go through the Unit of Work.

### Unit of Work Pattern
`IUnitOfWork` groups all repositories and exposes a single `SaveChangesAsync`. This ensures all changes in a single request are committed in one database transaction.

### Pipeline Behavior (Decorator Pattern)
`ValidationBehavior<TRequest, TResponse>` wraps every MediatR handler. It runs all registered `IValidator<TRequest>` instances before the handler executes. Additional behaviors (logging, caching, performance monitoring) can be added to the pipeline without modifying handlers.

### Factory / Strategy Pattern (AI Parsing)
`OllamaAIService` uses a consistent strategy for all AI operations: build prompt → call Ollama → extract JSON with regex → deserialize → fallback on failure. Each operation is independently replaceable.

### Null Object Pattern
`NoOpCacheService` implements `ICacheService` with no-op methods. When Redis is unavailable, the DI container injects this instead of `RedisCacheService`, allowing the application to run without caching and without null checks throughout the codebase.

### Soft Delete Pattern
`BaseEntity.IsDeleted` combined with a global EF Core query filter means deleted records are never returned by any query. The `GenericRepository.Delete` method sets `IsDeleted = true` instead of issuing a `DELETE` SQL statement.

### Audit Trail Pattern
`ApplicationDbContext.ApplyAuditInfo()` automatically sets `CreatedDate` on insert and `UpdatedDate` on every update, providing a complete audit trail without any manual code in handlers.

### Options Pattern
All configuration sections (`JwtSettings`, `Ollama`, `EmailSettings`) are read via `IConfiguration` with null-coalescing defaults, making the application resilient to missing configuration values.

---

## 24. API Response Format

All endpoints return a consistent envelope:

### Success Response
```json
{
  "success": true,
  "message": "Operation Successful",
  "data": { ... }
}
```

### Created Response (HTTP 201)
```json
{
  "success": true,
  "message": "Resume uploaded successfully.",
  "data": {
    "id": 1,
    "fileName": "john_cv.pdf",
    "filePath": "resumes/abc123_john_cv.pdf",
    "extractedText": "John Doe...",
    "userId": 1,
    "createdDate": "2026-05-31T10:00:00Z"
  }
}
```

### Analysis Response
```json
{
  "success": true,
  "message": "Resume analyzed successfully.",
  "data": {
    "id": 1,
    "summary": "Experienced .NET developer with 5 years...",
    "matchScore": 82.5,
    "resumeId": 1,
    "jobDescription": {
      "id": 1,
      "title": "Senior .NET Developer",
      "descriptionText": "..."
    },
    "createdDate": "2026-05-31T10:05:00Z",
    "skills": [
      { "id": 1, "name": "C#", "isMissing": false },
      { "id": 2, "name": "Docker", "isMissing": true }
    ],
    "interviewQuestions": [
      {
        "id": 1,
        "question": "Explain the difference between IEnumerable and IQueryable.",
        "answerHint": "IEnumerable executes in memory; IQueryable translates to SQL"
      }
    ]
  }
}
```

### Paginated Response
```json
{
  "success": true,
  "message": "Operation Successful",
  "data": {
    "items": [ ... ],
    "pageNumber": 1,
    "totalPages": 5,
    "totalCount": 47,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

### Error Response
```json
{
  "success": false,
  "message": "One or more validation errors occurred.",
  "errors": {
    "JobDescriptionText": ["Job description must be at least 50 characters."]
  },
  "statusCode": 400
}
```

---

## 25. Security Considerations

### Password Storage
Passwords are hashed using **PBKDF2-SHA256** with 100,000 iterations and a 16-byte random salt. This is the NIST-recommended approach. Raw passwords are never stored or logged.

### JWT Security
- Tokens are signed with HMAC-SHA256
- `ClockSkew` is set to `TimeSpan.Zero` — tokens expire exactly at their stated time
- The secret key should be at least 32 characters and stored in environment variables or a secrets manager in production, never in source control

### Input Validation
All commands are validated by FluentValidation before reaching handlers. The validation pipeline runs automatically via MediatR's `ValidationBehavior`.

### Ownership Enforcement
Every handler that accesses a resource (resume, analysis) verifies that `resource.UserId == currentUserId`. Unauthorized access throws `UnauthorizedAccessException` → HTTP 401.

### File Upload Security
- Only `.pdf` extension is accepted (validated before processing)
- File size is capped at 10 MB at both the validator and `[RequestSizeLimit]` attribute levels
- Uploaded files are saved with a GUID prefix to prevent filename collisions and path traversal attacks
- Files are stored in `wwwroot/resumes/` — outside the application code directory

### SQL Injection
Entity Framework Core uses parameterized queries exclusively. No raw SQL is used anywhere in the codebase.

### Sensitive Data in Logs
Passwords, JWT secrets, and connection strings are never logged. The `RequestLoggingMiddleware` logs only method, path, status code, and duration.

### CORS
Currently configured as `AllowAll` for development convenience. In production, restrict to specific origins:

```csharp
options.AddPolicy("Production", policy =>
    policy.WithOrigins("https://yourfrontend.com")
          .AllowAnyMethod()
          .AllowAnyHeader());
```

### Production Checklist

- [ ] Change `JwtSettings:SecretKey` to a strong random value (≥ 32 chars)
- [ ] Store secrets in environment variables or Azure Key Vault / AWS Secrets Manager
- [ ] Restrict CORS to known origins
- [ ] Enable HTTPS only (remove HTTP profile from launchSettings)
- [ ] Set `Serilog:MinimumLevel:Default` to `Warning` in production
- [ ] Restrict Hangfire dashboard to Admin role (already implemented)
- [ ] Use a production SQL Server instance (not LocalDB)
- [ ] Configure Redis with authentication (`requirepass` in redis.conf)
- [ ] Review itext7 AGPL license compliance for your distribution model

---

*Documentation generated for AI Resume Analyzer v1.0.0 — ASP.NET Core 7.0 — Clean Architecture*
