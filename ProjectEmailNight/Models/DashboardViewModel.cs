// Models/DashboardViewModel.cs

namespace ProjectEmailNight.Models;

public class DashboardViewModel
{
    // Genel İstatistikler
    public int TotalEmails { get; set; }
    public int ReceivedEmails { get; set; }
    public int SentEmails { get; set; }
    public int UnreadEmails { get; set; }
    public int StarredEmails { get; set; }
    public int DraftEmails { get; set; }
    
    // Yüzdelik Değişimler
    public double ReceivedChangePercent { get; set; }
    public double SentChangePercent { get; set; }
    public double UnreadChangePercent { get; set; }
    
    // Performans Metrikleri (YENİ)
    public double ReadRate { get; set; } // Okunma oranı %
    public double ResponseRate { get; set; } // Yanıtlama oranı %
    public double AvgResponseTimeHours { get; set; } // Ortalama yanıt süresi (saat)
    public int RepliedCount { get; set; } // Bu hafta yanıtlanan
    
    // Grafik Verileri
    public List<ChartDataPoint> WeeklyEmailData { get; set; } = new();
    public List<ChartDataPoint> LastWeekEmailData { get; set; } = new(); // Karşılaştırma için (YENİ)
    public List<CategoryStat> CategoryStats { get; set; } = new();
    
    // Aktivite Haritası (YENİ)
    public List<HeatmapData> HeatmapData { get; set; } = new();
    
    // En Çok İletişim Kurulan Kişiler (YENİ)
    public List<TopContactDto> TopContacts { get; set; } = new();
    
    // Son Aktiviteler (YENİ)
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
    
    // Son Emailler
    public List<RecentEmailDto> RecentEmails { get; set; } = new();
    
    // AI Insights (YENİ)
    public List<AiInsightDto> AiInsights { get; set; } = new();
}

public class ChartDataPoint
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
    public int? SecondValue { get; set; }
}

public class CategoryStat
{
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#667eea";
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class RecentEmailDto
{
    public int Id { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderInitials { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Preview { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public bool IsStarred { get; set; }
    public string CategoryColor { get; set; } = "#667eea";
}

// YENİ CLASSLAR
public class HeatmapData
{
    public string Day { get; set; } = string.Empty; // Pzt, Sal, vs.
    public int Hour { get; set; } // 0-23
    public int Count { get; set; }
    public double Intensity { get; set; } // 0-1 arası
}

public class TopContactDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Initials { get; set; } = string.Empty;
    public string AvatarColor { get; set; } = "#7c3aed";
    public int EmailCount { get; set; }
}

public class RecentActivityDto
{
    public string SenderName { get; set; } = string.Empty;
    public string SenderInitials { get; set; } = string.Empty;
    public string AvatarColor { get; set; } = "#7c3aed";
    public string Description { get; set; } = string.Empty;
    public string TimeAgo { get; set; } = string.Empty;
    public int? EmailId { get; set; }
}

public class AiInsightDto
{
    public string Icon { get; set; } = "💡";
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}