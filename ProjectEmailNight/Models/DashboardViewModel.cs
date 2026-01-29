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
    
    // Grafik Verileri
    public List<ChartDataPoint> WeeklyEmailData { get; set; } = new();
    public List<ChartDataPoint> ReceivedVsSentData { get; set; } = new();
    public List<CategoryStat> CategoryStats { get; set; } = new();
    
    // Son Emailler
    public List<RecentEmailDto> RecentEmails { get; set; } = new();
}

public class ChartDataPoint
{
    public string Label { get; set; }
    public int Value { get; set; }
    public int? SecondValue { get; set; }
}

public class CategoryStat
{
    public string Name { get; set; }
    public string Color { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class RecentEmailDto
{
    public int Id { get; set; }
    public string SenderName { get; set; }
    public string SenderInitials { get; set; }
    public string Subject { get; set; }
    public string Preview { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public bool IsStarred { get; set; }
    public string CategoryColor { get; set; }
}