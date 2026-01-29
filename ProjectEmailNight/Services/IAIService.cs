namespace ProjectEmailNight.Services;

public interface IAIService
{
    Task<string> GenerateSummaryAsync(string subject, string body);
    Task<int> CategorizeEmailAsync(string subject, string body);
    Task<string> GenerateReplyAsync(string originalSubject, string originalBody, string replyTone = "professional");
    Task<EmailAnalysisResult> AnalyzeEmailAsync(string subject, string body);
}

public class EmailAnalysisResult
{
    public string Summary { get; set; } = "";
    public int CategoryId { get; set; } = 1;
    public string CategoryName { get; set; } = "Birincil";
    public int Priority { get; set; } = 3; // 1-5 (1 en yüksek)
    public List<string> Keywords { get; set; } = new();
    public string Sentiment { get; set; } = "neutral"; // positive, negative, neutral
}