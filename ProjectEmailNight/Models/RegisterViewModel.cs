using System.ComponentModel.DataAnnotations;

namespace ProjectEmailNight.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Ad gereklidir")]
    [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir")]
    [Display(Name = "Ad")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Soyad gereklidir")]
    [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir")]
    [Display(Name = "Soyad")]
    public string Surname { get; set; }

    [Required(ErrorMessage = "E-posta gereklidir")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
    [Display(Name = "E-posta")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Kullanıcı adı gereklidir")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3-30 karakter arasında olmalıdır")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Sadece harf, rakam ve alt çizgi")]
    [Display(Name = "Kullanıcı Adı")]
    public string UserName { get; set; }

    [Required(ErrorMessage = "Şifre gereklidir")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; }

    [Required(ErrorMessage = "Şifre tekrarı gereklidir")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor")]
    [Display(Name = "Şifre Tekrar")]
    public string ConfirmPassword { get; set; }

    [Display(Name = "Kullanım Şartları")]
    public bool AcceptTerms { get; set; }
}