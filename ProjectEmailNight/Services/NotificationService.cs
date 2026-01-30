using Microsoft.AspNetCore.SignalR;
using ProjectEmailNight.Hubs;

namespace ProjectEmailNight.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendEmailNotificationAsync(string receiverId, EmailNotificationDto notification)
    {
        await _hubContext.Clients.Group(receiverId).SendAsync("ReceiveEmailNotification", notification);
    }

    public async Task SendUnreadCountUpdateAsync(string userId, int unreadCount)
    {
        await _hubContext.Clients.Group(userId).SendAsync("UpdateUnreadCount", unreadCount);
    }

    public async Task SendToastNotificationAsync(string userId, string title, string message, string type = "info")
    {
        await _hubContext.Clients.Group(userId).SendAsync("ReceiveToast", new
        {
            title,
            message,
            type, // info, success, warning, error
            timestamp = DateTime.UtcNow
        });
    }
}