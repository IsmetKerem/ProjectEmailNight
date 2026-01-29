using Mscc.GenerativeAI;
using System.Text.Json;
using System.Text.RegularExpressions;
using Mscc.GenerativeAI.Types;

namespace ProjectEmailNight.Services;

public class GeminiAIService : IAIService
{
    private readonly GoogleAI _googleAI;
    private readonly GenerativeModel _model;
    private readonly ILogger<GeminiAIService> _logger;
    private readonly bool _isConfigured;

    public GeminiAIService(IConfiguration configuration, ILogger<GeminiAIService> logger)
    {
        _logger = logger;
        var apiKey = configuration["GeminiSettings:ApiKey"] ?? "";
        
        _isConfigured = !string.IsNullOrEmpty(apiKey) && apiKey != "YOUR_GEMINI_API_KEY_HERE";
        
        if (_isConfigured)
        {
            _googleAI = new GoogleAI(apiKey);
            _model = _googleAI.GenerativeModel(Model.Gemini3Flash);
        }
    }

    public async Task<string> GenerateSummaryAsync(string subject, string body)
    {
        if (!_isConfigured)
        {
            return GenerateFallbackSummary(subject, body);
        }

        try
        {
            var plainText = StripHtml(body);
            
            var prompt = $@"Aşağıdaki e-postayı analiz et ve Türkçe olarak maksimum 2 cümlelik kısa bir özet yaz.
Özet, e-postanın ana amacını net şekilde belirtmeli.
Sadece özeti yaz, başka açıklama ekleme.

Konu: {subject}

İçerik:
{TruncateText(plainText, 1500)}";

            var response = await _model.GenerateContent(prompt);
            var summary = response?.Text?.Trim();
            
            if (!string.IsNullOrEmpty(summary))
            {
                return summary;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini özet oluşturma hatası");
        }

        return GenerateFallbackSummary(subject, body);
    }

    public async Task<int> CategorizeEmailAsync(string subject, string body)
    {
        if (!_isConfigured)
        {
            return CategorizeWithKeywords(subject, body);
        }

        try
        {
            var plainText = StripHtml(body);
            
            var prompt = $@"Aşağıdaki e-postayı analiz et ve en uygun kategori numarasını belirle.

KATEGORİLER:
1 = Birincil (kişisel, genel yazışmalar, önemli bildirimler)
2 = Sosyal (davetler, kutlamalar, sosyal medya, etkinlikler)
3 = Promosyon (reklamlar, indirimler, kampanyalar, pazarlama)
4 = İş (profesyonel, toplantılar, projeler, acil iş konuları)
5 = Spam (istenmeyen, şüpheli, zararlı içerik)

SADECE kategori numarasını yaz (1, 2, 3, 4 veya 5). Başka hiçbir şey yazma.

Konu: {subject}
İçerik: {TruncateText(plainText, 1000)}";

            var response = await _model.GenerateContent(prompt);
            var result = response?.Text?.Trim();
            
            if (!string.IsNullOrEmpty(result))
            {
                // Sadece sayıyı al
                var match = Regex.Match(result, @"\d");
                if (match.Success && int.TryParse(match.Value, out int categoryId))
                {
                    if (categoryId >= 1 && categoryId <= 5)
                    {
                        return categoryId;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini kategorileme hatası");
        }

        return CategorizeWithKeywords(subject, body);
    }

    public async Task<string> GenerateReplyAsync(string originalSubject, string originalBody, string replyTone = "professional")
    {
        if (!_isConfigured)
        {
            return "";
        }

        try
        {
            var plainText = StripHtml(originalBody);
            
            var toneDescription = replyTone switch
            {
                "friendly" => "samimi ve sıcak",
                "formal" => "resmi ve kurumsal",
                "short" => "kısa ve öz",
                "thankful" => "teşekkür içeren",
                _ => "profesyonel ve nazik"
            };

            var prompt = $@"Aşağıdaki e-postaya {toneDescription} bir Türkçe yanıt taslağı oluştur.

Kurallar:
- Yanıt doğal ve samimi olmalı
- Uygun bir selamlama ile başla
- Konuya uygun içerik yaz
- İmza kısmını boş bırak (kullanıcı kendi imzasını ekleyecek)
- Sadece yanıt metnini yaz

Orijinal E-posta Konusu: {originalSubject}
Orijinal E-posta İçeriği:
{TruncateText(plainText, 1000)}";

            var response = await _model.GenerateContent(prompt);
            return response?.Text?.Trim() ?? "";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini yanıt oluşturma hatası");
            return "";
        }
    }

    public async Task<EmailAnalysisResult> AnalyzeEmailAsync(string subject, string body)
    {
        var result = new EmailAnalysisResult();

        if (!_isConfigured)
        {
            result.Summary = GenerateFallbackSummary(subject, body);
            result.CategoryId = CategorizeWithKeywords(subject, body);
            result.CategoryName = GetCategoryName(result.CategoryId);
            return result;
        }

        try
        {
            var plainText = StripHtml(body);
            
            var prompt = $@"Aşağıdaki e-postayı analiz et ve JSON formatında yanıt ver.

E-posta Konusu: {subject}
E-posta İçeriği: {TruncateText(plainText, 1500)}

Yanıtı SADECE aşağıdaki JSON formatında ver, başka hiçbir şey yazma:
{{
    ""summary"": ""maksimum 2 cümlelik Türkçe özet"",
    ""categoryId"": kategori numarası (1=Birincil, 2=Sosyal, 3=Promosyon, 4=İş, 5=Spam),
    ""priority"": öncelik seviyesi (1=çok acil, 2=acil, 3=normal, 4=düşük, 5=çok düşük),
    ""keywords"": [""anahtar"", ""kelimeler"", ""listesi""],
    ""sentiment"": ""positive/negative/neutral""
}}";

            var response = await _model.GenerateContent(prompt);
            var jsonText = response?.Text?.Trim();
            
            if (!string.IsNullOrEmpty(jsonText))
            {
                // JSON'ı temizle (bazen markdown code block içinde geliyor)
                jsonText = Regex.Replace(jsonText, @"```json\s*", "");
                jsonText = Regex.Replace(jsonText, @"```\s*", "");
                jsonText = jsonText.Trim();
                
                var parsed = JsonSerializer.Deserialize<EmailAnalysisResult>(jsonText, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                if (parsed != null)
                {
                    result = parsed;
                    result.CategoryName = GetCategoryName(result.CategoryId);
                    return result;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini tam analiz hatası");
        }

        // Fallback
        result.Summary = GenerateFallbackSummary(subject, body);
        result.CategoryId = CategorizeWithKeywords(subject, body);
        result.CategoryName = GetCategoryName(result.CategoryId);
        return result;
    }

    #region Helper Methods

    private string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html)) return "";
        
        var text = Regex.Replace(html, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"<p[^>]*>", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"</p>", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"<[^>]+>", "", RegexOptions.IgnoreCase);
        text = System.Net.WebUtility.HtmlDecode(text);
        text = Regex.Replace(text, @"\n{3,}", "\n\n");
        
        return text.Trim();
    }

    private string TruncateText(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;
        
        return text.Substring(0, maxLength) + "...";
    }

    private string GenerateFallbackSummary(string subject, string body)
    {
        var plainText = StripHtml(body);
        
        if (string.IsNullOrEmpty(plainText))
            return $"Konu: {subject}";
        
        var firstSentence = Regex.Match(plainText, @"^[^.!?]*[.!?]");
        if (firstSentence.Success && firstSentence.Value.Length > 20)
        {
            return firstSentence.Value.Trim();
        }
        
        return plainText.Length > 100 
            ? plainText.Substring(0, 100) + "..." 
            : plainText;
    }

    private int CategorizeWithKeywords(string subject, string body)
    {
        var text = $"{subject} {StripHtml(body)}".ToLower();

        // Spam keywords
        if (text.Contains("kazandınız") || text.Contains("tıklayın") || text.Contains("hemen şimdi") ||
            text.Contains("ücretsiz hediye") || text.Contains("penis") || text.Contains("viagra"))
            return 5;

        // İş keywords
        if (text.Contains("toplantı") || text.Contains("meeting") || text.Contains("acil") ||
            text.Contains("urgent") || text.Contains("proje") || text.Contains("deadline") ||
            text.Contains("rapor") || text.Contains("sprint") || text.Contains("server") ||
            text.Contains("production") || text.Contains("deployment") || text.Contains("müşteri"))
            return 4;

        // Promosyon keywords
        if (text.Contains("indirim") || text.Contains("kampanya") || text.Contains("fırsat") ||
            text.Contains("promosyon") || text.Contains("% off") || text.Contains("sale") ||
            text.Contains("teklif") || text.Contains("bedava") || text.Contains("ücretsiz"))
            return 3;

        // Sosyal keywords
        if (text.Contains("davet") || text.Contains("parti") || text.Contains("kutlama") ||
            text.Contains("doğum günü") || text.Contains("yemek") || text.Contains("buluşma") ||
            text.Contains("etkinlik") || text.Contains("düğün") || text.Contains("mezuniyet"))
            return 2;

        // Varsayılan: Birincil
        return 1;
    }

    private string GetCategoryName(int categoryId)
    {
        return categoryId switch
        {
            1 => "Birincil",
            2 => "Sosyal",
            3 => "Promosyon",
            4 => "İş",
            5 => "Spam",
            _ => "Birincil"
        };
    }

    #endregion
}