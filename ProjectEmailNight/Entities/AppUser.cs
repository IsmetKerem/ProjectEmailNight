using Microsoft.AspNetCore.Identity;

namespace ProjectEmailNight.Entities;

public class AppUser : IdentityUser
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string? ImageUrl { get; set; }
    public string? About { get; set; }
    
    // Email istatistikleri için
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsOnline { get; set; } = false;
    
    // Navigation Properties
    public virtual ICollection<Email> SentEmails { get; set; } = new List<Email>();
    public virtual ICollection<Email> ReceivedEmails { get; set; } = new List<Email>();
    public virtual ICollection<EmailCategory> Categories { get; set; } = new List<EmailCategory>();
}