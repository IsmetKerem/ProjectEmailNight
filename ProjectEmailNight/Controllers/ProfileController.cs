using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectEmailNight.Context;
using ProjectEmailNight.Entities;
using ProjectEmailNight.Models;

namespace ProjectEmailNight.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly EmailContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IWebHostEnvironment _environment;

    public ProfileController(
        EmailContext context, 
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IWebHostEnvironment environment)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
        _environment = environment;
    }

    private async Task SetCommonViewBagAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        ViewBag.CurrentUser = user;
        ViewBag.UnreadCount = await _context.Emails.CountAsync(e => 
            e.ReceiverId == userId && !e.IsRead && !e.IsDeleted && !e.ReceiverDeleted && !e.IsDraft);
    }

    // GET: /Profile
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        await SetCommonViewBagAsync(user.Id);

        // İstatistikleri hesapla
        var sentCount = await _context.Emails.CountAsync(e => e.SenderId == user.Id && !e.IsDraft && !e.IsDeleted);
        var receivedCount = await _context.Emails.CountAsync(e => e.ReceiverId == user.Id && !e.IsDraft && !e.IsDeleted);
        var starredCount = await _context.Emails.CountAsync(e => (e.SenderId == user.Id || e.ReceiverId == user.Id) && e.IsStarred && !e.IsDeleted);
        
        // Cevaplanma oranı (alınan ve okunan emaillerin yüzdesi)
        var readCount = await _context.Emails.CountAsync(e => e.ReceiverId == user.Id && e.IsRead && !e.IsDeleted);
        var replyRate = receivedCount > 0 ? Math.Round((double)readCount / receivedCount * 100) : 0;

        // Son aktiviteler
        var recentEmails = await _context.Emails
            .Include(e => e.Receiver)
            .Include(e => e.Sender)
            .Where(e => (e.SenderId == user.Id || e.ReceiverId == user.Id) && !e.IsDeleted)
            .OrderByDescending(e => e.CreatedAt)
            .Take(5)
            .ToListAsync();

        var activities = recentEmails.Select(e => new ActivityItem
        {
            Description = e.SenderId == user.Id 
                ? $"{e.Receiver?.Name} {e.Receiver?.Surname} adlı kişiye e-posta gönderildi"
                : $"{e.Sender?.Name} {e.Sender?.Surname} adlı kişiden e-posta alındı",
            CreatedAt = e.CreatedAt,
            Icon = e.SenderId == user.Id ? "fa-paper-plane" : "fa-envelope",
            IconColor = e.SenderId == user.Id ? "#10b981" : "#3b82f6"
        }).ToList();

        var viewModel = new ProfileViewModel
        {
            Id = user.Id,
            Name = user.Name,
            Surname = user.Surname,
            Email = user.Email ?? "",
            UserName = user.UserName,
            About = user.About,
            ImageUrl = user.ImageUrl,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            SentEmailsCount = sentCount,
            ReceivedEmailsCount = receivedCount,
            StarredCount = starredCount,
            ReplyRate = replyRate,
            RecentActivities = activities
        };

        ViewData["Title"] = "Profil";
        ViewData["PageTitle"] = "Profil ve hesap ayarları";

        return View(viewModel);
    }

    // GET: /Profile/Edit
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        await SetCommonViewBagAsync(user.Id);

        var viewModel = new EditProfileViewModel
        {
            Name = user.Name,
            Surname = user.Surname,
            About = user.About
        };

        ViewData["Title"] = "Profili Düzenle";
        ViewData["PageTitle"] = "Profili Düzenle";

        return View(viewModel);
    }

    // POST: /Profile/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        await SetCommonViewBagAsync(user.Id);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        user.Name = model.Name;
        user.Surname = model.Surname;
        user.About = model.About;

        // Profil fotoğrafı yükleme
        if (model.ProfileImage != null && model.ProfileImage.Length > 0)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Eski fotoğrafı sil
            if (!string.IsNullOrEmpty(user.ImageUrl))
            {
                var oldPath = Path.Combine(_environment.WebRootPath, user.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            var fileName = $"{user.Id}_{Guid.NewGuid()}{Path.GetExtension(model.ProfileImage.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.ProfileImage.CopyToAsync(stream);
            }

            user.ImageUrl = $"/uploads/profiles/{fileName}";
        }

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            TempData["Success"] = "Profil başarıyla güncellendi";
            return RedirectToAction("Index");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View(model);
    }

    // GET: /Profile/ChangePassword
    public async Task<IActionResult> ChangePassword()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        await SetCommonViewBagAsync(user.Id);

        ViewData["Title"] = "Şifre Değiştir";
        ViewData["PageTitle"] = "Şifre Değiştir";

        return View(new ChangePasswordViewModel());
    }

    // POST: /Profile/ChangePassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        await SetCommonViewBagAsync(user.Id);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        
        if (result.Succeeded)
        {
            await _signInManager.RefreshSignInAsync(user);
            TempData["Success"] = "Şifreniz başarıyla değiştirildi";
            return RedirectToAction("Index");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View(model);
    }

    // GET: /Profile/Settings
    public async Task<IActionResult> Settings()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        await SetCommonViewBagAsync(user.Id);

        ViewData["Title"] = "Ayarlar";
        ViewData["PageTitle"] = "Hesap Ayarları";

        return View();
    }
}