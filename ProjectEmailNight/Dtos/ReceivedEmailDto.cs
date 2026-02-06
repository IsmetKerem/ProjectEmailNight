// Dtos/ReceivedEmailDto.cs

namespace ProjectEmailNight.Dtos;

public class ReceivedEmailDto
{
    public string MessageId { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
    public DateTime Date { get; set; }
    public bool IsRead { get; set; }
    public List<EmailAttachmentDto>? Attachments { get; set; }
}