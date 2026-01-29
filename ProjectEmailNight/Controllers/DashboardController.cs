using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectEmailNight.Context;
using ProjectEmailNight.Entities;
using ProjectEmailNight.Models;

namespace ProjectEmailNight.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly EmailContext _context;
    private readonly UserManager<AppUser> _userManager;

    public DashboardController(EmailContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var userId = user.Id;

        // ViewBag'e kullanıcı bilgilerini ekle
        ViewBag.CurrentUser = user;
        ViewBag.UnreadCount = await _context.Emails
            .CountAsync(e => e.ReceiverId == userId && !e.IsRead && !e.IsDeleted && !e.ReceiverDeleted);

        // İstatistikleri hesapla
        var receivedEmails = await _context.Emails
            .Where(e => e.ReceiverId == userId && !e.IsDeleted && !e.ReceiverDeleted && !e.IsDraft)
            .ToListAsync();

        var sentEmails = await _context.Emails
            .Where(e => e.SenderId == userId && !e.IsDeleted && !e.SenderDeleted && !e.IsDraft)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var weekAgo = now.AddDays(-7);
        var twoWeeksAgo = now.AddDays(-14);

        // Bu hafta ve geçen hafta karşılaştırması
        var thisWeekReceived = receivedEmails.Count(e => e.CreatedAt >= weekAgo);
        var lastWeekReceived = receivedEmails.Count(e => e.CreatedAt >= twoWeeksAgo && e.CreatedAt < weekAgo);
        var thisWeekSent = sentEmails.Count(e => e.CreatedAt >= weekAgo);
        var lastWeekSent = sentEmails.Count(e => e.CreatedAt >= twoWeeksAgo && e.CreatedAt < weekAgo);

        // Haftalık email verisi (son 7 gün)
        var weeklyData = new List<ChartDataPoint>();
        for (int i = 6; i >= 0; i--)
        {
            var date = now.AddDays(-i).Date;
            var dayReceived = receivedEmails.Count(e => e.CreatedAt.Date == date);
            var daySent = sentEmails.Count(e => e.CreatedAt.Date == date);
            weeklyData.Add(new ChartDataPoint
            {
                Label = date.ToString("ddd"),
                Value = dayReceived,
                SecondValue = daySent
            });
        }

        // Kategori istatistikleri
        var categoryStats = await _context.Emails
            .Where(e => e.ReceiverId == userId && !e.IsDeleted && !e.ReceiverDeleted && e.CategoryId != null)
            .GroupBy(e => new { e.CategoryId, e.Category!.Name, e.Category.Color })
            .Select(g => new CategoryStat
            {
                Name = g.Key.Name,
                Color = g.Key.Color ?? "#667eea",
                Count = g.Count()
            })
            .ToListAsync();

        var totalCategoryEmails = categoryStats.Sum(c => c.Count);
        foreach (var stat in categoryStats)
        {
            stat.Percentage = totalCategoryEmails > 0 ? Math.Round((double)stat.Count / totalCategoryEmails * 100, 1) : 0;
        }

        // Son gelen emailler
        var recentEmails = await _context.Emails
            .Include(e => e.Sender)
            .Include(e => e.Category)
            .Where(e => e.ReceiverId == userId && !e.IsDeleted && !e.ReceiverDeleted && !e.IsDraft)
            .OrderByDescending(e => e.CreatedAt)
            .Take(5)
            .Select(e => new RecentEmailDto
            {
                Id = e.Id,
                SenderName = e.Sender.Name + " " + e.Sender.Surname,
                SenderInitials = e.Sender.Name.Substring(0, 1) + e.Sender.Surname.Substring(0, 1),
                Subject = e.Subject,
                Preview = e.Body.Length > 50 ? e.Body.Substring(0, 50) + "..." : e.Body,
                CreatedAt = e.CreatedAt,
                IsRead = e.IsRead,
                IsStarred = e.IsStarred,
                CategoryColor = e.Category != null ? e.Category.Color : "#667eea"
            })
            .ToListAsync();

        var viewModel = new DashboardViewModel
        {
            TotalEmails = receivedEmails.Count + sentEmails.Count,
            ReceivedEmails = receivedEmails.Count,
            SentEmails = sentEmails.Count,
            UnreadEmails = receivedEmails.Count(e => !e.IsRead),
            StarredEmails = receivedEmails.Count(e => e.IsStarred) + sentEmails.Count(e => e.IsStarred),
            DraftEmails = await _context.Emails.CountAsync(e => e.SenderId == userId && e.IsDraft),
            
            ReceivedChangePercent = lastWeekReceived > 0 ? Math.Round((double)(thisWeekReceived - lastWeekReceived) / lastWeekReceived * 100, 1) : 0,
            SentChangePercent = lastWeekSent > 0 ? Math.Round((double)(thisWeekSent - lastWeekSent) / lastWeekSent * 100, 1) : 0,
            
            WeeklyEmailData = weeklyData,
            CategoryStats = categoryStats,
            RecentEmails = recentEmails
        };

        ViewData["Title"] = "Kontrol Paneli";
        ViewData["PageTitle"] = "Genel Görünüm";

        return View(viewModel);
    }
}