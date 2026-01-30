using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectEmailNight.Context;
using ProjectEmailNight.Entities;
using ProjectEmailNight.Models;

namespace ProjectEmailNight.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly EmailContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(
        EmailContext context, 
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // GET: /Admin
    public async Task<IActionResult> Index()
    {
        var totalUsers = await _userManager.Users.CountAsync();
        var totalEmails = await _context.Emails.CountAsync();
        var totalEmailsToday = await _context.Emails.CountAsync(e => e.CreatedAt.Date == DateTime.UtcNow.Date);
        var activeUsersToday = await _context.Emails
            .Where(e => e.CreatedAt.Date == DateTime.UtcNow.Date)
            .Select(e => e.SenderId)
            .Distinct()
            .CountAsync();

        var viewModel = new AdminDashboardViewModel
        {
            TotalUsers = totalUsers,
            TotalEmails = totalEmails,
            TotalEmailsToday = totalEmailsToday,
            ActiveUsersToday = activeUsersToday,
            RecentUsers = await _userManager.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(10)
                .ToListAsync(),
            RecentEmails = await _context.Emails
                .Include(e => e.Sender)
                .Include(e => e.Receiver)
                .OrderByDescending(e => e.CreatedAt)
                .Take(10)
                .ToListAsync()
        };

        ViewData["Title"] = "Admin Panel";
        ViewData["PageTitle"] = "Admin Dashboard";

        return View(viewModel);
    }

    // GET: /Admin/Users
    public async Task<IActionResult> Users(int page = 1)
    {
        var pageSize = 20;
        var query = _userManager.Users.OrderByDescending(u => u.CreatedAt);
        
        var totalCount = await query.CountAsync();
        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var userViewModels = new List<AdminUserViewModel>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var emailCount = await _context.Emails.CountAsync(e => e.SenderId == user.Id || e.ReceiverId == user.Id);
            
            userViewModels.Add(new AdminUserViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email ?? "",
                Roles = roles.ToList(),
                EmailCount = emailCount,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                IsActive = true
            });
        }

        var viewModel = new AdminUserListViewModel
        {
            Users = userViewModels,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            TotalCount = totalCount
        };

        ViewData["Title"] = "Kullanıcılar";
        ViewData["PageTitle"] = "Kullanıcı Yönetimi";

        return View(viewModel);
    }

    // GET: /Admin/UserDetail/id
    public async Task<IActionResult> UserDetail(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        var sentEmails = await _context.Emails.CountAsync(e => e.SenderId == id && !e.IsDraft);
        var receivedEmails = await _context.Emails.CountAsync(e => e.ReceiverId == id);
        var drafts = await _context.Emails.CountAsync(e => e.SenderId == id && e.IsDraft);

        var recentEmails = await _context.Emails
            .Include(e => e.Sender)
            .Include(e => e.Receiver)
            .Where(e => e.SenderId == id || e.ReceiverId == id)
            .OrderByDescending(e => e.CreatedAt)
            .Take(10)
            .ToListAsync();

        var viewModel = new AdminUserDetailViewModel
        {
            Id = user.Id,
            Name = user.Name,
            Surname = user.Surname,
            Email = user.Email ?? "",
            Roles = roles.ToList(),
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            SentEmailsCount = sentEmails,
            ReceivedEmailsCount = receivedEmails,
            DraftsCount = drafts,
            RecentEmails = recentEmails
        };

        ViewData["Title"] = $"{user.Name} {user.Surname}";
        ViewData["PageTitle"] = "Kullanıcı Detayı";

        return View(viewModel);
    }

    // POST: /Admin/ToggleRole
    [HttpPost]
    public async Task<IActionResult> ToggleRole(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser?.Id == userId && role == "Admin")
        {
            TempData["Error"] = "Kendi Admin rolünüzü kaldıramazsınız!";
            return RedirectToAction("UserDetail", new { id = userId });
        }

        if (await _userManager.IsInRoleAsync(user, role))
        {
            await _userManager.RemoveFromRoleAsync(user, role);
            TempData["Success"] = $"{user.Name} kullanıcısından {role} rolü kaldırıldı.";
        }
        else
        {
            await _userManager.AddToRoleAsync(user, role);
            TempData["Success"] = $"{user.Name} kullanıcısına {role} rolü eklendi.";
        }

        return RedirectToAction("UserDetail", new { id = userId });
    }

    // POST: /Admin/DeleteUser
    [HttpPost]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser?.Id == userId)
        {
            TempData["Error"] = "Kendinizi silemezsiniz!";
            return RedirectToAction("Users");
        }

        var userEmails = await _context.Emails
            .Where(e => e.SenderId == userId || e.ReceiverId == userId)
            .ToListAsync();
        
        _context.Emails.RemoveRange(userEmails);
        await _context.SaveChangesAsync();

        await _userManager.DeleteAsync(user);
        
        TempData["Success"] = $"{user.Name} {user.Surname} kullanıcısı silindi.";
        return RedirectToAction("Users");
    }

    // GET: /Admin/Emails
    public async Task<IActionResult> Emails(int page = 1, string? search = null)
    {
        var pageSize = 25;
        var query = _context.Emails
            .Include(e => e.Sender)
            .Include(e => e.Receiver)
            .Include(e => e.Category)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower();
            query = query.Where(e => 
                e.Subject.ToLower().Contains(search) ||
                e.Sender.Email.ToLower().Contains(search) ||
                (e.Receiver != null && e.Receiver.Email.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync();
        var emails = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var viewModel = new AdminEmailListViewModel
        {
            Emails = emails,
            CurrentPage = page,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            TotalCount = totalCount,
            SearchQuery = search
        };

        ViewData["Title"] = "Tüm E-postalar";
        ViewData["PageTitle"] = "E-posta Yönetimi";

        return View(viewModel);
    }

    // GET: /Admin/Statistics
    public async Task<IActionResult> Statistics()
    {
        var last7Days = Enumerable.Range(0, 7)
            .Select(i => DateTime.UtcNow.Date.AddDays(-i))
            .Reverse()
            .ToList();

        var emailStats = new List<AdminDailyEmailStat>();
        foreach (var date in last7Days)
        {
            var count = await _context.Emails.CountAsync(e => e.CreatedAt.Date == date);
            emailStats.Add(new AdminDailyEmailStat
            {
                Date = date,
                Count = count
            });
        }

        var categoryStats = await _context.Emails
            .Where(e => e.CategoryId != null)
            .GroupBy(e => e.Category!.Name)
            .Select(g => new AdminCategoryStat
            {
                CategoryName = g.Key,
                Count = g.Count()
            })
            .ToListAsync();

        var topSenders = await _context.Emails
            .Where(e => !e.IsDraft)
            .GroupBy(e => e.SenderId)
            .Select(g => new { SenderId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        var topSenderUsers = new List<AdminTopUserStat>();
        foreach (var sender in topSenders)
        {
            var user = await _userManager.FindByIdAsync(sender.SenderId);
            if (user != null)
            {
                topSenderUsers.Add(new AdminTopUserStat
                {
                    UserId = user.Id,
                    UserName = $"{user.Name} {user.Surname}",
                    Email = user.Email ?? "",
                    Count = sender.Count
                });
            }
        }

        var viewModel = new AdminStatisticsViewModel
        {
            DailyEmailStats = emailStats,
            CategoryStats = categoryStats,
            TopSenders = topSenderUsers,
            TotalUsers = await _userManager.Users.CountAsync(),
            TotalEmails = await _context.Emails.CountAsync(),
            TotalDrafts = await _context.Emails.CountAsync(e => e.IsDraft),
            TotalAttachments = await _context.EmailAttachments.CountAsync()
        };

        ViewData["Title"] = "İstatistikler";
        ViewData["PageTitle"] = "Sistem İstatistikleri";

        return View(viewModel);
    }
}