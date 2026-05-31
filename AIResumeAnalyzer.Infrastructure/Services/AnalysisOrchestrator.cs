using AIResumeAnalyzer.Application.Common.Interfaces;
using AIResumeAnalyzer.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Infrastructure.Services;

/// <summary>
/// Orchestrates AI analysis tasks - used by Hangfire background jobs.
/// </summary>
public class AnalysisOrchestrator : IAnalysisOrchestrator
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAIResumeAnalyzerService _aiService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AnalysisOrchestrator> _logger;

    public AnalysisOrchestrator(
        IUnitOfWork unitOfWork,
        IAIResumeAnalyzerService aiService,
        IEmailService emailService,
        ILogger<AnalysisOrchestrator> logger)
    {
        _unitOfWork = unitOfWork;
        _aiService = aiService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task ProcessResumeAnalysisAsync(int resumeId, int analysisId)
    {
        _logger.LogInformation("Background: Processing resume analysis for ResumeId={ResumeId}, AnalysisId={AnalysisId}",
            resumeId, analysisId);

        try
        {
            var resume = await _unitOfWork.Resumes.GetByIdAsync(resumeId);
            if (resume == null)
            {
                _logger.LogWarning("Resume {ResumeId} not found for background analysis.", resumeId);
                return;
            }

            var analysis = await _unitOfWork.ResumeAnalyses.GetAnalysisDetailsAsync(analysisId);
            if (analysis == null)
            {
                _logger.LogWarning("Analysis {AnalysisId} not found for background processing.", analysisId);
                return;
            }

            var skills = await _aiService.ExtractSkillsAsync(resume.ExtractedText);
            var summary = await _aiService.GenerateSummaryAsync(resume.ExtractedText);

            analysis.Summary = summary;
            _unitOfWork.ResumeAnalyses.Update(analysis);

            foreach (var skillName in skills)
            {
                await _unitOfWork.Skills.AddAsync(new Skill
                {
                    Name = skillName,
                    IsMissing = false,
                    ResumeAnalysisId = analysis.Id
                });
            }

            await _unitOfWork.SaveChangesAsync();

            // Notify user
            var user = await _unitOfWork.Users.GetUserWithRoleAsync(resume.UserId);
            if (user != null)
            {
                await _emailService.SendEmailAsync(
                    user.Email,
                    "Your Resume Analysis is Ready",
                    $"<h2>Resume Analysis Complete</h2><p>Your resume <strong>{resume.FileName}</strong> has been analyzed successfully.</p>");
            }

            _logger.LogInformation("Background analysis completed for AnalysisId={AnalysisId}", analysisId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Background analysis failed for AnalysisId={AnalysisId}", analysisId);
            throw;
        }
    }

    public async Task ProcessJobMatchAnalysisAsync(int resumeId, int jobDescriptionId, int analysisId)
    {
        _logger.LogInformation(
            "Background: Processing job match for ResumeId={ResumeId}, JobId={JobId}, AnalysisId={AnalysisId}",
            resumeId, jobDescriptionId, analysisId);

        try
        {
            var resume = await _unitOfWork.Resumes.GetByIdAsync(resumeId);
            var jobDesc = await _unitOfWork.JobDescriptions.GetByIdAsync(jobDescriptionId);
            var analysis = await _unitOfWork.ResumeAnalyses.GetAnalysisDetailsAsync(analysisId);

            if (resume == null || jobDesc == null || analysis == null)
            {
                _logger.LogWarning("Required entities not found for job match analysis.");
                return;
            }

            var (matchScore, missingSkills) = await _aiService.CompareResumeWithJobAsync(
                resume.ExtractedText, jobDesc.DescriptionText);

            analysis.MatchScore = matchScore;
            _unitOfWork.ResumeAnalyses.Update(analysis);

            foreach (var missingSkill in missingSkills)
            {
                await _unitOfWork.Skills.AddAsync(new Skill
                {
                    Name = missingSkill,
                    IsMissing = true,
                    ResumeAnalysisId = analysis.Id
                });
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Job match analysis completed for AnalysisId={AnalysisId}", analysisId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job match analysis failed for AnalysisId={AnalysisId}", analysisId);
            throw;
        }
    }

    public async Task ProcessInterviewQuestionsGenerationAsync(int analysisId)
    {
        _logger.LogInformation("Background: Generating interview questions for AnalysisId={AnalysisId}", analysisId);

        try
        {
            var analysis = await _unitOfWork.ResumeAnalyses.GetAnalysisDetailsAsync(analysisId);
            if (analysis == null)
            {
                _logger.LogWarning("Analysis {AnalysisId} not found.", analysisId);
                return;
            }

            var skillNames = analysis.Skills
                .Where(s => !s.IsMissing)
                .Select(s => s.Name)
                .ToList();

            if (!skillNames.Any())
            {
                _logger.LogWarning("No skills found for interview question generation. AnalysisId={AnalysisId}", analysisId);
                return;
            }

            var questions = await _aiService.GenerateInterviewQuestionsAsync(skillNames);

            foreach (var (question, answerHint) in questions)
            {
                await _unitOfWork.InterviewQuestions.AddAsync(new InterviewQuestion
                {
                    Question = question,
                    AnswerHint = answerHint,
                    ResumeAnalysisId = analysis.Id
                });
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Interview questions generated for AnalysisId={AnalysisId}", analysisId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Interview question generation failed for AnalysisId={AnalysisId}", analysisId);
            throw;
        }
    }
}
