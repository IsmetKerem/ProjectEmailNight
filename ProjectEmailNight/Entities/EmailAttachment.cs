namespace ProjectEmailNight.Entities;

public class EmailAttachment
{
    public int Id { get; set; }
    public int EmailId { get; set; }
    public virtual Email Email { get; set; }
    
    public string FileName { get; set; }      
    public string StoredFileName { get; set; }
    public string FilePath { get; set; }
    public string ContentType { get; set; }   
    public long FileSize { get; set; }        
    
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}