namespace ProjectEmailNight.Entities;

public class Email
{
    public int Id { get; set; }
    
    
    public string SenderId { get; set; }
    public virtual AppUser Sender { get; set; }
    
    
    public string ReceiverId { get; set; }
    public virtual AppUser Receiver { get; set; }
    
   
    public string Subject { get; set; }
    public string Body { get; set; }  // HTML içerik (text editor'den)
    
    
    public string? AISummary { get; set; }
    
    
    public bool IsRead { get; set; } = false;
    public bool IsStarred { get; set; } = false;
    public bool IsDraft { get; set; } = false;
    public bool IsDeleted { get; set; } = false;
    public bool SenderDeleted { get; set; } = false;  // Gönderen sildi mi
    public bool ReceiverDeleted { get; set; } = false; // Alıcı sildi mi
    
   
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
    public DateTime? ScheduledAt { get; set; }  // Zamanlanmış email için
    
    
    public int? CategoryId { get; set; }
    public virtual EmailCategory? Category { get; set; }
    
  
    public virtual ICollection<EmailAttachment> Attachments { get; set; } = new List<EmailAttachment>();
}