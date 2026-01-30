using ProjectEmailNight.Entities;

namespace ProjectEmailNight.Models;

// ========== ADMIN DASHBOARD ==========
public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalEmails { get; set; }
    public int TotalEmailsToday { get; set; }
    public int ActiveUsersToday { get; set; }
    public List<AppUser> RecentUsers { get; set; } = new();
    public List<Email> RecentEmails { get; set; } = new();
}

// ========== ADMIN USER ==========
public class AdminUserViewModel
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Surname { get; set; } = "";
    public string FullName => $"{Name} {Surname}";
    public string Initials => $"{Name?[0]}{Surname?[0]}";
    public string Email { get; set; } = "";
    public List<string> Roles { get; set; } = new();
    public int EmailCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; }
}

public class AdminUserListViewModel
{
    public List<AdminUserViewModel> Users { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
}

public class AdminUserDetailViewModel
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Surname { get; set; } = "";
    public string FullName => $"{Name} {Surname}";
    public string Initials => $"{Name?[0]}{Surname?[0]}";
    public string Email { get; set; } = "";
    public List<string> Roles { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int SentEmailsCount { get; set; }
    public int ReceivedEmailsCount { get; set; }
    public int DraftsCount { get; set; }
    public List<Email> RecentEmails { get; set; } = new();
}

// ========== ADMIN EMAIL ==========
public class AdminEmailListViewModel
{
    public List<Email> Emails { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public string? SearchQuery { get; set; }
}

// ========== ADMIN STATISTICS ==========
public class AdminStatisticsViewModel
{
    public List<AdminDailyEmailStat> DailyEmailStats { get; set; } = new();
    public List<AdminCategoryStat> CategoryStats { get; set; } = new();
    public List<AdminTopUserStat> TopSenders { get; set; } = new();
    public int TotalUsers { get; set; }
    public int TotalEmails { get; set; }
    public int TotalDrafts { get; set; }
    public int TotalAttachments { get; set; }
}

public class AdminDailyEmailStat
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}

public class AdminCategoryStat
{
    public string CategoryName { get; set; } = "";
    public int Count { get; set; }
}

public class AdminTopUserStat
{
    public string UserId { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Email { get; set; } = "";
    public int Count { get; set; }
}