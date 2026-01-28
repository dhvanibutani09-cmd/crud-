using Microsoft.Extensions.Caching.Memory;
using System.Net.Http;
using System.Text.Json;
using System.Web;

namespace EmployeeCrudApp.Services
{
    public class GoogleTranslationService : ITranslationService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private const string BaseUrl = "https://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}";

        public GoogleTranslationService(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        public async Task<string> TranslateAsync(string text, string targetLanguage, string sourceLanguage = "auto")
        {
            if (string.IsNullOrWhiteSpace(text)) return text;
            if (sourceLanguage == targetLanguage) return text;

            string cacheKey = $"trans:{sourceLanguage}:{targetLanguage}:{text.GetHashCode()}";
            if (_cache.TryGetValue(cacheKey, out string cachedTranslation))
            {
                return cachedTranslation;
            }

            try
            {
                var sl = string.IsNullOrEmpty(sourceLanguage) ? "auto" : sourceLanguage;
                var url = string.Format(BaseUrl, sl, targetLanguage, HttpUtility.UrlEncode(text));
                var response = await _httpClient.GetStringAsync(url);
                
                using var doc = JsonDocument.Parse(response);
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 0)
                {
                    var paragraphs = root[0];
                    var translation = "";
                    foreach (var p in paragraphs.EnumerateArray())
                    {
                         if(p.ValueKind == JsonValueKind.Array && p.GetArrayLength() > 0)
                        {
                            translation += p[0].GetString();
                        }
                    }

                    if (!string.IsNullOrEmpty(translation))
                    {
                        var cacheOpts = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromHours(24));
                        _cache.Set(cacheKey, translation, cacheOpts);
                        return translation;
                    }
                }
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Translation failed: {ex.Message}");
            }

            return text;
        }

        public async Task<Dictionary<string, string>> TranslateBatchAsync(List<string> texts, string targetLanguage, string sourceLanguage = "auto")
        {
            var results = new Dictionary<string, string>();
            var uniqueTexts = texts.Distinct().Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
            
            var tasks = uniqueTexts.Select(async text => 
            {
                var trans = await TranslateAsync(text, targetLanguage, sourceLanguage);
                return new { Original = text, Translated = trans };
            });

            var outcomes = await Task.WhenAll(tasks);
            foreach(var item in outcomes)
            {
                results[item.Original] = item.Translated;
            }

            return results;
        }

        public async Task<List<LanguageInfo>> GetLanguagesAsync()
        {
            return await Task.FromResult(new List<LanguageInfo>
            {
                new LanguageInfo { Code = "en", Name = "English" },
                new LanguageInfo { Code = "hi", Name = "हिन्दी (Hindi)" },
                new LanguageInfo { Code = "gu", Name = "ગુજરાતી (Gujarati)" },
                new LanguageInfo { Code = "mr", Name = "मराठी (Marathi)" },
                new LanguageInfo { Code = "bn", Name = "বাংলা (Bengali)" },
                new LanguageInfo { Code = "ta", Name = "தமிழ் (Tamil)" },
                new LanguageInfo { Code = "te", Name = "తెలుగు (Telugu)" },
                new LanguageInfo { Code = "kn", Name = "ಕನ್ನಡ (Kannada)" },
                new LanguageInfo { Code = "ml", Name = "മലയാളം (Malayalam)" },
                new LanguageInfo { Code = "pa", Name = "ਪੰਜਾਬੀ (Punjabi)" },
                new LanguageInfo { Code = "ur", Name = "اردو (Urdu)" },
                new LanguageInfo { Code = "or", Name = "ଓଡ଼ିଆ (Odia)" },
                new LanguageInfo { Code = "as", Name = "অসমীয়া (Assamese)" },
                new LanguageInfo { Code = "sa", Name = "संस्कृतम् (Sanskrit)" },
                new LanguageInfo { Code = "ne", Name = "नेपाली (Nepali)" },
                new LanguageInfo { Code = "kok", Name = "कोंकणी (Konkani)" },
                new LanguageInfo { Code = "sd", Name = "सिंधी (Sindhi)" },
                new LanguageInfo { Code = "ks", Name = "कश्मीरी (Kashmiri)" },
                new LanguageInfo { Code = "mai", Name = "मैथिली (Maithili)" },
                new LanguageInfo { Code = "doi", Name = "डोगरी (Dogri)" },
                new LanguageInfo { Code = "brx", Name = "बोडो (Bodo)" },
                new LanguageInfo { Code = "sat", Name = "संताली (Santhali)" },
                new LanguageInfo { Code = "es", Name = "Español (Spanish)" },
                new LanguageInfo { Code = "fr", Name = "Français (French)" },
                new LanguageInfo { Code = "de", Name = "Deutsch (German)" },
                new LanguageInfo { Code = "it", Name = "Italiano (Italian)" },
                new LanguageInfo { Code = "pt", Name = "Português (Portuguese)" },
                new LanguageInfo { Code = "ru", Name = "Русский (Russian)" },
                new LanguageInfo { Code = "ja", Name = "日本語 (Japanese)" },
                new LanguageInfo { Code = "zh", Name = "中文 (Chinese)" },
                new LanguageInfo { Code = "ko", Name = "한국어 (Korean)" },
                new LanguageInfo { Code = "ar", Name = "العربية (Arabic)" },
                new LanguageInfo { Code = "tr", Name = "Türkçe (Turkish)" },
                new LanguageInfo { Code = "vi", Name = "Tiếng Việt (Vietnamese)" },
                new LanguageInfo { Code = "th", Name = "ไทย (Thai)" },
                new LanguageInfo { Code = "id", Name = "Bahasa Indonesia (Indonesian)" },
                new LanguageInfo { Code = "ms", Name = "Bahasa Melayu (Malay)" },
                new LanguageInfo { Code = "nl", Name = "Nederlands (Dutch)" },
                new LanguageInfo { Code = "pl", Name = "Polski (Polish)" },
                new LanguageInfo { Code = "sv", Name = "Svenska (Swedish)" },
                new LanguageInfo { Code = "no", Name = "Norsk (Norwegian)" },
                new LanguageInfo { Code = "da", Name = "Dansk (Danish)" },
                new LanguageInfo { Code = "fi", Name = "Suomi (Finnish)" },
                new LanguageInfo { Code = "el", Name = "Ελληνικά (Greek)" },
                new LanguageInfo { Code = "he", Name = "עברית (Hebrew)" },
                new LanguageInfo { Code = "fa", Name = "فارसी (Persian)" },
                new LanguageInfo { Code = "uk", Name = "Українська (Ukrainian)" },
                new LanguageInfo { Code = "cs", Name = "Čeština (Czech)" },
                new LanguageInfo { Code = "hu", Name = "Magyar (Hungarian)" },
                new LanguageInfo { Code = "ro", Name = "Română (Romanian)" },
                new LanguageInfo { Code = "bg", Name = "Български (Bulgarian)" }
            }.OrderBy(l => l.Name).ToList());
        }
    }
}
