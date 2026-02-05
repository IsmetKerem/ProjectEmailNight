namespace ProjectEmailNight.Services;

public interface IScheduledEmailService
{
    Task<string> ScheduleEmailAsync(string senderId, string receiverEmail, string subject, string body, DateTime scheduledAt);
    Task SendScheduledEmailAsync(int emailId);
    Task<bool> CancelScheduledEmailAsync(int emailId, string userId);
}