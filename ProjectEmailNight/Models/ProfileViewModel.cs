using System.ComponentModel.DataAnnotations;

namespace ProjectEmailNight.Models;

public class ProfileViewModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string FullName => $"{Name} {Surname}";
    public string Initials => $"{Name?[0]}{Surname?[0]}";
    public string Email { get; set; }
    public string? UserName { get; set; }
    public string? About { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    
    
    public int SentEmailsCount { get; set; }
    public int ReceivedEmailsCount { get; set; }
    public int StarredCount { get; set; }
    public double ReplyRate { get; set; } 
    
    
    public List<ActivityItem> RecentActivities { get; set; } = new();
}

public class ActivityItem
{
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Icon { get; set; }
    public string IconColor { get; set; }
    
    public string TimeAgo
    {
        get
        {
            var diff = DateTime.UtcNow - CreatedAt;
            if (diff.TotalMinutes < 1) return "Şimdi";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} dk önce";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} saat önce";
            if (diff.TotalDays < 2) return "Dün";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} gün önce";
            return CreatedAt.ToString("dd MMM");
        }
    }
}

public class EditProfileViewModel
{
    [Required(ErrorMessage = "Ad gereklidir")]
    [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir")]
    [Display(Name = "Ad")]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "Soyad gereklidir")]
    [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir")]
    [Display(Name = "Soyad")]
    public string Surname { get; set; }
    
    [StringLength(500, ErrorMessage = "Hakkımda en fazla 500 karakter olabilir")]
    [Display(Name = "Hakkımda")]
    public string? About { get; set; }
    
    public IFormFile? ProfileImage { get; set; }
}

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Mevcut şifre gereklidir")]
    [DataType(DataType.Password)]
    [Display(Name = "Mevcut Şifre")]
    public string CurrentPassword { get; set; }
    
    [Required(ErrorMessage = "Yeni şifre gereklidir")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
    [DataType(DataType.Password)]
    [Display(Name = "Yeni Şifre")]
    public string NewPassword { get; set; }
    
    [Required(ErrorMessage = "Şifre tekrarı gereklidir")]
    [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor")]
    [DataType(DataType.Password)]
    [Display(Name = "Yeni Şifre (Tekrar)")]
    public string ConfirmPassword { get; set; }
}