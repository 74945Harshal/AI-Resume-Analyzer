using AIResumeAnalyzer.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace AIResumeAnalyzer.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");
        var host = emailSettings["SmtpHost"] ?? "localhost";
        var port = int.Parse(emailSettings["SmtpPort"] ?? "587");
        var username = emailSettings["Username"] ?? string.Empty;
        var password = emailSettings["Password"] ?? string.Empty;
        var fromEmail = emailSettings["FromEmail"] ?? "noreply@airesumeanalyzer.com";
        var fromName = emailSettings["FromName"] ?? "AI Resume Analyzer";

        try
        {
            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(username, password)
            };

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(to);

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent to {To} with subject: {Subject}", to, subject);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            // Don't rethrow - email failures should not break the main flow
        }
    }
}
