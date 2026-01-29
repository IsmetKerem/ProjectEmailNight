namespace ProjectEmailNight.Models;

public class EmailListViewModel
{
    public List<EmailItemDto> Emails { get; set; } = new();
    public int TotalCount { get; set; }
    public int UnreadCount { get; set; }
    public int StarredCount { get; set; }
    public int DraftCount { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public string CurrentFolder { get; set; } = "inbox";
}

public class EmailItemDto
{
    public int Id { get; set; }
    public string SenderId { get; set; }
    public string SenderName { get; set; }
    public string SenderEmail { get; set; }
    public string SenderInitials { get; set; }
    public string ReceiverName { get; set; }
    public string Subject { get; set; }
    public string Preview { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public bool IsStarred { get; set; }
    public bool IsDraft { get; set; }
    public int? CategoryId { get; set; }
    public string CategoryName { get; set; }
    public string CategoryColor { get; set; }
    public bool HasAttachment { get; set; }
}