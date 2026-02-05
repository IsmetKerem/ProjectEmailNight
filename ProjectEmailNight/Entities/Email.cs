namespace ProjectEmailNight.Entities;

public class Email
{
    public int Id { get; set; }
    
    public string SenderId { get; set; }
    public virtual AppUser Sender { get; set; }
    
    public string? ReceiverId { get; set; }
    public virtual AppUser? Receiver { get; set; }
    
    public string Subject { get; set; } = "";
    public string Body { get; set; } = "";
    
    public string? AISummary { get; set; }
    
    public bool IsRead { get; set; } = false;
    public bool IsStarred { get; set; } = false;
    public bool IsDraft { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    public bool SenderDeleted { get; set; } = false;
    public bool ReceiverDeleted { get; set; } = false;
    
    // Zamanlanmış email için
    public bool IsScheduled { get; set; } = false;
    public DateTime? ScheduledAt { get; set; }
    public string? HangfireJobId { get; set; }
    public bool ScheduleSent { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
    
    public int? CategoryId { get; set; }
    public virtual EmailCategory? Category { get; set; }
    
    public virtual ICollection<EmailAttachment> Attachments { get; set; } = new List<EmailAttachment>();
}