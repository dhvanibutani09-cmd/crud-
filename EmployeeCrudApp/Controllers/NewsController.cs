using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

namespace EmployeeCrudApp.Controllers
{
    public class NewsController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiKey;

        public NewsController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _apiKey = _configuration["NewsApiSettings:ApiKey"];
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetNews(string country = "", string category = "", string query = "")
        {
            if (string.IsNullOrEmpty(_apiKey) || _apiKey == "YOUR_NEWS_API_KEY_HERE")
            {
                return Json(new { status = "error", message = "API Key is missing or invalid in appsettings.json" });
            }

            var client = _httpClientFactory.CreateClient();
            var baseUrl = "https://newsapi.org/v2/top-headlines";
            
            var queryParams = new List<string>();
            queryParams.Add($"apiKey={_apiKey}");
            queryParams.Add("language=en"); // Default to English for global coverage

            if (!string.IsNullOrEmpty(country))
            {
                queryParams.Add($"country={country}");
            }

            if (!string.IsNullOrEmpty(category))
            {
                queryParams.Add($"category={category}");
            }

            if (!string.IsNullOrEmpty(query))
            {
                queryParams.Add($"q={query}");
            }
            
            // If no filters are provided, NewsAPI top-headlines requires at least country, category or source.
            // However, providing just language often works for 'general' global news or falls back gracefully.
            // If strictly needed, we could default category to 'general' if country is missing.
            if (string.IsNullOrEmpty(country) && string.IsNullOrEmpty(category) && string.IsNullOrEmpty(query))
            {
                 queryParams.Add("category=general");
            }

            var url = $"{baseUrl}?{string.Join("&", queryParams)}";

            try
            {
                client.DefaultRequestHeaders.Add("User-Agent", "EmployeeCrudApp");
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Content(content, "application/json");
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                return Json(new { status = "error", message = $"API Error: {response.StatusCode}", details = errorContent });
            }
            catch (System.Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetEverything(string query, string sortBy = "publishedAt")
        {
             if (string.IsNullOrEmpty(_apiKey) || _apiKey == "YOUR_NEWS_API_KEY_HERE")
            {
                return Json(new { status = "error", message = "API Key is missing or invalid in appsettings.json" });
            }

            var client = _httpClientFactory.CreateClient();
            var baseUrl = "https://newsapi.org/v2/everything";
            var url = $"{baseUrl}?apiKey={_apiKey}&q={query}&sortBy={sortBy}";

            try
            {
                client.DefaultRequestHeaders.Add("User-Agent", "EmployeeCrudApp");
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Content(content, "application/json");
                }
                return Json(new { status = "error", message = $"API Error: {response.StatusCode}" });
            }
            catch (System.Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }
        }
    }
}
