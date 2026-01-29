using System.ComponentModel.DataAnnotations;

namespace ProjectEmailNight.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "E-posta veya kullanıcı adı gereklidir")]
    [Display(Name = "E-posta veya Kullanıcı Adı")]
    public string EmailOrUsername { get; set; }

    [Required(ErrorMessage = "Şifre gereklidir")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; }

    [Display(Name = "Beni Hatırla")]
    public bool RememberMe { get; set; }
}