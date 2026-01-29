using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectEmailNight.Entities;

namespace ProjectEmailNight.Context;

public class EmailContext : IdentityDbContext<AppUser>
{
    public EmailContext(DbContextOptions<EmailContext> options) : base(options)
    {
    }

    public DbSet<Email> Emails { get; set; }
    public DbSet<EmailCategory> EmailCategories { get; set; }
    public DbSet<EmailAttachment> EmailAttachments { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Email - Sender ilişkisi
        builder.Entity<Email>()
            .HasOne(e => e.Sender)
            .WithMany(u => u.SentEmails)
            .HasForeignKey(e => e.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Email - Receiver ilişkisi
        builder.Entity<Email>()
            .HasOne(e => e.Receiver)
            .WithMany(u => u.ReceivedEmails)
            .HasForeignKey(e => e.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        // Email - Category ilişkisi
        builder.Entity<Email>()
            .HasOne(e => e.Category)
            .WithMany(c => c.Emails)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // EmailCategory - User ilişkisi
        builder.Entity<EmailCategory>()
            .HasOne(c => c.User)
            .WithMany(u => u.Categories)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // EmailAttachment - Email ilişkisi
        builder.Entity<EmailAttachment>()
            .HasOne(a => a.Email)
            .WithMany(e => e.Attachments)
            .HasForeignKey(a => a.EmailId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexler
        builder.Entity<Email>().HasIndex(e => e.SenderId);
        builder.Entity<Email>().HasIndex(e => e.ReceiverId);
        builder.Entity<Email>().HasIndex(e => e.CreatedAt);
        builder.Entity<Email>().HasIndex(e => e.IsRead);

        // Varsayılan kategoriler
        builder.Entity<EmailCategory>().HasData(
            new EmailCategory { Id = 1, Name = "Birincil", Color = "#4285F4", Icon = "fa-inbox", IsSystem = true },
            new EmailCategory { Id = 2, Name = "Sosyal", Color = "#34A853", Icon = "fa-users", IsSystem = true },
            new EmailCategory { Id = 3, Name = "Promosyon", Color = "#FBBC05", Icon = "fa-tag", IsSystem = true },
            new EmailCategory { Id = 4, Name = "İş", Color = "#EA4335", Icon = "fa-briefcase", IsSystem = true },
            new EmailCategory { Id = 5, Name = "Spam", Color = "#9E9E9E", Icon = "fa-ban", IsSystem = true }
        );
    }
}