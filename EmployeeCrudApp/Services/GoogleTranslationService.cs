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
        private const string BaseUrl = "https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl={0}&dt=t&q={1}";

        public GoogleTranslationService(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        public async Task<string> TranslateAsync(string text, string targetLanguage)
        {
            if (string.IsNullOrWhiteSpace(text) || targetLanguage == "en") return text;

            string cacheKey = $"trans:{targetLanguage}:{text.GetHashCode()}";
            if (_cache.TryGetValue(cacheKey, out string cachedTranslation))
            {
                return cachedTranslation;
            }

            try
            {
                var url = string.Format(BaseUrl, targetLanguage, HttpUtility.UrlEncode(text));
                var response = await _httpClient.GetStringAsync(url);
                
                // Parse format: [[["translated","source",null,null,1]],...]
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
                // Fallback to original
                Console.WriteLine($"Translation failed: {ex.Message}");
            }

            return text;
        }

        public async Task<Dictionary<string, string>> TranslateBatchAsync(List<string> texts, string targetLanguage)
        {
            var results = new Dictionary<string, string>();
            // Since the free API doesn't support batching well, we do parallel requests but limited
            // For production, use paid API with proper batching.
            // Here we just map over items.
            
            var uniqueTexts = texts.Distinct().Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
            
            var tasks = uniqueTexts.Select(async text => 
            {
                var trans = await TranslateAsync(text, targetLanguage);
                return new { Original = text, Translated = trans };
            });

            var outcomes = await Task.WhenAll(tasks);
            foreach(var item in outcomes)
            {
                results[item.Original] = item.Translated;
            }

            return results;
        }
    }
}
