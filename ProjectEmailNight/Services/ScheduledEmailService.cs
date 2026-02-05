using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProjectEmailNight.Context;
using ProjectEmailNight.Entities;

namespace ProjectEmailNight.Services;

public class ScheduledEmailService : IScheduledEmailService
{
    private readonly EmailContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly IAIService _aiService;
    private readonly INotificationService _notificationService;
    private readonly IBackgroundJobClient _backgroundJobClient;

    public ScheduledEmailService(
        EmailContext context,
        UserManager<AppUser> userManager,
        IAIService aiService,
        INotificationService notificationService,
        IBackgroundJobClient backgroundJobClient)
    {
        _context = context;
        _userManager = userManager;
        _aiService = aiService;
        _notificationService = notificationService;
        _backgroundJobClient = backgroundJobClient;
    }

    public async Task<string> ScheduleEmailAsync(string senderId, string receiverEmail, string subject, string body, DateTime scheduledAt)
    {
        var receiver = await _userManager.FindByEmailAsync(receiverEmail);
        if (receiver == null)
            throw new ArgumentException("Alıcı bulunamadı");

        // Zamanlanmış email oluştur
        var email = new Email
        {
            SenderId = senderId,
            ReceiverId = receiver.Id,
            Subject = subject,
            Body = body,
            IsDraft = false,
            IsScheduled = true,
            ScheduledAt = scheduledAt,
            ScheduleSent = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Emails.Add(email);
        await _context.SaveChangesAsync();

        // Hangfire job'ı zamanla
        var delay = scheduledAt - DateTime.UtcNow;
        if (delay < TimeSpan.Zero)
            delay = TimeSpan.FromSeconds(5); // Geçmiş tarih seçildiyse hemen gönder

        var jobId = _backgroundJobClient.Schedule(
            () => SendScheduledEmailAsync(email.Id),
            delay
        );

        // Job ID'yi kaydet
        email.HangfireJobId = jobId;
        await _context.SaveChangesAsync();

        return jobId;
    }

    public async Task SendScheduledEmailAsync(int emailId)
    {
        var email = await _context.Emails
            .Include(e => e.Sender)
            .Include(e => e.Receiver)
            .FirstOrDefaultAsync(e => e.Id == emailId);

        if (email == null || !email.IsScheduled || email.ScheduleSent || email.IsDeleted)
            return;

        // AI ile analiz et
        var analysis = await _aiService.AnalyzeEmailAsync(email.Subject, email.Body);
        email.AISummary = analysis.Summary;
        email.CategoryId = analysis.CategoryId;

        // Email'i gönderildi olarak işaretle
        email.IsScheduled = false;
        email.ScheduleSent = true;
        email.CreatedAt = DateTime.UtcNow; // Gönderim zamanını güncelle

        await _context.SaveChangesAsync();

        // Bildirim gönder
        if (email.Receiver != null && email.Sender != null)
        {
            var category = await _context.EmailCategories.FindAsync(email.CategoryId);
            
            var notification = new EmailNotificationDto
            {
                EmailId = email.Id,
                SenderName = $"{email.Sender.Name} {email.Sender.Surname}",
                SenderEmail = email.Sender.Email ?? "",
                SenderInitials = $"{email.Sender.Name?[0]}{email.Sender.Surname?[0]}",
                Subject = email.Subject,
                Preview = email.Body.Length > 100 ? email.Body.Substring(0, 100) + "..." : email.Body,
                AISummary = email.AISummary,
                CategoryName = category?.Name ?? "Birincil",
                CategoryColor = category?.Color ?? "#4285F4",
                CreatedAt = email.CreatedAt
            };

            await _notificationService.SendEmailNotificationAsync(email.Receiver.Id, notification);

            var unreadCount = await _context.Emails.CountAsync(e =>
                e.ReceiverId == email.Receiver.Id && !e.IsRead && !e.IsDeleted && !e.ReceiverDeleted && !e.IsDraft && !e.IsScheduled);
            await _notificationService.SendUnreadCountUpdateAsync(email.Receiver.Id, unreadCount);
        }
    }

    public async Task<bool> CancelScheduledEmailAsync(int emailId, string userId)
    {
        var email = await _context.Emails.FindAsync(emailId);
        
        if (email == null || email.SenderId != userId || !email.IsScheduled || email.ScheduleSent)
            return false;

        // Hangfire job'ı iptal et
        if (!string.IsNullOrEmpty(email.HangfireJobId))
        {
            BackgroundJob.Delete(email.HangfireJobId);
        }

        // Email'i sil veya taslağa çevir
        email.IsDeleted = true;
        await _context.SaveChangesAsync();

        return true;
    }
}