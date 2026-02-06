// Services/EmailSyncService.cs

using Microsoft.EntityFrameworkCore;
using ProjectEmailNight.Context;
using ProjectEmailNight.Entities;

namespace ProjectEmailNight.Services;

public class EmailSyncService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailSyncService> _logger;
    private readonly TimeSpan _syncInterval = TimeSpan.FromMinutes(5);

    public EmailSyncService(IServiceProvider serviceProvider, ILogger<EmailSyncService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Sync Service started");

        // İlk başlangıçta 30 saniye bekle (uygulama tam başlasın)
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncEmailsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email sync");
            }

            await Task.Delay(_syncInterval, stoppingToken);
        }
    }

    private async Task SyncEmailsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        
        var imapService = scope.ServiceProvider.GetRequiredService<IImapEmailService>();
        var context = scope.ServiceProvider.GetRequiredService<EmailContext>();
        var aiService = scope.ServiceProvider.GetService<IAIService>();

        try
        {
            // Son 50 emaili çek
            var emails = await imapService.GetEmailsAsync(50);
            
            _logger.LogInformation("Fetched {Count} emails from IMAP", emails.Count);

            if (!emails.Any())
            {
                _logger.LogInformation("No emails to sync");
                return;
            }

            // Admin kullanıcıyı bul (alıcı - IMAP hesabının sahibi)
            var receiver = await context.Users
                .FirstOrDefaultAsync(u => !u.IsExternalUser && u.Email != null);
            
            if (receiver == null)
            {
                _logger.LogWarning("No receiver user found for email sync");
                return;
            }

            foreach (var email in emails)
            {
                try
                {
                    // Bu email daha önce kaydedilmiş mi?
                    var exists = await context.Emails
                        .AnyAsync(e => e.ExternalMessageId == email.MessageId);

                    if (exists)
                    {
                        continue;
                    }

                    // Göndereni bul veya oluştur
                    var sender = await GetOrCreateExternalUserAsync(context, email.From, email.FromName);

                    // Kategoriyi AI ile belirle
                    int categoryId = 1; // Default: Birincil
                    string? aiSummary = null;

                    if (aiService != null)
                    {
                        try
                        {
                            // CategorizeEmailAsync int döndürüyor
                            categoryId = await aiService.CategorizeEmailAsync(email.Subject, email.Body);
                            
                            // Özet de oluştur
                            aiSummary = await aiService.GenerateSummaryAsync(email.Subject, email.Body);
                        }
                        catch (Exception aiEx)
                        {
                            _logger.LogWarning(aiEx, "AI categorization failed for email {Subject}", email.Subject);
                        }
                    }

                    // Kategori ID'nin geçerli olduğundan emin ol
                    var categoryExists = await context.EmailCategories.AnyAsync(c => c.Id == categoryId);
                    if (!categoryExists)
                    {
                        categoryId = 1;
                    }

                    var newEmail = new Email
                    {
                        SenderId = sender.Id,
                        ReceiverId = receiver.Id,
                        Subject = email.Subject ?? "(Konu yok)",
                        Body = !string.IsNullOrEmpty(email.HtmlBody) ? email.HtmlBody : (email.Body ?? ""),
                        CreatedAt = email.Date,
                        IsRead = email.IsRead,
                        IsStarred = false,
                        IsDeleted = false,
                        IsDraft = false,
                        CategoryId = categoryId,
                        ExternalMessageId = email.MessageId,
                        AISummary = aiSummary
                    };

                    context.Emails.Add(newEmail);
                    _logger.LogInformation("Added email: {Subject} from {From}", email.Subject, email.From);
                }
                catch (Exception emailEx)
                {
                    _logger.LogError(emailEx, "Error processing email {MessageId}", email.MessageId);
                }
            }

            await context.SaveChangesAsync();
            _logger.LogInformation("Email sync completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email sync failed");
        }
    }

    private async Task<AppUser> GetOrCreateExternalUserAsync(EmailContext context, string email, string name)
    {
        if (string.IsNullOrEmpty(email))
        {
            email = "unknown@external.com";
            name = "Unknown Sender";
        }

        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        
        if (user == null)
        {
            var nameParts = (name ?? email.Split('@')[0]).Split(' ', 2);
            
            user = new AppUser
            {
                UserName = email,
                NormalizedUserName = email.ToUpper(),
                Email = email,
                NormalizedEmail = email.ToUpper(),
                Name = nameParts.Length > 0 ? nameParts[0] : "External",
                Surname = nameParts.Length > 1 ? nameParts[1] : "User",
                EmailConfirmed = true,
                IsExternalUser = true,
                CreatedAt = DateTime.UtcNow,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            
            context.Users.Add(user);
            await context.SaveChangesAsync();
            
            _logger.LogInformation("Created external user: {Email}", email);
        }

        return user;
    }
}