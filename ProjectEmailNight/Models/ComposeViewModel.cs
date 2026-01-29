using System.ComponentModel.DataAnnotations;

namespace ProjectEmailNight.Models;

public class ComposeViewModel
{
    public int? Id { get; set; } 
    
    [Required(ErrorMessage = "Alıcı gereklidir")]
    [Display(Name = "Alıcı")]
    public string ReceiverEmail { get; set; }
    
    [Required(ErrorMessage = "Konu gereklidir")]
    [StringLength(200, ErrorMessage = "Konu en fazla 200 karakter olabilir")]
    [Display(Name = "Konu")]
    public string Subject { get; set; }
    
    [Display(Name = "İçerik")]
    public string Body { get; set; }
    
    public int? CategoryId { get; set; }
    
    public bool IsDraft { get; set; }
    
    // Reply için
    public int? ReplyToId { get; set; }
    public string ReplyToSubject { get; set; }
    public string ReplyToSender { get; set; }
}

public class EmailDetailViewModel
{
    public int Id { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public string AISummary { get; set; }
    
    // Gönderen bilgileri
    public string SenderId { get; set; }
    public string SenderName { get; set; }
    public string SenderEmail { get; set; }
    public string SenderInitials { get; set; }
    
    // Alıcı bilgileri
    public string ReceiverId { get; set; }
    public string ReceiverName { get; set; }
    public string ReceiverEmail { get; set; }
    
    // Durum
    public bool IsRead { get; set; }
    public bool IsStarred { get; set; }
    public bool IsMine { get; set; } // Ben mi gönderdim
    
    // Tarihler
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }
    
    // Kategori
    public int? CategoryId { get; set; }
    public string CategoryName { get; set; }
    public string CategoryColor { get; set; }
    
    // Ekler
    public List<AttachmentDto> Attachments { get; set; } = new();
}

public class AttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string ContentType { get; set; }
    public long FileSize { get; set; }
    public string FileSizeFormatted => FormatFileSize(FileSize);
    
    private string FormatFileSize(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
        return $"{bytes / (1024.0 * 1024.0):F1} MB";
    }
}