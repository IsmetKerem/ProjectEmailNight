namespace ProjectEmailNight.Entities;

public class EmailCategory
{
    public int Id { get; set; }
    public string Name { get; set; }  
    public string? Color { get; set; }  
    public string? Icon { get; set; }   
    public bool IsSystem { get; set; } = false; 
    public bool IsAIGenerated { get; set; } = false; 
    
   
    public string? UserId { get; set; }
    public virtual AppUser? User { get; set; }
    
    
    public virtual ICollection<Email> Emails { get; set; } = new List<Email>();
}