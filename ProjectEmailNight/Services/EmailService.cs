using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectEmailNight.Context;
using ProjectEmailNight.Entities;
using ProjectEmailNight.Models;
using System.Text.RegularExpressions;

namespace ProjectEmailNight.Services;

public class EmailService : IEmailService
{
    private readonly EmailContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly IWebHostEnvironment _environment;
    private readonly IAIService _aiService;

    public EmailService(
        EmailContext context, 
        UserManager<AppUser> userManager, 
        IWebHostEnvironment environment,
        IAIService aiService)
    {
        _context = context;
        _userManager = userManager;
        _environment = environment;
        _aiService = aiService;
    }

    public async Task<int> SendEmailAsync(string senderId, string receiverEmail, string subject, string htmlBody, List<IFormFile>? attachments = null)
    {
        var receiver = await _userManager.FindByEmailAsync(receiverEmail);
        if (receiver == null)
            throw new ArgumentException("Alıcı bulunamadı");

        var sanitizedHtml = SanitizeHtml(htmlBody);

        // AI ile analiz et
        var analysis = await _aiService.AnalyzeEmailAsync(subject, sanitizedHtml);

        var email = new Email
        {
            SenderId = senderId,
            ReceiverId = receiver.Id,
            Subject = subject,
            Body = sanitizedHtml,
            AISummary = analysis.Summary,
            IsDraft = false,
            CreatedAt = DateTime.UtcNow,
            CategoryId = analysis.CategoryId
        };

        _context.Emails.Add(email);
        await _context.SaveChangesAsync();

        if (attachments != null && attachments.Any())
        {
            await SaveAttachmentsAsync(email.Id, attachments);
        }

        return email.Id;
    }

    public async Task<int> SaveDraftAsync(string senderId, string? receiverEmail, string subject, string htmlBody, int? existingDraftId = null)
    {
        AppUser? receiver = null;
        if (!string.IsNullOrEmpty(receiverEmail))
        {
            receiver = await _userManager.FindByEmailAsync(receiverEmail);
        }

        Email email;

        if (existingDraftId.HasValue)
        {
            email = await _context.Emails.FindAsync(existingDraftId.Value);
            if (email == null || email.SenderId != senderId)
                throw new ArgumentException("Taslak bulunamadı");

            email.ReceiverId = receiver?.Id;
            email.Subject = subject ?? "";
            email.Body = SanitizeHtml(htmlBody ?? "");
        }
        else
        {
            email = new Email
            {
                SenderId = senderId,
                ReceiverId = receiver?.Id,
                Subject = subject ?? "",
                Body = SanitizeHtml(htmlBody ?? ""),
                IsDraft = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Emails.Add(email);
        }

        await _context.SaveChangesAsync();
        return email.Id;
    }

    public async Task<EmailDetailViewModel?> GetEmailDetailAsync(int emailId, string userId)
    {
        var email = await _context.Emails
            .Include(e => e.Sender)
            .Include(e => e.Receiver)
            .Include(e => e.Category)
            .Include(e => e.Attachments)
            .FirstOrDefaultAsync(e => e.Id == emailId);

        if (email == null)
            return null;

        if (email.SenderId != userId && email.ReceiverId != userId)
            return null;

        // Eğer AI özeti yoksa oluştur
        if (string.IsNullOrEmpty(email.AISummary) && email.ReceiverId == userId)
        {
            email.AISummary = await _aiService.GenerateSummaryAsync(email.Subject, email.Body);
            await _context.SaveChangesAsync();
        }

        return new EmailDetailViewModel
        {
            Id = email.Id,
            Subject = email.Subject,
            Body = email.Body,
            AISummary = email.AISummary,
            SenderId = email.SenderId,
            SenderName = $"{email.Sender.Name} {email.Sender.Surname}",
            SenderEmail = email.Sender.Email ?? "",
            SenderInitials = $"{email.Sender.Name[0]}{email.Sender.Surname[0]}",
            ReceiverId = email.ReceiverId ?? "",
            ReceiverName = email.Receiver != null ? $"{email.Receiver.Name} {email.Receiver.Surname}" : "",
            ReceiverEmail = email.Receiver?.Email ?? "",
            IsRead = email.IsRead,
            IsStarred = email.IsStarred,
            IsMine = email.SenderId == userId,
            CreatedAt = email.CreatedAt,
            ReadAt = email.ReadAt,
            CategoryId = email.CategoryId,
            CategoryName = email.Category?.Name,
            CategoryColor = email.Category?.Color,
            Attachments = email.Attachments.Select(a => new AttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                ContentType = a.ContentType,
                FileSize = a.FileSize
            }).ToList()
        };
    }

    public string SanitizeHtml(string html)
    {
        if (string.IsNullOrEmpty(html))
            return "";

        var sanitized = html;
        
        // Script taglerini kaldır
        sanitized = Regex.Replace(sanitized, @"<script[^>]*>[\s\S]*?</script>", "", RegexOptions.IgnoreCase);
        
        // Event handler'ları kaldır
        sanitized = Regex.Replace(sanitized, @"\s*on\w+\s*=\s*""[^""]*""", "", RegexOptions.IgnoreCase);
        sanitized = Regex.Replace(sanitized, @"\s*on\w+\s*=\s*'[^']*'", "", RegexOptions.IgnoreCase);
        
        // javascript: linklerini kaldır
        sanitized = Regex.Replace(sanitized, @"javascript\s*:", "", RegexOptions.IgnoreCase);
        
        // Tehlikeli tagleri kaldır
        sanitized = Regex.Replace(sanitized, @"<iframe[^>]*>[\s\S]*?</iframe>", "", RegexOptions.IgnoreCase);
        sanitized = Regex.Replace(sanitized, @"<object[^>]*>[\s\S]*?</object>", "", RegexOptions.IgnoreCase);
        sanitized = Regex.Replace(sanitized, @"<embed[^>]*>", "", RegexOptions.IgnoreCase);

        return sanitized;
    }

    public string ConvertToPlainText(string html)
    {
        if (string.IsNullOrEmpty(html))
            return "";

        // HTML taglerini kaldır
        var text = Regex.Replace(html, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"<p[^>]*>", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"</p>", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"<div[^>]*>", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"</div>", "", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"<li[^>]*>", "• ", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"</li>", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"<[^>]+>", "", RegexOptions.IgnoreCase);
        text = System.Net.WebUtility.HtmlDecode(text);
        text = Regex.Replace(text, @"\n{3,}", "\n\n");
        
        return text.Trim();
    }

    private async Task SaveAttachmentsAsync(int emailId, List<IFormFile> files)
    {
        var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "attachments");
        
        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);

        foreach (var file in files)
        {
            if (file.Length > 0 && file.Length <= 25 * 1024 * 1024)
            {
                var storedFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var filePath = Path.Combine(uploadPath, storedFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var attachment = new EmailAttachment
                {
                    EmailId = emailId,
                    FileName = file.FileName,
                    StoredFileName = storedFileName,
                    FilePath = $"/uploads/attachments/{storedFileName}",
                    ContentType = file.ContentType,
                    FileSize = file.Length,
                    UploadedAt = DateTime.UtcNow
                };

                _context.EmailAttachments.Add(attachment);
            }
        }

        await _context.SaveChangesAsync();
    }
}