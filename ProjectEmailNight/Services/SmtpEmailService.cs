using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using ProjectEmailNight.Dtos;
using ProjectEmailNight.Models;

namespace ProjectEmailNight.Services;

public class SmtpEmailService : ISmtpEmailService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<SmtpSettings> settings, ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(SendEmailDto email)
    {
        try
        {
            using var client = new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.UserName, _settings.Password),
                EnableSsl = _settings.EnableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = email.Subject,
                Body = email.Body,
                IsBodyHtml = email.IsHtml
            };

            mailMessage.To.Add(email.To);

            // Ekleri ekle
            if (email.Attachments != null && email.Attachments.Any())
            {
                foreach (var attachment in email.Attachments)
                {
                    var stream = new MemoryStream(attachment.Content);
                    mailMessage.Attachments.Add(new Attachment(stream, attachment.FileName, attachment.ContentType));
                }
            }

            await client.SendMailAsync(mailMessage);
            
            _logger.LogInformation("Email sent successfully to {To}", email.To);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", email.To);
            return false;
        }
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        return await SendEmailAsync(new SendEmailDto
        {
            To = to,
            Subject = subject,
            Body = body,
            IsHtml = isHtml
        });
    }
}