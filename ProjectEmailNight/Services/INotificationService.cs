namespace ProjectEmailNight.Services;

public interface INotificationService
{
    Task SendEmailNotificationAsync(string receiverId, EmailNotificationDto notification);
    Task SendUnreadCountUpdateAsync(string userId, int unreadCount);
    Task SendToastNotificationAsync(string userId, string title, string message, string type = "info");
}

public class EmailNotificationDto
{
    public int EmailId { get; set; }
    public string SenderName { get; set; } = "";
    public string SenderEmail { get; set; } = "";
    public string SenderInitials { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Preview { get; set; } = "";
    public string? AISummary { get; set; }
    public string CategoryName { get; set; } = "";
    public string CategoryColor { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}