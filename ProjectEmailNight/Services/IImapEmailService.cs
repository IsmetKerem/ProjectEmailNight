using ProjectEmailNight.Dtos;

namespace ProjectEmailNight.Services;

public interface IImapEmailService
{
    Task<List<ReceivedEmailDto>> GetEmailsAsync(int count = 20, int skip = 0);
    Task<ReceivedEmailDto?> GetEmailByIdAsync(string messageId);
    Task<bool> MarkAsReadAsync(string messageId);
    Task<bool> DeleteEmailAsync(string messageId);
    Task<int> GetUnreadCountAsync();
}