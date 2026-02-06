using ProjectEmailNight.Dtos;

namespace ProjectEmailNight.Services;

public interface ISmtpEmailService
{
    Task<bool> SendEmailAsync(SendEmailDto email);
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
}