using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectEmailNight.Context;
using ProjectEmailNight.Entities;
using ProjectEmailNight.Models;
using ProjectEmailNight.Services;

namespace ProjectEmailNight.Controllers;

[Authorize]
public class EmailController : Controller
{
    private readonly EmailContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IWebHostEnvironment _environment;

    public EmailController(EmailContext context, UserManager<AppUser> userManager, IEmailService emailService, IWebHostEnvironment environment)
    {
        _context = context;
        _userManager = userManager;
        _emailService = emailService;
        _environment = environment;
    }

    private async Task SetCommonViewBagAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        ViewBag.CurrentUser = user;
        ViewBag.UnreadCount = await _context.Emails.CountAsync(e => e.ReceiverId == userId && !e.IsRead && !e.IsDeleted && !e.ReceiverDeleted && !e.IsDraft);
        ViewBag.StarredCount = await _context.Emails.CountAsync(e => (e.ReceiverId == userId || e.SenderId == userId) && e.IsStarred && !e.IsDeleted && !e.SenderDeleted && !e.ReceiverDeleted);
        ViewBag.DraftCount = await _context.Emails.CountAsync(e => e.SenderId == userId && e.IsDraft && !e.IsDeleted);
        ViewBag.TotalInbox = await _context.Emails.CountAsync(e => e.ReceiverId == userId && !e.IsDeleted && !e.ReceiverDeleted && !e.IsDraft);
        ViewBag.TrashCount = await _context.Emails.CountAsync(e => 
            ((e.ReceiverId == userId && e.ReceiverDeleted) || (e.SenderId == userId && e.SenderDeleted)) && !e.IsDeleted);
        ViewBag.ScheduledCount = await _context.Emails.CountAsync(e => 
            e.SenderId == userId && e.IsScheduled && !e.ScheduleSent && !e.IsDeleted);
    }

    // GET: /Email/Inbox
    public async Task<IActionResult> Inbox(int page = 1)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        await SetCommonViewBagAsync(user.Id);

        var query = _context.Emails
            .Include(e => e.Sender)
            .Include(e => e.Category)
            .Include(e => e.Attachments)
            .Where(e => e.ReceiverId == user.Id && !e.IsDeleted && !e.ReceiverDeleted && !e.IsDraft)
            .OrderByDescending(e => e.CreatedAt);

        var totalCount = await query.CountAsync();
        var pageSize = 25;

        var emails = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EmailItemDto
            {
                Id = e.Id,
                SenderId = e.SenderId,
                SenderName = e.Sender.Name + " " + e.Sender.Surname,
                SenderEmail = e.Sender.Email,
                SenderInitials = e.Sender.Name.Substring(0, 1) + e.Sender.Surname.Substring(0, 1),
                Subject = e.Subject,
                Preview = e.Body.Length > 80 ? e.Body.Substring(0, 80) + "..." : e.Body,
                CreatedAt = e.CreatedAt,
                IsRead = e.IsRead,
                IsStarred = e.IsStarred,
                CategoryId = e.CategoryId,
                CategoryName = e.Category != null ? e.Category.Name : null,
                CategoryColor = e.Category != null ? e.Category.Color : null,
                HasAttachment = e.Attachments.Any()
            })
            .ToListAsync();

        var viewModel = new EmailListViewModel
        {
            Emails = emails,
            TotalCount = totalCount,
            UnreadCount = (int)ViewBag.UnreadCount,
            StarredCount = (int)ViewBag.StarredCount,
            DraftCount = (int)ViewBag.DraftCount,
            CurrentPage = page,
            PageSize = pageSize,
            CurrentFolder = "inbox"
        };

        ViewData["Title"] = "Gelen Kutusu";
        ViewData["PageTitle"] = "Gelen Kutusu";

        return View(viewModel);
    }

    // GET: /Email/Sent
    public async Task<IActionResult> Sent(int page = 1)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        await SetCommonViewBagAsync(user.Id);

        var query = _context.Emails
            .Include(e => e.Receiver)
            .Include(e => e.Category)
            .Include(e => e.Attachments)
            .Where(e => e.SenderId == user.Id && !e.IsDeleted && !e.SenderDeleted && !e.IsDraft)
            .OrderByDescending(e => e.CreatedAt);

        var totalCount = await query.CountAsync();
        var pageSize = 25;

        var emails = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EmailItemDto
            {
                Id = e.Id,
                SenderName = "Ben",
                ReceiverName = e.Receiver.Name + " " + e.Receiver.Surname,
                SenderInitials = e.Receiver.Name.Substring(0, 1) + e.Receiver.Surname.Substring(0, 1),
                Subject = e.Subject,
                Preview = e.Body.Length > 80 ? e.Body.Substring(0, 80) + "..." : e.Body,
                CreatedAt = e.CreatedAt,
                IsRead = e.IsRead,
                IsStarred = e.IsStarred,
                CategoryId = e.CategoryId,
                CategoryName = e.Category != null ? e.Category.Name : null,
                CategoryColor = e.Category != null ? e.Category.Color : null,
                HasAttachment = e.Attachments.Any()
            })
            .ToListAsync();

        var viewModel = new EmailListViewModel
        {
            Emails = emails,
            TotalCount = totalCount,
            UnreadCount = (int)ViewBag.UnreadCount,
            StarredCount = (int)ViewBag.StarredCount,
            DraftCount = (int)ViewBag.DraftCount,
            CurrentPage = page,
            PageSize = pageSize,
            CurrentFolder = "sent"
        };

        ViewData["Title"] = "Gönderilenler";
        ViewData["PageTitle"] = "Gönderilenler";

        return View("Inbox", viewModel);
    }

    // GET: /Email/Starred
    public async Task<IActionResult> Starred(int page = 1)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        await SetCommonViewBagAsync(user.Id);

        var query = _context.Emails
            .Include(e => e.Sender)
            .Include(e => e.Receiver)
            .Include(e => e.Category)
            .Where(e => (e.ReceiverId == user.Id || e.SenderId == user.Id) && 
                        e.IsStarred && 
                        !e.IsDeleted &&
                        !e.SenderDeleted &&
                        !e.ReceiverDeleted)
            .OrderByDescending(e => e.CreatedAt);

        var totalCount = await query.CountAsync();
        var pageSize = 25;

        var emails = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EmailItemDto
            {
                Id = e.Id,
                SenderId = e.SenderId,
                SenderName = e.SenderId == user.Id ? "Ben" : e.Sender.Name + " " + e.Sender.Surname,
                SenderInitials = e.SenderId == user.Id 
                    ? e.Receiver.Name.Substring(0, 1) + e.Receiver.Surname.Substring(0, 1)
                    : e.Sender.Name.Substring(0, 1) + e.Sender.Surname.Substring(0, 1),
                Subject = e.Subject,
                Preview = e.Body.Length > 80 ? e.Body.Substring(0, 80) + "..." : e.Body,
                CreatedAt = e.CreatedAt,
                IsRead = e.IsRead,
                IsStarred = e.IsStarred,
                CategoryId = e.CategoryId,
                CategoryName = e.Category != null ? e.Category.Name : null,
                CategoryColor = e.Category != null ? e.Category.Color : null
            })
            .ToListAsync();

        var viewModel = new EmailListViewModel
        {
            Emails = emails,
            TotalCount = totalCount,
            UnreadCount = (int)ViewBag.UnreadCount,
            StarredCount = (int)ViewBag.StarredCount,
            DraftCount = (int)ViewBag.DraftCount,
            CurrentPage = page,
            PageSize = pageSize,
            CurrentFolder = "starred"
        };

        ViewData["Title"] = "Yıldızlı";
        ViewData["PageTitle"] = "Yıldızlı";

        return View("Inbox", viewModel);
    }

    // GET: /Email/Drafts
    public async Task<IActionResult> Drafts(int page = 1)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        await SetCommonViewBagAsync(user.Id);

        var query = _context.Emails
            .Include(e => e.Receiver)
            .Include(e => e.Category)
            .Where(e => e.SenderId == user.Id && e.IsDraft && !e.IsDeleted)
            .OrderByDescending(e => e.CreatedAt);

        var totalCount = await query.CountAsync();
        var pageSize = 25;

        var emails = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EmailItemDto
            {
                Id = e.Id,
                SenderName = "Taslak",
                ReceiverName = e.Receiver != null ? e.Receiver.Name + " " + e.Receiver.Surname : "Alıcı yok",
                SenderInitials = "TS",
                Subject = string.IsNullOrEmpty(e.Subject) ? "(Konu yok)" : e.Subject,
                Preview = e.Body.Length > 80 ? e.Body.Substring(0, 80) + "..." : e.Body,
                CreatedAt = e.CreatedAt,
                IsRead = true,
                IsStarred = e.IsStarred,
                IsDraft = true,
                CategoryId = e.CategoryId,
                CategoryName = e.Category != null ? e.Category.Name : null,
                CategoryColor = e.Category != null ? e.Category.Color : null
            })
            .ToListAsync();

        var viewModel = new EmailListViewModel
        {
            Emails = emails,
            TotalCount = totalCount,
            UnreadCount = (int)ViewBag.UnreadCount,
            StarredCount = (int)ViewBag.StarredCount,
            DraftCount = (int)ViewBag.DraftCount,
            CurrentPage = page,
            PageSize = pageSize,
            CurrentFolder = "drafts"
        };

        ViewData["Title"] = "Taslaklar";
        ViewData["PageTitle"] = "Taslaklar";

        return View("Inbox", viewModel);
    }

    // GET: /Email/Trash
    public async Task<IActionResult> Trash(int page = 1)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        await SetCommonViewBagAsync(user.Id);

        var query = _context.Emails
            .Include(e => e.Sender)
            .Include(e => e.Receiver)
            .Include(e => e.Category)
            .Include(e => e.Attachments)
            .Where(e => ((e.ReceiverId == user.Id && e.ReceiverDeleted) || 
                        (e.SenderId == user.Id && e.SenderDeleted)) && 
                        !e.IsDeleted)
            .OrderByDescending(e => e.CreatedAt);

        var totalCount = await query.CountAsync();
        var pageSize = 25;

        var emails = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EmailItemDto
            {
                Id = e.Id,
                SenderId = e.SenderId,
                SenderName = e.SenderId == user.Id ? "Ben" : e.Sender.Name + " " + e.Sender.Surname,
                SenderEmail = e.Sender.Email,
                SenderInitials = e.SenderId == user.Id 
                    ? (e.Receiver != null ? e.Receiver.Name.Substring(0, 1) + e.Receiver.Surname.Substring(0, 1) : "??")
                    : e.Sender.Name.Substring(0, 1) + e.Sender.Surname.Substring(0, 1),
                ReceiverName = e.Receiver != null ? e.Receiver.Name + " " + e.Receiver.Surname : "",
                Subject = e.Subject,
                Preview = e.Body.Length > 80 ? e.Body.Substring(0, 80) + "..." : e.Body,
                CreatedAt = e.CreatedAt,
                IsRead = e.IsRead,
                IsStarred = e.IsStarred,
                CategoryId = e.CategoryId,
                CategoryName = e.Category != null ? e.Category.Name : null,
                CategoryColor = e.Category != null ? e.Category.Color : null,
                HasAttachment = e.Attachments.Any()
            })
            .ToListAsync();

        var viewModel = new EmailListViewModel
        {
            Emails = emails,
            TotalCount = totalCount,
            UnreadCount = (int)ViewBag.UnreadCount,
            StarredCount = (int)ViewBag.StarredCount,
            DraftCount = (int)ViewBag.DraftCount,
            CurrentPage = page,
            PageSize = pageSize,
            CurrentFolder = "trash"
        };

        ViewData["Title"] = "Çöp Kutusu";
        ViewData["PageTitle"] = "Çöp Kutusu";

        return View("Inbox", viewModel);
    }

    // POST: /Email/RestoreFromTrash
    [HttpPost]
    public async Task<IActionResult> RestoreFromTrash(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(new { success = false });

        var email = await _context.Emails.FindAsync(id);
        if (email == null) return Json(new { success = false });

        if (email.ReceiverId == user.Id)
            email.ReceiverDeleted = false;
        
        if (email.SenderId == user.Id)
            email.SenderDeleted = false;

        await _context.SaveChangesAsync();
        
        return Json(new { success = true });
    }

    // POST: /Email/PermanentDelete
    [HttpPost]
    public async Task<IActionResult> PermanentDelete(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(new { success = false });

        var email = await _context.Emails
            .Include(e => e.Attachments)
            .FirstOrDefaultAsync(e => e.Id == id);
            
        if (email == null) return Json(new { success = false });

        if (email.ReceiverId != user.Id && email.SenderId != user.Id)
            return Json(new { success = false });

        // Ekleri fiziksel olarak sil
        if (email.Attachments.Any())
        {
            foreach (var att in email.Attachments)
            {
                var filePath = Path.Combine(_environment.WebRootPath, att.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            _context.EmailAttachments.RemoveRange(email.Attachments);
        }

        _context.Emails.Remove(email);
        await _context.SaveChangesAsync();
        
        return Json(new { success = true });
    }

    // POST: /Email/EmptyTrash
    [HttpPost]
    public async Task<IActionResult> EmptyTrash()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(new { success = false });

        var trashEmails = await _context.Emails
            .Include(e => e.Attachments)
            .Where(e => ((e.ReceiverId == user.Id && e.ReceiverDeleted) || 
                        (e.SenderId == user.Id && e.SenderDeleted)) && 
                        !e.IsDeleted)
            .ToListAsync();

        foreach (var email in trashEmails)
        {
            if (email.Attachments.Any())
            {
                foreach (var att in email.Attachments)
                {
                    var filePath = Path.Combine(_environment.WebRootPath, att.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                _context.EmailAttachments.RemoveRange(email.Attachments);
            }
            
            _context.Emails.Remove(email);
        }

        await _context.SaveChangesAsync();
        
        return Json(new { success = true, count = trashEmails.Count });
    }

    // GET: /Email/Compose
    public async Task<IActionResult> Compose(int? replyTo = null, int? forwardId = null)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        await SetCommonViewBagAsync(user.Id);

        var viewModel = new ComposeViewModel();

        // Yanıtlama
        if (replyTo.HasValue)
        {
            var originalEmail = await _context.Emails
                .Include(e => e.Sender)
                .FirstOrDefaultAsync(e => e.Id == replyTo.Value);

            if (originalEmail != null)
            {
                viewModel.ReceiverEmail = originalEmail.Sender.Email;
                viewModel.Subject = originalEmail.Subject.StartsWith("RE:") 
                    ? originalEmail.Subject 
                    : $"RE: {originalEmail.Subject}";
                viewModel.ReplyToId = originalEmail.Id;
                viewModel.ReplyToSender = $"{originalEmail.Sender.Name} {originalEmail.Sender.Surname}";
                viewModel.ReplyToSubject = originalEmail.Subject;
                
                viewModel.Body = $"<br><br><div style=\"padding-left: 1rem; border-left: 3px solid #6b7280; color: #9ca3af; margin-top: 1rem;\">" +
                    $"<p style=\"margin: 0 0 0.5rem 0;\"><strong>{originalEmail.Sender.Name} {originalEmail.Sender.Surname}</strong> yazdı:</p>" +
                    $"{originalEmail.Body}</div>";
            }
        }

        // İletme
        if (forwardId.HasValue)
        {
            var originalEmail = await _context.Emails
                .Include(e => e.Sender)
                .Include(e => e.Receiver)
                .FirstOrDefaultAsync(e => e.Id == forwardId.Value);

            if (originalEmail != null && (originalEmail.ReceiverId == user.Id || originalEmail.SenderId == user.Id))
            {
                viewModel.Subject = originalEmail.Subject.StartsWith("FW:") 
                    ? originalEmail.Subject 
                    : $"FW: {originalEmail.Subject}";
                    
                viewModel.Body = $"<br><br><hr style=\"border: none; border-top: 1px solid #374151; margin: 1rem 0;\">" +
                    $"<p style=\"color: #9ca3af; margin: 0 0 1rem 0;\"><strong>---------- İletilen ileti ---------</strong><br>" +
                    $"<strong>Kimden:</strong> {originalEmail.Sender.Name} {originalEmail.Sender.Surname} &lt;{originalEmail.Sender.Email}&gt;<br>" +
                    $"<strong>Tarih:</strong> {originalEmail.CreatedAt:dd MMM yyyy HH:mm}<br>" +
                    $"<strong>Konu:</strong> {originalEmail.Subject}<br>" +
                    $"<strong>Kime:</strong> {originalEmail.Receiver?.Email ?? "Bilinmiyor"}</p>" +
                    $"{originalEmail.Body}";
            }
        }

        ViewData["Title"] = "Yeni E-posta";
        ViewData["PageTitle"] = "Yeni mail oluştur";

        ViewBag.Users = await _context.Users
            .Where(u => u.Id != user.Id)
            .Select(u => new { u.Id, u.Email, FullName = u.Name + " " + u.Surname })
            .ToListAsync();

        return View(viewModel);
    }

    // POST: /Email/Compose
    // POST: /Email/Compose
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Compose(ComposeViewModel model, List<IFormFile>? attachments)
{
    var user = await _userManager.GetUserAsync(User);
    if (user == null) return RedirectToAction("Login", "Account");

    await SetCommonViewBagAsync(user.Id);

    // Taslak kaydetme
    if (model.IsDraft)
    {
        try
        {
            await _emailService.SaveDraftAsync(user.Id, model.ReceiverEmail, model.Subject ?? "", model.Body ?? "", model.Id);
            TempData["Success"] = "Taslak kaydedildi";
            return RedirectToAction("Drafts");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            ViewBag.Users = await GetUsersExceptCurrentAsync(user.Id);
            return View(model);
        }
    }

    // Validasyon
    if (string.IsNullOrEmpty(model.ReceiverEmail))
    {
        ModelState.AddModelError(nameof(model.ReceiverEmail), "Alıcı gereklidir");
        ViewBag.Users = await GetUsersExceptCurrentAsync(user.Id);
        return View(model);
    }

    if (string.IsNullOrEmpty(model.Subject))
    {
        ModelState.AddModelError(nameof(model.Subject), "Konu gereklidir");
        ViewBag.Users = await GetUsersExceptCurrentAsync(user.Id);
        return View(model);
    }

    // Zamanlanmış gönderim
    if (model.IsScheduled && model.ScheduledAt.HasValue)
    {
        try
        {
            var scheduledService = HttpContext.RequestServices.GetRequiredService<IScheduledEmailService>();
            await scheduledService.ScheduleEmailAsync(
                user.Id, 
                model.ReceiverEmail, 
                model.Subject, 
                model.Body ?? "", 
                model.ScheduledAt.Value
            );
            TempData["Success"] = $"E-posta {model.ScheduledAt.Value:dd MMM yyyy HH:mm} tarihinde gönderilmek üzere zamanlandı";
            return RedirectToAction("Scheduled");
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(nameof(model.ReceiverEmail), ex.Message);
            ViewBag.Users = await GetUsersExceptCurrentAsync(user.Id);
            return View(model);
        }
    }

    // Normal gönderim
    try
    {
        await _emailService.SendEmailAsync(user.Id, model.ReceiverEmail, model.Subject, model.Body ?? "", attachments);
        TempData["Success"] = "E-posta başarıyla gönderildi";
        return RedirectToAction("Sent");
    }
    catch (ArgumentException ex)
    {
        ModelState.AddModelError(nameof(model.ReceiverEmail), ex.Message);
        ViewBag.Users = await GetUsersExceptCurrentAsync(user.Id);
        return View(model);
    }
}

    // GET: /Email/Detail/5
    public async Task<IActionResult> Detail(int id, string? from = null)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        await SetCommonViewBagAsync(user.Id);

        var viewModel = await _emailService.GetEmailDetailAsync(id, user.Id);
        
        if (viewModel == null)
            return NotFound();

        // Okundu olarak işaretle
        if (!viewModel.IsMine && !viewModel.IsRead)
        {
            var email = await _context.Emails.FindAsync(id);
            if (email != null)
            {
                email.IsRead = true;
                email.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                viewModel.IsRead = true;
                viewModel.ReadAt = DateTime.UtcNow;
            }
        }

        // Çöp kutusundan mı geldi?
        var emailEntity = await _context.Emails.FindAsync(id);
        ViewBag.IsInTrash = (emailEntity?.ReceiverDeleted == true && emailEntity.ReceiverId == user.Id) ||
                           (emailEntity?.SenderDeleted == true && emailEntity.SenderId == user.Id);
        ViewBag.FromFolder = from;

        ViewData["Title"] = viewModel.Subject;
        ViewData["PageTitle"] = "Mail detayı";

        return View(viewModel);
    }

    // GET: /Email/DownloadAttachment/5
    public async Task<IActionResult> DownloadAttachment(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var attachment = await _context.EmailAttachments
            .Include(a => a.Email)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (attachment == null)
            return NotFound();

        if (attachment.Email.SenderId != user.Id && attachment.Email.ReceiverId != user.Id)
            return Forbid();

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", attachment.FilePath.TrimStart('/'));
        
        if (!System.IO.File.Exists(filePath))
            return NotFound();

        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        return File(fileBytes, attachment.ContentType, attachment.FileName);
    }

    // POST: /Email/ToggleStar
    [HttpPost]
    public async Task<IActionResult> ToggleStar(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var email = await _context.Emails.FindAsync(id);
        if (email == null) return NotFound();

        if (email.SenderId != user.Id && email.ReceiverId != user.Id)
            return Forbid();

        email.IsStarred = !email.IsStarred;
        await _context.SaveChangesAsync();

        return Json(new { success = true, isStarred = email.IsStarred });
    }

    // POST: /Email/MarkAsRead
    [HttpPost]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var email = await _context.Emails.FindAsync(id);
        if (email == null) return NotFound();

        if (email.ReceiverId != user.Id)
            return Forbid();

        email.IsRead = true;
        email.ReadAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return Json(new { success = true });
    }

    // POST: /Email/Delete (Çöp kutusuna taşı)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? returnUrl = null)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var email = await _context.Emails.FindAsync(id);
        if (email == null) return NotFound();

        if (email.SenderId == user.Id)
            email.SenderDeleted = true;
    
        if (email.ReceiverId == user.Id)
            email.ReceiverDeleted = true;

        await _context.SaveChangesAsync();

        TempData["Success"] = "E-posta çöp kutusuna taşındı";
    
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        
        return RedirectToAction("Inbox");
    }

    // POST: /Email/BulkDelete
    [HttpPost]
    public async Task<IActionResult> BulkDelete([FromBody] List<int> ids)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(new { success = false });

        var emails = await _context.Emails
            .Where(e => ids.Contains(e.Id))
            .Where(e => e.ReceiverId == user.Id || e.SenderId == user.Id)
            .ToListAsync();

        foreach (var email in emails)
        {
            if (email.ReceiverId == user.Id)
                email.ReceiverDeleted = true;
            if (email.SenderId == user.Id)
                email.SenderDeleted = true;
        }

        await _context.SaveChangesAsync();
        
        return Json(new { success = true, count = emails.Count });
    }

    // POST: /Email/BulkMarkAsRead
    [HttpPost]
    public async Task<IActionResult> BulkMarkAsRead([FromBody] List<int> ids)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(new { success = false });

        var emails = await _context.Emails
            .Where(e => ids.Contains(e.Id))
            .Where(e => e.ReceiverId == user.Id)
            .ToListAsync();

        foreach (var email in emails)
        {
            email.IsRead = true;
            email.ReadAt ??= DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        
        return Json(new { success = true, count = emails.Count });
    }

    // POST: /Email/BulkStar
    [HttpPost]
    public async Task<IActionResult> BulkStar([FromBody] List<int> ids)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(new { success = false });

        var emails = await _context.Emails
            .Where(e => ids.Contains(e.Id))
            .Where(e => e.ReceiverId == user.Id || e.SenderId == user.Id)
            .ToListAsync();

        foreach (var email in emails)
        {
            email.IsStarred = true;
        }

        await _context.SaveChangesAsync();
        
        return Json(new { success = true, count = emails.Count });
    }

    // POST: /Email/BulkRestore
    [HttpPost]
    public async Task<IActionResult> BulkRestore([FromBody] List<int> ids)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(new { success = false });

        var emails = await _context.Emails
            .Where(e => ids.Contains(e.Id))
            .Where(e => e.ReceiverId == user.Id || e.SenderId == user.Id)
            .ToListAsync();

        foreach (var email in emails)
        {
            if (email.ReceiverId == user.Id)
                email.ReceiverDeleted = false;
            if (email.SenderId == user.Id)
                email.SenderDeleted = false;
        }

        await _context.SaveChangesAsync();
        
        return Json(new { success = true, count = emails.Count });
    }

    // POST: /Email/BulkPermanentDelete
    [HttpPost]
    public async Task<IActionResult> BulkPermanentDelete([FromBody] List<int> ids)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(new { success = false });

        var emails = await _context.Emails
            .Include(e => e.Attachments)
            .Where(e => ids.Contains(e.Id))
            .Where(e => e.ReceiverId == user.Id || e.SenderId == user.Id)
            .ToListAsync();

        foreach (var email in emails)
        {
            if (email.Attachments.Any())
            {
                foreach (var att in email.Attachments)
                {
                    var filePath = Path.Combine(_environment.WebRootPath, att.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                _context.EmailAttachments.RemoveRange(email.Attachments);
            }
            _context.Emails.Remove(email);
        }

        await _context.SaveChangesAsync();
        
        return Json(new { success = true, count = emails.Count });
    }

    private async Task<object> GetUsersExceptCurrentAsync(string userId)
    {
        return await _context.Users
            .Where(u => u.Id != userId)
            .Select(u => new { u.Id, u.Email, FullName = u.Name + " " + u.Surname })
            .ToListAsync();
    }

    // POST: /Email/GenerateReply
    [HttpPost]
    public async Task<IActionResult> GenerateReply(int emailId, string tone = "professional")
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var email = await _context.Emails.FindAsync(emailId);
        if (email == null) return NotFound();

        if (email.ReceiverId != user.Id)
            return Forbid();

        var aiService = HttpContext.RequestServices.GetRequiredService<IAIService>();
        var reply = await aiService.GenerateReplyAsync(email.Subject, email.Body, tone);

        return Json(new { success = true, reply });
    }

    // POST: /Email/RegenerateAnalysis
    [HttpPost]
    public async Task<IActionResult> RegenerateAnalysis(int emailId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var email = await _context.Emails
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == emailId);
        
        if (email == null) return NotFound();

        if (email.SenderId != user.Id && email.ReceiverId != user.Id)
            return Forbid();

        var aiService = HttpContext.RequestServices.GetRequiredService<IAIService>();
        var analysis = await aiService.AnalyzeEmailAsync(email.Subject, email.Body);

        email.AISummary = analysis.Summary;
        email.CategoryId = analysis.CategoryId;
        await _context.SaveChangesAsync();

        return Json(new { 
            success = true, 
            summary = analysis.Summary,
            categoryId = analysis.CategoryId,
            categoryName = analysis.CategoryName
        });
    }

    // GET: /Email/Category/1
    public async Task<IActionResult> Category(int id, int page = 1)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        await SetCommonViewBagAsync(user.Id);

        var category = await _context.EmailCategories.FindAsync(id);
        if (category == null)
            return NotFound();

        var query = _context.Emails
            .Include(e => e.Sender)
            .Include(e => e.Category)
            .Include(e => e.Attachments)
            .Where(e => e.ReceiverId == user.Id && 
                        e.CategoryId == id && 
                        !e.IsDeleted && 
                        !e.ReceiverDeleted && 
                        !e.IsDraft)
            .OrderByDescending(e => e.CreatedAt);

        var totalCount = await query.CountAsync();
        var pageSize = 25;

        var emails = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EmailItemDto
            {
                Id = e.Id,
                SenderId = e.SenderId,
                SenderName = e.Sender.Name + " " + e.Sender.Surname,
                SenderEmail = e.Sender.Email,
                SenderInitials = e.Sender.Name.Substring(0, 1) + e.Sender.Surname.Substring(0, 1),
                Subject = e.Subject,
                Preview = e.Body.Length > 80 ? e.Body.Substring(0, 80) + "..." : e.Body,
                CreatedAt = e.CreatedAt,
                IsRead = e.IsRead,
                IsStarred = e.IsStarred,
                CategoryId = e.CategoryId,
                CategoryName = e.Category != null ? e.Category.Name : null,
                CategoryColor = e.Category != null ? e.Category.Color : null,
                HasAttachment = e.Attachments.Any()
            })
            .ToListAsync();

        var viewModel = new EmailListViewModel
        {
            Emails = emails,
            TotalCount = totalCount,
            UnreadCount = (int)ViewBag.UnreadCount,
            StarredCount = (int)ViewBag.StarredCount,
            DraftCount = (int)ViewBag.DraftCount,
            CurrentPage = page,
            PageSize = pageSize,
            CurrentFolder = $"category-{id}"
        };

        ViewData["Title"] = category.Name;
        ViewData["PageTitle"] = category.Name;
        ViewBag.CurrentCategoryId = id;

        return View("Inbox", viewModel);
    }

    // GET: /Email/Search?q=aranacak
    public async Task<IActionResult> Search(string q, int page = 1)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        await SetCommonViewBagAsync(user.Id);

        if (string.IsNullOrWhiteSpace(q))
        {
            return RedirectToAction("Inbox");
        }

        var searchTerm = q.ToLower().Trim();

        var query = _context.Emails
            .Include(e => e.Sender)
            .Include(e => e.Receiver)
            .Include(e => e.Category)
            .Include(e => e.Attachments)
            .Where(e => (e.ReceiverId == user.Id || e.SenderId == user.Id) && 
                        !e.IsDeleted && 
                        !e.IsDraft &&
                        !e.SenderDeleted &&
                        !e.ReceiverDeleted &&
                        (e.Subject.ToLower().Contains(searchTerm) ||
                         e.Body.ToLower().Contains(searchTerm) ||
                         e.Sender.Name.ToLower().Contains(searchTerm) ||
                         e.Sender.Surname.ToLower().Contains(searchTerm) ||
                         e.Sender.Email.ToLower().Contains(searchTerm) ||
                         e.Receiver.Name.ToLower().Contains(searchTerm) ||
                         e.Receiver.Surname.ToLower().Contains(searchTerm) ||
                         e.Receiver.Email.ToLower().Contains(searchTerm) ||
                         (e.Category != null && e.Category.Name.ToLower().Contains(searchTerm))))
            .OrderByDescending(e => e.CreatedAt);

        var totalCount = await query.CountAsync();
        var pageSize = 25;

        var emails = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EmailItemDto
            {
                Id = e.Id,
                SenderId = e.SenderId,
                SenderName = e.SenderId == user.Id ? "Ben" : e.Sender.Name + " " + e.Sender.Surname,
                SenderEmail = e.Sender.Email,
                SenderInitials = e.SenderId == user.Id 
                    ? e.Receiver.Name.Substring(0, 1) + e.Receiver.Surname.Substring(0, 1)
                    : e.Sender.Name.Substring(0, 1) + e.Sender.Surname.Substring(0, 1),
                ReceiverName = e.Receiver.Name + " " + e.Receiver.Surname,
                Subject = e.Subject,
                Preview = e.Body.Length > 80 ? e.Body.Substring(0, 80) + "..." : e.Body,
                CreatedAt = e.CreatedAt,
                IsRead = e.IsRead,
                IsStarred = e.IsStarred,
                CategoryId = e.CategoryId,
                CategoryName = e.Category != null ? e.Category.Name : null,
                CategoryColor = e.Category != null ? e.Category.Color : null,
                HasAttachment = e.Attachments.Any()
            })
            .ToListAsync();

        var viewModel = new EmailListViewModel
        {
            Emails = emails,
            TotalCount = totalCount,
            UnreadCount = (int)ViewBag.UnreadCount,
            StarredCount = (int)ViewBag.StarredCount,
            DraftCount = (int)ViewBag.DraftCount,
            CurrentPage = page,
            PageSize = pageSize,
            CurrentFolder = "search"
        };

        ViewData["Title"] = $"Arama: {q}";
        ViewData["PageTitle"] = $"Arama sonuçları: \"{q}\"";
        ViewBag.SearchQuery = q;

        return View("Inbox", viewModel);
    }
    // GET: /Email/Scheduled
    public async Task<IActionResult> Scheduled()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        await SetCommonViewBagAsync(user.Id);

        var scheduledEmails = await _context.Emails
            .Include(e => e.Receiver)
            .Where(e => e.SenderId == user.Id && e.IsScheduled && !e.ScheduleSent && !e.IsDeleted)
            .OrderBy(e => e.ScheduledAt)
            .Select(e => new ScheduledEmailViewModel
            {
                Id = e.Id,
                ReceiverName = e.Receiver != null ? e.Receiver.Name + " " + e.Receiver.Surname : "",
                ReceiverEmail = e.Receiver != null ? e.Receiver.Email : "",
                Subject = e.Subject,
                ScheduledAt = e.ScheduledAt ?? DateTime.UtcNow,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();

        ViewData["Title"] = "Zamanlanmış";
        ViewData["PageTitle"] = "Zamanlanmış E-postalar";

        return View(scheduledEmails);
    }

// POST: /Email/CancelScheduled
    [HttpPost]
    public async Task<IActionResult> CancelScheduled(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(new { success = false });

        var scheduledService = HttpContext.RequestServices.GetRequiredService<IScheduledEmailService>();
        var result = await scheduledService.CancelScheduledEmailAsync(id, user.Id);

        return Json(new { success = result });
    }
}