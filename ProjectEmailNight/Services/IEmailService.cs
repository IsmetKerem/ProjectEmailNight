using ProjectEmailNight.Entities;
using ProjectEmailNight.Models;

namespace ProjectEmailNight.Services;

public interface IEmailService
{
    Task<int> SendEmailAsync(string senderId, string receiverEmail, string subject, string htmlBody, List<IFormFile>? attachments = null);
    Task<int> SaveDraftAsync(string senderId, string? receiverEmail, string subject, string htmlBody, int? existingDraftId = null);
    Task<EmailDetailViewModel> GetEmailDetailAsync(int emailId, string userId);
    string SanitizeHtml(string html);
    string ConvertToPlainText(string html);
}