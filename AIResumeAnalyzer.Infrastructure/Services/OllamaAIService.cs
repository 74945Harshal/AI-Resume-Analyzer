using AIResumeAnalyzer.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Infrastructure.Services;

public class OllamaAIService : IAIResumeAnalyzerService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OllamaAIService> _logger;
    private readonly string _model;
    private readonly string _baseUrl;

    public OllamaAIService(HttpClient httpClient, IConfiguration configuration, ILogger<OllamaAIService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
        _model = configuration["Ollama:Model"] ?? "llama3.2";
        _httpClient.BaseAddress = new Uri(_baseUrl);
        _httpClient.Timeout = TimeSpan.FromMinutes(5);
    }

    public async Task<List<string>> ExtractSkillsAsync(string resumeText, CancellationToken cancellationToken = default)
    {
        var prompt =
            "Extract all technical and professional skills from the following resume text.\n" +
            "Return ONLY a JSON array of skill names as strings. No explanation, no markdown, just the JSON array.\n" +
            "Example: [\"C#\", \".NET\", \"SQL Server\", \"REST APIs\"]\n\n" +
            "Resume Text:\n" +
            TruncateText(resumeText, 3000);

        var response = await SendPromptAsync(prompt, cancellationToken);

        try
        {
            var jsonMatch = Regex.Match(response, @"\[.*?\]", RegexOptions.Singleline);
            if (jsonMatch.Success)
            {
                var skills = JsonSerializer.Deserialize<List<string>>(jsonMatch.Value);
                return skills?.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList()
                    ?? new List<string>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse skills JSON from AI response. Falling back to text parsing.");
        }

        return ParseSkillsFromText(response);
    }

    public async Task<string> GenerateSummaryAsync(string resumeText, CancellationToken cancellationToken = default)
    {
        var prompt =
            "Write a professional 3-4 sentence summary for the following resume.\n" +
            "Focus on key skills, experience level, and career highlights.\n" +
            "Be concise and professional.\n\n" +
            "Resume Text:\n" +
            TruncateText(resumeText, 3000);

        return await SendPromptAsync(prompt, cancellationToken);
    }

    public async Task<(double MatchScore, List<string> MissingSkills)> CompareResumeWithJobAsync(
        string resumeText, string jobDescription, CancellationToken cancellationToken = default)
    {
        var prompt =
            "Compare the resume with the job description and provide:\n" +
            "1. A match score from 0 to 100 (integer)\n" +
            "2. A list of skills/requirements mentioned in the job description that are missing from the resume\n\n" +
            "Return ONLY valid JSON in this exact format (no markdown, no explanation):\n" +
            "{\n" +
            "  \"matchScore\": 75,\n" +
            "  \"missingSkills\": [\"Docker\", \"Kubernetes\", \"Azure\"]\n" +
            "}\n\n" +
            "Resume:\n" +
            TruncateText(resumeText, 2000) +
            "\n\nJob Description:\n" +
            TruncateText(jobDescription, 1500);

        var response = await SendPromptAsync(prompt, cancellationToken);

        try
        {
            var jsonMatch = Regex.Match(response, @"\{.*?\}", RegexOptions.Singleline);
            if (jsonMatch.Success)
            {
                var result = JsonSerializer.Deserialize<MatchResult>(
                    jsonMatch.Value,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result != null)
                {
                    var score = Math.Clamp(result.MatchScore, 0, 100);
                    var missing = result.MissingSkills?
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToList() ?? new List<string>();
                    return (score, missing);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse match result JSON from AI response.");
        }

        return (0, new List<string>());
    }

    public async Task<List<(string Question, string AnswerHint)>> GenerateInterviewQuestionsAsync(
        List<string> skills, CancellationToken cancellationToken = default)
    {
        var skillsList = string.Join(", ", skills.Take(15));

        var prompt =
            "Generate exactly 10 technical interview questions based on these skills: " + skillsList + "\n\n" +
            "Return ONLY valid JSON array in this exact format (no markdown, no explanation):\n" +
            "[\n" +
            "  {\n" +
            "    \"question\": \"What is dependency injection and why is it important?\",\n" +
            "    \"answerHint\": \"Explain IoC container, loose coupling, testability benefits\"\n" +
            "  }\n" +
            "]";

        var response = await SendPromptAsync(prompt, cancellationToken);

        try
        {
            var jsonMatch = Regex.Match(response, @"\[.*?\]", RegexOptions.Singleline);
            if (jsonMatch.Success)
            {
                var questions = JsonSerializer.Deserialize<List<InterviewQuestionResult>>(
                    jsonMatch.Value,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (questions != null)
                {
                    return questions
                        .Where(q => !string.IsNullOrWhiteSpace(q.Question))
                        .Select(q => (q.Question, q.AnswerHint ?? string.Empty))
                        .Take(10)
                        .ToList();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse interview questions JSON from AI response.");
        }

        return new List<(string, string)>();
    }

    private async Task<string> SendPromptAsync(string prompt, CancellationToken cancellationToken)
    {
        var requestBody = new
        {
            model = _model,
            prompt,
            stream = false
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/generate", requestBody, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaResponse>(
                cancellationToken: cancellationToken);

            return result?.Response?.Trim() ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error communicating with Ollama API at {BaseUrl}", _baseUrl);
            throw new InvalidOperationException(
                "Failed to communicate with Ollama AI service. " +
                $"Ensure Ollama is running at {_baseUrl}. Error: {ex.Message}", ex);
        }
    }

    private static string TruncateText(string text, int maxLength)
        => text.Length <= maxLength ? text : text[..maxLength] + "...";

    private static List<string> ParseSkillsFromText(string text)
    {
        return text
            .Split(new[] { '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim().Trim('"', '\'', '[', ']', '-', '*', ' '))
            .Where(s => !string.IsNullOrWhiteSpace(s) && s.Length > 1 && s.Length < 50)
            .Distinct()
            .Take(30)
            .ToList();
    }

    // ── Private response models ───────────────────────────────────────────────

    private sealed class OllamaResponse
    {
        [JsonPropertyName("response")]
        public string Response { get; set; } = string.Empty;
    }

    private sealed class MatchResult
    {
        [JsonPropertyName("matchScore")]
        public double MatchScore { get; set; }

        [JsonPropertyName("missingSkills")]
        public List<string>? MissingSkills { get; set; }
    }

    private sealed class InterviewQuestionResult
    {
        [JsonPropertyName("question")]
        public string Question { get; set; } = string.Empty;

        [JsonPropertyName("answerHint")]
        public string AnswerHint { get; set; } = string.Empty;
    }
}
