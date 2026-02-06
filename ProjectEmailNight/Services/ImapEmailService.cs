// Services/ImapEmailService.cs

using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.Extensions.Options;
using MimeKit;
using ProjectEmailNight.Dtos;
using ProjectEmailNight.Models;

namespace ProjectEmailNight.Services;

public class ImapEmailService : IImapEmailService
{
    private readonly ImapSettings _settings;
    private readonly ILogger<ImapEmailService> _logger;

    public ImapEmailService(IOptions<ImapSettings> settings, ILogger<ImapEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    private async Task<ImapClient> ConnectAsync()
    {
        var client = new ImapClient();
        await client.ConnectAsync(_settings.Host, _settings.Port, _settings.EnableSsl);
        await client.AuthenticateAsync(_settings.UserName, _settings.Password);
        return client;
    }

    public async Task<List<ReceivedEmailDto>> GetEmailsAsync(int count = 20, int skip = 0)
    {
        var emails = new List<ReceivedEmailDto>();

        try
        {
            using var client = await ConnectAsync();
            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly);

            // En son emaillerden başla
            var totalCount = inbox.Count;
            var startIndex = Math.Max(0, totalCount - skip - count);
            var endIndex = Math.Max(0, totalCount - skip - 1);

            if (startIndex > endIndex)
            {
                await client.DisconnectAsync(true);
                return emails;
            }

            var items = await inbox.FetchAsync(startIndex, endIndex, 
                MessageSummaryItems.UniqueId | 
                MessageSummaryItems.Envelope | 
                MessageSummaryItems.Flags |
                MessageSummaryItems.BodyStructure);

            foreach (var item in items.Reverse()) // En yeni önce
            {
                var message = await inbox.GetMessageAsync(item.UniqueId);
                
                emails.Add(new ReceivedEmailDto
                {
                    MessageId = item.UniqueId.ToString(),
                    From = message.From.Mailboxes.FirstOrDefault()?.Address ?? "",
                    FromName = message.From.Mailboxes.FirstOrDefault()?.Name ?? 
                               message.From.Mailboxes.FirstOrDefault()?.Address ?? "",
                    To = message.To.Mailboxes.FirstOrDefault()?.Address ?? "",
                    Subject = message.Subject ?? "(Konu yok)",
                    Body = message.TextBody ?? "",
                    HtmlBody = message.HtmlBody,
                    Date = message.Date.DateTime,
                    IsRead = item.Flags?.HasFlag(MessageFlags.Seen) ?? false,
                    Attachments = GetAttachments(message)
                });
            }

            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch emails");
        }

        return emails;
    }

    public async Task<ReceivedEmailDto?> GetEmailByIdAsync(string messageId)
    {
        try
        {
            using var client = await ConnectAsync();
            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly);

            if (!uint.TryParse(messageId, out var uid))
                return null;

            var uniqueId = new UniqueId(uid);
            var message = await inbox.GetMessageAsync(uniqueId);

            var summary = await inbox.FetchAsync(new[] { uniqueId }, 
                MessageSummaryItems.Flags);

            await client.DisconnectAsync(true);

            return new ReceivedEmailDto
            {
                MessageId = messageId,
                From = message.From.Mailboxes.FirstOrDefault()?.Address ?? "",
                FromName = message.From.Mailboxes.FirstOrDefault()?.Name ?? 
                           message.From.Mailboxes.FirstOrDefault()?.Address ?? "",
                To = message.To.Mailboxes.FirstOrDefault()?.Address ?? "",
                Subject = message.Subject ?? "(Konu yok)",
                Body = message.TextBody ?? "",
                HtmlBody = message.HtmlBody,
                Date = message.Date.DateTime,
                IsRead = summary.FirstOrDefault()?.Flags?.HasFlag(MessageFlags.Seen) ?? false,
                Attachments = GetAttachments(message)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get email {MessageId}", messageId);
            return null;
        }
    }

    public async Task<bool> MarkAsReadAsync(string messageId)
    {
        try
        {
            using var client = await ConnectAsync();
            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadWrite);

            if (!uint.TryParse(messageId, out var uid))
                return false;

            var uniqueId = new UniqueId(uid);
            await inbox.AddFlagsAsync(uniqueId, MessageFlags.Seen, true);

            await client.DisconnectAsync(true);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark email as read {MessageId}", messageId);
            return false;
        }
    }

    public async Task<bool> DeleteEmailAsync(string messageId)
    {
        try
        {
            using var client = await ConnectAsync();
            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadWrite);

            if (!uint.TryParse(messageId, out var uid))
                return false;

            var uniqueId = new UniqueId(uid);
            await inbox.AddFlagsAsync(uniqueId, MessageFlags.Deleted, true);
            await inbox.ExpungeAsync();

            await client.DisconnectAsync(true);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete email {MessageId}", messageId);
            return false;
        }
    }

    public async Task<int> GetUnreadCountAsync()
    {
        try
        {
            using var client = await ConnectAsync();
            var inbox = client.Inbox;
            await inbox.OpenAsync(FolderAccess.ReadOnly);

            var uids = await inbox.SearchAsync(SearchQuery.NotSeen);
            
            await client.DisconnectAsync(true);
            return uids.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get unread count");
            return 0;
        }
    }

    private List<EmailAttachmentDto> GetAttachments(MimeMessage message)
    {
        var attachments = new List<EmailAttachmentDto>();

        foreach (var attachment in message.Attachments)
        {
            if (attachment is MimePart part)
            {
                using var stream = new MemoryStream();
                part.Content.DecodeTo(stream);
                
                attachments.Add(new EmailAttachmentDto
                {
                    FileName = part.FileName ?? "attachment",
                    Content = stream.ToArray(),
                    ContentType = part.ContentType.MimeType
                });
            }
        }

        return attachments;
    }
}