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

        var userId = user.Id; // Bu int veya string olabilir, entity'ne göre değişir

        // ViewBag'e kullanıcı bilgilerini ekle
        ViewBag.CurrentUser = user;
        ViewBag.UnreadCount = await _context.Emails
            .CountAsync(e => e.ReceiverId == userId && !e.IsRead && !e.IsDeleted && !e.ReceiverDeleted);

        var now = DateTime.UtcNow;
        var weekAgo = now.AddDays(-7);
        var twoWeeksAgo = now.AddDays(-14);

        // Email listelerini çek
        var receivedEmails = await _context.Emails
            .Include(e => e.Sender)
            .Where(e => e.ReceiverId == userId && !e.IsDeleted && !e.ReceiverDeleted && !e.IsDraft)
            .ToListAsync();

        var sentEmails = await _context.Emails
            .Include(e => e.Receiver)
            .Where(e => e.SenderId == userId && !e.IsDeleted && !e.SenderDeleted && !e.IsDraft)
            .ToListAsync();

        // Bu hafta ve geçen hafta karşılaştırması
        var thisWeekReceived = receivedEmails.Count(e => e.CreatedAt >= weekAgo);
        var lastWeekReceived = receivedEmails.Count(e => e.CreatedAt >= twoWeeksAgo && e.CreatedAt < weekAgo);
        var thisWeekSent = sentEmails.Count(e => e.CreatedAt >= weekAgo);
        var lastWeekSent = sentEmails.Count(e => e.CreatedAt >= twoWeeksAgo && e.CreatedAt < weekAgo);

        // Haftalık email verisi (son 7 gün) - Bu hafta
        var weeklyData = new List<ChartDataPoint>();
        for (int i = 6; i >= 0; i--)
        {
            var date = now.AddDays(-i).Date;
            var dayReceived = receivedEmails.Count(e => e.CreatedAt.Date == date);
            var daySent = sentEmails.Count(e => e.CreatedAt.Date == date);
            weeklyData.Add(new ChartDataPoint
            {
                Label = date.ToString("ddd", new System.Globalization.CultureInfo("tr-TR")),
                Value = dayReceived,
                SecondValue = daySent
            });
        }

        // Geçen hafta verisi (karşılaştırma için)
        var lastWeekData = new List<ChartDataPoint>();
        for (int i = 13; i >= 7; i--)
        {
            var date = now.AddDays(-i).Date;
            var dayReceived = receivedEmails.Count(e => e.CreatedAt.Date == date);
            var daySent = sentEmails.Count(e => e.CreatedAt.Date == date);
            lastWeekData.Add(new ChartDataPoint
            {
                Label = date.ToString("ddd", new System.Globalization.CultureInfo("tr-TR")),
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

        // Heatmap verisi (saatlik aktivite)
        var heatmapData = GenerateHeatmapData(receivedEmails, sentEmails);

        // En çok iletişim kurulan kişiler
        var topContacts = GetTopContacts(receivedEmails);

        // Son aktiviteler
        var recentActivities = GetRecentActivities(receivedEmails);

        // Son gelen emailler
        var recentEmails = receivedEmails
            .OrderByDescending(e => e.CreatedAt)
            .Take(5)
            .Select(e => new RecentEmailDto
            {
                Id = e.Id,
                SenderName = (e.Sender?.Name ?? "") + " " + (e.Sender?.Surname ?? ""),
                SenderInitials = GetInitials(e.Sender?.Name, e.Sender?.Surname),
                Subject = e.Subject ?? "",
                Preview = string.IsNullOrEmpty(e.Body) ? "" : (e.Body.Length > 50 ? e.Body.Substring(0, 50) + "..." : e.Body),
                CreatedAt = e.CreatedAt,
                IsRead = e.IsRead,
                IsStarred = e.IsStarred,
                CategoryColor = e.Category?.Color ?? "#667eea"
            })
            .ToList();

        // Performans metrikleri
        var readCount = receivedEmails.Count(e => e.IsRead);
        var readRate = receivedEmails.Count > 0 ? Math.Round((double)readCount / receivedEmails.Count * 100, 1) : 0;
        
        // Yanıtlama oranı (basit hesaplama)
        var repliedCount = sentEmails.Count(e => e.CreatedAt >= weekAgo);
        var responseRate = thisWeekReceived > 0 ? Math.Min(Math.Round((double)repliedCount / thisWeekReceived * 100, 1), 100) : 0;

        // AI Insights
        var aiInsights = GenerateAiInsights(
            thisWeekReceived, lastWeekReceived, 
            thisWeekSent, lastWeekSent,
            readRate, responseRate, 
            receivedEmails.Count(e => !e.IsRead),
            heatmapData
        );

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
            
            // Performans
            ReadRate = readRate,
            ResponseRate = responseRate,
            AvgResponseTimeHours = 2.4,
            RepliedCount = repliedCount,
            
            WeeklyEmailData = weeklyData,
            LastWeekEmailData = lastWeekData,
            CategoryStats = categoryStats,
            HeatmapData = heatmapData,
            TopContacts = topContacts,
            RecentActivities = recentActivities,
            RecentEmails = recentEmails,
            AiInsights = aiInsights
        };

        ViewData["Title"] = "Kontrol Paneli";
        ViewData["PageTitle"] = "Dashboard";

        return View(viewModel);
    }

    private string GetInitials(string? name, string? surname)
    {
        var n = string.IsNullOrEmpty(name) ? "?" : name.Substring(0, 1);
        var s = string.IsNullOrEmpty(surname) ? "" : surname.Substring(0, 1);
        return n + s;
    }

    private List<HeatmapData> GenerateHeatmapData(List<Email> received, List<Email> sent)
    {
        var result = new List<HeatmapData>();
        var allEmails = received.Concat(sent).ToList();
        var days = new[] { "Pzt", "Sal", "Çar", "Per", "Cum", "Cmt", "Paz" };
        
        // Max count bul (normalizasyon için)
        var maxCount = 1;
        foreach (var day in days)
        {
            for (int hour = 0; hour < 24; hour++)
            {
                var count = allEmails.Count(e => 
                    GetTurkishDayAbbr(e.CreatedAt.DayOfWeek) == day && 
                    e.CreatedAt.Hour == hour);
                if (count > maxCount) maxCount = count;
            }
        }

        foreach (var day in days)
        {
            for (int hour = 0; hour < 24; hour++)
            {
                var count = allEmails.Count(e => 
                    GetTurkishDayAbbr(e.CreatedAt.DayOfWeek) == day && 
                    e.CreatedAt.Hour == hour);
                
                result.Add(new HeatmapData
                {
                    Day = day,
                    Hour = hour,
                    Count = count,
                    Intensity = maxCount > 0 ? (double)count / maxCount : 0
                });
            }
        }

        return result;
    }

    private string GetTurkishDayAbbr(DayOfWeek day)
    {
        return day switch
        {
            DayOfWeek.Monday => "Pzt",
            DayOfWeek.Tuesday => "Sal",
            DayOfWeek.Wednesday => "Çar",
            DayOfWeek.Thursday => "Per",
            DayOfWeek.Friday => "Cum",
            DayOfWeek.Saturday => "Cmt",
            DayOfWeek.Sunday => "Paz",
            _ => ""
        };
    }

    private List<TopContactDto> GetTopContacts(List<Email> receivedEmails)
    {
        var avatarColors = new[] { "#ec4899", "#06b6d4", "#84cc16", "#f97316", "#8b5cf6" };
        
        var senders = receivedEmails
            .Where(e => e.Sender != null)
            .GroupBy(e => new { e.SenderId, e.Sender!.Name, e.Sender.Surname, e.Sender.Email })
            .Select(g => new
            {
                Name = g.Key.Name ?? "",
                Surname = g.Key.Surname ?? "",
                Email = g.Key.Email ?? "",
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToList();

        return senders.Select((s, i) => new TopContactDto
        {
            Name = s.Name + " " + s.Surname,
            Email = s.Email,
            Initials = GetInitials(s.Name, s.Surname),
            AvatarColor = avatarColors[i % avatarColors.Length],
            EmailCount = s.Count
        }).ToList();
    }

    private List<RecentActivityDto> GetRecentActivities(List<Email> receivedEmails)
    {
        var avatarColors = new[] { "#10b981", "#3b82f6", "#8b5cf6", "#f59e0b", "#ec4899" };
        
        var recentEmails = receivedEmails
            .OrderByDescending(e => e.CreatedAt)
            .Take(5)
            .ToList();

        return recentEmails.Select((e, i) => new RecentActivityDto
        {
            SenderName = (e.Sender?.Name ?? "") + " " + (e.Sender?.Surname ?? ""),
            SenderInitials = GetInitials(e.Sender?.Name, e.Sender?.Surname),
            AvatarColor = avatarColors[i % avatarColors.Length],
            Description = string.IsNullOrEmpty(e.Subject) ? "Konu yok" : (e.Subject.Length > 30 ? e.Subject.Substring(0, 30) + "..." : e.Subject),
            TimeAgo = GetTimeAgo(e.CreatedAt),
            EmailId = e.Id
        }).ToList();
    }

    private string GetTimeAgo(DateTime dateTime)
    {
        var span = DateTime.UtcNow - dateTime;
        
        if (span.TotalMinutes < 1) return "Şimdi";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} dk önce";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours} saat önce";
        if (span.TotalDays < 7) return $"{(int)span.TotalDays} gün önce";
        return dateTime.ToString("dd MMM");
    }

    private List<AiInsightDto> GenerateAiInsights(
        int thisWeekReceived, int lastWeekReceived,
        int thisWeekSent, int lastWeekSent,
        double readRate, double responseRate,
        int unreadCount, List<HeatmapData> heatmap)
    {
        var insights = new List<AiInsightDto>();

        // Yanıt süresi insight
        if (responseRate > 80)
        {
            insights.Add(new AiInsightDto
            {
                Icon = "💡",
                Title = "Harika yanıt performansı!",
                Description = $"Yanıtlama oranınız %{responseRate} ile mükemmel seviyede."
            });
        }
        else if (responseRate < 50)
        {
            insights.Add(new AiInsightDto
            {
                Icon = "⚠️",
                Title = "Yanıt bekleyen emailler var",
                Description = $"Yanıtlama oranınız %{responseRate}. Biraz daha aktif olabilirsiniz."
            });
        }

        // En yoğun saat
        var peakHour = heatmap
            .GroupBy(h => h.Hour)
            .OrderByDescending(g => g.Sum(x => x.Count))
            .FirstOrDefault();
        
        if (peakHour != null)
        {
            insights.Add(new AiInsightDto
            {
                Icon = "📊",
                Title = $"En yoğun saatiniz: {peakHour.Key:00}:00",
                Description = "Bu saatte email trafiğiniz en yüksek seviyede."
            });
        }

        // Okunmamış email uyarısı
        if (unreadCount > 10)
        {
            insights.Add(new AiInsightDto
            {
                Icon = "📬",
                Title = $"{unreadCount} okunmamış email",
                Description = "Gelen kutunuzda bekleyen emailler var."
            });
        }

        // Haftalık trend
        var changePercent = lastWeekReceived > 0 ? 
            Math.Round((double)(thisWeekReceived - lastWeekReceived) / lastWeekReceived * 100, 1) : 0;
        
        if (changePercent > 20)
        {
            insights.Add(new AiInsightDto
            {
                Icon = "📈",
                Title = $"Email trafiği %{changePercent} arttı",
                Description = "Geçen haftaya göre daha fazla email alıyorsunuz."
            });
        }
        else if (changePercent < -20)
        {
            insights.Add(new AiInsightDto
            {
                Icon = "📉",
                Title = $"Email trafiği %{Math.Abs(changePercent)} azaldı",
                Description = "Bu hafta daha sakin bir dönemdesiniz."
            });
        }

        return insights.Take(3).ToList();
    }
}